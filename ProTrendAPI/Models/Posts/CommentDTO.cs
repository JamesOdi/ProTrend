using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProTrendAPI.Models.Posts
{
    public class CommentDTO
    {

        [JsonPropertyName("uploadid")]
        [Required]
        public Guid PostId { get; set; }
        
        [JsonPropertyName("comment")]
        [Required(ErrorMessage = "Comment field can't be empty")]
        public string CommentContent { get; set; } = string.Empty;

        //[JsonPropertyName("time")]
        //public DateTime Time { get; set; } = DateTime.Now;
    }
}
