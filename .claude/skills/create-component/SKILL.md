---
name: create-component
description: Yeni bir ViewComponent veya Partial View oluşturur. Razor view, C# sınıfı ve gerekli kayıtları birlikte üretir.
disable-model-invocation: true
allowed-tools: Bash, Read, Write, Glob, Grep
---

# Create Component Workflow

`$ARGUMENTS` adında yeni component oluştur.

## Adımlar

1. **Mevcut kalıpları incele**
   - `ViewComponents/` dizinindeki mevcut component'leri analiz et
   - Proje genelinde kullanılan pattern'ı tespit et (ViewComponent vs Partial View)
   - Tailwind CSS class kullanımını incele

2. **ViewComponent oluştur** (etkileşimli bileşen için)
   - `ViewComponents/$ARGUMENTSViewComponent.cs` sınıfını oluştur
   - `ViewComponent` base class'tan miras al
   - `InvokeAsync` method'unu implement et
   - Gerekli service'leri constructor injection ile al
   - View: `Views/Shared/Components/$ARGUMENTS/Default.cshtml`

3. **Veya Partial View oluştur** (basit UI parçası için)
   - `Views/Shared/_$ARGUMENTS.cshtml` dosyasını oluştur
   - ViewModel gerekiyorsa `ViewModels/` altında tanımla

4. **Razor View'ı yaz**
   - Tailwind CSS utility class'ları kullan
   - Semantik HTML elementleri kullan
   - Lucide Icons entegrasyonuna uy
   - Responsive tasarım (mobile-first)
   - Erişilebilirlik (a11y): ARIA attribute'ları

5. **JavaScript** (etkileşim gerekiyorsa)
   - `wwwroot/js/` altında ilgili JS dosyasını oluştur
   - Vanilla JS kullan (framework yok)
   - AJAX istekleri için `fetch` API kullan

6. **Doğrulama**
   - `dotnet build` ile projenin derlenebildiğini doğrula
   - Component'in view'da çağrılabildiğini kontrol et
