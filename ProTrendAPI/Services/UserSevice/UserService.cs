using MongoDB.Bson;
using ProTrendAPI.Models;
using ProTrendAPI.Models.User;
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

        public Profile GetUserProfile()
        {
            var result = new Profile();
            if (_contextAccessor.HttpContext != null)
            {
                result.Name = _contextAccessor.HttpContext.User.FindFirstValue(Constants.Name);
                result.Identifier = Guid.Parse(_contextAccessor.HttpContext.User.FindFirstValue(Constants.Identifier));
                result.Email = _contextAccessor.HttpContext.User.FindFirst(Constants.Email).Value;
                result.AccountType = _contextAccessor.HttpContext.User.FindFirstValue(Constants.AccType);
                result.Country = _contextAccessor.HttpContext.User.FindFirstValue(Constants.Country);
                result.Id = Guid.Parse(_contextAccessor.HttpContext.User.FindFirstValue(Constants.ID));
                result.Disabled = bool.Parse(_contextAccessor.HttpContext.User.FindFirstValue(Constants.Disabled));
            }
            return result;
        }
    }
}
