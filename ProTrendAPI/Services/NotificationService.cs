using Microsoft.Extensions.Options;
using ProTrendAPI.Models.User;
using ProTrendAPI.Settings;
using MongoDB.Driver;
using ProTrendAPI.Models.Payments;

namespace ProTrendAPI.Services
{
    public class NotificationService: BaseService
    {
        public NotificationService(IOptions<DBSettings> options):base(options) {}

        public async Task ChatNotification(Profile sender, Guid receiverId)
        {
            var message = sender.UserName + Constants.SentMessage;
            await _notificationsCollection.InsertOneAsync(Notification(sender.Identifier, receiverId, message));
            return;
        }

        public async Task FollowNotification(Profile sender, Guid receiverId)
        {
            var message = sender.UserName + Constants.StartedFollowing;
            await _notificationsCollection.InsertOneAsync(Notification(sender.Identifier, receiverId, message));
            return;
        }

        public async Task LikeNotification(Profile sender, Guid receiverId)
        {
            var message = sender.UserName + Constants.Liked;
            await _notificationsCollection.InsertOneAsync(Notification(sender.Identifier, receiverId, message));
            return;
        }

        public async Task CommentNotification(Profile sender, Guid receiverId)
        {
            var message = sender.UserName + Constants.Commented;
            await _notificationsCollection.InsertOneAsync(Notification(sender.Identifier, receiverId, message));
            return;
        }

        public async Task SupportNotification(Profile sender, Guid receiverId)
        {
            var message = sender.UserName + Constants.Commented;
            await _notificationsCollection.InsertOneAsync(Notification(sender.Identifier, receiverId, message));
            return;
        }

        public async Task SendGiftNotification(Profile sender, Post post)
        {
            var message = sender.UserName + " sent a gift to your post: " + post.Identifier;
            await _notificationsCollection.InsertOneAsync(Notification(sender.Identifier, post.ProfileId, message));
            return;
        }

        public async Task<List<Notification>> GetNotificationsAsync(Guid id)
        {
            return await _notificationsCollection.Find(Builders<Notification>.Filter.Where(n => n.ReceiverId == id)).SortBy(n => n.Time).ToListAsync();
        }

        public async Task<Notification> GetNotificationByIdAsync(Guid id)
        {
            return await _notificationsCollection.Find(Builders<Notification>.Filter.Where(n => n.Identifier == id)).SingleOrDefaultAsync();
        }

        public async Task<List<Notification>> GetGiftNotificationsByIdAsync(string id)
        {
            return await _notificationsCollection.Find(Builders<Notification>.Filter.Where(n => n.Message.Contains(id))).ToListAsync();
        }

        public async Task SetNotificationViewedAsync(Guid id)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Identifier, id);
            var notification = await GetNotificationByIdAsync(id);
            notification.Viewed = true;
            await _notificationsCollection.ReplaceOneAsync(filter, notification);
            return;
        }

        private static Notification Notification(Guid senderId, Guid receiverId, string message)
        {
            var notification = new Notification { SenderId = senderId, ReceiverId = receiverId, Message = message };
            notification.Identifier = notification.Id;
            return notification;
        }
    }
}
