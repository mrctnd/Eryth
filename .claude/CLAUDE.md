# Eryth Music Platform - Claude Code Instructions

> Bu dosya her oturumda otomatik yüklenir. Kısa, somut ve doğrulanabilir talimatlar içerir.

## Project Info

- **Type**: Music Streaming/Sharing Platform (Spotify/SoundCloud benzeri)
- **Stack**: ASP.NET Core 8.0 MVC, Entity Framework Core 9.0, SQL Server (LocalDB), Tailwind CSS 3.4
- **Language**: C# (backend), Vanilla JavaScript (frontend), Razor Views (.cshtml)
- **Package Manager**: NuGet (.NET), npm (Tailwind CSS build)

## Build & Run Commands

```bash
# Geliştirme sunucusu
dotnet run                          # https://localhost:7126
dotnet run --launch-profile http    # http://localhost:5099

# Build
dotnet restore
dotnet build

# Veritabanı
dotnet ef database update                   # Migration uygula
dotnet ef migrations add <MigrationName>    # Yeni migration

# Frontend CSS
npm install                    # İlk kurulum
npm run build-css              # Watch mode (development)
npm run build-css-prod         # Minified build (production)
```

Henüz test projesi ve linter yapılandırması bulunmuyor.

## Code Style

- C# naming: PascalCase (class, method, property), _camelCase (private field), I prefix (interface)
- async/await kullan, .Result veya .Wait() kullanma
- LINQ tercih et, raw SQL yazma
- Razor: Tag Helper kullan, Html.Raw() kullanma
- JavaScript: Vanilla JS, fetch API, const/let (var kullanma)
- Tailwind CSS utility-first yaklaşım, mobile-first responsive

## Architecture

- **Katmanlı monolith**: Controllers → Services → Data (EF Core)
- **Controllers/**: BaseController'dan miras, 17 MVC controller
- **Services/**: Interface/Implementation çiftleri, Scoped DI
- **Infrastructure/**: Dosya upload, email, cache servisleri
- **Models/**: Domain entity'ler, Enums/ alt dizini
- **ViewModels/**: Controller-View veri transferi
- **ViewComponents/**: Yeniden kullanılabilir Razor bileşenleri (FollowButton, LikeButton, UserProfileImage)
- **DI kayıt**: Extensions/ServiceExtensions.cs

## Key Patterns

- Cookie-based auth (ClaimsPrincipal), BaseController'da user claims
- Soft delete (DeletedAt), otomatik timestamp (CreatedAt/UpdatedAt)
- Localization: EN-US ve TR-TR (.resx dosyaları)
- Dosya upload: wwwroot/uploads/
- Audio player: wwwroot/js/audioPlayer.js (~1200 satır)
- Dev seed: TestDataSeeder.cs (user: mehmet, email: test@test.com)

## Database

- SQL Server LocalDB, connection string: appsettings.json
- 18 DbSet, 8 migration, ApplicationDbContext.cs
- Soft delete: Users, Tracks, Albums, Playlists
- Many-to-many: PlaylistTrack, PlaylistFollower, Follows

## Git Workflow

- Conventional Commits: `feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`
- Branch: `feature/kisa-aciklama`, `fix/bug-aciklama`

## Security Rules

- Entity yerine ViewModel kullan
- [Authorize] attribute ile endpoint koru
- Anti-forgery token kullan
- EF Core LINQ ile sorgula (raw SQL yazma)
- Dosya upload: uzantı, boyut, MIME type kontrolü
- Html.Raw() kullanma, innerHTML kullanma

## Additional Context

- @README.md
