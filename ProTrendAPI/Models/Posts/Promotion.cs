using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ProTrendAPI.Models.Posts
{
    public class Promotion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("start")]
        public DateTime Start { get; set; }
        [JsonPropertyName("expires")]
        public DateTime Expires { get; set; }
        [JsonPropertyName("status")]
        public string PromotionStatus { get; set; } = string.Empty;
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;
        [JsonPropertyName("ptotal")]
        public string PaymentTotal { get; set; } = string.Empty;
        [JsonPropertyName("audience")]
        public string Audience { get; set; } = string.Empty;
    }
}
