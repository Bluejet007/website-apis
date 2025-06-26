using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace WebsiteAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileJobsController : ControllerBase
    {
        private readonly HashSet<string> supportedFileTypes = new HashSet<string> { ".png", ".jpg", ".jpeg", "image/png", "image/jpg", "image/jpeg"};
        private readonly HashSet<string> supportedJobTypes = new HashSet<string> { "kuwa" };
        private readonly BlobContainerClient inputContainerClient;
        private readonly BlobContainerClient outputContainerClient;

        public FileJobsController(BlobServiceClient client)
        {
            this.inputContainerClient = client.GetBlobContainerClient("inputs");
            this.outputContainerClient = client.GetBlobContainerClient("inputs");
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
            else if (outputContainerClient.GetBlockBlobClient(id).Exists()) // Check if id exists
            {
                errorMessage = "Id doesn't exist.";
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
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(job.File.FileName);
            await inputContainerClient.CreateIfNotExistsAsync();
            BlobClient blobClient = inputContainerClient.GetBlobClient(fileName);

            // Upload file to blob
            using(Stream stream = job.File.OpenReadStream())
            {
                await blobClient.UploadAsync(stream);
            }
            return this.Ok(new {id = fileName});
        }

        // Validate get request
        bool ValidatePostRequest(FileJob job, out string errorMessage)
        {
            if (job.File == null || job.File.Length == 0) // Check if file provided
            {
                errorMessage = "No file provided.";
                return false;
            }
            else if (String.IsNullOrEmpty(job.JobType)) // Check if job type provided
            {
                errorMessage = "No job type provided.";
                return false;
            }
            else if (!supportedFileTypes.Contains(Path.GetExtension(job.File.ContentType))) // Check if file is supported
            {
                errorMessage = "File is not a supported image.";
                return false;
            }
            else if (!supportedJobTypes.Contains(job.JobType)) // Check if job type is valid
            {
                errorMessage = "Job type is not valid.";
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