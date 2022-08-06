using System.Net;

namespace ProTrendAPI.Models.Response
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
