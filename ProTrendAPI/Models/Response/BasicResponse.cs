using System.Text.Json.Serialization;
namespace ProTrendAPI.Models.Response
{
    public class BasicResponse
    {
        [JsonPropertyName("status")]
        public bool Success { get; set; } = false;
        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;
    }
}
