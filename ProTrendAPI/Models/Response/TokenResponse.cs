using System.Text.Json.Serialization;
namespace ProTrendAPI.Models.Response
{
    public class TokenResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = Constants.OK;
        [JsonPropertyName("token")]
        public string Token { get; set; } = null!;
    }
}
