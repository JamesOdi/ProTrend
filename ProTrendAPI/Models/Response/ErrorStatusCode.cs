using System.Net;

namespace ProTrendAPI.Models.Response
{
    public class ErrorStatusCode
    {
        public const int notFound = 404;
        public const int requestTimeout = 408;
        public const int unAuthorized = 401;
        public const int forbidden = 403;
        public const int noContent = 204;
        public const int partialContent = 206;
        public const int badReq = 400;
        public const int okReq = 200;
        public const int serverErr = 500;
    }
}
