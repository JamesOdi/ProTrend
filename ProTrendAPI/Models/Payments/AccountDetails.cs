namespace ProTrendAPI.Models.Payments
{
    public class AccountDetails
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CardNumber { get; set; } = string.Empty;
        public string ExpirtyDate { get; set; } = string.Empty;
        public string CVV { get; set; } = string.Empty;
        public Guid ProfileId { get; set; }
    }
}
