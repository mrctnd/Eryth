using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eryth.Services;
using Eryth.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace Eryth.Controllers
{
    // API endpoint'lerini yöneten controller
    [ApiController]
    [Route("api")]
    [Authorize]
    public class ApiController : BaseController
    {
        private readonly ITrackService _trackService;
        private readonly IPlaylistService _playlistService;
        private readonly IUserService _userService;
        private readonly ILikeService _likeService;
        private readonly IFollowService _followService;
        private readonly IAlbumService _albumService;
        private readonly ICommentService _commentService;
        private readonly ISearchService _searchService;
        
        public ApiController(
            ITrackService trackService,
            IPlaylistService playlistService,
            IUserService userService,
            ILikeService likeService,
            IFollowService followService,
            IAlbumService albumService,
            ICommentService commentService,
            ISearchService searchService,
            IMemoryCache cache) : base(cache)
        {
            _trackService = trackService;
            _playlistService = playlistService;
            _userService = userService;
            _likeService = likeService;
            _followService = followService;
            _albumService = albumService;
            _commentService = commentService;
            _searchService = searchService;
        }

        // Arama API
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search(string query, string type = "all", int page = 1, int pageSize = 20)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("api-search", 100, TimeSpan.FromMinutes(5)))
            {
                return StatusCode(429, "Çok fazla arama isteği. 5 dakika bekleyip tekrar deneyin.");
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Arama sorgusu boş olamaz");
            }

            try
            {
                // Input sanitization
                query = SanitizeInput(query);

                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var (validPage, validPageSize) = ValidatePagination(page, pageSize);

                var result = new
                {
                    Query = query,
                    Type = type,
                    Page = validPage,
                    PageSize = validPageSize,
                    Data = new object()
                };

                switch (type.ToLower())
                {
                    case "tracks":
                        var tracks = await _trackService.SearchTracksAsync(query, validPage, validPageSize);
                        return Ok(new { result.Query, result.Type, result.Page, result.PageSize, Data = tracks });

                    case "users":
                        var users = await _userService.SearchUsersAsync(query, currentUserId, validPage, validPageSize);
                        return Ok(new { result.Query, result.Type, result.Page, result.PageSize, Data = users });

                    case "playlists":
                        var playlists = await _playlistService.GetPublicPlaylistsAsync(validPage, validPageSize);
                        var filteredPlaylists = playlists.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
                        return Ok(new { result.Query, result.Type, result.Page, result.PageSize, Data = filteredPlaylists });

                    default:
                        var allTracks = await _trackService.SearchTracksAsync(query, validPage, 10);
                        var allUsers = await _userService.SearchUsersAsync(query, currentUserId, validPage, 5);
                        var allPlaylists = (await _playlistService.GetPublicPlaylistsAsync(validPage, 5))
                            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

                        return Ok(new
                        {
                            result.Query,
                            result.Type,
                            result.Page,
                            result.PageSize,
                            Data = new
                            {
                                Tracks = allTracks,
                                Users = allUsers,
                                Playlists = allPlaylists
                            }
                        });
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "Arama yapılırken bir hata oluştu");
            }
        }

        // Trend müzikler API
        [HttpGet("trending")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTrending(int page = 1, int pageSize = 20)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("api-trending", 30, TimeSpan.FromMinutes(1)))
            {
                return StatusCode(429, "Çok fazla istek. 1 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var (validPage, validPageSize) = ValidatePagination(page, pageSize);
                var tracks = await _trackService.GetTrendingTracksAsync(validPage, validPageSize);

                return Ok(new
                {
                    Page = validPage,
                    PageSize = validPageSize,
                    Data = tracks
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "Trend müzikler alınırken bir hata oluştu");
            }
        }

        // Müzik detayları API
        [HttpGet("track/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTrack(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var track = await _trackService.GetTrackViewModelAsync(id, currentUserId);

                if (track == null)
                {
                    return NotFound("Müzik bulunamadı");
                }

                return Ok(track);
            }
            catch (Exception)
            {
                return StatusCode(500, "Müzik bilgileri alınırken bir hata oluştu");
            }
        }

        // Çalma listesi detayları API
        [HttpGet("playlist/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlaylist(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var playlist = await _playlistService.GetPlaylistViewModelAsync(id, currentUserId);
                var tracks = await _playlistService.GetPlaylistTracksAsync(id, currentUserId);

                return Ok(new
                {
                    Playlist = playlist,
                    Tracks = tracks
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "Çalma listesi bilgileri alınırken bir hata oluştu");
            }
        }

        // Kullanıcı profil API
        [HttpGet("user/{username}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUser(string username)
        {
            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var user = await _userService.GetUserByUsernameAsync(username);

                if (user == null)
                {
                    return NotFound("Kullanıcı bulunamadı");
                }

                var userViewModel = await _userService.GetUserViewModelAsync(user.Id, currentUserId);
                return Ok(userViewModel);
            }
            catch (Exception)
            {
                return StatusCode(500, "Kullanıcı bilgileri alınırken bir hata oluştu");
            }
        }

        // Play count artırma API
        [HttpPost("track/{id}/play")]
        [AllowAnonymous]
        public async Task<IActionResult> IncrementPlayCount(Guid id)
        {
            // Rate limiting kontrolü - aynı müziği çok hızlı dinlemeyi önle
            if (IsRateLimited($"api-play-count-{id}", 1, TimeSpan.FromMinutes(1)))
            {
                return Ok(new { Message = "Play count güncellendi" }); // Sessizce başarılı döndür
            }

            try
            {
                var userId = GetCurrentUserId();
                await _trackService.IncrementPlayCountAsync(id, userId);

                return Ok(new { Message = "Play count güncellendi" });
            }
            catch (Exception)
            {
                return StatusCode(500, "Play count güncellenirken bir hata oluştu");
            }        }

        // Takip durumu kontrol API
        [HttpGet("user/{userId}/follow-status")]
        public async Task<IActionResult> GetFollowStatus(Guid userId)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("api-follow-status", 60, TimeSpan.FromMinutes(1)))
            {
                return StatusCode(429, "Çok fazla istek. 1 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var currentUserId = GetRequiredUserId();
                var isFollowing = await _followService.IsFollowingAsync(currentUserId, userId);
                var followerCount = await _followService.GetFollowerCountAsync(userId);

                return Ok(new { IsFollowing = isFollowing, FollowerCount = followerCount });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Giriş yapmanız gerekiyor");
            }
            catch (Exception)
            {
                return StatusCode(500, "Takip durumu alınırken bir hata oluştu");
            }
        }

        // Kullanıcının track'lerini getir
        [HttpGet("user/{username}/tracks")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserTracks(string username, int page = 1, int pageSize = 20)
        {
            if (IsRateLimited("api-user-tracks", 100, TimeSpan.FromMinutes(5)))
            {
                return StatusCode(429, "Çok fazla istek. 5 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var user = await _userService.GetUserByUsernameAsync(username);
                
                if (user == null)
                {
                    return NotFound("Kullanıcı bulunamadı");
                }

                var (validPage, validPageSize) = ValidatePagination(page, pageSize);
                var tracks = await _trackService.GetUserTracksAsync(user.Id, currentUserId, validPage, validPageSize);

                return Ok(new { success = true, tracks });
            }
            catch (Exception)
            {
                return StatusCode(500, "Track'ler yüklenirken bir hata oluştu");
            }
        }

        // Kullanıcının albümlerini getir
        [HttpGet("user/{username}/albums")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserAlbums(string username, int page = 1, int pageSize = 20)
        {
            if (IsRateLimited("api-user-albums", 100, TimeSpan.FromMinutes(5)))
            {
                return StatusCode(429, "Çok fazla istek. 5 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var user = await _userService.GetUserByUsernameAsync(username);
                
                if (user == null)
                {
                    return NotFound("Kullanıcı bulunamadı");
                }

                var (validPage, validPageSize) = ValidatePagination(page, pageSize);
                var albums = await _albumService.GetUserAlbumsAsync(user.Id, currentUserId, validPage, validPageSize);

                return Ok(new { success = true, albums });
            }
            catch (Exception)
            {
                return StatusCode(500, "Albümler yüklenirken bir hata oluştu");
            }
        }

        // Kullanıcının playlist'lerini getir
        [HttpGet("user/{username}/playlists")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserPlaylists(string username, int page = 1, int pageSize = 20)
        {
            if (IsRateLimited("api-user-playlists", 100, TimeSpan.FromMinutes(5)))
            {
                return StatusCode(429, "Çok fazla istek. 5 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var user = await _userService.GetUserByUsernameAsync(username);
                
                if (user == null)
                {
                    return NotFound("Kullanıcı bulunamadı");
                }

                var (validPage, validPageSize) = ValidatePagination(page, pageSize);
                var playlists = await _playlistService.GetUserPlaylistsAsync(user.Id, currentUserId, validPage, validPageSize);

                return Ok(new { success = true, playlists });
            }
            catch (Exception)
            {
                return StatusCode(500, "Playlist'ler yüklenirken bir hata oluştu");
            }
        }

        // Kullanıcının takip ettiklerini getir
        [HttpGet("user/{username}/following")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserFollowing(string username, int limit = 10)
        {
            if (IsRateLimited("api-user-following", 100, TimeSpan.FromMinutes(5)))
            {
                return StatusCode(429, "Çok fazla istek. 5 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var user = await _userService.GetUserByUsernameAsync(username);
                
                if (user == null)
                {
                    return NotFound("Kullanıcı bulunamadı");
                }

                var following = await _followService.GetFollowingAsync(user.Id, 1, limit);

                return Ok(new { success = true, users = following });
            }
            catch (Exception)
            {
                return StatusCode(500, "Takip edilenler yüklenirken bir hata oluştu");
            }
        }

        // Kullanıcının yorumlarını getir
        [HttpGet("user/{username}/comments")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserComments(string username, int limit = 10)
        {
            if (IsRateLimited("api-user-comments", 100, TimeSpan.FromMinutes(5)))
            {
                return StatusCode(429, "Çok fazla istek. 5 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var user = await _userService.GetUserByUsernameAsync(username);
                
                if (user == null)
                {
                    return NotFound("Kullanıcı bulunamadı");
                }

                var comments = await _commentService.GetUserCommentsAsync(user.Id, 1, limit);

                return Ok(new { success = true, comments });
            }
            catch (Exception)
            {
                return StatusCode(500, "Yorumlar yüklenirken bir hata oluştu");
            }
        }

        // Beğenilen track'leri getir (kendi profili için)
        [HttpGet("user/liked-tracks")]
        public async Task<IActionResult> GetLikedTracks(int limit = 10)
        {
            if (IsRateLimited("api-liked-tracks", 100, TimeSpan.FromMinutes(5)))
            {
                return StatusCode(429, "Çok fazla istek. 5 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var currentUserId = GetRequiredUserId();
                var tracks = await _likeService.GetLikedTracksAsync(currentUserId, 1, limit);

                return Ok(new { success = true, tracks });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Giriş yapmanız gerekiyor");
            }
            catch (Exception)
            {
                return StatusCode(500, "Beğenilen track'ler yüklenirken bir hata oluştu");
            }
        }

        // Parça Beğenme Uç Noktaları
        [HttpPost("tracks/{id}/like")]
        public async Task<IActionResult> LikeTrack(Guid id)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("api-like-track", 30, TimeSpan.FromMinutes(5)))
            {
                return StatusCode(429, new { error = "Too many requests. Please try again in 5 minutes." });
            }

            try
            {
                var userId = GetRequiredUserId();
                var isAlreadyLiked = await _likeService.IsTrackLikedAsync(id, userId);
                
                if (isAlreadyLiked)
                {
                    return BadRequest(new { error = "Track is already liked" });
                }

                var success = await _likeService.LikeTrackAsync(id, userId);
                if (success)
                {
                    var likeCount = await _likeService.GetTrackLikeCountAsync(id);
                    return Ok(new { liked = true, likeCount });
                }

                return BadRequest(new { error = "Failed to like track" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Authentication required" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpDelete("tracks/{id}/like")]
        public async Task<IActionResult> UnlikeTrack(Guid id)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("api-unlike-track", 30, TimeSpan.FromMinutes(5)))
            {
                return StatusCode(429, new { error = "Too many requests. Please try again in 5 minutes." });
            }

            try
            {
                var userId = GetRequiredUserId();
                var isLiked = await _likeService.IsTrackLikedAsync(id, userId);
                
                if (!isLiked)
                {
                    return BadRequest(new { error = "Track is not liked" });
                }

                var success = await _likeService.UnlikeTrackAsync(id, userId);
                if (success)
                {
                    var likeCount = await _likeService.GetTrackLikeCountAsync(id);
                    return Ok(new { liked = false, likeCount });
                }

                return BadRequest(new { error = "Failed to unlike track" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Authentication required" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("tracks/{id}/like")]
        public async Task<IActionResult> GetTrackLikeStatus(Guid id)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("api-get-like-status", 60, TimeSpan.FromMinutes(1)))
            {
                return StatusCode(429, new { error = "Too many requests. Please try again in 1 minute." });
            }

            try
            {
                var userId = GetCurrentUserId();
                bool isLiked = false;

                if (userId.HasValue)
                {
                    isLiked = await _likeService.IsTrackLikedAsync(id, userId.Value);
                }
                var likeCount = await _likeService.GetTrackLikeCountAsync(id);
                return Ok(new { liked = isLiked, likeCount });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }


        // Kullanıcı Takip Uç Noktaları
        [HttpPost("user/{userId}/follow")]
        public async Task<IActionResult> FollowUser(Guid userId)
        {
            if (IsRateLimited("api-follow-user", 20, TimeSpan.FromMinutes(10)))
            {
                return StatusCode(429, new { error = "Too many follow requests. Please wait 10 minutes." });
            }

            try
            {
                var currentUserId = GetRequiredUserId();
                if (currentUserId == userId)
                {
                    return BadRequest(new { error = "Cannot follow yourself" });
                }

                var result = await _followService.FollowUserAsync(currentUserId, userId);
                if (result)
                {
                    var followerCount = await _followService.GetFollowerCountAsync(userId);
                    return Ok(new { success = true, following = true, followerCount });
                }
                return BadRequest(new { error = "Already following this user" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Login required" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpDelete("user/{userId}/follow")]
        public async Task<IActionResult> UnfollowUser(Guid userId)
        {
            if (IsRateLimited("api-unfollow-user", 20, TimeSpan.FromMinutes(10)))
            {
                return StatusCode(429, new { error = "Too many unfollow requests. Please wait 10 minutes." });
            }

            try
            {
                var currentUserId = GetRequiredUserId();
                var result = await _followService.UnfollowUserAsync(currentUserId, userId);
                if (result)
                {
                    var followerCount = await _followService.GetFollowerCountAsync(userId);
                    return Ok(new { success = true, following = false, followerCount });
                }
                return BadRequest(new { error = "Not following this user" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Login required" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Sanatçı Takip Uç Noktaları
        [HttpPost("artists/{artistId}/follow")]
        public async Task<IActionResult> ToggleArtistFollow(Guid artistId)
        {
            if (IsRateLimited("api-follow-artist", 30, TimeSpan.FromMinutes(10)))
            {
                return StatusCode(429, new { error = "Too many requests. Please wait 10 minutes." });
            }

            try
            {
                var currentUserId = GetRequiredUserId();
                var isFollowing = await _followService.ToggleArtistFollowAsync(currentUserId, artistId);
                var followerCount = await _followService.GetArtistFollowerCountAsync(artistId);
                
                return Ok(new { 
                    success = true, 
                    following = isFollowing, 
                    followerCount 
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Login required" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("artists/{artistId}/follow-status")]
        public async Task<IActionResult> GetArtistFollowStatus(Guid artistId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    var followerCount = await _followService.GetArtistFollowerCountAsync(artistId);
                    return Ok(new { following = false, followerCount });
                }

                var isFollowing = await _followService.IsArtistFollowedAsync(currentUserId.Value, artistId);
                var followerCountResult = await _followService.GetArtistFollowerCountAsync(artistId);
                
                return Ok(new { following = isFollowing, followerCount = followerCountResult });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Çalma Listesi Takip Uç Noktaları
        [HttpPost("playlists/{playlistId}/follow")]
        public async Task<IActionResult> TogglePlaylistFollow(Guid playlistId)
        {
            if (IsRateLimited("api-follow-playlist", 30, TimeSpan.FromMinutes(10)))
            {
                return StatusCode(429, new { error = "Too many requests. Please wait 10 minutes." });
            }

            try
            {
                var currentUserId = GetRequiredUserId();
                var isFollowing = await _followService.TogglePlaylistFollowAsync(currentUserId, playlistId);
                var followerCount = await _followService.GetPlaylistFollowerCountAsync(playlistId);
                
                return Ok(new { 
                    success = true, 
                    following = isFollowing, 
                    followerCount 
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Login required" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("playlists/{playlistId}/follow-status")]
        public async Task<IActionResult> GetPlaylistFollowStatus(Guid playlistId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    var followerCount = await _followService.GetPlaylistFollowerCountAsync(playlistId);
                    return Ok(new { following = false, followerCount });
                }

                var isFollowing = await _followService.IsPlaylistFollowedAsync(currentUserId.Value, playlistId);
                var followerCountResult = await _followService.GetPlaylistFollowerCountAsync(playlistId);
                
                return Ok(new { following = isFollowing, followerCount = followerCountResult });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Arama önerileri API'si
        [HttpGet("search/suggest")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSearchSuggestions(string q, int limit = 5)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("api-search-suggest", 200, TimeSpan.FromMinutes(5)))
            {
                return StatusCode(429, "Too many requests. Please try again in 5 minutes.");
            }

            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Ok(new { suggestions = new List<string>() });
            }

            try
            {
                var suggestions = await _searchService.GetSearchSuggestionsAsync(q, Math.Min(limit, 10));
                return Ok(new { suggestions });
            }
            catch (Exception)
            {
                return StatusCode(500, "Error retrieving search suggestions");
            }
        }

        // Albüm parçaları API uç noktası
        [HttpGet("album/{id}/tracks")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAlbumTracks(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var tracks = await _albumService.GetAlbumTracksAsync(id, currentUserId);
                
                return Ok(tracks);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error retrieving album tracks");
            }
        }
    }
}
