namespace ProTrendAPI.Models
{
    public class UserDTO
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string? Location { get; set; } = null;
        public DateTime RegistrationDate { get; set; }
    }
}
