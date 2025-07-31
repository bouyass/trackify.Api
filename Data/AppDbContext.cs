using Microsoft.EntityFrameworkCore;
using Trackify.Api.Models;

namespace Trackify.Api.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Content> Contents => Set<Content>();
        public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
        public DbSet<UpdateLog> Updates => Set<UpdateLog>();
        public DbSet<UserUpdate> UserUpdates => Set<UserUpdate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relation User <-> UserUpdate
            modelBuilder.Entity<UserUpdate>()
                .HasOne(u => u.User)
                .WithMany()
                .HasForeignKey(u => u.UserId);

            // Relation Update <-> UserUpdate
            modelBuilder.Entity<UserUpdate>()
                .HasOne(u => u.Update)
                .WithMany()
                .HasForeignKey(u => u.UpdateId);
        }
    }
}
