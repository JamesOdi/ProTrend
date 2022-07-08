namespace ProTrendAPI.Models.User
{
    public class ProfileDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string? Country { get; set; } = null;
        public DateTime? RegistrationDate { get; set; } = DateTime.Now;
    }
}
