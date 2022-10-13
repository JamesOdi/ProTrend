﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProTrendAPI.Models.Posts
{
    public class PostDTO
    {
        [JsonPropertyName("profileid")]
        [Required]
        public Guid ProfileId { get; set; }

        [JsonPropertyName("caption")]
        [Required(ErrorMessage = "Field can't be empty")]
        public string Caption { get; set; } = string.Empty;

        [JsonPropertyName("uploadurls")]
        public List<string> UploadUrls { get; set; } = new List<string>();

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public List<string> Category { get; set; } = new List<string>();

        //[JsonPropertyName("acceptgift")]
        //public bool AcceptGift { get; set; } = false;
    }
}
