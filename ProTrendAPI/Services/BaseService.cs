using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProTrendAPI.Models;
using ProTrendAPI.Settings;
using Tag = ProTrendAPI.Models.Tag;

namespace ProTrendAPI.Services
{
    public class BaseService
    {
        public readonly IMongoDatabase Database;
        public readonly IMongoCollection<Post> _postsCollection;
        public readonly IMongoCollection<UserProfile> _profileCollection;
        public readonly IMongoCollection<Category> _categoriesCollection;
        public readonly IMongoCollection<Register> _registrationCollection;
        public readonly IMongoCollection<Like> _likeCollection;
        public readonly IMongoCollection<Comment> _commentCollection;
        public readonly IMongoCollection<Tag> _tagsCollection;

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
            _profileCollection = Database.GetCollection<UserProfile>(settings.Value.UsersProfileCollection);
            _tagsCollection = Database.GetCollection<Tag>(settings.Value.TagsCollection);
        }
    }
}
