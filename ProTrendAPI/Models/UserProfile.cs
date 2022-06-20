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
        [JsonPropertyName("location")]
        public string? Location { get; set; } = null;
        [JsonPropertyName("regdate")]
        public DateTime RegistrationDate { get; set; }

    }
}