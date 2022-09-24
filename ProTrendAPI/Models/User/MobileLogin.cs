namespace ProTrendAPI.Models.User
{
    public class MobileLogin
    {
        public int ID { get; set; }
        public DateTime Time { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
