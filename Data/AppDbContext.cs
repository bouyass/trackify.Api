using Microsoft.EntityFrameworkCore;
using Trackify.Api.Models;

namespace Trackify.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Entity> Entities => Set<Entity>();
        public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
        public DbSet<UserUpdate> UserUpdates => Set<UserUpdate>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RefreshToken>()
               .HasOne(r => r.User)
               .WithMany(u => u.RefreshTokens)
               .HasForeignKey(r => r.UserId);

            // Relation User <-> UserUpdate
            modelBuilder.Entity<UserUpdate>()
                .HasOne(u => u.User)
                .WithMany()
                .HasForeignKey(u => u.UserId);

            // Relation User <-> UserPreference
            modelBuilder.Entity<UserPreference>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId);

            // Relation Content <-> UserPreference
            modelBuilder.Entity<UserPreference>()
                .HasOne(p => p.Entity)
                .WithMany()
                .HasForeignKey(p => p.EntityId);
        }

    }
}
