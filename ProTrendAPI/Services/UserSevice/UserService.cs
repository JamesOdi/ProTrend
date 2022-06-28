using ProTrendAPI.Models;
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

        public UserProfile GetUserProfile()
        {
            var result = new UserProfile();
            if (_contextAccessor.HttpContext != null)
            {
                result.Email = _contextAccessor.HttpContext.User.FindFirstValue("email");
                result.Name = _contextAccessor.HttpContext.User.FindFirstValue("name");
                result.AccountType = _contextAccessor.HttpContext.User.FindFirstValue("acctype");
                result.Country = _contextAccessor.HttpContext.User.FindFirstValue("country");
                result.Id = _contextAccessor.HttpContext.User.FindFirstValue("_id");
                result.Disabled = bool.Parse(_contextAccessor.HttpContext.User.FindFirstValue("disabled"));
            }
            return result;
        }
    }
}
