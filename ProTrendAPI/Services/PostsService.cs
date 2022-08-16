using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MongoDB.Driver;
using ProTrendAPI.Models.Payments;
using ProTrendAPI.Settings;

namespace ProTrendAPI.Services
{
    public class PostsService : BaseService
    {
        private readonly CategoriesService _categoryService;
        private readonly ProfileService _profileService;
        private readonly NotificationService _notificationService;

        public PostsService(IOptions<DBSettings> settings) : base(settings)
        {
            _categoryService = new CategoriesService(settings);
            _profileService = new ProfileService(settings);
            _notificationService = new NotificationService(settings);
        }

        public async Task<List<Post>> GetAllPostsAsync()
        {
            return await _postsCollection.Find(Builders<Post>.Filter.Where(p => !p.Disabled)).ToListAsync();
        }

        public async Task<bool> PromoteAsync(Profile profile, Promotion promotion)
        {
            promotion.Identifier = promotion.Id;
            promotion.ProfileId = profile.Identifier;
            try
            {
                await _promotionCollection.InsertOneAsync(promotion);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> BuyGiftsAsync(Guid profileId, int count)
        {
            var gifts = Enumerable.Repeat(new Gift { Id = null, ProfileId = profileId, Disabled = false }, count);
            try
            {
                await _giftsCollection.InsertManyAsync(gifts);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<long> SendGiftToPostAsync(Post post, int count, Guid userId)
        {
            if (post != null && post.AcceptGift)
            {
                var filter = Builders<Gift>.Filter.Where(g => g.ProfileId == userId && !g.Disabled);
                var gifts = await _giftsCollection.Find(filter).ToListAsync();
                long updateResult = 0;
                for (int i = 0; i < count; i++)
                {
                    var updateBuilder = Builders<Gift>.Update;
                    var update = updateBuilder.Set(g => g.ProfileId, post.ProfileId).Set(g => g.PostId, post.Identifier);
                    var updateOne = await _giftsCollection.UpdateOneAsync(filter, update);
                    updateResult += updateOne.ModifiedCount;
                }
                return updateResult;
            }
            return 0;
        }

        public async Task<List<Gift>> GetAllGiftAsync(Guid profileId)
        {
            var filter = Builders<Gift>.Filter.Where(s => s.ProfileId == profileId && s.Disabled == false);
            var gifts = await _giftsCollection.Find(filter).ToListAsync();
            return gifts;
        }

        public async Task<List<Gift>> GetAllGiftOnPostAsync(Guid postId)
        {
            return await _giftsCollection.Find(Builders<Gift>.Filter.Where(s => s.PostId == postId)).ToListAsync();
        }

        public async Task<List<Profile>> GetGiftersAsync(Guid id)
        {
            var profiles = new List<Profile>();
            var giftNotifications = await _notificationService.GetGiftNotificationsByIdAsync(id.ToString());
            foreach (var notification in giftNotifications)
            {
                var sender = await _profileService.GetProfileByIdAsync(notification.SenderId);
                if (profiles.Find(p => p.Identifier == sender.Identifier) == null)
                    profiles.Add(sender);
            }
            return profiles;
        }

        public async Task<int> GetTotalGiftsAsync(Guid profileId)
        {
            var gifts = await GetAllGiftAsync(profileId);
            return gifts.Count;
        }

        public async Task<bool> RequestWithdrawalAsync(Profile profile, int total)
        {
            int balance = await GetTotalGiftsAsync(profile.Identifier);
            if (balance < 1 || total < 1)
                return false;

            await _giftsCollection.Find(g => !g.Disabled).Limit(total).ForEachAsync(g => g.Disabled = true);
            var transaction = new Transaction { Amount = total, CreatedAt = DateTime.Now, ProfileId = profile.Identifier, Status = true, TrxRef = Generate().ToString() };
            await _transactionCollection.InsertOneAsync(transaction);
            var companyBody = $"Reqest for withdrawal of {total} Gifts from {profile.Email}";
            SendMail("maryse.abshire24@ethereal.email", companyBody);
            var senderBody = $"Your reqest for withdrawal of <b>{total} Gifts</b> is being processed. Your withdrawal will be sent to you within <b>24hrs</b>. Thank you. <p>If you face any challenges please send an email to customer support with request ID {transaction.Identifier} and we will get back to you as soon as we can</p>";
            SendMail(profile.Email, senderBody);
            return true;
        }

        private static void SendMail(string To, string Body)
        {
            var companyAddress = "maryse.abshire24@ethereal.email";
            var _email = new MimeMessage();
            _email.From.Add(MailboxAddress.Parse(companyAddress));
            _email.To.Add(MailboxAddress.Parse(To));
            _email.Cc.Add(MailboxAddress.Parse("Jamesodike26@gmail.com"));
            _email.Subject = $"Request for withdrawal";
            _email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = Body };
            using var smtp1 = new SmtpClient();
            smtp1.Connect("smtp.ethereal.email", 587, MailKit.Security.SecureSocketOptions.StartTls);
            smtp1.Authenticate(companyAddress, "9sSpFDJsceTZ1aUD8E");
            smtp1.Send(_email);
            smtp1.Disconnect(true);
        }

        public async Task<bool> InsertTransactionAsync(Transaction transaction)
        {
            try
            {
                await _transactionCollection.InsertOneAsync(transaction);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Transaction> GetTransactionByRefAsync(string reference)
        {
            return await _transactionCollection.Find(Builders<Transaction>.Filter.Eq(t => t.TrxRef, reference)).SingleOrDefaultAsync();
        }

        public async Task<bool> AcceptGift(Guid id)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.Identifier, id);
            var update = Builders<Post>.Update.Set(p => p.AcceptGift, true);
            var result = await _postsCollection.FindOneAndUpdateAsync(filter, update);
            if (result != null)
                return true;
            return false;
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

        public async Task<bool> AddLikeAsync(Like like)
        {
            var liked = await _likeCollection.Find(l => l.SenderId == like.SenderId && l.UploadId == like.UploadId).FirstOrDefaultAsync();
            if (liked == null)
            {
                await _likeCollection.InsertOneAsync(like);
                return true;
            }
            return false;
        }

        public async Task<bool> RemoveLike(Guid postId, Guid profileId)
        {
            var filter = Builders<Like>.Filter.Where(l => l.SenderId == profileId && l.UploadId == postId);
            var liked = await _likeCollection.Find(filter).FirstOrDefaultAsync();
            if (liked != null)
            {
                var result = await _likeCollection.DeleteOneAsync(filter);
                return result.DeletedCount > 0;
            }
            return false;
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
            return await _postsCollection.Find(Builders<Post>.Filter.Where(p => p.ProfileId == userId && !p.Disabled)).SortBy(p => p.Time).ToListAsync();
        }

        public async Task<bool> DeletePostAsync(Guid postId)
        {
            var filter = Builders<Post>.Filter.Eq<Guid>(p => p.Id, postId);
            var post = await _postsCollection.Find(filter).FirstOrDefaultAsync();
            if (post != null)
            {
                post.Disabled = true;
                var result = await _postsCollection.ReplaceOneAsync(filter, post);
                return result.ModifiedCount > 0;
            }
            return false;
        }

        public async Task<List<Post>> GetPostsInCategoryAsync(string category)
        {
            return await _postsCollection.Find(Builders<Post>.Filter.Where(p => p.Category.Contains(category))).ToListAsync();
        }

        private static int Generate()
        {
            Random r = new((int)DateTime.Now.Ticks);
            return r.Next(100000000, 999999999);
        }
    }
}
