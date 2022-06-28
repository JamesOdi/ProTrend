using System.Text.Json.Serialization;

namespace ProTrendAPI.Models
{
    public class DataResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = Constants.OK;
        [JsonPropertyName("data")]
        public object Data { get; set; } = null!;
    }
}
