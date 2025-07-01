using Microsoft.EntityFrameworkCore;
using Eryth.Models;

namespace Eryth.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<ArtistFollower> ArtistFollowers { get; set; }
        public DbSet<PlaylistFollower> PlaylistFollowers { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserPlayHistory> UserPlayHistories { get; set; }
        public DbSet<PlaylistTrack> PlaylistTracks { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Message> Messages { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }        private void UpdateTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => x.Entity is User || x.Entity is Track || x.Entity is Album ||
                           x.Entity is Playlist || x.Entity is Comment || x.Entity is Notification || 
                           x.Entity is Report || x.Entity is Message)
                .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);

            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    var now = DateTime.UtcNow;

                    if (entity.Entity is User user)
                    {
                        if (user.CreatedAt == default)
                            user.CreatedAt = now;
                        user.UpdatedAt = now;
                    }

                    if (entity.Entity is Track track)
                    {
                        if (track.CreatedAt == default)
                            track.CreatedAt = now;
                        track.UpdatedAt = now;
                    }

                    if (entity.Entity is Album album)
                    {
                        if (album.CreatedAt == default)
                            album.CreatedAt = now;
                        album.UpdatedAt = now;
                    }

                    if (entity.Entity is Playlist playlist)
                    {
                        if (playlist.CreatedAt == default)
                            playlist.CreatedAt = now;
                        playlist.UpdatedAt = now;
                    }

                    if (entity.Entity is Comment comment)
                    {
                        if (comment.CreatedAt == default)
                            comment.CreatedAt = now;
                        comment.UpdatedAt = now;
                    }
                    if (entity.Entity is Notification notification)
                    {
                        if (notification.CreatedAt == default)
                            notification.CreatedAt = now;
                    }

                    if (entity.Entity is Report report)
                    {
                        if (report.CreatedAt == default)
                            report.CreatedAt = now;
                    }
                }
                else if (entity.State == EntityState.Modified)
                {
                    var now = DateTime.UtcNow;

                    if (entity.Entity is User user)
                    {
                        user.UpdatedAt = now;
                    }

                    if (entity.Entity is Track track)
                    {
                        track.UpdatedAt = now;
                    }

                    if (entity.Entity is Album album)
                    {
                        album.UpdatedAt = now;
                    }

                    if (entity.Entity is Playlist playlist)
                    {
                        playlist.UpdatedAt = now;
                    }

                    if (entity.Entity is Comment comment)
                    {
                        comment.UpdatedAt = now;
                    }
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });
            // Configure Track entity
            modelBuilder.Entity<Track>(entity =>
            {
                entity.HasOne(t => t.Artist)
                    .WithMany(u => u.Tracks)
                    .HasForeignKey(t => t.ArtistId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Album)
                    .WithMany(a => a.Tracks)
                    .HasForeignKey(t => t.AlbumId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            // Configure Album entity
            modelBuilder.Entity<Album>(entity =>
            {
                entity.HasOne(a => a.Artist)
                    .WithMany(u => u.Albums)
                    .HasForeignKey(a => a.ArtistId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            // Configure Playlist entity
            modelBuilder.Entity<Playlist>(entity =>
            {
                entity.HasOne(p => p.CreatedByUser)
                    .WithMany(u => u.Playlists)
                    .HasForeignKey(p => p.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Follow entity
            modelBuilder.Entity<Follow>(entity =>
            {
                entity.HasOne(f => f.Follower)
                    .WithMany(u => u.Following)
                    .HasForeignKey(f => f.FollowerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Following)
                    .WithMany(u => u.Followers)
                    .HasForeignKey(f => f.FollowingId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.FollowerId, e.FollowingId }).IsUnique();
            });

            // Configure ArtistFollower entity
            modelBuilder.Entity<ArtistFollower>(entity =>
            {
                entity.HasOne(af => af.User)
                    .WithMany()
                    .HasForeignKey(af => af.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(af => af.Artist)
                    .WithMany()
                    .HasForeignKey(af => af.ArtistId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.UserId, e.ArtistId }).IsUnique();
                entity.HasIndex(e => e.FollowedAt);
            });

            // Configure PlaylistFollower entity
            modelBuilder.Entity<PlaylistFollower>(entity =>
            {
                entity.HasOne(pf => pf.User)
                    .WithMany()
                    .HasForeignKey(pf => pf.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(pf => pf.Playlist)
                    .WithMany()
                    .HasForeignKey(pf => pf.PlaylistId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.UserId, e.PlaylistId }).IsUnique();
                entity.HasIndex(e => e.FollowedAt);
            });
            // Configure Comment entity
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Track)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(c => c.TrackId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Playlist)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(c => c.PlaylistId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(c => c.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            // Configure Like entity
            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasOne(l => l.User)
                    .WithMany(u => u.Likes)
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Track)
                    .WithMany(t => t.Likes)
                    .HasForeignKey(l => l.TrackId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Playlist)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(l => l.PlaylistId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Comment)
                    .WithMany(c => c.Likes)
                    .HasForeignKey(l => l.CommentId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Ensure a user can only like each entity once
                entity.HasIndex(e => new { e.UserId, e.TrackId }).IsUnique()
                    .HasFilter("[TrackId] IS NOT NULL");
                entity.HasIndex(e => new { e.UserId, e.PlaylistId }).IsUnique()
                    .HasFilter("[PlaylistId] IS NOT NULL");
                entity.HasIndex(e => new { e.UserId, e.CommentId }).IsUnique()
                    .HasFilter("[CommentId] IS NOT NULL");
            });
            // Configure Notification entity
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Restrict); entity.HasOne(n => n.TriggeredByUser)
                    .WithMany()
                    .HasForeignKey(n => n.TriggeredByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.RelatedTrack)
                    .WithMany()
                    .HasForeignKey(n => n.RelatedTrackId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.RelatedPlaylist)
                    .WithMany()
                    .HasForeignKey(n => n.RelatedPlaylistId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.RelatedComment)
                    .WithMany()
                    .HasForeignKey(n => n.RelatedCommentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.UserId, e.IsRead });
                entity.HasIndex(e => e.CreatedAt);
            });
            // Configure UserPlayHistory entity
            modelBuilder.Entity<UserPlayHistory>(entity =>
            {
                entity.HasOne(h => h.User)
                    .WithMany(u => u.PlayHistory)
                    .HasForeignKey(h => h.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(h => h.Track)
                    .WithMany(t => t.PlayHistory)
                    .HasForeignKey(h => h.TrackId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(h => h.Playlist)
                    .WithMany()
                    .HasForeignKey(h => h.PlaylistId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.UserId, e.PlayedAt });
                entity.HasIndex(e => new { e.TrackId, e.PlayedAt });
            });
            // Configure PlaylistTrack entity
            modelBuilder.Entity<PlaylistTrack>(entity =>
            {
                entity.HasOne(pt => pt.Playlist)
                    .WithMany(p => p.PlaylistTracks)
                    .HasForeignKey(pt => pt.PlaylistId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pt => pt.Track)
                    .WithMany(t => t.PlaylistTracks)
                    .HasForeignKey(pt => pt.TrackId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pt => pt.AddedByUser)
                    .WithMany()
                    .HasForeignKey(pt => pt.AddedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.PlaylistId, e.TrackId }).IsUnique();
                entity.HasIndex(e => new { e.PlaylistId, e.OrderIndex });
            });

            // Configure Message entity
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasOne(m => m.Sender)
                    .WithMany()
                    .HasForeignKey(m => m.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Receiver)
                    .WithMany()
                    .HasForeignKey(m => m.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.ParentMessage)
                    .WithMany(m => m.Replies)
                    .HasForeignKey(m => m.ParentMessageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.SenderId);
                entity.HasIndex(e => e.ReceiverId);
                entity.HasIndex(e => e.SentAt);
                entity.HasIndex(e => e.IsRead);
            });
        }
    }
}
