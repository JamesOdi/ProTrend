using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProTrendAPI.Models;
using ProTrendAPI.Settings;

namespace ProTrendAPI.Services
{
    public class RegistrationService: BaseService
    {
        public RegistrationService(IOptions<DBSettings> settings): base(settings) {}

        public async Task<UserProfile> InsertAsync(Register register)
        {
            await _registrationCollection.InsertOneAsync(register);
            var userProfile = new UserProfile
            {
                Name = register.Name,
                Email = register.Email,
                AccountType = register.AccountType,
                Country = register.Country,
                RegistrationDate = register.RegistrationDate,
                Phone = register.Phone,
                Disabled = false
            };
            await _profileCollection.InsertOneAsync(userProfile);
            return userProfile;
        }

        public async Task<Register?> FindRegisteredUserAsync(UserDTO register)
        {
            try
            {
                var user = await _registrationCollection.Find(r => r.Email.ToLower() == register.Email.ToLower()).SingleAsync();
                if (user == null)
                    return null;
                return user;
            } catch (Exception)
            {
                return null;
            }
        }
    }
}
