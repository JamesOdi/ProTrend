using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProTrendAPI.Services.UserSevice
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _contextAccessor = httpContextAccessor;
        }

        public Profile? GetProfile()
        {
            var result = new Profile();
            if (_contextAccessor != null && _contextAccessor.HttpContext != null)
            {
                try
                {
                    string token = string.Empty;
                    token = _contextAccessor.HttpContext.Request.Cookies.First(x => x.Key == Constants.AUTH).Value;
                    result.Email = GetUser(token).Claims.First(x => x.Type == Constants.Email).Value;
                    result.Id = Guid.Parse(GetUser(token).Claims.First(x => x.Type == Constants.ID).Value);
                    result.Identifier = Guid.Parse(GetUser(token).Claims.First(x => x.Type == Constants.Identifier).Value);
                    result.UserName = GetUser(token).Claims.First(x => x.Type == Constants.Name).Value;
                    result.FullName = GetUser(token).Claims.First(x => x.Type == Constants.FullName).Value;
                    result.AccountType = GetUser(token).Claims.First(x => x.Type == Constants.AccType).Value;
                    result.Country = GetUser(token).Claims.First(x => x.Type == Constants.Country).Value;
                    result.Disabled = bool.Parse(GetUser(token).Claims.First(x => x.Type == Constants.Disabled).Value);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return result;
        }

        private JwtSecurityToken? GetUser(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadToken(token) as JwtSecurityToken;
        }
    }
}