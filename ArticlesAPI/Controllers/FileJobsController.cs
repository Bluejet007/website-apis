using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Queues;
using JobLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Text.Json;

namespace WebsiteAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileJobsController : ControllerBase
    {
        private readonly HashSet<string> supportedFileTypes = new HashSet<string> { ".png", ".jpg", ".jpeg", "image/png", "image/jpg", "image/jpeg"};

        private readonly BlobContainerClient inputContainerClient;
        private readonly BlobContainerClient outputContainerClient;
        private readonly QueueClient queueClient;

        public FileJobsController(BlobServiceClient blobServiceClient, QueueServiceClient queueServiceClient)
        {
            this.inputContainerClient = blobServiceClient.GetBlobContainerClient("inputs");
            this.outputContainerClient = blobServiceClient.GetBlobContainerClient("outputs");

            this.queueClient = queueServiceClient.GetQueueClient("image-jobs");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobResult(string id)
        {
            FileExtensionContentTypeProvider typeProvider = new FileExtensionContentTypeProvider();

            // Validate id
            if (ValidateGetRequest(id, out string errorMessage))
            {
                return this.ValidationProblem(errorMessage);
            }

            // Check if output file is ready
            if (!outputContainerClient.GetBlockBlobClient(id).Exists())
            {
                return this.Accepted();
            }

            // Get blob
            BlobClient client = outputContainerClient.GetBlobClient(id);
            BlobDownloadResult blobDownload;
            try
            {
                blobDownload = await client.DownloadContentAsync();
            } catch(Exception ex)
            {
                return this.NotFound(ex);
            }

            // Return file
            if (!typeProvider.TryGetContentType(client.Name, out string? mimeType))
            {
                mimeType = blobDownload.Details.ContentType;
            }

            return this.File(blobDownload.Content.ToStream(), mimeType, client.Name, true);
        }

        // Validate get request
        bool ValidateGetRequest(string id, out string errorMessage)
        {
            if (String.IsNullOrEmpty(id)) // Check if id provided
            {
                errorMessage = "No id provided.";
                return false;
            }
            else if (!supportedFileTypes.Contains(Path.GetExtension(id))) // Check if id represents a support file type
            {
                errorMessage = "Id is not a supported image.";
                return false;
            }
            else // Return as valid
            {
                errorMessage = "";
                return true;
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
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(job.File!.FileName);
            await inputContainerClient.CreateIfNotExistsAsync();
            BlobClient blobClient = inputContainerClient.GetBlobClient(fileName);

            // Upload file to blob
            using(Stream stream = job.File.OpenReadStream())
            {
                await blobClient.UploadAsync(stream);
            }

            // Construct requet message
            QueuedJob queuedJob = new QueuedJob(fileName, job.JobType, job.Parameters);
            string queueMessage = JsonSerializer.Serialize(queuedJob, queuedJob.GetType());

            // Push request message to queue
            await queueClient.SendMessageAsync(queueMessage);

            return this.Ok(new {id = fileName});
        }

        // Validate post request
        bool ValidatePostRequest(FileJob job, out string errorMessage)
        {
            if (job.File == null || job.File.Length == 0) // Check if file provided
            {
                errorMessage = "No file provided.";
                return false;
            }
            else if (!supportedFileTypes.Contains(Path.GetExtension(job.File.ContentType))) // Check if file is supported
            {
                errorMessage = "File is not a supported image.";
                return false;
            }
            else if (job.File.Length > 25 * 1024 * 1024) // Check is file size is greater than 25MB
            {
                errorMessage = "File size is too large (> 25MB).";
                return false;
            }
            else // Return as valid
            {
                errorMessage = "";
                return true;
            }
        }
    }
}