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
                result.Email = _contextAccessor.HttpContext.User.FindFirstValue(Constants.Email);
                result.FullName = _contextAccessor.HttpContext.User.FindFirstValue(Constants.FullName);
                result.UserName = _contextAccessor.HttpContext.User.FindFirstValue(Constants.Name);
                result.Identifier = Guid.Parse(_contextAccessor.HttpContext.User.FindFirstValue(Constants.Identifier));
                result.Id = Guid.Parse(_contextAccessor.HttpContext.User.FindFirstValue(Constants.ID));
                result.Country = _contextAccessor.HttpContext.User.FindFirstValue(Constants.Country);
                result.Disabled = bool.Parse(_contextAccessor.HttpContext.User.FindFirstValue(Constants.Disabled));
                result.AccountType = _contextAccessor.HttpContext.User.FindFirstValue(Constants.AccType);
                if (result.Disabled)
                    return null;
            }
            return result;
        }
    }
}