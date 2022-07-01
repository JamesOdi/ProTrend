using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ProTrendAPI.Models.Posts
{
    public class Saved
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("userid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;
        [JsonPropertyName("postid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostId { get; set; } = string.Empty;
    }
}
