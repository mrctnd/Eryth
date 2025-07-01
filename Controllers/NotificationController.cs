using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eryth.Services;
using Eryth.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace Eryth.Controllers
{
    // Bildirim işlemlerini yöneten controller
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService, IMemoryCache cache) : base(cache)
        {
            _notificationService = notificationService;
        }

        // Bildirimler sayfası
        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                var userId = GetRequiredUserId();
                var (validPage, pageSize) = ValidatePagination(page);

                var notifications = await _notificationService.GetUserNotificationsAsync(userId, validPage, pageSize);

                ViewBag.CurrentPage = validPage;
                ViewBag.HasNextPage = notifications.Count() == pageSize;

                return View(notifications);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                TempData["Error"] = "Bildirimler yüklenirken bir hata oluştu";
                return View(new List<NotificationViewModel>());
            }
        }

        // Okunmamış bildirim sayısını alma
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            // Rate limiting kontrolü
            if (IsRateLimited("get-unread-count", 60, TimeSpan.FromMinutes(1)))
            {
                return ErrorResult("Çok fazla istek. Biraz bekleyip tekrar deneyin.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var count = await _notificationService.GetUnreadNotificationCountAsync(userId);

                return SuccessResult(new { unreadCount = count });
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("Giriş yapmanız gerekiyor");
            }
            catch (Exception)
            {
                return ErrorResult("Bildirim sayısı alınamadı");
            }
        }

        // Bildirimi okundu olarak işaretleme
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("mark-as-read", 100, TimeSpan.FromMinutes(5)))
            {
                return ErrorResult("Çok fazla bildirim işlemi. 5 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var success = await _notificationService.MarkAsReadAsync(id, userId);

                if (success)
                {
                    return SuccessResult(message: "Bildirim okundu olarak işaretlendi");
                }

                return ErrorResult("Bildirim güncellenemedi");
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("Giriş yapmanız gerekiyor");
            }
            catch (Exception)
            {
                return ErrorResult("Bildirim güncellenemedi");
            }
        }

        // Tüm bildirimleri okundu olarak işaretleme
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            // Rate limiting kontrolü
            if (IsRateLimited("mark-all-as-read", 3, TimeSpan.FromMinutes(15)))
            {
                return ErrorResult("Çok fazla toplu işlem. 15 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var success = await _notificationService.MarkAllAsReadAsync(userId);

                if (success)
                {
                    return SuccessResult(message: "Tüm bildirimler okundu olarak işaretlendi");
                }

                return ErrorResult("Bildirimler güncellenemedi");
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("Giriş yapmanız gerekiyor");
            }
            catch (Exception)
            {
                return ErrorResult("Bildirimler güncellenemedi");
            }
        }

        // Bildirimi silme
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("delete-notification", 20, TimeSpan.FromMinutes(10)))
            {
                return ErrorResult("Çok fazla silme işlemi. 10 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var success = await _notificationService.DeleteNotificationAsync(id, userId);

                if (success)
                {
                    return SuccessResult(message: "Bildirim silindi");
                }

                return ErrorResult("Bildirim silinemedi");
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("Giriş yapmanız gerekiyor");
            }
            catch (Exception)
            {
                return ErrorResult("Bildirim silinemedi");
            }
        }

        // Tüm bildirimleri silme
        [HttpPost]
        public async Task<IActionResult> DeleteAll()
        {
            // Rate limiting kontrolü
            if (IsRateLimited("delete-all-notifications", 2, TimeSpan.FromHours(1)))
            {
                return ErrorResult("Çok fazla toplu silme işlemi. 1 saat bekleyip tekrar deneyin.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var success = await _notificationService.DeleteAllNotificationsAsync(userId);

                if (success)
                {
                    return SuccessResult(message: "Tüm bildirimler silindi");
                }

                return ErrorResult("Bildirimler silinemedi");
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("Giriş yapmanız gerekiyor");
            }
            catch (Exception)
            {
                return ErrorResult("Bildirimler silinemedi");
            }
        }

        // Son bildirimleri getirme (AJAX için)
        [HttpGet]
        public async Task<IActionResult> GetLatest(int count = 5)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("get-latest-notifications", 30, TimeSpan.FromMinutes(1)))
            {
                return ErrorResult("Çok fazla istek. Biraz bekleyip tekrar deneyin.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var notifications = await _notificationService.GetUserNotificationsAsync(userId, 1, count);

                return SuccessResult(notifications);
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("Giriş yapmanız gerekiyor");
            }
            catch (Exception)
            {
                return ErrorResult("Bildirimler alınamadı");
            }
        }
    }
}
