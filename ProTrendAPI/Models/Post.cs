using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ProTrendAPI.Models
{
    public class Post
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("userid")]
        public string UserId { get; set; } = string.Empty;
        [JsonPropertyName("caption")]
        public string Caption { get; set; } = string.Empty;
        [JsonPropertyName("uploadurls")]
        public List<Dictionary<string,string>>? UploadUrls { get; set; } = null;
        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;
        [JsonPropertyName("disabled")]
        public bool Disabled { get; set; } = false;
    }
}
