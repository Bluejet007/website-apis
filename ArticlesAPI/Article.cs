using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace WebsiteAPIs
{
    public class Article
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("link")]
        [JsonProperty("link")]
        public string Link { get; set; } = string.Empty;

        [JsonPropertyName("image-url")]
        [JsonProperty("image-url")]
        public string? ImageUrl { get; set; }
    }
}
