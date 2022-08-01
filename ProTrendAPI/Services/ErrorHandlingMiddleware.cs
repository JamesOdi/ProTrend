using Newtonsoft.Json;
using System.Net;

namespace ProTrendAPI.Services
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await httpContext.Response.WriteAsync(ex.Message);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger<ErrorHandlingMiddleware> logger)
        {
            object? errors = null;
            switch (ex)
            {
                case AccessViolationException re:
                    logger.LogError((ex), "ACCESS ERROR");
                    errors = re.Message;
                    context.Response.StatusCode = 404;
                    break;
                case Exception e:
                    logger.LogError((e), "SERVER ERROR");
                    errors = string.IsNullOrWhiteSpace(e.Message) ? "Error" : e.Message;
                    context.Response.StatusCode = 500;
                    break;
            }
            context.Response.ContentType = "application/json";
            if (errors != null)
            {
                var result = JsonConvert.SerializeObject(new
                {
                    context.Response.StatusCode,
                    errors
                });
                await context.Response.WriteAsJsonAsync(result);
            }
            
        }
    }
}
