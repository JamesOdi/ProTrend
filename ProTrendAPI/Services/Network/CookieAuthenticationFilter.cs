using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProTrendAPI.Services.Network
{
    public class CookieAuthenticationFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var result = true;
            var userProfile = (IUserService)context.HttpContext.RequestServices.GetService(typeof(IUserService));
            if (userProfile == null || userProfile.GetProfile() == null)
            {
                result = false;
            }

            if (!result)
            {
                context.ModelState.AddModelError("UnnAuthorized", "You are not Authorized");
                context.Result = new UnauthorizedObjectResult(context.ModelState);
            }
        }
    }
}
