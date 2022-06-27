using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProTrendAPI.Models;
using ProTrendAPI.Settings;

namespace ProTrendAPI.Services
{
    public class PostsService: BaseService
    {
        private readonly CategoriesService _categoryService;

        public PostsService(IOptions<DBSettings> settings): base(settings)
        {
            _categoryService = new CategoriesService(settings);
        }

        public async Task<Post> AddPostAsync(Post upload)
        {
            await _postsCollection.InsertOneAsync(upload);
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

        public async Task<Post?> GetSinglePostAsync(string id)
        {
            var post = await _postsCollection.Find(Builders<Post>.Filter.Where(p => p.Id == id && !p.Disabled)).FirstOrDefaultAsync();
            if (post == null)
                return null;
            return post;
        }
        
        public async Task<List<Post>> GetUserPostsAsync(string userId)
        {
            return await _postsCollection.Find(Builders<Post>.Filter.Where(p => p.UserId == userId && !p.Disabled)).ToListAsync();
        }

        public async Task DeletePostAsync(string postId)
        {
            var post = await _postsCollection.Find(Builders<Post>.Filter.Where(p => p.Id == postId)).FirstOrDefaultAsync();
            if (post != null)
                post.Disabled = true;
            return;
        }

        public async Task<List<Post>> GetAllPostsAsync()
        {
            return await _postsCollection.Find(Builders<Post>.Filter.Where(p => !p.Disabled)).ToListAsync();
        }

        public async Task<List<Post>> GetPostsInCategoryAsync(string category)
        {
            return await _postsCollection.Find(Builders<Post>.Filter.Where(p => p.Category == category)).ToListAsync();
        }
    }
}
