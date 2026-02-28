using LandEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LandEase.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<KycRecord> KycRecords => Set<KycRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(150).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(u => u.Role).HasConversion<string>();
            entity.Property(u => u.MigrationStatus).HasConversion<string>();
            entity.Property(u => u.AverageRating).HasPrecision(3, 2);
        });

        modelBuilder.Entity<KycRecord>(entity =>
        {
            entity.HasKey(k => k.Id);
            entity.Property(k => k.Status).HasConversion<string>();

            entity.HasOne(k => k.User)
                  .WithMany()
                  .HasForeignKey(k => k.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(k => k.ReviewedByUser)
                  .WithMany()
                  .HasForeignKey(k => k.ReviewedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}