using System.Text.Json.Serialization;
namespace ProTrendAPI.Models
{
    public class Response
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;
    }
}
