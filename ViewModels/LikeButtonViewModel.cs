namespace Eryth.ViewModels
{
    public class LikeButtonViewModel
    {
        public Guid TrackId { get; set; }
        public bool IsLiked { get; set; }
        public int LikeCount { get; set; }
        public bool ShowCount { get; set; } = true;
        public string? ButtonClass { get; set; }

        public static LikeButtonViewModel Create(Guid trackId, bool isLiked, int likeCount, bool showCount = true, string? buttonClass = null)
        {
            return new LikeButtonViewModel
            {
                TrackId = trackId,
                IsLiked = isLiked,
                LikeCount = likeCount,
                ShowCount = showCount,
                ButtonClass = buttonClass
            };
        }
    }
}
