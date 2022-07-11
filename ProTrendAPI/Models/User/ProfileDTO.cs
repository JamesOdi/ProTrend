using System.Text.Json.Serialization;

namespace ProTrendAPI.Models.User
{
    public class ProfileDTO
    {
        [JsonPropertyName("fullname")]
        public string FullName { get; set; } = string.Empty;
        [JsonPropertyName("username")]
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        [JsonPropertyName("accounttype")]
        public string AccountType { get; set; } = string.Empty;
        public string? Country { get; set; } = null;
    }
}
