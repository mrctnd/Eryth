using Eryth.Services;
using Eryth.Infrastructure;

namespace Eryth.Extensions
{
    // Dependency Injection konfigürasyonu
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // ASP.NET Core services
            services.AddHttpContextAccessor();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITrackService, TrackService>();
            services.AddScoped<IPlaylistService, PlaylistService>();
            services.AddScoped<IAlbumService, AlbumService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IHistoryService, HistoryService>();
            services.AddScoped<IFollowService, FollowService>();
            services.AddScoped<ILikeService, LikeService>();            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ISearchService, SearchService>();

            // Infrastructure
            services.AddScoped<IFileUploadService, LocalFileUploadService>();
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddSingleton<ICacheService, MemoryCacheService>();

            // Memory Cache
            services.AddMemoryCache();

            return services;
        }

        public static IServiceCollection AddEmailConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // SMTP ayarları configuration'dan alınacak
            var smtpSection = configuration.GetSection("SmtpSettings");
            if (!smtpSection.Exists())
            {
                throw new InvalidOperationException("SMTP ayarları configuration'da bulunamadı");
            }

            return services;
        }

        public static IServiceCollection AddFileUploadConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Dosya yükleme ayarları
            var uploadSection = configuration.GetSection("FileUpload");
            if (!uploadSection.Exists())
            {
                // Varsayılan ayarları kullan
                configuration["FileUpload:MaxImageSize"] = "5242880"; // 5MB
                configuration["FileUpload:MaxAudioSize"] = "52428800"; // 50MB
                configuration["FileUpload:AllowedImageTypes"] = ".jpg,.jpeg,.png,.gif,.webp";
                configuration["FileUpload:AllowedAudioTypes"] = ".mp3,.wav,.flac,.aac,.ogg";
            }

            return services;
        }
    }
}
