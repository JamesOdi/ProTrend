using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ProTrendAPI.Models
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("uploadid")]
        public string UploadId { get; set; } = string.Empty;
        [JsonPropertyName("userid")]
        public string UserId { get; set; } = string.Empty;
        [JsonPropertyName("comment")]
        public string CommentContent { get; set; } = string.Empty;
    }
}
