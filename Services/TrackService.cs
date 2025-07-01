using Microsoft.EntityFrameworkCore;
using Eryth.Data;
using Eryth.Models;
using Eryth.Models.Enums;
using Eryth.ViewModels;
using Eryth.Infrastructure;

namespace Eryth.Services
{    public class TrackService : ITrackService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<TrackService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TrackService(ApplicationDbContext context, IFileUploadService fileUploadService, ILogger<TrackService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }        public async Task<IEnumerable<Track>> GetAllTracksAsync()
        {
            return await _context.Tracks
                .Include(t => t.Artist)
                .Where(t => t.Status == TrackStatus.Active && t.DeletedAt == null)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TrackViewModel>> GetAllTracksAsync(int page, int pageSize)
        {
            try
            {                // Get all active, non-deleted tracks
                var tracks = await _context.Tracks
                    .Include(t => t.Artist)
                    .Include(t => t.Likes)
                    .Where(t => t.Status == TrackStatus.Active && t.DeletedAt == null)
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation($"Found {tracks.Count} total tracks. Status breakdown: {string.Join(", ", tracks.GroupBy(t => t.Status).Select(g => $"{g.Key}: {g.Count()}"))}");

                // Only return active tracks for now, but log all
                var activeTracks = tracks.Where(t => t.Status == TrackStatus.Active).ToList();
                
                return activeTracks.Select(t => TrackViewModel.FromTrack(t, false, false, false, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tracks for page {Page}", page);
                return Enumerable.Empty<TrackViewModel>();
            }
        }        public async Task<Track?> GetTrackByIdAsync(Guid id)
        {
            return await _context.Tracks
                .Include(t => t.Artist)
                .Include(t => t.Album)
                .Include(t => t.Likes)
                .Include(t => t.Comments)
                .FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);
        }        public async Task<Track?> GetByIdAsync(Guid id)
        {
            return await GetTrackByIdAsync(id);
        }        public async Task<TrackViewModel> GetTrackViewModelAsync(Guid trackId, Guid currentUserId)
        {
            var track = await _context.Tracks
                .Include(t => t.Artist)
                .Include(t => t.Album)
                .Include(t => t.Likes)
                .Include(t => t.Comments)
                .FirstOrDefaultAsync(t => t.Id == trackId && t.DeletedAt == null);
                
            if (track == null)
                return null!;

            // Debug log
            System.Diagnostics.Debug.WriteLine($"Track found: {track.Title}, ArtistId: {track.ArtistId}");
            System.Diagnostics.Debug.WriteLine($"Artist: {track.Artist?.Username ?? "null"}");

            // Like durumunu kontrol et
            var isLiked = currentUserId != Guid.Empty && 
                         track.Likes?.Any(l => l.UserId == currentUserId) == true;

            return TrackViewModel.FromTrack(track, 
                currentUserId == track.ArtistId, 
                true, 
                true, 
                isLiked);
        }

        // Original method for TrackViewModel
        public async Task<Track> CreateTrackAsync(TrackViewModel model, Guid userId)
        {
            var track = new Track
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Description = model.Description,
                Genre = model.Genre,
                SubGenre = model.SubGenre,
                IsExplicit = model.IsExplicit,
                DurationInSeconds = model.DurationInSeconds,
                AudioFileUrl = model.AudioFileUrl,
                CoverImageUrl = model.CoverImageUrl,
                ArtistId = userId,
                Status = TrackStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tracks.Add(track);
            await _context.SaveChangesAsync();
            return track;
        }        // Overload for UploadTrackViewModel  
        public async Task<Track> CreateTrackAsync(UploadTrackViewModel model, Guid userId)
        {
            var track = new Track
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Description = model.Description,
                Genre = Enum.TryParse<Genre>(model.Genre, out var genre) ? genre : Genre.Other,
                IsExplicit = model.IsExplicit,
                ArtistId = userId,
                Status = TrackStatus.Active,
                DurationInSeconds = 0, // Will be set after file processing
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (model.AudioFile != null)
            {
                track.AudioFileUrl = await _fileUploadService.UploadAudioAsync(model.AudioFile, "audio");
                
                // Calculate duration from uploaded file
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, track.AudioFileUrl);
                track.DurationInSeconds = Utilities.AudioHelper.GetAudioDurationInSeconds(fullPath);
            }

            if (model.CoverImage != null)
            {
                track.CoverImageUrl = await _fileUploadService.UploadImageAsync(model.CoverImage, "images");
            }

            _context.Tracks.Add(track);
            await _context.SaveChangesAsync();
            return track;
        }

        // Original method for TrackViewModel
        public async Task<bool> UpdateTrackAsync(Guid trackId, TrackViewModel model, Guid userId)
        {
            var track = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == trackId && t.ArtistId == userId);
            if (track == null) return false;

            track.Title = model.Title;
            track.Description = model.Description;
            track.Genre = model.Genre;
            track.SubGenre = model.SubGenre;
            track.IsExplicit = model.IsExplicit;
            track.DurationInSeconds = model.DurationInSeconds;
            track.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }        // Overload for EditTrackViewModel
        public async Task<bool> UpdateTrackAsync(Guid trackId, EditTrackViewModel model, Guid userId)
        {
            var track = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == trackId && t.ArtistId == userId);
            if (track == null) return false;            track.Title = model.Title ?? track.Title;
            track.Description = model.Description;
            track.Genre = model.Genre;
            track.IsExplicit = model.IsExplicit;
            track.UpdatedAt = DateTime.UtcNow;

            if (model.NewCoverImage != null)
            {
                track.CoverImageUrl = await _fileUploadService.UploadImageAsync(model.NewCoverImage, "images");
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Track entity'sini doğrudan güncelleyen method
        public async Task<bool> UpdateTrackAsync(Track track)
        {
            try
            {
                _context.Tracks.Update(track);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteTrackAsync(Guid trackId, Guid userId)
        {
            var track = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == trackId && t.ArtistId == userId);
            if (track == null) return false;

            track.Status = TrackStatus.Removed;
            track.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Track>> GetUserTracksAsync(Guid userId, Guid? currentUserId = null)
        {
            var query = _context.Tracks
                .Where(t => t.ArtistId == userId && t.Status == TrackStatus.Active)
                .Include(t => t.Artist);

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        // Overloaded GetUserTracksAsync for pagination with TrackViewModel return type
        public async Task<IEnumerable<TrackViewModel>> GetUserTracksAsync(Guid userId, Guid currentUserId, int page, int pageSize)
        {
            var query = _context.Tracks
                .Where(t => t.ArtistId == userId && t.Status == TrackStatus.Active)
                .Include(t => t.Artist)
                .Include(t => t.Album);

            var tracks = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var canEdit = userId == currentUserId;
            return tracks.Select(t => TrackViewModel.FromTrack(t, canEdit, canEdit, true, false));
        }

        public async Task<int> GetUserTrackCountAsync(Guid userId)
        {
            return await _context.Tracks
                .Where(t => t.ArtistId == userId && t.Status == TrackStatus.Active)
                .CountAsync();
        }

        public async Task<IEnumerable<Track>> SearchTracksAsync(string query)
        {
            return await _context.Tracks
                .Where(t => t.Status == TrackStatus.Active &&
                           (t.Title.Contains(query) ||
                            t.Description!.Contains(query) ||
                            t.Artist.DisplayName.Contains(query)))
                .Include(t => t.Artist)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Track>> GetTracksByGenreAsync(string genre)
        {
            if (Enum.TryParse<Genre>(genre, out var genreEnum))
            {
                return await _context.Tracks
                    .Where(t => t.Status == TrackStatus.Active && t.Genre == genreEnum)
                    .Include(t => t.Artist)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            return new List<Track>();
        }

        public async Task<IEnumerable<Track>> GetPopularTracksAsync(int count = 10)
        {
            return await _context.Tracks
                .Where(t => t.Status == TrackStatus.Active)
                .Include(t => t.Artist)
                .Include(t => t.Likes)
                .OrderByDescending(t => t.PlayCount)
                .ThenByDescending(t => t.LikeCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Track>> GetRecentTracksAsync(int count = 10)
        {
            return await _context.Tracks
                .Where(t => t.Status == TrackStatus.Active)
                .Include(t => t.Artist)
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> IncrementPlayCountAsync(Guid trackId, Guid? userId = null)
        {
            var track = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == trackId);
            if (track == null) return false;

            track.PlayCount++;
            track.UpdatedAt = DateTime.UtcNow;

            // Optionally log the play history if userId is provided
            if (userId.HasValue)
            {
                var playHistory = new UserPlayHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = userId.Value,
                    TrackId = trackId,
                    PlayedAt = DateTime.UtcNow
                };
                _context.UserPlayHistories.Add(playHistory);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TrackViewModel> IncrementPlayCountAsync(Guid trackId, Guid userId)
        {
            try
            {
                var track = await _context.Tracks
                    .Include(t => t.Artist)
                    .Include(t => t.Likes)
                    .FirstOrDefaultAsync(t => t.Id == trackId);

                if (track == null)
                    throw new ArgumentException("Track not found", nameof(trackId));

                track.PlayCount++;

                // Record play history if user is provided
                if (userId != Guid.Empty)
                {
                    var playHistory = new UserPlayHistory
                    {
                        UserId = userId,
                        TrackId = trackId,
                        PlayedAt = DateTime.UtcNow
                    };
                    _context.UserPlayHistories.Add(playHistory);
                }

                await _context.SaveChangesAsync();
                return TrackViewModel.FromTrack(track, false, false, false, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing play count for track {TrackId}", trackId);
                throw;
            }
        }

        public async Task<IEnumerable<TrackViewModel>> GetTrendingTracksAsync(int page = 1, int pageSize = 20)
        {
            try
            {
                var tracks = await _context.Tracks
                    .Include(t => t.Artist)
                    .Include(t => t.Likes)
                    .Where(t => t.Status == TrackStatus.Active)
                    .OrderByDescending(t => t.PlayCount)
                    .ThenByDescending(t => t.Likes.Count)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize).ToListAsync();

                return tracks.Select(t => TrackViewModel.FromTrack(t, false, false, false, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending tracks");
                return Enumerable.Empty<TrackViewModel>();
            }
        }

        public async Task<IEnumerable<TrackViewModel>> GetDiscoverTracksAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            try
            {
                // Get tracks from users that the current user doesn't follow
                var followingIds = await _context.Follows
                    .Where(f => f.FollowerId == userId)
                    .Select(f => f.FollowingId)
                    .ToListAsync();

                var tracks = await _context.Tracks
                    .Include(t => t.Artist)
                    .Include(t => t.Likes)
                    .Where(t => t.Status == TrackStatus.Active)
                    .Where(t => t.ArtistId != userId && !followingIds.Contains(t.ArtistId))
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize).ToListAsync();

                return tracks.Select(t => TrackViewModel.FromTrack(t, false, false, false, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting discover tracks for user {UserId}", userId);
                return Enumerable.Empty<TrackViewModel>();
            }
        }

        public async Task<IEnumerable<TrackViewModel>> SearchTracksAsync(string query, int page = 1, int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return Enumerable.Empty<TrackViewModel>();

                var tracks = await _context.Tracks
                    .Include(t => t.Artist)
                    .Include(t => t.Likes)
                    .Where(t => t.Status == TrackStatus.Active)
                    .Where(t => t.Title.Contains(query) ||
                               t.Artist.Username.Contains(query) ||
                               (t.Description != null && t.Description.Contains(query)))
                    .OrderByDescending(t => t.PlayCount)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize).ToListAsync();

                return tracks.Select(t => TrackViewModel.FromTrack(t, false, false, false, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tracks with query: {Query}", query); return Enumerable.Empty<TrackViewModel>();
            }
        }

        // Admin-specific methods
        public async Task<int> GetTotalTrackCountAsync()
        {
            return await _context.Tracks.CountAsync();
        }

        public async Task<int> GetNewTrackCountAsync(DateTime fromDate)
        {
            return await _context.Tracks.CountAsync(t => t.CreatedAt >= fromDate);
        }

        public async Task<IEnumerable<TrackViewModel>> GetRecentTracksForAdminAsync(int count)
        {
            var tracks = await _context.Tracks
                .Include(t => t.Artist)
                .Where(t => t.Status == TrackStatus.Active)
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .ToListAsync();

            return tracks.Select(t => TrackViewModel.FromTrack(t, false, false, false, false));
        }

        public async Task<IEnumerable<TrackViewModel>> GetTracksForAdminAsync(int page, int pageSize, string? search = null, TrackStatus? status = null)
        {
            var query = _context.Tracks
                .Include(t => t.Artist)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Title.Contains(search) ||
                                        t.Artist.DisplayName.Contains(search) ||
                                        (t.Description != null && t.Description.Contains(search)));
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            var tracks = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return tracks.Select(t => TrackViewModel.FromTrack(t, false, false, false, false));
        }

        public async Task<bool> UpdateTrackStatusAsync(Guid trackId, TrackStatus newStatus, Guid modifiedBy, string? reason = null)
        {
            var track = await _context.Tracks.FindAsync(trackId);
            if (track == null) return false;

            track.Status = newStatus;
            track.UpdatedAt = DateTime.UtcNow;            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddTrackToAlbumAsync(AddTrackToAlbumViewModel model, Guid userId)
        {
            try
            {
                // Check if album exists and belongs to user
                var album = await _context.Albums
                    .FirstOrDefaultAsync(a => a.Id == model.AlbumId && a.ArtistId == userId && a.DeletedAt == null);

                if (album == null)
                {
                    _logger.LogWarning($"Album {model.AlbumId} not found or doesn't belong to user {userId}");
                    return false;
                }

                // Handle audio file upload
                string? audioFileUrl = null;
                if (model.AudioFile != null)
                {
                    try
                    {
                        audioFileUrl = await _fileUploadService.UploadAudioAsync(model.AudioFile, "tracks");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to upload audio file for track {Title}", model.Title);
                        throw new InvalidOperationException($"Audio file upload failed: {ex.Message}");
                    }
                }                // Create new track
                var track = new Track
                {
                    Id = Guid.NewGuid(),
                    Title = model.Title,
                    Description = model.Description ?? string.Empty,
                    Genre = model.Genre,
                    DurationInSeconds = model.DurationInSeconds,
                    AudioFileUrl = audioFileUrl ?? string.Empty,
                    ArtistId = userId,
                    AlbumId = model.AlbumId,
                    IsExplicit = model.IsExplicit,
                    AllowComments = model.AllowComments,
                    AllowDownloads = model.AllowDownloads,
                    Status = TrackStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Tracks.Add(track);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Track {track.Title} successfully added to album {album.Title}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding track to album {AlbumId}", model.AlbumId);
                return false;
            }
        }    }
}