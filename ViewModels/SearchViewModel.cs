using System.ComponentModel.DataAnnotations;
using Eryth.Models;
using Eryth.Models.Enums;

namespace Eryth.ViewModels
{
    public class SearchViewModel
    {
        [Display(Name = "Search Query")]
        [StringLength(200, ErrorMessage = "Search query cannot exceed 200 characters")]
        public string Query { get; set; } = string.Empty;

        [Display(Name = "Search Type")]
        public SearchType SearchType { get; set; } = SearchType.All;

        [Display(Name = "Genre Filter")]
        public Genre? GenreFilter { get; set; }

        [Display(Name = "Date Range")]
        public DateRangeFilter DateRange { get; set; } = DateRangeFilter.Any;

        [Display(Name = "Sort By")]
        public SearchSortOption SortBy { get; set; } = SearchSortOption.Relevance;

        [Display(Name = "Sort Order")]
        public SortOrder SortOrder { get; set; } = SortOrder.Descending;

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int ItemsPerPage { get; set; } = 20;

        // Results
        public SearchResultsViewModel Results { get; set; } = new();
        public List<SearchTrackViewModel> Tracks
        {
            get => Results.Tracks;
            set => Results.Tracks = value;
        }
        public List<SearchUserViewModel> Users
        {
            get => Results.Users;
            set => Results.Users = value;
        }
        public List<SearchPlaylistViewModel> Playlists
        {
            get => Results.Playlists;
            set => Results.Playlists = value;
        }

        public List<string> Suggestions { get; set; } = new();
        public List<string> RecentSearches { get; set; } = new();
        public List<string> TrendingSearches { get; set; } = new();

        public Guid CurrentUserId { get; set; }

        public bool HasQuery => !string.IsNullOrWhiteSpace(Query);
        public bool HasResults => Results.HasAnyResults;
        public bool IsFiltered => GenreFilter.HasValue || DateRange != DateRangeFilter.Any;
        public string TrimmedQuery => Query?.Trim() ?? string.Empty;
        public int TotalResults => Results.TotalResults;

        public string GetSearchCriteriaText()
        {
            var criteria = new List<string>();

            if (SearchType != SearchType.All)
                criteria.Add($"Type: {SearchType}");

            if (GenreFilter.HasValue)
                criteria.Add($"Genre: {GenreFilter}");

            if (DateRange != DateRangeFilter.Any)
                criteria.Add($"Date: {GetDateRangeDisplayText()}");

            if (SortBy != SearchSortOption.Relevance)
                criteria.Add($"Sort: {SortBy} {SortOrder}");

            return criteria.Any() ? string.Join(" | ", criteria) : "All content";
        }

        public string GetSearchUrl(string? newQuery = null, SearchType? newType = null,
                                  int? newPage = null, SearchSortOption? newSort = null)
        {
            var query = newQuery ?? Query;
            var type = newType ?? SearchType;
            var page = newPage ?? CurrentPage;
            var sort = newSort ?? SortBy;

            var url = $"/search?q={Uri.EscapeDataString(query)}&type={type}&page={page}&sort={sort}&order={SortOrder}";

            if (GenreFilter.HasValue)
                url += $"&genre={GenreFilter}";

            if (DateRange != DateRangeFilter.Any)
                url += $"&dateRange={DateRange}";

            return url;
        }

        private string GetDateRangeDisplayText()
        {
            return DateRange switch
            {
                DateRangeFilter.Today => "Today",
                DateRangeFilter.ThisWeek => "This Week",
                DateRangeFilter.ThisMonth => "This Month",
                DateRangeFilter.ThisYear => "This Year",
                DateRangeFilter.LastWeek => "Last Week",
                DateRangeFilter.LastMonth => "Last Month",
                DateRangeFilter.LastYear => "Last Year",
                _ => "Any Time"
            };
        }
    }

    /// ViewModel tüm arama sonuçları için
    public class SearchResultsViewModel
    {
        public List<SearchTrackViewModel> Tracks { get; set; } = new();
        public List<SearchAlbumViewModel> Albums { get; set; } = new();
        public List<SearchPlaylistViewModel> Playlists { get; set; } = new();
        public List<SearchUserViewModel> Users { get; set; } = new();

        public int TrackCount => Tracks.Count;
        public int AlbumCount => Albums.Count;
        public int PlaylistCount => Playlists.Count;
        public int UserCount => Users.Count;
        public int TotalResults => TrackCount + AlbumCount + PlaylistCount + UserCount;

        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;

        public TimeSpan SearchDuration { get; set; }
        public DateTime SearchTimestamp { get; set; } = DateTime.UtcNow;

        public bool HasAnyResults => TotalResults > 0;
        public bool HasTracks => TrackCount > 0;
        public bool HasAlbums => AlbumCount > 0;
        public bool HasPlaylists => PlaylistCount > 0;
        public bool HasUsers => UserCount > 0;

        public string FormattedSearchDuration => $"{SearchDuration.TotalMilliseconds:F0}ms";

        public string GetResultSummary()
        {
            if (!HasAnyResults) return "No results found";

            var parts = new List<string>();
            if (HasTracks) parts.Add($"{TrackCount} track{(TrackCount != 1 ? "s" : "")}");
            if (HasAlbums) parts.Add($"{AlbumCount} album{(AlbumCount != 1 ? "s" : "")}");
            if (HasPlaylists) parts.Add($"{PlaylistCount} playlist{(PlaylistCount != 1 ? "s" : "")}");
            if (HasUsers) parts.Add($"{UserCount} user{(UserCount != 1 ? "s" : "")}");

            return string.Join(", ", parts);
        }
    }

    /// ViewModel şarkı arama sonuçları için
    public class SearchTrackViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public string ArtistUsername { get; set; } = string.Empty;
        public string? AlbumName { get; set; }
        public int Duration { get; set; }
        public Genre Genre { get; set; }
        public string? CoverArtUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PlayCount { get; set; }
        public int LikeCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public bool CanPlay { get; set; }

        public double RelevanceScore { get; set; }
        public List<string> MatchedFields { get; set; } = new();

        public string FormattedDuration => TimeSpan.FromSeconds(Duration).ToString(@"mm\:ss");
        public string RelativeDate => GetRelativeTime(CreatedAt);
        public static SearchTrackViewModel FromTrack(Track track, Guid currentUserId, double relevanceScore = 0)
        {
            return new SearchTrackViewModel
            {
                Id = track.Id,
                Title = track.Title?.Trim() ?? string.Empty,
                ArtistName = track.Artist?.DisplayName?.Trim() ?? track.Artist?.Username?.Trim() ?? string.Empty,
                ArtistUsername = track.Artist?.Username?.Trim() ?? string.Empty,
                AlbumName = track.Album?.Title?.Trim(),
                Duration = track.DurationInSeconds,
                Genre = track.Genre,
                CoverArtUrl = track.CoverImageUrl?.Trim(),
                CreatedAt = track.CreatedAt,
                PlayCount = (int)track.PlayCount,
                LikeCount = track.Likes?.Count ?? 0,
                IsLikedByCurrentUser = track.Likes?.Any(l => l.UserId == currentUserId) ?? false,
                CanPlay = track.IsPublished,
                RelevanceScore = relevanceScore
            };
        }

        private static string GetRelativeTime(DateTime dateTime)
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

    /// ViewModel albüm arama sonuçları için
    public class SearchAlbumViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
        public string? CoverArtUrl { get; set; }
        public int TrackCount { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public int LikeCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }

        public double RelevanceScore { get; set; }
        public List<string> MatchedFields { get; set; } = new();

        public string FormattedDuration => TotalDuration.TotalHours >= 1
            ? $"{(int)TotalDuration.TotalHours}h {TotalDuration.Minutes}m"
            : $"{TotalDuration.Minutes}m {TotalDuration.Seconds}s";
        public string RelativeDate => ReleaseDate.HasValue ? GetRelativeTime(ReleaseDate.Value) : "Unknown";        public static SearchAlbumViewModel FromAlbum(Album album, Guid currentUserId, double relevanceScore = 0)
        {
            return new SearchAlbumViewModel
            {
                Id = album.Id,
                Title = album.Title?.Trim() ?? string.Empty,
                ArtistName = album.Artist?.DisplayName?.Trim() ?? album.Artist?.Username?.Trim() ?? string.Empty,
                ReleaseDate = album.ReleaseDate,
                CoverArtUrl = album.CoverImageUrl?.Trim(),
                TrackCount = album.Tracks?.Count ?? 0,
                TotalDuration = album.TotalDuration,
                LikeCount = (int)album.TotalLikeCount,
                IsLikedByCurrentUser = false,
                RelevanceScore = relevanceScore
            };
        }

        private static string GetRelativeTime(DateTime dateTime)
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

    /// ViewModel çalma listesi arama sonuçları için
    public class SearchPlaylistViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string OwnerUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TrackCount { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public PlaylistPrivacy Privacy { get; set; }
        public int LikeCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public string? CoverArtUrl { get; set; }

        public double RelevanceScore { get; set; }
        public List<string> MatchedFields { get; set; } = new();

        public string FormattedDuration => TotalDuration.TotalHours >= 1
            ? $"{(int)TotalDuration.TotalHours}h {TotalDuration.Minutes}m"
            : $"{TotalDuration.Minutes}m {TotalDuration.Seconds}s";
        public string RelativeDate => GetRelativeTime(CreatedAt);
        public bool IsPublic => Privacy == PlaylistPrivacy.Public;       
         public static SearchPlaylistViewModel FromPlaylist(Playlist playlist, Guid currentUserId, double relevanceScore = 0)
        {
            var coverArt = playlist.PlaylistTracks?.Where(pt => pt.Track?.CoverImageUrl != null)
                                                  .OrderBy(pt => pt.OrderIndex)
                                                  .FirstOrDefault()?.Track?.CoverImageUrl?.Trim();

            return new SearchPlaylistViewModel
            {
                Id = playlist.Id,
                Name = playlist.Title?.Trim() ?? string.Empty,
                Description = playlist.Description?.Trim(),
                OwnerUsername = playlist.CreatedByUser?.DisplayName?.Trim() ?? playlist.CreatedByUser?.Username?.Trim() ?? string.Empty,
                CreatedAt = playlist.CreatedAt,
                TrackCount = playlist.PlaylistTracks?.Count ?? 0,
                TotalDuration = playlist.TotalDuration,
                Privacy = playlist.Privacy,
                LikeCount = playlist.Likes?.Count ?? 0,
                IsLikedByCurrentUser = playlist.Likes?.Any(l => l.UserId == currentUserId) ?? false,
                CoverArtUrl = coverArt,
                RelevanceScore = relevanceScore
            };
        }

        private static string GetRelativeTime(DateTime dateTime)
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

    /// ViewModel kullanıcı arama sonuçları için
    public class SearchUserViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime JoinedAt { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public int TrackCount { get; set; }
        public int AlbumCount { get; set; }
        public int PlaylistCount { get; set; }
        public bool IsFollowedByCurrentUser { get; set; }
        public bool IsCurrentUser { get; set; }

        public double RelevanceScore { get; set; }
        public List<string> MatchedFields { get; set; } = new();

        public string DisplayNameOrUsername => DisplayName?.Trim() ?? Username;
        public string RelativeJoinDate => GetRelativeTime(JoinedAt);
        public int TotalContent => TrackCount + AlbumCount + PlaylistCount;

        public static SearchUserViewModel FromUser(User user, Guid currentUserId, double relevanceScore = 0)
        {
            return new SearchUserViewModel
            {
                Id = user.Id,
                Username = user.Username?.Trim() ?? string.Empty,
                DisplayName = user.DisplayName?.Trim(),
                Bio = user.Bio?.Trim(),
                AvatarUrl = user.ProfileImageUrl?.Trim(),
                JoinedAt = user.CreatedAt,
                FollowerCount = user.Followers?.Count ?? 0,
                FollowingCount = user.Following?.Count ?? 0,
                TrackCount = user.Tracks?.Count ?? 0,
                AlbumCount = user.Albums?.Count ?? 0,
                PlaylistCount = user.Playlists?.Count ?? 0,
                IsFollowedByCurrentUser = user.Followers?.Any(f => f.FollowerId == currentUserId) ?? false,
                IsCurrentUser = user.Id == currentUserId,
                RelevanceScore = relevanceScore
            };
        }

        private static string GetRelativeTime(DateTime dateTime)
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

    public enum SearchType
    {
        [Display(Name = "All")]
        All,
        [Display(Name = "Tracks")]
        Tracks,
        [Display(Name = "Albums")]
        Albums,
        [Display(Name = "Playlists")]
        Playlists,
        [Display(Name = "Users")]
        Users
    }

    public enum DateRangeFilter
    {
        [Display(Name = "Any Time")]
        Any,
        [Display(Name = "Today")]
        Today,
        [Display(Name = "This Week")]
        ThisWeek,
        [Display(Name = "This Month")]
        ThisMonth,
        [Display(Name = "This Year")]
        ThisYear,
        [Display(Name = "Last Week")]
        LastWeek,
        [Display(Name = "Last Month")]
        LastMonth,
        [Display(Name = "Last Year")]
        LastYear
    }

    public enum SearchSortOption
    {
        [Display(Name = "Relevance")]
        Relevance,
        [Display(Name = "Date Created")]
        DateCreated,
        [Display(Name = "Date Updated")]
        DateUpdated,
        [Display(Name = "Popularity")]
        Popularity,
        [Display(Name = "Title/Name")]
        Title,
        [Display(Name = "Duration")]
        Duration,
        [Display(Name = "Play Count")]
        PlayCount,
        [Display(Name = "Like Count")]
        LikeCount
    }

    public class SearchPageViewModel
    {
        public string Query { get; set; } = string.Empty;
        public SearchResultsViewModel Results { get; set; } = new();
    }
}
