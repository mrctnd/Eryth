using Microsoft.EntityFrameworkCore;
using Eryth.Data;
using Eryth.Models;
using Eryth.ViewModels;

namespace Eryth.Services
{
    // Beğeni işlemleri servis implementasyonu
    public class LikeService : ILikeService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public LikeService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }        public async Task<bool> LikeTrackAsync(Guid trackId, Guid userId)
        {
            var track = await _context.Tracks.FindAsync(trackId);
            if (track == null) return false;

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.TrackId == trackId && l.UserId == userId);

            if (existingLike != null) return false;

            var like = new Like
            {
                TrackId = trackId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Likes.Add(like);
            
            // Update cached like count
            track.LikeCount++;
            _context.Tracks.Update(track);
            
            await _context.SaveChangesAsync();

            // Bildirim gönder
            if (track.ArtistId != userId)
            {
                await _notificationService.CreateLikeNotificationAsync(userId, track.ArtistId, trackId);
            }

            return true;
        }

        public async Task<bool> UnlikeTrackAsync(Guid trackId, Guid userId)
        {
            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.TrackId == trackId && l.UserId == userId);

            if (like == null) return false;

            var track = await _context.Tracks.FindAsync(trackId);
            if (track == null) return false;

            _context.Likes.Remove(like);
            
            // Update cached like count
            track.LikeCount = Math.Max(0, track.LikeCount - 1);
            _context.Tracks.Update(track);
            
            await _context.SaveChangesAsync();
            return true;
        }public async Task<bool> LikePlaylistAsync(Guid playlistId, Guid userId)
        {
            try
            {
                var playlist = await _context.Playlists.FindAsync(playlistId);
                if (playlist == null) 
                {
                    Console.WriteLine($"Playlist not found: {playlistId}");
                    return false;
                }

                var existingLike = await _context.Likes
                    .FirstOrDefaultAsync(l => l.PlaylistId == playlistId && l.UserId == userId);

                if (existingLike != null) 
                {
                    Console.WriteLine($"Like already exists for playlist {playlistId} and user {userId}");
                    return false;
                }

                var like = new Like
                {
                    PlaylistId = playlistId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Likes.Add(like);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Successfully liked playlist {playlistId} by user {userId}");

                // Bildirim gönder
                if (playlist.CreatedByUserId != userId)
                {
                    try
                    {
                        await _notificationService.CreateLikeNotificationAsync(userId, playlist.CreatedByUserId, null, playlistId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to create notification: {ex.Message}");
                        // Don't fail the like operation if notification fails
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LikePlaylistAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }        public async Task<bool> UnlikePlaylistAsync(Guid playlistId, Guid userId)
        {
            try
            {
                var like = await _context.Likes
                    .FirstOrDefaultAsync(l => l.PlaylistId == playlistId && l.UserId == userId);

                if (like == null) 
                {
                    Console.WriteLine($"No like found for playlist {playlistId} and user {userId}");
                    return false;
                }

                _context.Likes.Remove(like);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Successfully unliked playlist {playlistId} by user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UnlikePlaylistAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> IsTrackLikedAsync(Guid trackId, Guid userId)
        {
            return await _context.Likes
                .AnyAsync(l => l.TrackId == trackId && l.UserId == userId);
        }
        public async Task<bool> IsPlaylistLikedAsync(Guid playlistId, Guid userId)
        {
            return await _context.Likes
                .AnyAsync(l => l.PlaylistId == playlistId && l.UserId == userId);
        }

        public async Task<bool> IsLikedByUserAsync(Guid entityId, Guid userId, string entityType)
        {
            return entityType.ToLower() switch
            {
                "track" => await IsTrackLikedAsync(entityId, userId),
                "playlist" => await IsPlaylistLikedAsync(entityId, userId),
                "comment" => await _context.Likes.AnyAsync(l => l.CommentId == entityId && l.UserId == userId),
                _ => false
            };
        }

        public async Task<int> GetTrackLikeCountAsync(Guid trackId)
        {
            return await _context.Likes
                .CountAsync(l => l.TrackId == trackId);
        }

        public async Task<int> GetPlaylistLikeCountAsync(Guid playlistId)
        {
            return await _context.Likes
                .CountAsync(l => l.PlaylistId == playlistId);
        }
        public async Task<IEnumerable<TrackViewModel>> GetLikedTracksAsync(Guid userId, int page, int pageSize)
        {
            var tracks = await _context.Likes
                .Include(l => l.Track)
                    .ThenInclude(t => t!.Artist)
                .Include(l => l.Track)
                    .ThenInclude(t => t!.Album)
                .Where(l => l.UserId == userId && l.TrackId != null)
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => l.Track!)
                .ToListAsync();

            return tracks.Select(t => TrackViewModel.FromTrack(t, true, true, true, true));
        }
        public async Task<IEnumerable<PlaylistViewModel>> GetLikedPlaylistsAsync(Guid userId, int page, int pageSize)
        {
            var playlists = await _context.Likes
                .Include(l => l.Playlist)
                    .ThenInclude(p => p!.CreatedByUser)
                .Include(l => l.Playlist)
                    .ThenInclude(p => p!.PlaylistTracks)
                .Where(l => l.UserId == userId && l.PlaylistId != null)
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => l.Playlist!)
                .ToListAsync();

            return playlists.Select(p => PlaylistViewModel.FromPlaylist(p, userId));
        }
    }
}
