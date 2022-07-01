using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ProTrendAPI.Models.User
{
    public class Followings
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [JsonPropertyName("senderid")]
        public Guid SenderId { get; set; }
        [JsonPropertyName("receiverid")]
        public Guid ReceiverId { get; set; }
    }
}
