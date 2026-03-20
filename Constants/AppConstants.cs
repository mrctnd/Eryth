namespace Eryth.Constants
{
    public static class AppConstants
    {
        // File size limits
        public const long MaxAudioFileSizeBytes = 50 * 1024 * 1024; // 50MB
        public const long MaxImageFileSizeBytes = 5 * 1024 * 1024;  // 5MB
        public const long MaxProfileImageSizeBytes = 2 * 1024 * 1024; // 2MB

        // Rate limiting
        public const int DailyTrackUploadLimit = 10;
        public const int HourlyTrackEditLimit = 20;
        public const int HourlyTrackDeleteLimit = 10;
        public const int LikeToggleLimit = 50;
        public const int ProfileUpdateLimit = 5;
        public const int CommentCreateLimit = 10;
        public const int CommentViewLimit = 60;
        public const int SearchRequestLimit = 30;
        public const int LoginAttemptLimit = 5;
        public const int RegisterAttemptLimit = 3;

        // Pagination
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;

        // Allowed file extensions
        public static readonly string[] AllowedAudioExtensions = { ".mp3", ".wav", ".flac", ".m4a" };
        public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    }
}
