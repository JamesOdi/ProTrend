namespace ProTrendAPI.Models.Response
{
    public class ActionResponse
    {
        public int StatusCode { get; set; } = 200;
        public bool Successful { get; set; } = true;
        public string Message { get; set; } = "";
        public object Data { get; set; } = null!;
    }
}
