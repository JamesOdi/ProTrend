using System.Text.Json.Serialization;
namespace ProTrendAPI.Models.Response
{
    public class BasicResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = Constants.OK;
        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;
    }
}
