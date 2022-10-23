namespace ProTrendAPI.Models.User
{
    public class AccountDetailsDTO
    {
        public string CardNumber { get; set; } = string.Empty;
        public string ExpirtyDate { get; set; } = string.Empty;
        public string CVV { get; set; } = string.Empty;
        public Guid ProfileId { get; set; }
    }
}
