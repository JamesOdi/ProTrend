using System.Security.Cryptography;
using System.Text;

namespace ProTrendAPI.Services.UserSevice
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
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
                    var claim = _contextAccessor.HttpContext.User;
                    result.Email = claim.Claims.First(x => x.Type == Constants.Email).Value;
                    result.Id = Guid.Parse(claim.Claims.First(x => x.Type == Constants.ID).Value);
                    result.Identifier = Guid.Parse(claim.Claims.First(x => x.Type == Constants.Identifier).Value);
                    result.UserName = claim.Claims.First(x => x.Type == Constants.Name).Value;
                    result.FullName = claim.Claims.First(x => x.Type == Constants.FullName).Value;
                    result.AccountType = claim.Claims.First(x => x.Type == Constants.AccType).Value;
                    result.Location = claim.Claims.First(x => x.Type == Constants.Location).Value;
                    result.Disabled = bool.Parse(claim.Claims.First(x => x.Type == Constants.Disabled).Value);
                    return result;
                }
                catch (Exception)
                {
                    result = null;
                }
            }
            return result;
        }
    }
}