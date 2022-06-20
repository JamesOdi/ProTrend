using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProTrendAPI.Models;
using ProTrendAPI.Settings;

namespace ProTrendAPI.Services
{
    public class RegistrationService: BaseService
    {
        private readonly IMongoCollection<Register> _registrationCollection;
        private readonly IMongoCollection<UserProfile> _userProfileCollection;

        public RegistrationService(IOptions<DBSettings> settings): base(settings)
        {
            _registrationCollection = Database.GetCollection<Register>(settings.Value.UserCollection);
            _userProfileCollection = Database.GetCollection<UserProfile>(settings.Value.UsersProfileCollection);
        }

        public async Task<UserProfile> InsertAsync(Register register)
        {
            await _registrationCollection.InsertOneAsync(register);
            var userProfile = new UserProfile
            {
                Name = register.Name,
                Email = register.Email,
                AccountType = register.AccountType,
                Location = register.Location,
                RegistrationDate = register.RegistrationDate,
            };
            await _userProfileCollection.InsertOneAsync(userProfile);
            return userProfile;
        }

        public async Task<Register?> FindRegisteredUserAsync(UserDTO register)
        {
            try
            {
                var user = await _registrationCollection.Find(r => r.Email.ToLower() == register.Email.ToLower()).SingleAsync();
                return user;
            } catch (Exception)
            {
                return null;
            }
        }
    }
}
