using System.Text.Json.Serialization;

namespace ProTrendAPI.Models.Payments
{
    public class VerifyAcceptGiftTransaction
    {
        [JsonPropertyName("profile_id")]
        public string Profile_id { get; set; } = string.Empty;
        [JsonPropertyName("post_id")]
        public string Post_id { get; set; } = string.Empty;
        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;
    }
}
