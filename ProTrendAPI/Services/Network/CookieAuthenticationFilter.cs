using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace ProTrendAPI.Services.Network
{
    public class CookieAuthenticationFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var result =  context.HttpContext.Session.GetString(Constants.AUTH) != null;
            
            if (!result)
            {
                context.ModelState.AddModelError("UnAuthorized", "User is Unauthorized");
                context.Result = new UnauthorizedObjectResult(context.ModelState);
            }
        }
    }
}
