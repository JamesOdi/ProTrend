using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ProTrendAPI.Models.Posts
{
    public class Comment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [JsonPropertyName("identifier")]
        public Guid Identifier { get; set; }
        [JsonPropertyName("uploadid")]
        public Guid PostId { get; set; }
        [JsonPropertyName("userid")]
        public Guid UserId { get; set; }
        [JsonPropertyName("comment")]
        public string CommentContent { get; set; } = string.Empty;
        [JsonPropertyName("time")]
        public DateTime Time { get; set; } = DateTime.Now;
    }
}
