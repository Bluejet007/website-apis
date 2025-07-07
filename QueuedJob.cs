using System.Text.Json.Serialization;

namespace SolutionItems
{
    public class QueuedJob
    {
        [JsonPropertyName("fileName")]
        public required string FileName { get; set; }

        [JsonPropertyName("jobType")]
        public required JobType JobType { get; set; }

        [JsonPropertyName("parameters")]
        public required int[] Parameters { get; set; }
    }
}