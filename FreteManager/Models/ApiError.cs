using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FreteManager.Models
{
    // Baseado na especificação RFC 7807 - Problem Details for HTTP APIs
    public class ApiError
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "https://fretemanager.com/errors";

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("detail")]
        public string Detail { get; set; }

        [JsonPropertyName("errors")]
        public Dictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("instance")]
        public string Instance { get; set; }

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }
    }
}