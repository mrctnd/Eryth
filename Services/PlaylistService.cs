using Microsoft.EntityFrameworkCore;
using Eryth.Data;
using Eryth.Models;
using Eryth.Models.Enums;
using Eryth.ViewModels;
using Eryth.Infrastructure;

namespace Eryth.Services
{
    // Çalma listesi işlemleri servis implementasyonu
    public class PlaylistService : IPlaylistService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;

        public PlaylistService(ApplicationDbContext context, IFileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }
        public async Task<Playlist?> GetByIdAsync(Guid id)
        {
            return await _context.Playlists
                .Include(p => p.CreatedByUser)
                .Include(p => p.PlaylistTracks)
                    .ThenInclude(pt => pt.Track)
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PlaylistViewModel> GetPlaylistViewModelAsync(Guid playlistId, Guid currentUserId)
        {
            var playlist = await GetByIdAsync(playlistId);
            if (playlist == null)
                throw new ArgumentException("Çalma listesi bulunamadı", nameof(playlistId));

            return PlaylistViewModel.FromPlaylist(playlist, currentUserId);
        }
        public async Task<IEnumerable<PlaylistViewModel>> GetUserPlaylistsAsync(Guid userId, Guid currentUserId, int page, int pageSize)
        {
            var playlists = await _context.Playlists
                .Include(p => p.CreatedByUser)
                .Include(p => p.PlaylistTracks)
                .Where(p => p.CreatedByUserId == userId &&
                           (p.Privacy == PlaylistPrivacy.Public || userId == currentUserId))
                .OrderByDescending(p => p.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return playlists.Select(p => PlaylistViewModel.FromPlaylist(p, currentUserId));
        }
        public async Task<IEnumerable<PlaylistViewModel>> GetPublicPlaylistsAsync(int page, int pageSize)
        {
            var playlists = await _context.Playlists
                .Include(p => p.CreatedByUser)
                .Include(p => p.PlaylistTracks)
                .Where(p => p.Privacy == PlaylistPrivacy.Public)
                .OrderByDescending(p => p.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return playlists.Select(p => PlaylistViewModel.FromPlaylist(p, Guid.Empty));
        }        public async Task<Playlist> CreatePlaylistAsync(CreatePlaylistViewModel model, Guid userId)
        {
            var playlist = model.ToPlaylist(userId);

            // Handle cover image upload if provided
            if (model.CoverImage != null)
            {
                try
                {
                    var imageUrl = await _fileUploadService.UploadImageAsync(model.CoverImage, "playlists");
                    playlist.CoverImageUrl = imageUrl;
                }
                catch (Exception)
                {
                    // If image upload fails, continue without image
                    playlist.CoverImageUrl = null;
                }
            }

            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();
            return playlist;
        }
        public async Task<bool> UpdatePlaylistAsync(Guid playlistId, PlaylistViewModel model, Guid userId)
        {
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.CreatedByUserId == userId);

            if (playlist == null) return false;

            playlist.Title = model.Name;
            playlist.Description = model.Description;
            playlist.Privacy = model.Privacy;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeletePlaylistAsync(Guid playlistId, Guid userId)
        {
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.CreatedByUserId == userId);

            if (playlist == null) return false;

            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> AddTrackToPlaylistAsync(Guid playlistId, Guid trackId, Guid userId)
        {
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.CreatedByUserId == userId);

            if (playlist == null) return false;

            var track = await _context.Tracks.FindAsync(trackId);
            if (track == null || track.Status != TrackStatus.Active) return false;

            var existingTrack = await _context.PlaylistTracks
                .FirstOrDefaultAsync(pt => pt.PlaylistId == playlistId && pt.TrackId == trackId);

            if (existingTrack != null) return false;

            var maxOrder = await _context.PlaylistTracks
                .Where(pt => pt.PlaylistId == playlistId)
                .MaxAsync(pt => (int?)pt.OrderIndex) ?? 0;

            var playlistTrack = new PlaylistTrack
            {
                PlaylistId = playlistId,
                TrackId = trackId,
                OrderIndex = maxOrder + 1,
                AddedAt = DateTime.UtcNow,
                AddedByUserId = userId
            };

            _context.PlaylistTracks.Add(playlistTrack);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RemoveTrackFromPlaylistAsync(Guid playlistId, Guid trackId, Guid userId)
        {
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.CreatedByUserId == userId);

            if (playlist == null) return false;

            var playlistTrack = await _context.PlaylistTracks
                .FirstOrDefaultAsync(pt => pt.PlaylistId == playlistId && pt.TrackId == trackId);

            if (playlistTrack == null) return false;

            _context.PlaylistTracks.Remove(playlistTrack);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsTrackInPlaylistAsync(Guid playlistId, Guid trackId)
        {
            return await _context.PlaylistTracks
                .AnyAsync(pt => pt.PlaylistId == playlistId && pt.TrackId == trackId);
        }
        public async Task<IEnumerable<TrackViewModel>> GetPlaylistTracksAsync(Guid playlistId, Guid userId)
        {
            var tracks = await _context.PlaylistTracks
                .Include(pt => pt.Track)
                    .ThenInclude(t => t.Artist)
                .Include(pt => pt.Track)
                    .ThenInclude(t => t.Album)
                .Where(pt => pt.PlaylistId == playlistId)
                .OrderBy(pt => pt.OrderIndex)
                .Select(pt => pt.Track)
                .ToListAsync();

            return tracks.Select(t => TrackViewModel.FromTrack(t, true, true, true, true));
        }

        // Admin-specific methods implementation
        public async Task<int> GetTotalPlaylistCountAsync()
        {
            return await _context.Playlists.CountAsync();
        }

        public async Task<int> GetNewPlaylistCountAsync(int days = 7)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            return await _context.Playlists
                .Where(p => p.CreatedAt >= cutoffDate)
                .CountAsync();
        }

        public async Task<IEnumerable<PlaylistViewModel>> GetRecentPlaylistsAsync(int count = 10)
        {
            var playlists = await _context.Playlists
                .Include(p => p.CreatedByUser)
                .Include(p => p.PlaylistTracks)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();

            return playlists.Select(p => PlaylistViewModel.FromPlaylist(p, Guid.Empty));
        }

        public async Task<IEnumerable<PlaylistViewModel>> GetPlaylistsForAdminAsync(int page = 1, int pageSize = 20, string? search = null, bool? isPublic = null)
        {
            var query = _context.Playlists
                .Include(p => p.CreatedByUser)
                .Include(p => p.PlaylistTracks)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Title.Contains(search) ||
                                        p.Description != null && p.Description.Contains(search) ||
                                        p.CreatedByUser.Username.Contains(search));
            }

            if (isPublic.HasValue)
            {
                var privacy = isPublic.Value ? PlaylistPrivacy.Public : PlaylistPrivacy.Private;
                query = query.Where(p => p.Privacy == privacy);
            }

            var playlists = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return playlists.Select(p => PlaylistViewModel.FromPlaylist(p, Guid.Empty));
        }

        public async Task<bool> UpdatePlaylistStatusAsync(Guid playlistId, bool isActive)
        {
            var playlist = await _context.Playlists.FindAsync(playlistId);
            if (playlist == null) return false;

            // Since there's no IsActive field in the Playlist model, we can use Privacy
            // to simulate active/inactive status (Private = inactive, Public = active)
            // This is a placeholder implementation - adjust based on actual requirements
            playlist.Privacy = isActive ? PlaylistPrivacy.Public : PlaylistPrivacy.Private;
            playlist.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // Çalma listesi kopyalama metodu
        public async Task<Playlist?> DuplicatePlaylistAsync(Guid playlistId, Guid userId)
        {
            try
            {
                var originalPlaylist = await _context.Playlists
                    .Include(p => p.PlaylistTracks)
                        .ThenInclude(pt => pt.Track)
                    .FirstOrDefaultAsync(p => p.Id == playlistId);

                if (originalPlaylist == null)
                {
                    return null;
                }

                // Create new playlist with similar properties
                var newPlaylist = new Playlist
                {
                    Title = $"{originalPlaylist.Title} (Copy)",
                    Description = originalPlaylist.Description,
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Privacy = PlaylistPrivacy.Private, // Default to private for safety
                    IsCollaborative = false,           // Default to non-collaborative
                    CoverImageUrl = originalPlaylist.CoverImageUrl
                };

                // Add to database to get the ID
                _context.Playlists.Add(newPlaylist);
                await _context.SaveChangesAsync();

                // Copy all track references
                if (originalPlaylist.PlaylistTracks != null && originalPlaylist.PlaylistTracks.Any())
                {
                    var playlistTracks = originalPlaylist.PlaylistTracks
                        .OrderBy(pt => pt.OrderIndex)
                        .Select((pt, index) => new PlaylistTrack
                        {
                            PlaylistId = newPlaylist.Id,
                            TrackId = pt.TrackId,
                            OrderIndex = index,
                            AddedAt = DateTime.UtcNow
                        });

                    _context.PlaylistTracks.AddRange(playlistTracks);
                    await _context.SaveChangesAsync();
                }

                return newPlaylist;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
