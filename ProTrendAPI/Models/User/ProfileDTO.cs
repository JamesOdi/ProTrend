namespace ProTrendAPI.Models.User
{
    public class ProfileDTO
    {
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string BackgroundImageUrl { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
        public string PaymentPin { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string Bank { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Phone { get; set; } = null;
        public DateTime RegistrationDate { get; set; }
        public bool Disabled { get; set; } = false;
    }
}
