using Microsoft.AspNetCore.Mvc;
using Eryth.Services;
using System.Security.Claims;

namespace Eryth.ViewComponents
{
    public class UserProfileImageViewComponent : ViewComponent
    {
        private readonly IUserService _userService;

        public UserProfileImageViewComponent(IUserService userService)
        {
            _userService = userService;
        }        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Kullanıcı giriş yapmamışsa default image döndür
            if (!User.Identity?.IsAuthenticated == true)
            {
                return View("Default");
            }

            var claimsPrincipal = User as ClaimsPrincipal;
            var userIdClaim = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return View("Default");
            }

            try
            {
                var user = await _userService.GetByIdAsync(userId);
                var profileImageUrl = user?.ProfileImageUrl;

                // ProfileImageUrl varsa onu kullan, yoksa default
                var imageUrl = !string.IsNullOrEmpty(profileImageUrl) 
                    ? profileImageUrl 
                    : "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='32' height='32' viewBox='0 0 24 24' fill='none' stroke='%23666' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2'/%3E%3Ccircle cx='12' cy='7' r='4'/%3E%3C/svg%3E";

                return View("ProfileImage", imageUrl);
            }
            catch
            {
                return View("Default");
            }
        }
    }
}
