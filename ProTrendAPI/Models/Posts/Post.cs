using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ProTrendAPI.Models.Posts
{
    public class Post
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [JsonPropertyName("identifier")]
        public Guid Identifier { get; set; }
        [JsonPropertyName("userid")]
        public Guid UserId { get; set; }
        [JsonPropertyName("caption")]
        public string Caption { get; set; } = string.Empty;
        [JsonPropertyName("uploadurls")]
        public List<string>? UploadUrls { get; set; } = null;
        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;
        [JsonPropertyName("category")]
        public List<string>? Category { get; set; } = null;
        [JsonPropertyName("disabled")]
        public bool Disabled { get; set; } = false;
        [JsonPropertyName("time")]
        public DateTime Time { get; set; } = DateTime.Now;
    }
}
