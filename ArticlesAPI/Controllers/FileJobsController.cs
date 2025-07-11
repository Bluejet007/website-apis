using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Azure.Storage.Sas;
using JobLibrary;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebsiteAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileJobsController : ControllerBase
    {

        private readonly BlobContainerClient inputContainerClient;
        private readonly BlobContainerClient outputContainerClient;

        private readonly QueueClient queueClient;

        private readonly BlobSasBuilder blobSasBuilder;

        public FileJobsController(BlobServiceClient blobServiceClient, QueueServiceClient queueServiceClient)
        {
            this.inputContainerClient = blobServiceClient.GetBlobContainerClient("inputs");
            Task t1 = inputContainerClient.CreateIfNotExistsAsync();
            this.outputContainerClient = blobServiceClient.GetBlobContainerClient("outputs");
            Task t2 = outputContainerClient.CreateIfNotExistsAsync();

            this.queueClient = queueServiceClient.GetQueueClient("image-jobs");
            Task t3 = queueClient.CreateIfNotExistsAsync();

            this.blobSasBuilder = new
            (
                BlobSasPermissions.PermanentDelete | BlobSasPermissions.Read,
                DateTime.UtcNow + TimeSpan.FromHours(1)
            );

            Task.WaitAll(t1, t2, t3);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobResult(string id)
        {
            // Search blobs
            await foreach(BlobItem blobItem in outputContainerClient.GetBlobsAsync(prefix: id))
            {
                id = blobItem.Name;
                break;
            }

            // Get blob client
            BlobClient blobClient = outputContainerClient.GetBlobClient(id);

            // Check if output file is ready
            if (!SupportedFileTypes.Contains(Path.GetExtension(id)) || !await blobClient.ExistsAsync())
            {
                return this.Accepted("Result not present right now");
            }
            else
            {
                return this.Ok(blobClient.GenerateSasUri(blobSasBuilder).AbsoluteUri);
            }
        }


        [HttpPost]
        public async Task<IActionResult> PostJob([FromForm] FileJob job)
        {
            // Validate file
            if (ValidatePostRequest(job, out string errorMessage))
            {
                return this.ValidationProblem(errorMessage);
            }

            // Create blob
            string fileName = Guid.NewGuid().ToString();
            BlobClient blobClient = inputContainerClient.GetBlobClient(fileName + Path.GetExtension(job.File!.FileName));

            using (Stream stream = job.File.OpenReadStream())
            {
                // Upload file to blob
                Task uploadTask = blobClient.UploadAsync(stream);

                // Construct requet message
                QueuedJob queuedJob = new(fileName + Path.GetExtension(job.File!.FileName), job.JobType, job.Parameters);
                string queueMessage = JsonSerializer.Serialize(queuedJob, queuedJob.GetType());

                // Push request message to queue
                await uploadTask;
                await queueClient.SendMessageAsync(queueMessage);
            }

            return this.Ok(fileName);
        }

        // Validate post request
        private static bool ValidatePostRequest(FileJob job, out string errorMessage)
        {
            if (job.File.Length == 0) // Check if file provided
            {
                errorMessage = "No file provided.";
                return false;
            }
            else if (!SupportedFileTypes.Contains(Path.GetExtension(job.File.ContentType))) // Check if file is supported
            {
                errorMessage = "File is not a supported image.";
                return false;
            }
            else if (job.File.Length > 25 * 1024 * 1024) // Check if file size is greater than 25MB
            {
                errorMessage = "File size is too large (> 25MB).";
                return false;
            }
            else // Return as valid
            {
                errorMessage = String.Empty;
                return true;
            }
        }
    }
}