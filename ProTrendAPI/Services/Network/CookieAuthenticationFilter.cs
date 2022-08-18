using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProTrendAPI.Services.Network
{
    public class CookieAuthenticationFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var result = true;
            var authorizationExists = context.HttpContext.Request.Cookies.ContainsKey(Constants.AUTH);
            if (!authorizationExists)
            {
                result = false;
            }

            if (!result)
            {
                context.ModelState.AddModelError("UnAuthorized", "User is Unauthorized");
                context.Result = new UnauthorizedObjectResult(context.ModelState);
            }
        }
    }
}
