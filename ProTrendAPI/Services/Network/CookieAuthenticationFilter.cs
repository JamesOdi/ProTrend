using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProTrendAPI.Services.Network
{
    public class CookieAuthenticationFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var path = context.HttpContext.Request.Path.Value;
            var names = path.Split("/");
            if (!names[3].Equals("mobile"))
            {
                var result = true;
                var authorizationExists = context.HttpContext.Request.Cookies.ContainsKey(Constants.AUTH);
                var user = (IUserService)context.HttpContext.RequestServices.GetService(typeof(IUserService));

                if (!authorizationExists || user.GetProfile() == null)
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
}
