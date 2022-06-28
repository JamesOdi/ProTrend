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
                result.Email = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
                result.Name = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
                result.AccountType = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                result.Country = _contextAccessor.HttpContext.User.FindFirstValue("country");
                result.Id = _contextAccessor.HttpContext.User.FindFirstValue("_id");
                if (result.AccountType == "disabled")
                    result.Disabled = true;
                result.RegistrationDate = DateTime.Parse(_contextAccessor.HttpContext.User.FindFirstValue("regDate"));
            }
            return result;
        }
    }
}
