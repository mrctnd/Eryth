---
paths:
  - "Controllers/**/*"
  - "ViewModels/**/*"
  - "Services/**/*"
---

# MVC Controller & API Design Rules

## Controller Kuralları

- Tüm controller'lar `BaseController`'dan miras almalı
- Controller isimleri PascalCase ve `Controller` suffix: `TrackController`
- Action method'ları açıklayıcı isimler taşımalı: `Index`, `Details`, `Create`, `Edit`, `Delete`
- DI ile service inject et (constructor injection)
- Controller'da business logic yazma - service katmanına taşı

## Routing

- Varsayılan route: `{controller=Home}/{action=Index}/{id?}`
- RESTful isimlendirme: `GET /Track/Details/5`, `POST /Track/Create`
- `[Route]` attribute ile özel route tanımla gerektiğinde
- `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]` attribute'lerini kullan

## ViewModel Kullanımı

- View'a Entity yerine ViewModel gönder
- Create/Edit form'ları için ayrı ViewModel'ler
- ViewModel'lerde Data Annotations ile validasyon: `[Required]`, `[StringLength]`, `[EmailAddress]`
- Listeleme için pagination destekli ViewModel'ler

## Response Patterns

- View dönen action'lar: `return View(viewModel)`
- Redirect: `return RedirectToAction("Index")`
- AJAX endpoint'leri: `return Json(new { success = true, data = ... })`
- Hata durumu: `return NotFound()`, `return BadRequest()`, `return Unauthorized()`
- Partial view: `return PartialView("_PartialName", model)`

## Service Katmanı

- Her service için interface tanımla: `ITrackService` / `TrackService`
- `Extensions/ServiceExtensions.cs`'de Scoped olarak register et
- Service'ler arası bağımlılık: constructor injection ile
- Async method'lar `Async` suffix taşımalı: `GetTrackByIdAsync`

## Hata Yönetimi

- Controller'da `ModelState.IsValid` kontrolü yap
- Service katmanında exception fırlat, controller'da yakala
- Kullanıcıya anlamlı hata mesajları göster (localization destekli)
- `TempData` ile başarı/hata mesajları taşı
