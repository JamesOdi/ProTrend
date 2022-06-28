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
                result.Email = _contextAccessor.HttpContext.User.FindFirstValue(Constants.Email);
                result.Name = _contextAccessor.HttpContext.User.FindFirstValue(Constants.Name);
                result.AccountType = _contextAccessor.HttpContext.User.FindFirstValue(Constants.AccType);
                result.Country = _contextAccessor.HttpContext.User.FindFirstValue(Constants.Country);
                result.Id = _contextAccessor.HttpContext.User.FindFirstValue(Constants.ID);
                result.Disabled = bool.Parse(_contextAccessor.HttpContext.User.FindFirstValue(Constants.Disabled));
            }
            return result;
        }
    }
}
