using Microsoft.AspNetCore.Mvc;
using Eryth.ViewModels;

namespace Eryth.ViewComponents
{
    public class LikeButtonViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Guid entityId, string entityType, bool isLiked = false, int likeCount = 0)
        {
            var model = new LikeButtonViewModel
            {
                TrackId = entityId,
                IsLiked = isLiked,
                LikeCount = likeCount,
                ShowCount = true
            };

            return View(model);
        }
    }
}
