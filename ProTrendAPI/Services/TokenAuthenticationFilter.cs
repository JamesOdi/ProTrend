using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProTrendAPI.Services
{
    public class TokenAuthenticationFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var result = true;
            if (!context.HttpContext.Response.Headers.ContainsKey("Authorization"))
            {
                result = false;
            }
            //string token = string.Empty;
            //if (result) 
            //{
            //    token = context.HttpContext.Response.Headers.First(x => x.Key == "Authorization").Value;
            //    if (!tokenManager.VerifyToken(token))
            //    {
            //        result = false;
            //    }
            //}
            if (!result)
            {
                context.ModelState.AddModelError("UnnAuthorized", "You are not Authorized");
                context.Result = new UnauthorizedObjectResult(context.ModelState);
            }
        }
    }
}
