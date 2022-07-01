using Microsoft.Extensions.Options;
using ProTrendAPI.Settings;
using MongoDB.Driver;
using ProTrendAPI.Models.User;

namespace ProTrendAPI.Services
{
    public class SearchService: BaseService
    {
        public SearchService(IOptions<DBSettings> settings) : base(settings) { }

        public async Task<List<List<string>>> GetSearchResultAsync(string search)
        {
            var posts = await GetPostsWithNameAsync(search);
            string? postsCount;
            if (posts == null)
                postsCount = "0";
            else
                postsCount = FormatNumber(posts.Count);
            var people = await GetProfilesWithNameAsync(search);
            string? peopleCount;
            if (people == null)
                peopleCount = "0";
            else
                peopleCount = FormatNumber(people.Count);
            var category = await GetPostsInCategoryAsync(search);
            string? categoryCount;
            if (category == null)
                categoryCount = "0";
            else
                categoryCount = FormatNumber(category.Count);
            var postsFormat = new List<string> { postsCount , "Posts" };
            var peopleFormat = new List<string> { peopleCount, "People" };
            var categoryFormat = new List<string> { categoryCount, "Category" };

            return new List<List<string>> { postsFormat, peopleFormat, categoryFormat };
        }

        public async Task<List<Post>> GetPostsWithNameAsync(string name)
        {
            return await _postsCollection.Find(Builders<Post>.Filter.Where(post => post.Caption.ToLower().Contains(name.ToLower()))).ToListAsync();
        }

        public async Task<List<Post>> GetPostsInCategoryAsync(string category)
        {
            return await _postsCollection.Find(Builders<Post>.Filter.Where(post => post.Category.Contains(category.ToLower()))).ToListAsync();
        }

        public async Task<List<Profile>> GetProfilesWithNameAsync(string name)
        {
            return await _profileCollection.Find(Builders<Profile>.Filter.Where(profile => profile.Name.ToLower().Contains(name.ToLower()))).ToListAsync();
        }
    }
}
