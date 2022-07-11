using MongoDB.Driver;
using Microsoft.Extensions.Options;
using ProTrendAPI.Settings;
using ProTrendAPI.Models.User;

namespace ProTrendAPI.Services.UserSevice
{
    public class ProfileService : BaseService
    {
        public ProfileService(IOptions<DBSettings> settings) : base(settings) {}

        public async Task<Profile?> GetProfileByIdAsync(Guid id)
        {
            return await _profileCollection.Find(Builders<Profile>.Filter.Where(profile => profile.Identifier == id && !profile.Disabled)).FirstOrDefaultAsync();
        }

        public async Task<Profile?> UpdateProfile(Guid id, Profile profile)
        {
            var user = await GetProfileByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            user.UserName = profile.UserName;
            user.Country = profile.Country;
            user.BackgroundImageUrl = profile.BackgroundImageUrl;

            var filter = Builders<Profile>.Filter.Eq(p => p.Identifier, id);
            var updateQueryResult = await _profileCollection.ReplaceOneAsync(filter, user);
            if (updateQueryResult == null)
                return null;
            return user;
        }

        public async Task<object> Follow(Profile profile, Guid receiver)
        {
            if (profile != null)
            {
                var follow = await _followingsCollection.Find(follow => follow.SenderId == profile.Identifier && follow.ReceiverId == receiver && !profile.Disabled).FirstOrDefaultAsync();
                if (follow != null)
                    return Constants.ErrorFollowing;
                await _followingsCollection.InsertOneAsync(new Followings { SenderId = profile.Identifier, ReceiverId = receiver });
                return new BasicResponse { Status = Constants.OK, Message = Constants.Success };
            }
            return new BasicResponse { Status = Constants.Error, Message = Constants.ErrorFollowing};
        }

        public async Task<BasicResponse> UnFollow(Profile profile, Guid receiver)
        {
            if (profile != null)
            {
                await _followingsCollection.DeleteOneAsync(Builders<Followings>.Filter.Where(f => f.SenderId == profile.Identifier && f.ReceiverId == receiver && !profile.Disabled));
                return new BasicResponse { Status = Constants.OK, Message = Constants.Success };
            }
            return new BasicResponse { Status = Constants.Error, Message = Constants.ErrorUnFollowing };
        }

        public async Task<List<Profile>> GetFollowersAsync(Guid id)
        {
            var followers = await _followingsCollection.Find(f => f.ReceiverId == id).ToListAsync();
            
            var followerProfiles = new List<Profile>();
            foreach (var follower in followers)
            {
                var profile = await GetProfileByIdAsync(follower.SenderId);
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
                var profile = await GetProfileByIdAsync(following.ReceiverId);
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
