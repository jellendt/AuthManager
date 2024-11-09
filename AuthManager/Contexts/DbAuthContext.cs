using AuthManager.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthManager.Contexts
{
    public class DbAuthContext(DbContextOptions<DbAuthContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne()
                .HasForeignKey(rt => rt.UserId);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.EMail)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasMany(u => u.Credentials)
                .WithOne()
                .HasForeignKey(c => c.UserId);

        }
    }
}
