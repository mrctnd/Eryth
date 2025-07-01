using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Eryth.Data;
using Eryth.ViewModels;
using Eryth.Models.Enums;

namespace Eryth.Services
{
    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _context;

        public SearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SearchResultsViewModel> SearchAsync(ClaimsPrincipal user, string query)
        {
            var results = new SearchResultsViewModel();
            var startTime = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(query))
                return results;

            var trimmedQuery = query.Trim();
            var currentUserId = GetCurrentUserId(user);

            // Search tracks (limit to 10)
            var tracks = await SearchTracksAsync(trimmedQuery, currentUserId, 10);
            results.Tracks = tracks;

            // Search albums (limit to 10)
            var albums = await SearchAlbumsAsync(trimmedQuery, currentUserId, 10);
            results.Albums = albums;

            // Search playlists (limit to 10)
            var playlists = await SearchPlaylistsAsync(trimmedQuery, currentUserId, 10);
            results.Playlists = playlists;

            // Search users (limit to 10)
            var users = await SearchUsersAsync(trimmedQuery, currentUserId, 10);
            results.Users = users;

            results.SearchDuration = DateTime.UtcNow - startTime;
            results.SearchTimestamp = DateTime.UtcNow;

            return results;
        }

        public async Task<List<string>> GetSearchSuggestionsAsync(string query, int maxResults = 5)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return new List<string>();

            var trimmedQuery = query.Trim().ToLower();
            var suggestions = new List<string>();            // Get track suggestions
            var trackSuggestions = await _context.Tracks
                .Where(t => t.DeletedAt == null && 
                           t.Status == TrackStatus.Active &&
                           t.Title.ToLower().Contains(trimmedQuery))
                .Select(t => t.Title)
                .Take(maxResults)
                .ToListAsync();

            suggestions.AddRange(trackSuggestions);

            // Get artist suggestions if we haven't reached maxResults
            if (suggestions.Count < maxResults)
            {
                var artistSuggestions = await _context.Users
                    .Where(u => u.DeletedAt == null &&
                               (u.Username.ToLower().Contains(trimmedQuery) ||
                                u.DisplayName.ToLower().Contains(trimmedQuery)))
                    .Select(u => u.DisplayName ?? u.Username)
                    .Take(maxResults - suggestions.Count)
                    .ToListAsync();

                suggestions.AddRange(artistSuggestions);
            }

            // Get album suggestions if we haven't reached maxResults
            if (suggestions.Count < maxResults)
            {
                var albumSuggestions = await _context.Albums
                    .Where(a => a.DeletedAt == null &&
                               a.Title.ToLower().Contains(trimmedQuery))
                    .Select(a => a.Title)
                    .Take(maxResults - suggestions.Count)
                    .ToListAsync();

                suggestions.AddRange(albumSuggestions);
            }

            return suggestions.Distinct().Take(maxResults).ToList();
        }

        private async Task<List<SearchTrackViewModel>> SearchTracksAsync(string query, Guid currentUserId, int limit)
        {
            var queryLower = query.ToLower();            var tracks = await _context.Tracks
                .Where(t => t.DeletedAt == null && 
                           t.Status == TrackStatus.Active &&
                           (t.Title.ToLower().Contains(queryLower) ||
                            t.Artist.Username.ToLower().Contains(queryLower) ||
                            t.Artist.DisplayName.ToLower().Contains(queryLower)))
                .Include(t => t.Artist)
                .Include(t => t.Album)
                .Include(t => t.Likes)
                .OrderByDescending(t => t.PlayCount)
                .ThenByDescending(t => t.LikeCount)
                .ThenByDescending(t => t.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return tracks.Select(t => SearchTrackViewModel.FromTrack(t, currentUserId)).ToList();
        }

        private async Task<List<SearchAlbumViewModel>> SearchAlbumsAsync(string query, Guid currentUserId, int limit)
        {
            var queryLower = query.ToLower();

            var albums = await _context.Albums
                .Where(a => a.DeletedAt == null &&
                           (a.Title.ToLower().Contains(queryLower) ||
                            a.Artist.Username.ToLower().Contains(queryLower) ||
                            a.Artist.DisplayName.ToLower().Contains(queryLower)))
                .Include(a => a.Artist)
                .Include(a => a.Tracks)
                .OrderByDescending(a => a.TotalPlayCount)
                .ThenByDescending(a => a.TotalLikeCount)
                .ThenByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return albums.Select(a => SearchAlbumViewModel.FromAlbum(a, currentUserId)).ToList();
        }

        private async Task<List<SearchPlaylistViewModel>> SearchPlaylistsAsync(string query, Guid currentUserId, int limit)
        {
            var queryLower = query.ToLower();

            var playlists = await _context.Playlists
                .Where(p => p.DeletedAt == null &&
                           p.Privacy == PlaylistPrivacy.Public &&
                           (p.Title.ToLower().Contains(queryLower) ||
                            p.CreatedByUser.Username.ToLower().Contains(queryLower) ||
                            p.CreatedByUser.DisplayName.ToLower().Contains(queryLower)))
                .Include(p => p.CreatedByUser)
                .Include(p => p.PlaylistTracks)
                    .ThenInclude(pt => pt.Track)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.PlayCount)
                .ThenByDescending(p => p.LikeCount)
                .ThenByDescending(p => p.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return playlists.Select(p => SearchPlaylistViewModel.FromPlaylist(p, currentUserId)).ToList();
        }

        private async Task<List<SearchUserViewModel>> SearchUsersAsync(string query, Guid currentUserId, int limit)
        {
            var queryLower = query.ToLower();

            var users = await _context.Users
                .Where(u => u.DeletedAt == null &&
                           (u.Username.ToLower().Contains(queryLower) ||
                            u.DisplayName.ToLower().Contains(queryLower) ||
                            (u.Bio != null && u.Bio.ToLower().Contains(queryLower))))
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .Include(u => u.Tracks)
                .Include(u => u.Albums)
                .Include(u => u.Playlists)
                .OrderByDescending(u => u.Followers.Count)
                .ThenByDescending(u => u.Tracks.Count)
                .ThenByDescending(u => u.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return users.Select(u => SearchUserViewModel.FromUser(u, currentUserId)).ToList();
        }

        private Guid GetCurrentUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user?.FindFirst("UserId")?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
