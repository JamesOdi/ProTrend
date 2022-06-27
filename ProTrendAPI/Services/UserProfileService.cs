using MongoDB.Driver;
using ProTrendAPI.Models;
using Microsoft.Extensions.Options;
using ProTrendAPI.Settings;

namespace ProTrendAPI.Services
{
    public class UserProfileService: BaseService
    {
        private readonly IConfiguration _configuration;
        private readonly string _profileDeactivated;
        public UserProfileService(IConfiguration configuration, IOptions<DBSettings> settings): base(settings)
        {
            _configuration = configuration;
            _profileDeactivated = _configuration.GetSection("AppSettings:AccState").Value;
        }

        public async Task<UserProfile> GetUserProfileAsync(string id)
        {
            return await _profileCollection.Find(Builders<UserProfile>.Filter.Where(profile => profile.Id == id && profile.AccountType != _profileDeactivated)).SingleAsync();
        }

        public async Task<UserProfile?> UpdateProfile(string id, UserProfile profile)
        {
            var user = await GetUserProfileAsync(id);
            if (user == null)
            {
                return null;
            }

            user.Name = profile.Name;
            user.Country = profile.Country;

            var filter = Builders<UserProfile>.Filter.Eq<string>(id => id.Id, id);
            var updateQueryResult = await _profileCollection.ReplaceOneAsync(filter, user);
            if (updateQueryResult == null)
                return null;
            return user;
        }
    }
}
