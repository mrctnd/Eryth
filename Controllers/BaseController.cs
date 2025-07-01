using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace Eryth.Controllers
{
    // Tüm controller'lar için ortak işlevsellik sağlayan temel controller
    public abstract class BaseController : Controller
    {
        private readonly IMemoryCache? _cache;

        protected BaseController()
        {
            // Dependency injection için boş constructor
        }

        protected BaseController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // Giriş yapmış kullanıcının ID'sini alır
        protected Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        // Giriş yapmış kullanıcının ID'sini zorunlu olarak alır, yoksa exception fırlatır
        protected Guid GetRequiredUserId()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("Kullanıcı giriş yapmamış");
            return userId.Value;
        }

        // Giriş yapmış kullanıcının kullanıcı adını alır
        protected string? GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value;
        }

        // Giriş yapmış kullanıcının display name'ini alır
        protected string? GetCurrentDisplayName()
        {
            return User.FindFirst("DisplayName")?.Value ?? GetCurrentUsername();
        }

        // Giriş yapmış kullanıcının profil fotoğrafı URL'ini alır
        protected string? GetCurrentProfileImageUrl()
        {
            return User.FindFirst("ProfileImageUrl")?.Value;
        }

        // Kullanıcının belirtilen role sahip olup olmadığını kontrol eder
        protected bool IsInRole(string role)
        {
            return User.IsInRole(role);
        }

        // Rate limiting kontrolü
        protected bool IsRateLimited(string action, int maxAttempts = 5, TimeSpan? timeWindow = null)
        {
            if (_cache == null) return false;

            timeWindow ??= TimeSpan.FromMinutes(15);
            var key = $"RateLimit_{GetClientIdentifier()}_{action}";

            if (_cache.TryGetValue(key, out int currentAttempts))
            {
                if (currentAttempts >= maxAttempts)
                {
                    return true; // Rate limited
                }
                _cache.Set(key, currentAttempts + 1, timeWindow.Value);
            }
            else
            {
                _cache.Set(key, 1, timeWindow.Value);
            }

            return false;
        }

        // Async rate limiting kontrolü (yeni controller'lar için)
        protected async Task<bool> CheckRateLimitAsync(string action, int maxAttempts = 5, TimeSpan? timeWindow = null)
        {
            if (_cache == null) return true; // Rate limit geçmiş sayılır (güvenli taraf)

            timeWindow ??= TimeSpan.FromMinutes(15);
            var key = $"RateLimit_{GetClientIdentifier()}_{action}";

            await Task.CompletedTask; // Bu metodu async yapmak için minimal task

            if (_cache.TryGetValue(key, out int currentAttempts))
            {
                if (currentAttempts >= maxAttempts)
                {
                    return false; // Rate limited (tersine döndürür - false = rate limited)
                }
                _cache.Set(key, currentAttempts + 1, timeWindow.Value);
            }
            else
            {
                _cache.Set(key, 1, timeWindow.Value);
            }

            return true; // Rate limit geçilmemiş
        }

        // Client identifier (IP + User ID if logged in)
        protected string GetClientIdentifier()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userId = GetCurrentUserId()?.ToString() ?? "anonymous";
            return $"{ip}_{userId}";
        }

        // Başarılı işlem sonucu için JSON response döner
        protected IActionResult SuccessResult(object? data = null, string? message = null)
        {
            return Json(new
            {
                success = true,
                message = message,
                data = data
            });
        }

        // Hata durumu için JSON response döner
        protected IActionResult ErrorResult(string message, object? errors = null)
        {
            return Json(new
            {
                success = false,
                message = message,
                errors = errors
            });
        }

        // Rate limit hatası için özel response
        protected IActionResult RateLimitErrorResult(string message = "Çok fazla deneme yaptınız. Lütfen daha sonra tekrar deneyin.")
        {
            Response.StatusCode = 429; // Too Many Requests
            return Json(new
            {
                success = false,
                message = message,
                rateLimited = true
            });
        }

        // Sayfalama bilgilerini doğrular ve düzenler
        protected (int page, int pageSize) ValidatePagination(int page = 1, int pageSize = 20)
        {
            page = Math.Max(1, page);
            pageSize = Math.Min(Math.Max(1, pageSize), 100);
            return (page, pageSize);
        }

        // Model state hatalarını formatlar
        protected object GetModelStateErrors()
        {
            return ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );
        }

        // Güvenli dosya yükleme kontrolü
        protected bool IsValidFileUpload(IFormFile file, string[] allowedExtensions, long maxSizeBytes)
        {
            if (file == null || file.Length == 0) return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension)) return false;

            if (file.Length > maxSizeBytes) return false;

            return true;
        }

        // XSS koruması için güvenli string sanitization
        protected string SanitizeInput(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // Basit HTML encoding
            return System.Web.HttpUtility.HtmlEncode(input);
        }
    }
}
