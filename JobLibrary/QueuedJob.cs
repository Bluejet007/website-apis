using System.Text.Json.Serialization;

namespace JobLibrary
{
    public class QueuedJob
    {
        public QueuedJob(string fileName, JobType jobType, int[]? parameters)
        {
            this.FileName = fileName;
            this.JobType = jobType;
            this.Parameters = parameters ?? Array.Empty<int>();
        }

        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        [JsonPropertyName("jobType")]
        public JobType JobType { get; set; }

        [JsonPropertyName("parameters")]
        public int[] Parameters { get; set; }
    }
}