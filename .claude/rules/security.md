---
paths:
  - "Controllers/**/*"
  - "Services/**/*"
  - "Infrastructure/**/*"
  - "Data/**/*"
  - "Utilities/**/*"
---

# Security Rules

## Input Validation

- Tüm kullanıcı inputlarını sunucu tarafında doğrula (Data Annotations + FluentValidation)
- Dosya yükleme: tip, boyut ve içerik kontrolü yap (`LocalFileUploadService`)
- URL parametrelerini doğrula ve sanitize et
- Model binding ile gelen veriyi ViewModel üzerinden al, doğrudan Entity kullanma

## Authentication & Authorization

- Cookie-based authentication kullanılıyor (`ClaimsPrincipal`)
- `[Authorize]` attribute'ü ile endpoint'leri koru
- `BaseController`'daki `CurrentUserId`, `CurrentUsername` property'lerini kullan
- Role-based authorization: `[Authorize(Roles = "Admin")]`
- Anti-forgery token: tüm POST/PUT/DELETE form'larında `@Html.AntiForgeryToken()` kullan
- Rate limiting: `BaseController`'daki rate limit helper'larını kullan

## Data Protection

- Şifreleri hash'le (`Utilities/` içindeki password hashing)
- Hassas veriyi loglama: password, token
- Connection string'leri `appsettings.json`'da tut, koda gömme
- EF Core parametreli sorgular kullanıyor - raw SQL yazma
- LINQ ile sorgu yaz, string concatenation ile SQL oluşturma

## XSS Koruması

- Razor otomatik HTML encoding yapar - `Html.Raw()` kullanmaktan kaçın
- JavaScript'te kullanıcı verisini DOM'a eklerken `textContent` kullan, `innerHTML` değil
- AJAX response'larını render ederken escape et

## Soft Delete

- Kullanıcı, Track, Album, Playlist'lerde soft delete uygulanıyor (`DeletedAt`)
- Sorgularda soft delete filtresi uygulandığından emin ol
- Hard delete sadece admin yetkisi ile ve onay sonrası yapılmalı

## Dosya Güvenliği

- Upload edilen dosyalar `wwwroot/uploads/` altında
- Dosya uzantısı ve MIME type kontrolü yap
- Dosya boyutu limiti uygula
- Yüklenen dosya adlarını sanitize et (path traversal önleme)
