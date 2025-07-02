using ImageFilters.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ImageFilters;

public class Function1
{
    private readonly ILogger<Function1> _logger;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }

    [Function(nameof(Function1))]
    [BlobOutput("outputs/{fileName}")]
    public byte[] Run([QueueTrigger("image-jobs", Connection = "AzureWebJobsStorage")] QueuedJob job, [BlobInput("inputs/{fileName}")] Stream inputStream)
    {
        _logger.LogInformation($"C# Queue trigger function processed: {job.FileName}, {job.JobType}, {job.Parameters}");

        _logger.LogInformation("Converting to bytes");
        ByteImage byteImage = new ByteImage(inputStream);
        _logger.LogInformation("Conversion completed");

        _logger.LogInformation("Processing image");
        ByteImage outputImage = job.JobType switch
        {
            JobType.greyscale => CommonFilters.GreyScale(byteImage),
            _ => byteImage
        };
        _logger.LogInformation("Processing completed");

        _logger.LogInformation("Converting to stream");
        MemoryStream outputStream = outputImage.ToMemoryStream();
        _logger.LogInformation("Conversion completed");

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false, true);
        return outputStream.ToArray();
    }
}