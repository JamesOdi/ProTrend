using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProTrendAPI.Models.Response;

namespace ProTrendAPI.Services.Network
{
    public class ProTrndAuthorizationFilter : Attribute, IAuthorizationFilter
    {
        readonly string[] _requiredClaims;

        public ProTrndAuthorizationFilter(params string[] claims)
        {
            _requiredClaims = claims;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var isAuthenticated = context.HttpContext.User.Identity.IsAuthenticated;
            if (!isAuthenticated)
            {
                context.Result = new UnauthorizedObjectResult(new ActionResponse { StatusCode = 401, Message = ActionResponseMessage.Unauthorized });
            }

            var hasAllRequiredClaims = _requiredClaims.All(claim => context.HttpContext.User.HasClaim(x => x.Type == claim));
            if (!hasAllRequiredClaims)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
