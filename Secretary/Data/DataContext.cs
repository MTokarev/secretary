using Microsoft.EntityFrameworkCore;
using Secretary.Models;

namespace Secretary.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Secret> Secrets { get; set; }
        public DbSet<RemovalKey> RemovalKeys { get; set; }
        
        // Service stores decryption keys only for logged in users.
        // This is required when user tries to access keys that were shared by him/her.
        public DbSet<DecryptionKey> DecryptionKeys { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        {
            this.Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuring one-to-one relationship
            modelBuilder.Entity<Secret>()
                .HasOne(r => r.RemovalKey)
                .WithOne(r => r.Secret)
                .HasForeignKey<RemovalKey>(r => r.SecretId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Secret>()
                .HasOne(r => r.DecryptionKey)
                .WithOne(r => r.Secret)
                .HasForeignKey<DecryptionKey>(r => r.SecretId)
                .OnDelete(DeleteBehavior.Cascade);

            // Loading removal entity as a navigation property
            modelBuilder.Entity<Secret>()
                .Navigation(s => s.RemovalKey)
                .AutoInclude();
            
            // Loading removal entity as a navigation property
            modelBuilder.Entity<Secret>()
                .Navigation(s => s.DecryptionKey)
                .AutoInclude();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite();
    }
}

