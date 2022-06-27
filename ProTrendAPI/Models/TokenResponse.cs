using System.Text.Json.Serialization;
namespace ProTrendAPI.Models
{
    public class TokenResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
        [JsonPropertyName("token")]
        public string Token { get; set; } = null!;
    }
}
