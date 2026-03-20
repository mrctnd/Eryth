---
name: create-api
description: Yeni bir MVC Controller action veya AJAX endpoint oluşturur. Validasyon, yetkilendirme ve service katmanı dahil.
disable-model-invocation: true
allowed-tools: Bash, Read, Write, Edit, Glob, Grep
---

# Create Controller Action / API Endpoint Workflow

`$ARGUMENTS` için yeni endpoint oluştur.

## Adımlar

1. **Mevcut yapıyı incele**
   - İlgili Controller'ı bul veya yeni Controller oluştur (BaseController'dan miras)
   - Mevcut Service interface/implementation çiftlerini analiz et
   - ViewModel yapısını incele
   - Routing pattern'ını belirle

2. **Service katmanını oluştur/güncelle**
   - `Services/I{Name}Service.cs` interface'ine method ekle
   - `Services/{Name}Service.cs` implementation'ını yaz
   - Yeni service ise `Extensions/ServiceExtensions.cs`'e Scoped olarak kaydet
   - EF Core ile veritabanı işlemlerini yaz (LINQ, Include, soft delete filtresi)

3. **Controller action'ı oluştur**
   - `[HttpGet]`, `[HttpPost]` vb. attribute ile işaretle
   - `[Authorize]` gerekiyorsa ekle
   - ViewModel ile veri al/gönder
   - `ModelState.IsValid` kontrolü yap
   - Service'i constructor injection ile al

4. **ViewModel oluştur** (gerekiyorsa)
   - `ViewModels/` altında Create/Edit/Details ViewModel
   - Data Annotations: `[Required]`, `[StringLength]`, `[EmailAddress]`

5. **View oluştur** (MVC action ise)
   - `Views/{Controller}/{Action}.cshtml`
   - Tag Helper'lar kullan
   - Tailwind CSS ile stil ver

6. **Doğrulama**
   - `dotnet build` ile derleme kontrolü
   - Endpoint'in çalıştığını test et
