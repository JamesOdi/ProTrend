using MongoDB.Driver;
using Microsoft.Extensions.Options;
using ProTrendAPI.Settings;
using ProTrendAPI.Models.User;

namespace ProTrendAPI.Services.UserSevice
{
    public class ProfileService : BaseService
    {
        public ProfileService(IOptions<DBSettings> settings) : base(settings) {}

        public async Task<Profile?> GetUserProfileByEmailAsync(string email)
        {
            return await _profileCollection.Find(Builders<Profile>.Filter.Where(profile => profile.Email == email && profile.Disabled == false)).FirstOrDefaultAsync();
        }

        public async Task<Profile?> GetUserProfileByIdAsync(Guid id)
        {
            return await _profileCollection.Find(Builders<Profile>.Filter.Where(profile => profile.Identifier == id)).FirstOrDefaultAsync();
        }

        public async Task<Profile?> UpdateProfile(Guid id, Profile profile)
        {
            var user = await GetUserProfileByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            user.Name = profile.Name;
            user.Country = profile.Country;

            var filter = Builders<Profile>.Filter.Eq(p => p.Identifier, id);
            var updateQueryResult = await _profileCollection.ReplaceOneAsync(filter, user);
            if (updateQueryResult == null)
                return null;
            return user;
        }

        public async Task<object> Follow(Guid id, Guid receiver)
        {
            var user = await GetUserProfileByIdAsync(id);
            if (user != null)
            {
                var follow = await _followingsCollection.Find(follow => follow.SenderId == user.Identifier && follow.ReceiverId == receiver).FirstOrDefaultAsync();
                if (follow != null)
                    return Constants.ErrorFollowing;
                await _followingsCollection.InsertOneAsync(new Followings { SenderId = user.Identifier, ReceiverId = receiver });
                return new BasicResponse { Status = Constants.OK, Message = Constants.Success };
            }
            return new BasicResponse { Status = Constants.Error, Message = Constants.ErrorFollowing};
        }

        public async Task<List<Profile>> GetFollowersAsync(Guid id)
        {
            var followers = await _followingsCollection.Find(f => f.ReceiverId == id).ToListAsync();
            
            var followerProfiles = new List<Profile>();
            foreach (var follower in followers)
            {
                var profile = await GetUserProfileByIdAsync(follower.SenderId);
                if (profile != null)
                    followerProfiles.Add(profile);
            }
            return followerProfiles;
        }

        public async Task<List<Profile>> GetFollowings(Guid id)
        {
            var followings = await _followingsCollection.Find(Builders<Followings>.Filter.Where(f => f.SenderId == id)).ToListAsync();
            var followingProfiles = new List<Profile>();
            foreach (var following in followings)
            {
                var profile = await GetUserProfileByIdAsync(following.ReceiverId);
                if (profile != null)
                {
                    followingProfiles.Add(profile);
                }
            }
            return followingProfiles;
        }

        public async Task<string> GetFollowerCount(Guid id)
        {
            var followers = await GetFollowersAsync(id);
            if (followers != null)
                return FormatNumber(followers.Count);
            return "0";
        }

        public async Task<string> GetFollowingCount(Guid id)
        {
            var followings = await GetFollowings(id);
            if (followings != null)
                return FormatNumber(followings.Count);
            return "0";
        }
    }
}
