using System.Security.Claims;

namespace ProTrendAPI
{
    public class UserManagementMiddleware
    {
        private readonly RequestDelegate _next;
        public UserManagementMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var userIdentity = context.User.Claims.Where(u => u.Type == ClaimTypes.Anonymous).FirstOrDefault().Value;
            if (userIdentity == "Personal")
                await context.Response.WriteAsync("Personal account!");
        }
    }
}
