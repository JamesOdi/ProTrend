using Microsoft.Extensions.Options;
using ProTrendAPI.Models;
using ProTrendAPI.Settings;
using MongoDB.Driver;

namespace ProTrendAPI.Services
{
    public class SearchService: BaseService
    {
        public SearchService(IOptions<DBSettings> options): base(options) {}

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
            return await _postsCollection.Find(Builders<Post>.Filter.Where(post => post.Category.ToLower() == category.ToLower())).ToListAsync();
        }

        public async Task<List<UserProfile>> GetProfilesWithNameAsync(string name)
        {
            return await _profileCollection.Find(Builders<UserProfile>.Filter.Where(profile => profile.Name.ToLower().Contains(name.ToLower()))).ToListAsync();
        }

        //public async Task<List<string>> GetRelatedSearchAsync(string name)
        //{
        //    return await _postsCollection;
        //}

        private static string FormatNumber(int number)
        {
            var numberInString = number.ToString();
            if (numberInString.Length < 4)
                return numberInString;
            return Result(numberInString);
        }

        private static string Result(string numberString)
        {
            string? returnResult;
            if (numberString.Length % 3 == 0)
                returnResult = numberString[..3] + "." + numberString[3].ToString();
            else
                returnResult = numberString[..2] + "." + numberString[2].ToString();

            if (numberString.Length == 4 || numberString.Length == 7 || numberString.Length == 10)
                returnResult = numberString[0].ToString() + "." + numberString[1].ToString();

            if (numberString.Length >= 4 && numberString.Length < 7)
                return returnResult + "K";
            else if (numberString.Length >= 7 && numberString.Length < 10)
                return returnResult + "M";
            else
                return returnResult + "B";
        }
    }
}
