using Microsoft.EntityFrameworkCore;
using Eryth.Data;
using Eryth.Models;
using Eryth.Models.Enums;
using Eryth.ViewModels;

namespace Eryth.Services
{
    // Bildirim işlemleri servis implementasyonu
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }        public async Task<IEnumerable<NotificationViewModel>> GetUserNotificationsAsync(Guid userId, int page, int pageSize)
        {
            var notifications = await _context.Notifications
                .Include(n => n.TriggeredByUser)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return notifications.Select(NotificationViewModel.FromNotification);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<int> GetUnreadNotificationCountAsync(Guid userId)
        {
            return await GetUnreadCountAsync(userId);
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null) return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            var readTime = DateTime.UtcNow;
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = readTime;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAllNotificationsAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (notifications.Any())
            {
                _context.Notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task CreateLikeNotificationAsync(Guid fromUserId, Guid toUserId, Guid? trackId = null, Guid? playlistId = null)
        {
            if (fromUserId == toUserId) return;            var notification = new Notification
            {
                UserId = toUserId,
                TriggeredByUserId = fromUserId,
                Type = NotificationType.Like,
                RelatedTrackId = trackId,
                RelatedPlaylistId = playlistId,
                Message = "İçeriğinizi beğendi",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreateCommentNotificationAsync(Guid fromUserId, Guid toUserId, Guid? trackId = null, Guid? playlistId = null)
        {
            if (fromUserId == toUserId) return;            var notification = new Notification
            {
                UserId = toUserId,
                TriggeredByUserId = fromUserId,
                Type = NotificationType.Comment,
                RelatedTrackId = trackId,
                RelatedPlaylistId = playlistId,
                Message = "İçeriğinize yorum yaptı",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreateFollowNotificationAsync(Guid fromUserId, Guid toUserId)
        {            var notification = new Notification
            {
                UserId = toUserId,
                TriggeredByUserId = fromUserId,
                Type = NotificationType.Follow,
                Message = "Sizi takip etmeye başladı",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreatePlaylistShareNotificationAsync(Guid fromUserId, Guid toUserId, Guid playlistId)
        {            var notification = new Notification
            {
                UserId = toUserId,
                TriggeredByUserId = fromUserId,
                Type = NotificationType.PlaylistShare,
                RelatedPlaylistId = playlistId,
                Message = "Bir çalma listesini sizinle paylaştı",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreateNewReleaseNotificationAsync(Guid artistId, Guid trackId)
        {            // Sanatçıyı takip eden kullanıcıları bul
            var followers = await _context.Follows
                .Where(f => f.FollowingId == artistId)
                .Select(f => f.FollowerId)
                .ToListAsync();

            var notifications = followers.Select(followerId => new Notification
            {
                UserId = followerId,
                TriggeredByUserId = artistId,
                Type = NotificationType.NewRelease,
                RelatedTrackId = trackId,
                Message = "Yeni bir parça yayınladı",
                CreatedAt = DateTime.UtcNow
            });

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();
        }        public async Task CreateNotificationAsync(Guid userId, NotificationType type, string message, Guid? triggeredByUserId = null, Guid? relatedTrackId = null, Guid? relatedPlaylistId = null, Guid? relatedCommentId = null, Guid? relatedMessageId = null, string? actionUrl = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Message = message,
                TriggeredByUserId = triggeredByUserId,
                RelatedTrackId = relatedTrackId,
                RelatedPlaylistId = relatedPlaylistId,
                RelatedCommentId = relatedCommentId,
                RelatedMessageId = relatedMessageId,
                ActionUrl = actionUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
