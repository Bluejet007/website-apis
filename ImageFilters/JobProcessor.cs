using Azure.Storage.Blobs;
using ImageFilters.Common;
using ImageFilters.Kuwahara;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using JobLibrary;

namespace ImageFilters;

public class JobProcessor
{
    private readonly ILogger<JobProcessor> _logger;

    public JobProcessor(ILogger<JobProcessor> logger)
    {
        _logger = logger;
    }

    [Function(nameof(JobProcessor))]
    [BlobOutput("outputs/{fileName}")]
    public byte[] Run([QueueTrigger("image-jobs", Connection = "AzureWebJobsStorage")] QueuedJob job, [BlobInput("inputs/{fileName}")] BlobClient inputBlob)
    {
        _logger.LogInformation($"C# Queue trigger function processed: {job.FileName}, {job.JobType}, {job.Parameters}");
        MemoryStream outputStream;

        using (Stream inputStream = inputBlob.OpenRead())
        {
            _logger.LogInformation("Converting to bytes");
            ByteImage byteImage = new(inputStream);
            _logger.LogInformation("Conversion completed");

            _logger.LogInformation("Processing image");
            byteImage = ColourSpace.SrgbToLinear(byteImage);
            ByteImage outputImage = job.JobType switch
            {
                JobType.greyscale => CommonFilters.GreyScale(byteImage),
                JobType.baseKuwa => KuwaharaFilters.BaseKuwahara(byteImage, (byte)job.Parameters[0]),
                _ => byteImage
            };
            outputImage = ColourSpace.LinearToSrgb(outputImage);
            _logger.LogInformation("Processing completed");

            _logger.LogInformation("Converting to stream");
            outputStream = outputImage.ToMemoryStream();
            _logger.LogInformation("Conversion completed");
        }

        inputBlob.DeleteIfExists();

        return outputStream.ToArray();
    }
}