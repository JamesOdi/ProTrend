using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProTrendAPI.Models;
using ProTrendAPI.Settings;

namespace ProTrendAPI.Services
{
    public class PostsService: BaseService
    {
        private readonly IMongoCollection<Post> _postCollection;
        private readonly IMongoCollection<Like> _likeCollection;
        private readonly IMongoCollection<Comment> _commentCollection;
        private readonly CategoriesService _categoryService;

        public PostsService(IOptions<DBSettings> settings): base(settings)
        {
            _categoryService = new CategoriesService(settings);
            _postCollection = Database.GetCollection<Post>(settings.Value.PostsCollection);
            _likeCollection = Database.GetCollection<Like>(settings.Value.LikesCollection);
            _commentCollection = Database.GetCollection<Comment>(settings.Value.CommentsCollection);
        }

        public async Task<Post> AddPostAsync(Post upload)
        {
            await _postCollection.InsertOneAsync(upload);
            await _categoryService.AddCategoryAsync(upload.Category);
            return upload;
        }

        public async Task<List<Like>> GetPostLikesAsync(string id)
        {
            return await _likeCollection.Find(Builders<Like>.Filter.Eq<string>(l => l.UploadId, id)).ToListAsync();
        }

        public async Task AddLikeAsync(Like like)
        {
            await _likeCollection.InsertOneAsync(like);
            return;
        }

        public async Task<int> GetLikesCountAsync(string id)
        {
            var likes = await GetPostLikesAsync(id);
            return likes.Count;
        }

        public async Task<Comment> InsertCommentAsync(Comment comment)
        {
            await _commentCollection.InsertOneAsync(comment);
            return comment;
        }

        public async Task<List<Comment>> GetCommentsAsync(string id)
        {
            return await _commentCollection.Find(Builders<Comment>.Filter.Eq<string>(c => c.UploadId, id)).ToListAsync();
        }

        public async Task<Post> GetSinglePostAsync(string id)
        {
            return await _postCollection.Find(Builders<Post>.Filter.Eq<string>(p => p.Id, id)).SingleAsync();
        }
        
        public async Task<List<Post>> GetUserPostsAsync(string userId)
        {
            return await _postCollection.Find(Builders<Post>.Filter.Eq<string>(p => p.UserId, userId)).ToListAsync();
        }
    }
}
