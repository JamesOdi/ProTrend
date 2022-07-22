using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
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
        private readonly ProfileService _profileService; 
        public PostsService(IOptions<DBSettings> settings) : base(settings)
        {
            _categoryService = new CategoriesService(settings);
            _profileService = new ProfileService(settings);
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

        public async Task<object> SupportAsync(Support support)
        {
            var post = await GetSinglePostAsync(support.PostId);
            if (post != null)
            {
                var userProfile = await _profileService.GetProfileByIdAsync(post.ProfileId);
                if (userProfile == null || userProfile.AccountType != Constants.Business)
                {
                    return new BasicResponse { Status = Constants.Error, Message = "Cannot support a non-business account" };
                }
                support.Identifier = support.Id;
                support.SenderId = userProfile.Identifier;
                support.ReceiverId = post.ProfileId;
                await _supportCollection.InsertOneAsync(support);
            }
            return new BasicResponse { Status = Constants.Error, Message = Constants.PostNotExist };
        }

        public async Task<List<Support>> GetAllSupportAsync(Profile profile)
        {
            return await _supportCollection.Find(Builders<Support>.Filter.Where(s => s.ReceiverId == profile.Identifier)).ToListAsync();
        }

        public async Task<List<Profile>> GetSupportersAsync(Post post)
        {
            var userProfile = await _profileService.GetProfileByIdAsync(post.ProfileId);
            var supporters = await GetAllSupportAsync(userProfile);
            var profiles = new List<Profile>();
            foreach(var supporter in supporters)
            {
                if (supporter.PostId == post.Identifier)
                {
                    var profile = await _profileService.GetProfileByIdAsync(supporter.SenderId);
                    if (profile != null)
                        profiles.Add(profile);
                }
            }
            return profiles;
        }

        public async Task<int> GetTotalBalanceAsync(Profile profile)
        {
            var support = await GetAllSupportAsync(profile);
            var total = 0;
            foreach(var s in support)
            {
                total += s.Amount;
            }
            return total;
        }

        public async Task<object?> RequestWithdrawalAsync(Profile profile, int total)
        {
            int balance = await GetTotalBalanceAsync(profile);
            if (balance <= 1000 || balance - total <= 1000 || total <= 1000)
            {
                return null;
            }

            if (total > 0)
            {
                var support = new Support { Amount = -total, ReceiverId = profile.Identifier, Currency = "ngn", Time = DateTime.Now };
                await _supportCollection.InsertOneAsync(support);
                var companyBody = $"Reqest for withdrawal of {total} Naira from {profile.Email}";
                SendMail("maryse.abshire24@ethereal.email", companyBody);
                var senderBody = $"Your reqest for withdrawal of <b>{total} Naira</b> is being processed. Your withdrawal will be sent to you within <b>24hrs</b>. Thank you. <p>If you face any challenges please send an email to customer support with request ID {support.Identifier} and we will get back to you as soon as we can</p>";
                SendMail(profile.Email, senderBody);
                return Constants.Success;
            }
            return null;
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
            return await _postsCollection.Find(Builders<Post>.Filter.Where(p => p.ProfileId == userId && !p.Disabled)).SortBy(p => p.Time).ToListAsync();
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
