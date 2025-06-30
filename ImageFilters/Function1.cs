using System.Drawing;
using System.Text.Json;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

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
    public async Task<byte[]> Run([QueueTrigger("image-jobs", Connection = "AzureWebJobsStorage")] string message, [BlobInput("inputs/{fileName}")] Stream inputStream)
    {
        _logger.LogInformation("C# Queue trigger function processed: {messageText}", message);
        QueuedJob job = JsonSerializer.Deserialize<QueuedJob>(message)!;

        byte[] output = new byte[inputStream.Length];
        await inputStream.ReadAsync(output, 0, output.Length);

        return output;
    }
}