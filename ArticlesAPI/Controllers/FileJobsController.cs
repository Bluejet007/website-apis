using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;

namespace ArticlesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileJobsController : ControllerBase
    {
        private readonly BlobContainerClient containerClient;

        public FileJobsController(BlobServiceClient client)
        {
            this.containerClient = client.GetBlobContainerClient("inputs");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> uploadImage([FromForm] FileJob job)
        {
            // Validate file
            if (job.File == null || job.File.Length == 0)
            {
                return this.BadRequest("No file provided.");
            }
            else if (job.JobType == String.Empty)
            {
                return this.BadRequest("No job selected.");
            }

            // Create job
            string jobId = Guid.NewGuid().ToString();

            // Create blob
            string fileName = jobId + Path.GetExtension(job.File.FileName);
            await containerClient.CreateIfNotExistsAsync();
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            // Upload file to blob
            using(Stream stream = job.File.OpenReadStream())
            {
                await blobClient.UploadAsync(stream);
            }

            return this.Ok(new {jobid = jobId});
        }
    }
}
