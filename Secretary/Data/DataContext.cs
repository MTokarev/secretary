using System;
using Microsoft.EntityFrameworkCore;
using Secretary.Models;

namespace Secretary.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Secret> Secrets { get; set; }
        public DbSet<RemovalKey> RemovalKeys { get; set; }

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

            // Loading removal entity as a navigation property
            modelBuilder.Entity<Secret>()
                .Navigation(s => s.RemovalKey)
                .AutoInclude();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite();
    }
}

