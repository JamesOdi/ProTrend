using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ProTrendAPI.Models.Posts
{
    public class Saved
    {
        public Guid Id { get; set; }
        [JsonPropertyName("userid")]
        public Guid UserId { get; set; }
        [JsonPropertyName("postid")]
        public Guid PostId { get; set; }
        [JsonPropertyName("time")]
        public DateTime Time { get; set; } = DateTime.Now;
    }
}
