---
paths:
  - "**/*.cs"
  - "**/*.cshtml"
  - "wwwroot/js/**/*.js"
  - "wwwroot/css/**/*.css"
---

# Code Style Rules

## C# / ASP.NET Core

- PascalCase: class, method, property, public field isimleri
- camelCase: local değişkenler, method parametreleri
- `_camelCase`: private field'lar (underscore prefix)
- `I` prefix: interface isimleri (`ITrackService`, `IUserService`)
- `async/await` kullan, `.Result` veya `.Wait()` kullanma (deadlock riski)
- `var` kullan: tip sağ taraftan açıkça anlaşılıyorsa
- Null kontrolü: null-conditional (`?.`) ve null-coalescing (`??`) operatörleri tercih et
- Early return pattern uygula - derin nesting'den kaçın
- String interpolation (`$"Hello {name}"`) kullan, `String.Format` yerine
- LINQ kullan ama okunabilirliği koru - çok karmaşık zincirlerde parçala
- Magic number kullanma, `const` veya `enum` tanımla
- Boş catch blokları yazma - exception'ı logla veya fırlat

## Razor Views (.cshtml)

- Tag Helper'ları HTML helper'lara tercih et (`asp-for`, `asp-action`, `asp-controller`)
- Partial view'ları `_` prefix ile isimlendir (`_Header.cshtml`)
- ViewComponent'leri tekrar kullanılabilir UI parçaları için kullan
- View'da business logic yazma - controller veya service'e taşı
- Model doğrudan Entity olmamalı, ViewModel kullan

## JavaScript (wwwroot/js/)

- Vanilla JS kullanılıyor (framework yok)
- AJAX istekleri için `fetch` API kullan
- Event delegation tercih et
- DOM manipülasyonlarını minimize et
- `const` tercih et, `let` sadece mutasyon gerekiyorsa, `var` asla

## Tailwind CSS

- Utility-first yaklaşımı koru
- Kaynak dosya: `wwwroot/css/input.css`, derlenen: `output.css`
- `@apply` direktifini tekrarlanan pattern'lar için kullan
- Responsive tasarım: mobile-first (`sm:`, `md:`, `lg:`)

## Dosya Organizasyonu

- Controllers/ → MVC controller'lar (BaseController'dan miras)
- Services/ → Interface/Implementation çiftleri
- Infrastructure/ → Harici servisler (dosya, email, cache)
- Models/ → Domain entity'leri, Enums/ alt dizininde enum'lar
- ViewModels/ → Controller-View veri transferi
- ViewComponents/ → Yeniden kullanılabilir Razor bileşenleri
- Utilities/ → Yardımcı sınıflar
