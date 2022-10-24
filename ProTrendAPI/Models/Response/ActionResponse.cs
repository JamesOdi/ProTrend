namespace ProTrendAPI.Models.Response
{
    public class ActionResponse
    {
        public bool Successful { get; set; } = false;
        public object? Data { get; set; } = null;
        public int StatusCode { get; set; } = 400;
        public string Message { get; set; } = ActionResponseMessage.BadRequest;
    }
}
