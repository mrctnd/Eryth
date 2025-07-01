using Eryth.Models;
using Eryth.Models.Enums;
using Eryth.ViewModels;

namespace Eryth.Services
{
    // Bildirim işlemleri için servis arayüzü
    public interface INotificationService
    {
        Task<IEnumerable<NotificationViewModel>> GetUserNotificationsAsync(Guid userId, int page, int pageSize);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<int> GetUnreadNotificationCountAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
        Task<bool> MarkAllAsReadAsync(Guid userId);
        Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId);
        Task<bool> DeleteAllNotificationsAsync(Guid userId);
        Task CreateLikeNotificationAsync(Guid fromUserId, Guid toUserId, Guid? trackId = null, Guid? playlistId = null);
        Task CreateCommentNotificationAsync(Guid fromUserId, Guid toUserId, Guid? trackId = null, Guid? playlistId = null);
        Task CreateFollowNotificationAsync(Guid fromUserId, Guid toUserId);
        Task CreatePlaylistShareNotificationAsync(Guid fromUserId, Guid toUserId, Guid playlistId);
        Task CreateNewReleaseNotificationAsync(Guid artistId, Guid trackId);
        Task CreateNotificationAsync(Guid userId, NotificationType type, string message, Guid? triggeredByUserId = null, Guid? relatedTrackId = null, Guid? relatedPlaylistId = null, Guid? relatedCommentId = null, Guid? relatedMessageId = null, string? actionUrl = null);
    }
}
