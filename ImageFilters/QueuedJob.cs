using System.Text.Json.Serialization;

namespace ImageFilters
{
    public class QueuedJob
    {
        [JsonPropertyName("fileName")]
        public required string FileName { get; set; }

        [JsonPropertyName("jobType")]
        public required JobType JobType { get; set; }

        [JsonPropertyName("parameters")]
        public string[]? Parameters { get; set; }
    }
}