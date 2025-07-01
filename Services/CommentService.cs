using Microsoft.EntityFrameworkCore;
using Eryth.Data;
using Eryth.Models;
using Eryth.ViewModels;
using Eryth.Utilities;

namespace Eryth.Services
{
    // Yorum işlemleri için servis implementasyonu
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILikeService _likeService;

        public CommentService(ApplicationDbContext context, ILikeService likeService)
        {
            _context = context;
            _likeService = likeService;
        }

        public async Task<Comment?> GetByIdAsync(Guid id)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Track)
                .Include(c => c.Playlist)
                .Include(c => c.Likes)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<CommentViewModel> GetCommentViewModelAsync(Guid commentId, Guid currentUserId)
        {
            var comment = await GetByIdAsync(commentId);
            if (comment == null)
                throw new InvalidOperationException("Comment not found");

            var canEdit = await CanUserEditCommentAsync(commentId, currentUserId);
            var canDelete = await CanUserDeleteCommentAsync(commentId, currentUserId);
            var isLiked = await _likeService.IsLikedByUserAsync(commentId, currentUserId, "Comment");

            return CommentViewModel.FromComment(comment, canEdit, canDelete, true, isLiked);
        }

        public async Task<IEnumerable<CommentViewModel>> GetTrackCommentsAsync(Guid trackId, Guid currentUserId, int page, int pageSize)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Likes)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Where(c => c.TrackId == trackId && !c.IsDeleted && !c.ParentCommentId.HasValue)
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<CommentViewModel>();
            foreach (var comment in comments)
            {
                var canEdit = await CanUserEditCommentAsync(comment.Id, currentUserId);
                var canDelete = await CanUserDeleteCommentAsync(comment.Id, currentUserId);
                var isLiked = await _likeService.IsLikedByUserAsync(comment.Id, currentUserId, "Comment");

                result.Add(CommentViewModel.FromComment(comment, canEdit, canDelete, true, isLiked));
            }

            return result;
        }

        public async Task<IEnumerable<CommentViewModel>> GetPlaylistCommentsAsync(Guid playlistId, Guid currentUserId, int page, int pageSize)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Likes)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Where(c => c.PlaylistId == playlistId && !c.IsDeleted && !c.ParentCommentId.HasValue)
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<CommentViewModel>();
            foreach (var comment in comments)
            {
                var canEdit = await CanUserEditCommentAsync(comment.Id, currentUserId);
                var canDelete = await CanUserDeleteCommentAsync(comment.Id, currentUserId);
                var isLiked = await _likeService.IsLikedByUserAsync(comment.Id, currentUserId, "Comment");

                result.Add(CommentViewModel.FromComment(comment, canEdit, canDelete, true, isLiked));
            }

            return result;
        }

        public async Task<IEnumerable<CommentViewModel>> GetCommentRepliesAsync(Guid parentCommentId, Guid currentUserId, int page, int pageSize)
        {
            var replies = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Likes)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Where(c => c.ParentCommentId == parentCommentId && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<CommentViewModel>();
            foreach (var reply in replies)
            {
                var canEdit = await CanUserEditCommentAsync(reply.Id, currentUserId);
                var canDelete = await CanUserDeleteCommentAsync(reply.Id, currentUserId);
                var isLiked = await _likeService.IsLikedByUserAsync(reply.Id, currentUserId, "Comment");

                result.Add(CommentViewModel.FromComment(reply, canEdit, canDelete, true, isLiked));
            }

            return result;
        }

        public async Task<Comment> CreateCommentAsync(CommentCreateViewModel model, Guid userId)
        {
            // Validate input
            if (!model.IsValid())
                throw new ArgumentException("Invalid comment target. Must specify either TrackId or PlaylistId, but not both.");

            // Sanitize content
            var sanitizedContent = SecurityHelper.SanitizeInput(model.Content);

            // Validate target exists
            if (model.TrackId.HasValue)
            {
                var trackExists = await _context.Tracks.AnyAsync(t => t.Id == model.TrackId.Value && t.Status == Models.Enums.TrackStatus.Active);
                if (!trackExists)
                    throw new InvalidOperationException("Track not found or not available for comments");
            }
            else if (model.PlaylistId.HasValue)
            {
                var playlistExists = await _context.Playlists.AnyAsync(p => p.Id == model.PlaylistId.Value && p.DeletedAt == null);
                if (!playlistExists)
                    throw new InvalidOperationException("Playlist not found or not available for comments");
            }

            // Validate parent comment if this is a reply
            if (model.ParentCommentId.HasValue)
            {
                var parentExists = await _context.Comments.AnyAsync(c => c.Id == model.ParentCommentId.Value && !c.IsDeleted);
                if (!parentExists)
                    throw new InvalidOperationException("Parent comment not found");
            }

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = sanitizedContent,
                UserId = userId,
                TrackId = model.TrackId,
                PlaylistId = model.PlaylistId,
                ParentCommentId = model.ParentCommentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Update comment count on track/playlist
            if (model.TrackId.HasValue)
            {
                await UpdateTrackCommentCountAsync(model.TrackId.Value);
            }
            else if (model.PlaylistId.HasValue)
            {
                // Playlist comment count is not in the model, but we could add it if needed
            }

            return comment;
        }

        public async Task<bool> UpdateCommentAsync(Guid commentId, string content, Guid userId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);
            if (comment == null || comment.UserId != userId)
                return false;

            comment.Content = content;
            comment.UpdatedAt = DateTime.UtcNow;
            comment.IsEdited = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _context.Comments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

            if (comment == null)
                return false;

            // Check if user can delete (owner or admin)
            var canDelete = await CanUserDeleteCommentAsync(commentId, userId);
            if (!canDelete)
                return false;

            // Soft delete the comment and all its replies
            comment.IsDeleted = true;
            comment.DeletedAt = DateTime.UtcNow;

            // Also delete all replies to this comment
            foreach (var reply in comment.Replies.Where(r => !r.IsDeleted))
            {
                reply.IsDeleted = true;
                reply.DeletedAt = DateTime.UtcNow;
            }

            // Update comment counts
            if (comment.TrackId.HasValue)
            {
                await UpdateTrackCommentCountAsync(comment.TrackId.Value);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CanUserEditCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);
            if (comment == null)
                return false;

            // Users can only edit their own comments
            return comment.UserId == userId;
        }

        public async Task<bool> CanUserDeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _context.Comments
                .Include(c => c.Track)
                .Include(c => c.Playlist)
                .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

            if (comment == null)
                return false;

            // Users can delete their own comments
            if (comment.UserId == userId)
                return true;

            // Track/Playlist owners can delete comments on their content
            if (comment.TrackId.HasValue && comment.Track?.ArtistId == userId)
                return true;

            if (comment.PlaylistId.HasValue && comment.Playlist?.CreatedByUserId == userId)
                return true;

            // Admin users could delete any comment (if we implement role system)
            return false;
        }

        public async Task<int> GetCommentCountAsync(Guid? trackId = null, Guid? playlistId = null)
        {
            var query = _context.Comments.Where(c => !c.IsDeleted);

            if (trackId.HasValue)
                query = query.Where(c => c.TrackId == trackId.Value);
            else if (playlistId.HasValue)
                query = query.Where(c => c.PlaylistId == playlistId.Value);

            return await query.CountAsync();
        }

        private async Task UpdateTrackCommentCountAsync(Guid trackId)
        {
            var commentCount = await _context.Comments
                .CountAsync(c => c.TrackId == trackId && !c.IsDeleted);

            var track = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == trackId);
            if (track != null)
            {
                track.CommentCount = commentCount;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CommentViewModel>> GetUserCommentsAsync(Guid userId, int page, int pageSize)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Track)
                .Include(c => c.Playlist)
                .Include(c => c.Likes)
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<CommentViewModel>();
            foreach (var comment in comments)
            {
                var canEdit = await CanUserEditCommentAsync(comment.Id, userId);
                var canDelete = await CanUserDeleteCommentAsync(comment.Id, userId);
                var isLiked = await _likeService.IsLikedByUserAsync(comment.Id, userId, "Comment");

                result.Add(CommentViewModel.FromComment(comment, canEdit, canDelete, true, isLiked));
            }

            return result;
        }
    }
}
