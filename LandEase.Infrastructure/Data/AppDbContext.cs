using LandEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LandEase.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<KycRecord> KycRecords => Set<KycRecord>();
    public DbSet<ServiceListing> ServiceListings => Set<ServiceListing>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();

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

        modelBuilder.Entity<ServiceListing>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Title).HasMaxLength(200).IsRequired();
            entity.Property(s => s.Category).HasConversion<string>();
            entity.Property(s => s.Price).HasPrecision(10, 2);

            entity.HasOne(s => s.Provider)
                  .WithMany()
                  .HasForeignKey(s => s.ProviderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Status).HasConversion<string>();

            entity.HasOne(b => b.Service)
                  .WithMany(s => s.Bookings)
                  .HasForeignKey(b => b.ServiceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.Migrant)
                  .WithMany()
                  .HasForeignKey(b => b.MigrantId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.BookingId).IsUnique();

            entity.HasOne(r => r.Booking)
                  .WithOne(b => b.Review)
                  .HasForeignKey<Review>(r => r.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Reviewer)
                  .WithMany()
                  .HasForeignKey(r => r.ReviewerId)
                  .OnDelete(DeleteBehavior.NoAction);
        });
    }
}