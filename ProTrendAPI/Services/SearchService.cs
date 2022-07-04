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
            var posts = await SearchPostsByNameAsync(search);
            string? postsCount;
            if (posts == null)
                postsCount = "0";
            else
                postsCount = FormatNumber(posts.Count);
            var people = await SearchProfilesByNameAsync(search);
            string? peopleCount;
            if (people == null)
                peopleCount = "0";
            else
                peopleCount = FormatNumber(people.Count);
            var category = await SearchPostsByCategoryAsync(search);
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

        public async Task<List<Post>> SearchPostsByNameAsync(string name)
        {
            return await _postsCollection.Find(Builders<Post>.Filter.Where(post => post.Caption.ToLower().Contains(name.ToLower()))).ToListAsync();
        }

        public async Task<List<Post>> SearchPostsByCategoryAsync(string category)
        {
            return await _postsCollection.Find(Builders<Post>.Filter.Where(post => post.Category.Contains(category.ToLower()))).ToListAsync();
        }

        public async Task<List<Profile>> SearchProfilesByNameAsync(string name)
        {
            return await _profileCollection.Find(Builders<Profile>.Filter.Where(profile => profile.Name.Contains(name.ToLower()) && profile.Disabled == false)).ToListAsync();
        }

        public async Task<List<Profile>> SearchProfilesByEmailAsync(string email)
        {
            return await _profileCollection.Find(Builders<Profile>.Filter.Where(profile => profile.Email.Contains(email.ToLower()) && profile.Disabled == false)).ToListAsync();
        }
    }
}
