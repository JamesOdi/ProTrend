using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace ProTrendAPI.Models
{
    public class UserProfile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("acctype")]
        public string AccountType { get; set; } = string.Empty;
        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;
        [JsonPropertyName("phone")]
        public string? Phone { get; set; } = null;
        [JsonPropertyName("regdate")]
        public DateTime RegistrationDate { get; set; }
        [JsonPropertyName("disabled")]
        public bool Disabled { get; set; } = false;
    }
}