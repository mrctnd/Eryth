# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Eryth is a music streaming/sharing platform (similar to Spotify/SoundCloud) built with ASP.NET Core 8.0 MVC, Entity Framework Core 9.0, SQL Server (LocalDB), and Tailwind CSS 3.4.

## Common Commands

```bash
# Build and run
dotnet restore
dotnet build
dotnet run                          # Dev server at https://localhost:7126
dotnet run --launch-profile http    # HTTP only at http://localhost:5099

# Database
dotnet ef database update                   # Apply migrations
dotnet ef migrations add <MigrationName>    # Create new migration

# Frontend CSS
npm install                    # First-time setup
npm run build-css              # Watch mode (development)
npm run build-css-prod         # Minified build (production)
```

No test projects exist yet. No linter or code analysis tools are configured.

## Architecture

**Layered monolith** — single ASP.NET Core project with these layers:

- **Controllers/** → 17 MVC controllers inheriting `BaseController` (provides current user claims, validation helpers, rate limiting)
- **Services/** → Business logic with interface/implementation pairs (`ITrackService`/`TrackService`). All registered as `Scoped` in `Extensions/ServiceExtensions.cs`
- **Infrastructure/** → External concerns: `LocalFileUploadService` (file I/O), `SmtpEmailService`, `MemoryCacheService` (singleton)
- **Data/ApplicationDbContext.cs** → EF Core context with 18 DbSets. Implements soft delete (`DeletedAt`), automatic timestamp updates (`CreatedAt`/`UpdatedAt`), and cascade/restrict delete rules
- **Models/** → Domain entities. Enums in `Models/Enums/` (Genre, UserRole, TrackStatus, etc.)
- **ViewModels/** → DTOs for controller-view data transfer
- **ViewComponents/** → Reusable Razor components (FollowButton, LikeButton, UserProfileImage)
- **Utilities/** → Helpers for audio (NAudio), files, security (password hashing), validation, dates, JSON

## Key Patterns

- **Authentication:** Cookie-based with `ClaimsPrincipal`. User claims (ID, Username, DisplayName, ProfileImageUrl, Role) extracted in `BaseController`. Login/Logout at `/Auth/Login`, `/Auth/Logout`
- **Localization:** EN-US and TR-TR via `.resx` files in `Resources/`
- **DI registration:** All services registered in `Extensions/ServiceExtensions.cs` — add new services there
- **Routing:** Default MVC route `{controller=Home}/{action=Index}/{id?}` plus Razor Pages
- **Static files:** Uploaded files go to `wwwroot/uploads/`. Tailwind source in `wwwroot/css/input.css`, compiled to `output.css`
- **Audio player:** Complex JS player in `wwwroot/js/audioPlayer.js` (~1200 lines) with companion CSS
- **Dev seeding:** `TestDataSeeder.cs` creates test user (username: `mehmet`, email: `test@test.com`) and seeds data on startup

## Frontend

Razor views with Tailwind CSS. JavaScript organized by feature under `wwwroot/js/` (e.g., `album/`, `playlist/`, `message/`). Uses Lucide Icons and HTML5 Audio API. No frontend framework — vanilla JS with AJAX for interactive features (follow/like toggles, search suggestions).

## Database

SQL Server LocalDB. Connection string in `appsettings.json`. Key relationships:
- Many-to-many: Users↔Follows, Tracks↔Playlists (`PlaylistTrack`), Users↔Playlists (`PlaylistFollower`)
- Soft delete on Users, Tracks, Albums, Playlists
- 8 migrations in `Migrations/`
