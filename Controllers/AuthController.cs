using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eryth.Services;
using Eryth.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace Eryth.Controllers
{
    // Kullanıcı kimlik doğrulama işlemlerini yöneten controller
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService, IMemoryCache cache) : base(cache)
        {
            _authService = authService;
        }

        // Giriş sayfası
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Giriş yapma işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Rate limiting kontrolü
            if (IsRateLimited("login", 5, TimeSpan.FromMinutes(15)))
            {
                ModelState.AddModelError(string.Empty, "Çok fazla başarısız giriş denemesi. 15 dakika bekleyip tekrar deneyin.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                // Girdi temizleme
                model.EmailOrUsername = SanitizeInput(model.EmailOrUsername);

                var result = await _authService.LoginAsync(model);
                if (result.Success)
                {
                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Giriş yapılırken bir hata oluştu");
                return View(model);
            }
        }

        // Kayıt sayfası
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // Kayıt olma işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("register", 3, TimeSpan.FromHours(1)))
            {
                ModelState.AddModelError(string.Empty, "Çok fazla kayıt denemesi. 1 saat bekleyip tekrar deneyin.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Girdi temizleme
                model.Email = SanitizeInput(model.Email);
                model.Username = SanitizeInput(model.Username);
                model.DisplayName = SanitizeInput(model.DisplayName);
                var result = await _authService.RegisterAsync(model);
                if (result.Success)
                {
                    TempData["Success"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Kayıt olurken bir hata oluştu");
                return View(model);
            }
        }

        // E-posta doğrulama
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("confirm-email", 10, TimeSpan.FromMinutes(30)))
            {
                TempData["Error"] = "Çok fazla e-posta doğrulama denemesi. 30 dakika bekleyip tekrar deneyin.";
                return RedirectToAction(nameof(Login));
            }

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Geçersiz doğrulama bağlantısı";
                return RedirectToAction(nameof(Login));
            }

            try
            {
                // Girdi temizleme
                userId = SanitizeInput(userId);
                token = SanitizeInput(token);

                var result = await _authService.ConfirmEmailAsync(userId, token);
                if (result.Success)
                {
                    TempData["Success"] = "E-posta adresiniz başarıyla doğrulandı";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Login));
            }
            catch (Exception)
            {
                TempData["Error"] = "E-posta doğrulanırken bir hata oluştu";
                return RedirectToAction(nameof(Login));
            }
        }

        // Şifre sıfırlama sayfası
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }       
        
         // Şifre sıfırlama e-postası gönderme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("forgot-password", 3, TimeSpan.FromMinutes(30)))
            {
                ModelState.AddModelError(string.Empty, "Çok fazla şifre sıfırlama denemesi. 30 dakika bekleyip tekrar deneyin.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Girdi Temizleme
                model.Email = SanitizeInput(model.Email);

                var result = await _authService.SendPasswordResetEmailAsync(model.Email);
                TempData["Success"] = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Şifre sıfırlama e-postası gönderilirken bir hata oluştu");
                return View(model);
            }
        }

        // Şifre sıfırlama sayfası
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Geçersiz şifre sıfırlama bağlantısı";
                return RedirectToAction(nameof(Login));
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        // Şifre sıfırlama işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("reset-password", 5, TimeSpan.FromMinutes(15)))
            {
                ModelState.AddModelError(string.Empty, "Çok fazla şifre sıfırlama denemesi. 15 dakika bekleyip tekrar deneyin.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Girdi temizleme
                model.Email = SanitizeInput(model.Email);
                model.Token = SanitizeInput(model.Token);

                var result = await _authService.ResetPasswordAsync(model);
                if (result.Success)
                {
                    TempData["Success"] = "Şifreniz başarıyla sıfırlandı";
                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Şifre sıfırlanırken bir hata oluştu");
                return View(model);
            }
        }

        // Çıkış yapma
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _authService.LogoutAsync();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // Yerel URL'e yönlendirme yardımcı metodu
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
