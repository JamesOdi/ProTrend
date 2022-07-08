﻿using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProTrendAPI.Models.Payments;
using ProTrendAPI.Models.Posts;
using ProTrendAPI.Models.User;
using ProTrendAPI.Settings;

namespace ProTrendAPI.Services
{
    public class PostsService: BaseService
    {
        private readonly CategoriesService _categoryService;
        public PostsService(IOptions<DBSettings> settings) : base(settings)
        {
            _categoryService = new CategoriesService(settings);
        }

        public async Task<List<Post>> GetAllPostsAsync()
        {
            return await _postsCollection.Find(Builders<Post>.Filter.Where(p => !p.Disabled)).ToListAsync();
        }

        public async Task PromoteAsync(Profile profile, Promotion promotion)
        {
            promotion.Identifier = promotion.Id;
            promotion.ProfileId = profile.Identifier;
            await _promotionCollection.InsertOneAsync(promotion);
            return;
        }

        public async Task InsertTransactionAsync(Transaction transaction)
        {
            await _transactionCollection.InsertOneAsync(transaction);
            return;
        }

        public async Task<Transaction> GetTransactionByRefAsync(string reference)
        {
            return await _transactionCollection.Find(Builders<Transaction>.Filter.Eq(t => t.TrxRef, reference)).SingleOrDefaultAsync();
        }

        public async Task<Transaction?> VerifyTransactionAsync(Transaction transaction)
        {
            transaction.Status = true;
            var filter = Builders<Transaction>.Filter.Eq(t => t.TrxRef, transaction.TrxRef);
            var update = await _transactionCollection.FindOneAndReplaceAsync(filter, transaction);
            if (update != null)
                return transaction;
            return null;
        }

        public async Task<Post> AddPostAsync(Post upload)
        {
            upload.Identifier = upload.Id;
            await _postsCollection.InsertOneAsync(upload);
            if (upload.Category != null && upload.Category.Count > 0)
            {
                foreach (var cat in upload.Category)
                {
                    await _categoryService.AddCategoryAsync(cat);
                }
            }
            return upload;
        }

        public async Task<List<Like>> GetPostLikesAsync(Guid id)
        {
            return await _likeCollection.Find(Builders<Like>.Filter.Eq<Guid>(l => l.UploadId, id)).ToListAsync();
        }

        public async Task<List<Promotion>> GetPromotionsAsync(Profile profile)
        {
            return await _promotionCollection.Find(Builders<Promotion>.Filter.Where(p => p.Audience == profile.Country || p.Audience == Constants.All)).ToListAsync();
        }
        public async Task AddLikeAsync(Like like)
        {
            await _likeCollection.InsertOneAsync(like);
            return;
        }

        public async Task<int> GetLikesCountAsync(Guid id)
        {
            var likes = await GetPostLikesAsync(id);
            return likes.Count;
        }

        public async Task<Comment> InsertCommentAsync(Comment comment)
        {
            await _commentCollection.InsertOneAsync(comment);
            return comment;
        }

        public async Task<List<Comment>> GetCommentsAsync(Guid id)
        {
            return await _commentCollection.Find(Builders<Comment>.Filter.Eq<Guid>(c => c.PostId, id)).ToListAsync();
        }

        public async Task<Post?> GetSinglePostAsync(Guid id)
        {
            var post = await _postsCollection.Find(Builders<Post>.Filter.Where(p => p.Id == id && !p.Disabled)).FirstOrDefaultAsync();
            if (post == null)
                return null;
            return post;
        }
        
        public async Task<List<Post>> GetUserPostsAsync(Guid userId)
        {
            return await _postsCollection.Find(Builders<Post>.Filter.Where(p => p.UserId == userId && !p.Disabled)).SortBy(p => p.Time).ToListAsync();
        }

        public async Task<bool> DeletePostAsync(Guid postId)
        {
            var filter = Builders<Post>.Filter.Eq<Guid>(p => p.Id, postId);
            var post = await _postsCollection.Find(filter).FirstOrDefaultAsync();
            if (post != null)
            {
                post.Disabled = true;
                await _postsCollection.ReplaceOneAsync(filter, post);
                return true;
            }
            return false;
        }

        public async Task<List<Post>> GetPostsInCategoryAsync(string category)
        {
            return await _postsCollection.Find(Builders<Post>.Filter.Where(p => p.Category.Contains(category))).ToListAsync();
        }
    }
}
