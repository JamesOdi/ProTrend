using System.Text.Json.Serialization;

namespace ProTrendAPI.Models.Posts
{
    public class CommentDTO
    {
       
        
        [JsonPropertyName("uploadid")]
        public Guid PostId { get; set; }
        
        [JsonPropertyName("comment")]
        public string CommentContent { get; set; } = string.Empty;

        [JsonPropertyName("time")]
        public DateTime Time { get; set; } = DateTime.Now;
    }
}
