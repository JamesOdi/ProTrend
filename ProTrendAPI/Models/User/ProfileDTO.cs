using System.ComponentModel.DataAnnotations;

namespace ProTrendAPI.Models.User
{
    public class ProfileDTO
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password field can't be empty")]
        public string Password { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string? Country { get; set; } = string.Empty;        
    }
}
