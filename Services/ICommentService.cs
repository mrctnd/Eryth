using Eryth.Models;
using Eryth.ViewModels;

namespace Eryth.Services
{
    // Yorum işlemleri için servis arayüzü
    public interface ICommentService
    {
        Task<Comment?> GetByIdAsync(Guid id);
        Task<CommentViewModel> GetCommentViewModelAsync(Guid commentId, Guid currentUserId);
        Task<IEnumerable<CommentViewModel>> GetTrackCommentsAsync(Guid trackId, Guid currentUserId, int page, int pageSize);
        Task<IEnumerable<CommentViewModel>> GetPlaylistCommentsAsync(Guid playlistId, Guid currentUserId, int page, int pageSize);
        Task<IEnumerable<CommentViewModel>> GetCommentRepliesAsync(Guid parentCommentId, Guid currentUserId, int page, int pageSize);
        Task<Comment> CreateCommentAsync(CommentCreateViewModel model, Guid userId);
        Task<bool> UpdateCommentAsync(Guid commentId, string content, Guid userId);
        Task<bool> DeleteCommentAsync(Guid commentId, Guid userId);
        Task<bool> CanUserEditCommentAsync(Guid commentId, Guid userId);
        Task<bool> CanUserDeleteCommentAsync(Guid commentId, Guid userId);
        Task<int> GetCommentCountAsync(Guid? trackId = null, Guid? playlistId = null);
        Task<IEnumerable<CommentViewModel>> GetUserCommentsAsync(Guid userId, int page, int pageSize);
    }
}
