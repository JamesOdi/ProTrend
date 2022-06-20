using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ProTrendAPI.Models
{
    public class Saved
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("userid")]
        public string UserId { get; set; } = string.Empty;
        [JsonPropertyName("postid")]
        public string PostId { get; set; } = string.Empty;
    }
}
