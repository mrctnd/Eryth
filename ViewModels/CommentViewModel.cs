using System.ComponentModel.DataAnnotations;
using Eryth.Models;

namespace Eryth.ViewModels
{
    public class CommentViewModel
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Yorum içeriği gereklidir")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Yorum 1 ile 1000 karakter arasında olmalıdır")]
        [Display(Name = "Yorum")]
        public string Content { get; set; } = string.Empty;

        public Guid UserId { get; set; }

        [Display(Name = "Kullanıcı")]
        public string UserDisplayName { get; set; } = string.Empty;

        public string? UserProfileImageUrl { get; set; }

        public Guid? TrackId { get; set; }
        public Guid? PlaylistId { get; set; }
        public Guid? ParentCommentId { get; set; }

        public string? TrackTitle { get; set; }
        public string? PlaylistTitle { get; set; }

        public TrackViewModel? Track { get; set; }
        public PlaylistViewModel? Playlist { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public bool IsEdited { get; set; }
        public string UserName => UserDisplayName;
        public string RelativeCreatedDate => GetRelativeTimeDisplay();
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanReply { get; set; }

        public List<CommentViewModel> Replies { get; set; } = new();
        public int ReplyCount => Replies.Count;
        public bool IsReply => ParentCommentId.HasValue;

        public long LikeCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }

        public static CommentViewModel FromComment(Comment comment, bool canEdit = false, bool canDelete = false, bool canReply = true, bool isLikedByCurrentUser = false)
        {
            var viewModel = new CommentViewModel
            {
                Id = comment.Id,
                Content = comment.Content,
                UserId = comment.UserId,
                UserDisplayName = comment.User?.DisplayName ?? "Unknown User",
                UserProfileImageUrl = comment.User?.ProfileImageUrl,
                TrackId = comment.TrackId,
                PlaylistId = comment.PlaylistId,
                ParentCommentId = comment.ParentCommentId,
                TrackTitle = comment.Track?.Title,
                PlaylistTitle = comment.Playlist?.Title,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                IsEdited = comment.IsEdited,
                LikeCount = comment.Likes?.Count ?? 0,
                CanEdit = canEdit,
                CanDelete = canDelete,
                CanReply = canReply,
                IsLikedByCurrentUser = isLikedByCurrentUser,
                Replies = comment.Replies?.Select(r => FromComment(r, canEdit, canDelete, canReply, isLikedByCurrentUser)).ToList() ?? new()
            };
            
            if (comment.Track != null)
            {
                viewModel.Track = new TrackViewModel 
                { 
                    Id = comment.Track.Id,
                    Title = comment.Track.Title,
                    ArtistName = comment.Track.Artist?.DisplayName ?? "Unknown Artist"
                };
            }
            
            if (comment.Playlist != null)
            {
                viewModel.Playlist = new PlaylistViewModel
                {
                    Id = comment.Playlist.Id,
                    Name = comment.Playlist.Title
                };
            }
            
            return viewModel;
        }

        public Comment ToComment()
        {
            return new Comment
            {
                Id = Id,
                Content = Content.Trim(),
                UserId = UserId,
                TrackId = TrackId,
                PlaylistId = PlaylistId,
                ParentCommentId = ParentCommentId
            };
        }

        public bool IsValid()
        {
            var targetCount = new[] { TrackId, PlaylistId }.Count(id => id.HasValue);
            return targetCount == 1;
        }

        public string GetRelativeTimeDisplay()
        {
            var timeSpan = DateTime.UtcNow - CreatedAt;
            return timeSpan.TotalMinutes switch
            {
                < 1 => "Şimdi",
                < 60 => $"{(int)timeSpan.TotalMinutes}dk önce",
                < 1440 => $"{(int)timeSpan.TotalHours}s önce",
                < 10080 => $"{(int)timeSpan.TotalDays}g önce",
                _ => CreatedAt.ToString("dd MMM yyyy")
            };
        }
    }

    public class CommentCreateViewModel
    {
        [Required(ErrorMessage = "Yorum içeriği gereklidir")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Yorum 1 ile 1000 karakter arasında olmalıdır")]
        [Display(Name = "Yorum")]
        public string Content { get; set; } = string.Empty;

        public Guid? TrackId { get; set; }
        public Guid? PlaylistId { get; set; }
        public Guid? ParentCommentId { get; set; }

        public string? ContextTitle { get; set; }
        public string? ContextType { get; set; }

        public bool IsValid()
        {
            var targetCount = new[] { TrackId, PlaylistId }.Count(id => id.HasValue);
            return targetCount == 1;
        }
    }

    public class CommentListViewModel
    {
        public List<CommentViewModel> Comments { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public Guid? TrackId { get; set; }
        public Guid? PlaylistId { get; set; }
        public string? ContextTitle { get; set; }

        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = false;

        // Security
        public bool CanAddComment { get; set; }
    }
}
