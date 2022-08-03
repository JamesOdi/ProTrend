using System.Net;

namespace ProTrendAPI.Services
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {

                await _next(httpContext);
            } catch(Exception)
            {
                await HandleExceptionAsync(httpContext);

            }
        }

        private static async Task HandleExceptionAsync(HttpContext context)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsJsonAsync(new DataResponse { Status = context.Response.StatusCode.ToString(), Data = "User is not authenticated" });
            }
        }
    }
}
