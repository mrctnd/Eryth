using Microsoft.AspNetCore.Mvc;
using Eryth.ViewModels;

namespace Eryth.ViewComponents
{
    public class FollowButtonViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Guid userId, bool isFollowing = false, int? followerCount = null, bool showCount = false)
        {
            var model = FollowButtonViewModel.ForUser(userId, isFollowing, followerCount, showCount);
            return View(model);
        }
    }
}
