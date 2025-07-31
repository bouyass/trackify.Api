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
        public DbSet<Category> Categories => Set<Category>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relation User <-> UserUpdate
            modelBuilder.Entity<UserUpdate>()
                .HasOne(u => u.User)
                .WithMany()
                .HasForeignKey(u => u.UserId);

            // Relation UpdateLog <-> UserUpdate
            modelBuilder.Entity<UserUpdate>()
                .HasOne(u => u.UpdateLog)
                .WithMany()
                .HasForeignKey(u => u.UpdateLogId);

            // Relation Content <-> UpdateLog
            modelBuilder.Entity<UpdateLog>()
                .HasOne(u => u.Content)
                .WithMany()
                .HasForeignKey(u => u.ContentId);

            // Relation User <-> UserPreference
            modelBuilder.Entity<UserPreference>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId);

            // Relation Content <-> UserPreference
            modelBuilder.Entity<UserPreference>()
                .HasOne(p => p.Content)
                .WithMany()
                .HasForeignKey(p => p.ContentId);

            // Relation Category <-> Content
            modelBuilder.Entity<Content>()
                .HasOne(c => c.Category)
                .WithMany(ca => ca.Contents)
                .HasForeignKey(c => c.CategoryId);
        }

    }
}
