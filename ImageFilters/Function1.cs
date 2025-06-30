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
    public byte[] Run([QueueTrigger("image-jobs", Connection = "AzureWebJobsStorage")] string message, [BlobInput("inputs/{fileName}")] Stream inputStream)
    {
        _logger.LogInformation($"C# Queue trigger function processed: {message}");
        QueuedJob job = JsonSerializer.Deserialize<QueuedJob>(message)!;

        _logger.LogInformation("Converting to bytes");
        ByteImage byteImage = new ByteImage(inputStream);
        _logger.LogInformation("Conversion completed");

        _logger.LogInformation("Converting to stream");
        MemoryStream outputStream = byteImage.ToMemoryStream();
        _logger.LogInformation("Conversion completed");

        return outputStream.ToArray();
    }
}