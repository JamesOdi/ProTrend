using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ProTrendAPI.Models
{
    public class Register
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("phash")]
        public byte[] PasswordHash { get; set; } = null!;
        [JsonPropertyName("psalt")]
        public byte[] PasswordSalt { get; set; } = null!;
        [JsonPropertyName("accounttype")]
        public string AccountType { get; set; } = string.Empty;
        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;
        [JsonPropertyName("country")]
        public string Country { get; set; } = null!;
        [JsonPropertyName("registrationdate")]
        public DateTime RegistrationDate { get; set; }
    }
}
