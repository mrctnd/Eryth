using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eryth.Services;
using Eryth.ViewModels;
using Eryth.Utilities;

namespace Eryth.Controllers
{
    [Route("[controller]")]
    public class CommentController : BaseController
    {
        private readonly ICommentService _commentService;
        private readonly ITrackService _trackService;
        private readonly IPlaylistService _playlistService;
        private readonly IUserService _userService;

        public CommentController(
            ICommentService commentService,
            ITrackService trackService,
            IPlaylistService playlistService,
            IUserService userService)
        {
            _commentService = commentService;
            _trackService = trackService;
            _playlistService = playlistService;
            _userService = userService;
        }

        // GET: Comment/Track/{trackId}
        [HttpGet("Track/{trackId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> TrackComments(Guid trackId, int page = 1, int pageSize = 20)
        {
            try
            {
                // Rate limiting: 5 dakikada 60 istek
                if (!await CheckRateLimitAsync("comment_view", 60, TimeSpan.FromMinutes(5)))
                {
                    return StatusCode(429);
                }
                var currentUserId = GetRequiredUserId();
                var comments = await _commentService.GetTrackCommentsAsync(trackId, currentUserId, page, pageSize);
                var totalCount = await _commentService.GetCommentCountAsync(trackId: trackId);

                var track = await _trackService.GetByIdAsync(trackId);
                if (track == null)
                {
                    return NotFound("Track not found");
                }

                var viewModel = new CommentListViewModel
                {
                    Comments = comments.ToList(),
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize,
                    TrackId = trackId,
                    ContextTitle = track.Title,
                    CanAddComment = User.Identity?.IsAuthenticated == true
                };

                return View("Comments", viewModel);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error loading comments: {ex.Message}");
            }
        }

        // GET: Comment/Playlist/{playlistId}
        [HttpGet("Playlist/{playlistId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> PlaylistComments(Guid playlistId, int page = 1, int pageSize = 20)
        {
            try
            {
                // Rate limiting: 5 dakikada 60 istek
                if (!await CheckRateLimitAsync("comment_view", 60, TimeSpan.FromMinutes(5)))
                {
                    return StatusCode(429);
                }
                var currentUserId = GetRequiredUserId();
                var comments = await _commentService.GetPlaylistCommentsAsync(playlistId, currentUserId, page, pageSize);
                var totalCount = await _commentService.GetCommentCountAsync(playlistId: playlistId);

                var playlist = await _playlistService.GetByIdAsync(playlistId);
                if (playlist == null)
                {
                    return NotFound("Playlist not found");
                }

                var viewModel = new CommentListViewModel
                {
                    Comments = comments.ToList(),
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize,
                    PlaylistId = playlistId,
                    ContextTitle = playlist.Title,
                    CanAddComment = User.Identity?.IsAuthenticated == true
                };

                return View("Comments", viewModel);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error loading comments: {ex.Message}");
            }
        }

        // GET: Comment/Replies/{parentCommentId}
        [HttpGet("Replies/{parentCommentId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> CommentReplies(Guid parentCommentId, int page = 1, int pageSize = 10)
        {
            try
            {
                // Rate limiting: 5 dakikada 30 istek
                if (!await CheckRateLimitAsync("comment_replies", 30, TimeSpan.FromMinutes(5)))
                {
                    return StatusCode(429);
                }

                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var replies = await _commentService.GetCommentRepliesAsync(parentCommentId, currentUserId, page, pageSize);

                return Json(new { success = true, replies = replies });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }       
        
         // POST: Comment/Create
        [HttpPost("Create")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment(CommentCreateViewModel model)
        {
            try
            {
                Console.WriteLine($"[CommentController.Create] Received request");
                Console.WriteLine($"[CommentController.Create] ModelState.IsValid: {ModelState.IsValid}");
                Console.WriteLine($"[CommentController.Create] Model data: TrackId={model?.TrackId}, PlaylistId={model?.PlaylistId}, Content='{model?.Content}'");
                Console.WriteLine($"[CommentController.Create] User authenticated: {User.Identity?.IsAuthenticated}");
                Console.WriteLine($"[CommentController.Create] User ID from claims: {GetCurrentUserId()}");

                // Form verisi alındı kaydedildi
                if (Request.HasFormContentType)
                {
                    Console.WriteLine("[CommentController.Create] Form data:");
                    foreach (var item in Request.Form)
                    {
                        Console.WriteLine($"  {item.Key}: {item.Value}");
                    }
                }

                // Rate limiting: 10 dakkada 10 yorum
                if (!await CheckRateLimitAsync("comment_create", 10, TimeSpan.FromMinutes(10)))
                {
                    Console.WriteLine("[CommentController.Create] Rate limit exceeded");
                    return StatusCode(429, new { success = false, message = "Too many comments. Please wait before commenting again." });
                }

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("[CommentController.Create] ModelState is invalid:");
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                    return BadRequest(new { success = false, message = "Invalid comment data", errors = ModelState });
                }

                if (model == null)
                {
                    Console.WriteLine("[CommentController.Create] Model is null");
                    return BadRequest(new { success = false, message = "Comment data is required" });
                }

                // Validasyon modeli
                if (!model.IsValid())
                {
                    Console.WriteLine("[CommentController.Create] Model validation failed - no valid target specified");
                    return BadRequest(new { success = false, message = "Invalid comment target. Must specify either track or playlist." });
                }

                // Girdiyi temizle
                model.Content = SecurityHelper.SanitizeInput(model.Content);

                var userId = GetRequiredUserId();
                Console.WriteLine($"[CommentController.Create] Creating comment for user: {userId}");
                
                var comment = await _commentService.CreateCommentAsync(model, userId);

                Console.WriteLine($"[CommentController.Create] Comment created successfully with ID: {comment.Id}");

                var commentViewModel = await _commentService.GetCommentViewModelAsync(comment.Id, userId);

                Console.WriteLine($"[CommentController.Create] Returning success response for comment ID: {comment.Id}");

                return Json(new
                {
                    success = true,
                    message = "Comment added successfully",
                    comment = commentViewModel
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CommentController.Create] Exception: {ex.Message}");
                Console.WriteLine($"[CommentController.Create] Stack trace: {ex.StackTrace}");
                return Json(new
                {
                    success = false,
                    message = $"Error creating comment: {ex.Message}"
                });
            }
        }

        // POST: Comment/Edit/{id}
        [HttpPost("Edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [FromBody] string content)
        {
            try
            {
                // Rate limiting: 10 dakkada 5 düzenleme
                if (!await CheckRateLimitAsync("comment_edit", 5, TimeSpan.FromMinutes(10)))
                {
                    return StatusCode(429, new { success = false, message = "Too many edit attempts. Please wait before trying again." });
                }

                if (string.IsNullOrWhiteSpace(content) || content.Length > 1000)
                {
                    return BadRequest(new { success = false, message = "Comment content must be between 1 and 1000 characters." });
                }                
                content = SecurityHelper.SanitizeInput(content);

                var userId = GetRequiredUserId();
                var success = await _commentService.UpdateCommentAsync(id, content, userId);

                if (!success)
                {
                    return BadRequest(new { success = false, message = "Comment not found or you don't have permission to edit it." });
                }

                var updatedComment = await _commentService.GetCommentViewModelAsync(id, userId);

                return Json(new
                {
                    success = true,
                    message = "Comment updated successfully",
                    comment = updatedComment
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error updating comment: {ex.Message}"
                });
            }
        }

        // POST: Comment/Delete/{id}
        [HttpPost("Delete/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            System.Diagnostics.Debug.WriteLine($"Delete comment called for ID: {id}");

            try
            {
                // Rate limiting: 10 dakkada 10 silme işlemi
                if (!await CheckRateLimitAsync("comment_delete", 10, TimeSpan.FromMinutes(10)))
                {
                    System.Diagnostics.Debug.WriteLine("Rate limit exceeded for comment delete");
                    return StatusCode(429, new { success = false, message = "Too many delete attempts. Please wait before trying again." });
                }

                var userId = GetRequiredUserId();
                System.Diagnostics.Debug.WriteLine($"User ID: {userId}");

                var success = await _commentService.DeleteCommentAsync(id, userId);
                System.Diagnostics.Debug.WriteLine($"Delete success: {success}");

                if (!success)
                {
                    return BadRequest(new { success = false, message = "Comment not found or you don't have permission to delete it." });
                }

                return Json(new
                {
                    success = true,
                    message = "Comment deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error deleting comment: {ex.Message}"
                });
            }
        }

        // GET: Comment/Details/{id}
        [HttpGet("Details/{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                // Rate limiting: 5 dakkada 30 istek
                if (!await CheckRateLimitAsync("comment_details", 30, TimeSpan.FromMinutes(5)))
                {
                    return StatusCode(429);
                }
                var currentUserId = GetRequiredUserId();
                var comment = await _commentService.GetCommentViewModelAsync(id, currentUserId);

                return Json(new { success = true, comment = comment });
            }
            catch (InvalidOperationException)
            {
                return NotFound(new { success = false, message = "Comment not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Comment/Reply
        [HttpPost("Reply")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply([FromBody] CommentCreateViewModel model)
        {
            try
            {
                // Rate limiting: 10 dakkada 15 istek
                if (!await CheckRateLimitAsync("comment_reply", 15, TimeSpan.FromMinutes(10)))
                {
                    return StatusCode(429, new { success = false, message = "Too many replies. Please wait before replying again." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid reply data", errors = ModelState });
                }

                if (!model.ParentCommentId.HasValue)
                {
                    return BadRequest(new { success = false, message = "Parent comment ID is required for replies." });
                }
                model.Content = SecurityHelper.SanitizeInput(model.Content);

                var userId = GetRequiredUserId();
                var reply = await _commentService.CreateCommentAsync(model, userId);

                var replyViewModel = await _commentService.GetCommentViewModelAsync(reply.Id, userId);

                return Json(new
                {
                    success = true,
                    message = "Reply added successfully",
                    reply = replyViewModel
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error creating reply: {ex.Message}"
                });
            }
        }

        // API: Track yorumlarını getir
        [HttpGet("GetTrackComments/{trackId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTrackComments(Guid trackId)
        {
            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var comments = await _commentService.GetTrackCommentsAsync(trackId, currentUserId, 1, 20);

                var commentData = comments.Select(c => new
                {
                    id = c.Id,
                    content = c.Content,
                    userDisplayName = c.UserDisplayName,
                    userProfileImageUrl = c.UserProfileImageUrl,
                    relativeCreatedDate = GetRelativeTime(c.CreatedAt),
                    canEdit = c.CanEdit,
                    canDelete = c.CanDelete
                });

                return Json(new { success = true, comments = commentData });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error loading comments" });
            }
        }        private string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            return timeSpan.TotalDays switch
            {
                < 1 when timeSpan.TotalHours < 1 => $"{(int)timeSpan.TotalMinutes}m ago",
                < 1 => $"{(int)timeSpan.TotalHours}h ago",
                < 7 => $"{(int)timeSpan.TotalDays}d ago",
                < 30 => $"{(int)(timeSpan.TotalDays / 7)}w ago",
                < 365 => $"{(int)(timeSpan.TotalDays / 30)}mo ago",
                _ => $"{(int)(timeSpan.TotalDays / 365)}y ago"
            };
        }
    }
}
