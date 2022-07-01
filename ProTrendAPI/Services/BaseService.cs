using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProTrendAPI.Models.Posts;
using ProTrendAPI.Models.User;
using ProTrendAPI.Settings;
using Tag = ProTrendAPI.Models.Posts.Tag;

namespace ProTrendAPI.Services
{
    public class BaseService
    {
        public readonly IMongoDatabase Database;
        public readonly IMongoCollection<Post> _postsCollection;
        public readonly IMongoCollection<Profile> _profileCollection;
        public readonly IMongoCollection<Category> _categoriesCollection;
        public readonly IMongoCollection<Register> _registrationCollection;
        public readonly IMongoCollection<Like> _likeCollection;
        public readonly IMongoCollection<Comment> _commentCollection;
        public readonly IMongoCollection<Tag> _tagsCollection;
        public readonly IMongoCollection<Followings> _followingsCollection;

        public BaseService(IOptions<DBSettings> settings)
        {
            MongoClient client = new(settings.Value.ConnectionURI);
            Database = client.GetDatabase(settings.Value.DatabaseName);
            _categoriesCollection = Database.GetCollection<Category>(settings.Value.CategoriesCollection);
            _postsCollection = Database.GetCollection<Post>(settings.Value.PostsCollection);
            _likeCollection = Database.GetCollection<Like>(settings.Value.LikesCollection);
            _commentCollection = Database.GetCollection<Comment>(settings.Value.CommentsCollection);
            _categoriesCollection = Database.GetCollection<Category>(settings.Value.CategoriesCollection);
            _registrationCollection = Database.GetCollection<Register>(settings.Value.UserCollection);
            _profileCollection = Database.GetCollection<Profile>(settings.Value.ProfilesCollection);
            _tagsCollection = Database.GetCollection<Tag>(settings.Value.TagsCollection);
            _followingsCollection = Database.GetCollection<Followings>(settings.Value.FollowingsCollection);
        }

        public static string FormatNumber(int number)
        {
            var numberInString = number.ToString();
            if (numberInString.Length < 4)
                return numberInString;
            return Result(numberInString);
        }

        public static string Result(string numberString)
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
