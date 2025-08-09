using Microsoft.EntityFrameworkCore;
using Trackify.Api.Models;

namespace Trackify.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Auth
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        // Business
        public DbSet<TrackifyUser> TrackifyUsers => Set<TrackifyUser>();
        public DbSet<Entity> Entities => Set<Entity>();
        public DbSet<Release> Releases => Set<Release>();
        public DbSet<Preference> Preferences => Set<Preference>();
        public DbSet<UserPreferenceLink> UserPreferenceLinks => Set<UserPreferenceLink>();
        public DbSet<UserUpdate> UserUpdates => Set<UserUpdate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Auth relationships
            modelBuilder.Entity<RefreshToken>()
               .HasOne(r => r.User)
               .WithMany(u => u.RefreshTokens)
               .HasForeignKey(r => r.UserId);

            // Business relationships
            modelBuilder.Entity<TrackifyUser>()
                .HasOne(tu => tu.User)
                .WithMany() // no navigation from User to TrackifyUser
                .HasForeignKey(tu => tu.UserId);

            modelBuilder.Entity<UserUpdate>()
                .HasOne(u => u.TrackifyUser)
                .WithMany(tu => tu.Updates)
                .HasForeignKey(u => u.TrackifyUserId);

            modelBuilder.Entity<Preference>()
                .HasOne(p => p.Entity)
                .WithMany()
                .HasForeignKey(p => p.EntityId);

            modelBuilder.Entity<UserPreferenceLink>()
            .HasOne(link => link.TrackifyUser)
            .WithMany()
            .HasForeignKey(link => link.TrackifyUserId);

            modelBuilder.Entity<UserPreferenceLink>()
                .HasOne(link => link.Preference)
                .WithMany(pref => pref.LinkedUsers)
                .HasForeignKey(link => link.PreferenceId);
        }
    }
}
