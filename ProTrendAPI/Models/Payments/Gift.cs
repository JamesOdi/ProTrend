namespace ProTrendAPI.Models.Payments
{
    public class Gift
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid Identifier { get; set; }
        public Guid ProfileId { get; set; }
        public Guid PostId { get; set; } = Guid.Empty;
        public bool Disabled = false;
    }
}
