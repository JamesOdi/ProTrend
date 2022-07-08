using System.Text.Json.Serialization;
namespace ProTrendAPI.Models.Payments
{
    public class Transaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [JsonPropertyName("identifier")]
        public Guid Identifier { get; set; }
        [JsonPropertyName("profileid")]
        public Guid ProfileId { get; set; }
        [JsonPropertyName("promotionid")]
        public Guid PromotionId { get; set; }
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
        [JsonPropertyName("status")]
        public bool Status { get; set; }
        [JsonPropertyName("trxref")]
        public string TrxRef { get; set; } = string.Empty;
        [JsonPropertyName("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
