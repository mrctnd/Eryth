---
paths:
  - "**/*.Tests/**/*"
  - "**/*Tests.cs"
  - "**/*Test.cs"
---

# Testing Rules

> Not: Projede henüz test projesi bulunmuyor. Test eklendiğinde bu kurallara uyulmalı.

## Unit Tests (xUnit)

- xUnit tercih et (ASP.NET Core ekosisteminde standart)
- Her test açıklayıcı bir isim taşımalı: `Should_ReturnError_When_EmailIsInvalid`
- Arrange-Act-Assert (AAA) pattern uygula
- Her test bağımsız olmalı, test sırasına bağımlı olmamalı
- Test başına tek bir davranışı doğrula

## Test Yapısı

- Test projesi: `Eryth.Tests` (ayrı proje olarak oluştur)
- Klasör yapısı kaynak projeyi yansıtmalı:
  - `Services/` → `Tests/Services/TrackServiceTests.cs`
  - `Controllers/` → `Tests/Controllers/TrackControllerTests.cs`

## Mocking

- Moq veya NSubstitute kullan
- Interface'ler üzerinden mock et (DI sayesinde kolay)
- Repository/Service katmanını mock et, controller testlerinde
- `ApplicationDbContext` için InMemory provider veya SQLite in-memory kullan

## Integration Tests

- `WebApplicationFactory<Program>` kullan
- Test veritabanı: EF Core InMemory veya SQLite
- Her test kendi verisini seed etmeli
- HTTP client ile endpoint'leri test et

## Kapsam

- Service katmanı: business logic testleri
- Controller: doğru view/redirect dönüyor mu?
- Validasyon: model validation kuralları
- Edge case'ler: null input, yetkisiz erişim, kayıt bulunamadı
