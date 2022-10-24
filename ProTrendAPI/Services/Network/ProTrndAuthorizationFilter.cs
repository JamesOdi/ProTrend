using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProTrendAPI.Services.Network
{
    public class ProTrndAuthorizationFilter : AuthorizeAttribute, IAuthorizationFilter
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
                context.Result = new UnauthorizedResult();
                return;
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
