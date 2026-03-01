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
    public DbSet<Community> Communities => Set<Community>();
    public DbSet<CommunityMember> CommunityMembers => Set<CommunityMember>();
    public DbSet<CommunityPost> CommunityPosts => Set<CommunityPost>();

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

        modelBuilder.Entity<Community>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
            entity.Property(c => c.OriginCountry).HasMaxLength(100).IsRequired();
            entity.Property(c => c.DestinationCountry).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<CommunityMember>(entity =>
        {
            entity.HasKey(cm => cm.Id);
            entity.HasIndex(cm => new { cm.CommunityId, cm.UserId }).IsUnique();
            entity.HasOne(cm => cm.Community)
                  .WithMany(c => c.Members)
                  .HasForeignKey(cm => cm.CommunityId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cm => cm.User)
                  .WithMany()
                  .HasForeignKey(cm => cm.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CommunityPost>(entity =>
        {
            entity.HasKey(cp => cp.Id);
            entity.Property(cp => cp.Content).IsRequired();
            entity.HasOne(cp => cp.Community)
                  .WithMany(c => c.Posts)
                  .HasForeignKey(cp => cp.CommunityId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cp => cp.Author)
                  .WithMany()
                  .HasForeignKey(cp => cp.AuthorId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // ── Seed Data — 20 Popular Migration Corridors ────────
        modelBuilder.Entity<Community>().HasData(
            new Community { Id = 1, Name = "Sri Lanka → Australia", OriginCountry = "Sri Lanka", DestinationCountry = "Australia", Description = "Community for Sri Lankans migrating to Australia.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 2, Name = "Sri Lanka → Canada", OriginCountry = "Sri Lanka", DestinationCountry = "Canada", Description = "Community for Sri Lankans migrating to Canada.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 3, Name = "Sri Lanka → United Kingdom", OriginCountry = "Sri Lanka", DestinationCountry = "United Kingdom", Description = "Community for Sri Lankans migrating to the UK.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 4, Name = "India → Australia", OriginCountry = "India", DestinationCountry = "Australia", Description = "Community for Indians migrating to Australia.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 5, Name = "India → Canada", OriginCountry = "India", DestinationCountry = "Canada", Description = "Community for Indians migrating to Canada.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 6, Name = "India → United Kingdom", OriginCountry = "India", DestinationCountry = "United Kingdom", Description = "Community for Indians migrating to the UK.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 7, Name = "India → United States", OriginCountry = "India", DestinationCountry = "United States", Description = "Community for Indians migrating to the US.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 8, Name = "Philippines → Australia", OriginCountry = "Philippines", DestinationCountry = "Australia", Description = "Community for Filipinos migrating to Australia.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 9, Name = "Philippines → Canada", OriginCountry = "Philippines", DestinationCountry = "Canada", Description = "Community for Filipinos migrating to Canada.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 10, Name = "Nepal → Australia", OriginCountry = "Nepal", DestinationCountry = "Australia", Description = "Community for Nepalese migrating to Australia.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 11, Name = "Nepal → United Kingdom", OriginCountry = "Nepal", DestinationCountry = "United Kingdom", Description = "Community for Nepalese migrating to the UK.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 12, Name = "Nigeria → United Kingdom", OriginCountry = "Nigeria", DestinationCountry = "United Kingdom", Description = "Community for Nigerians migrating to the UK.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 13, Name = "Nigeria → Canada", OriginCountry = "Nigeria", DestinationCountry = "Canada", Description = "Community for Nigerians migrating to Canada.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 14, Name = "Pakistan → United Kingdom", OriginCountry = "Pakistan", DestinationCountry = "United Kingdom", Description = "Community for Pakistanis migrating to the UK.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 15, Name = "Pakistan → Canada", OriginCountry = "Pakistan", DestinationCountry = "Canada", Description = "Community for Pakistanis migrating to Canada.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 16, Name = "Bangladesh → United Kingdom", OriginCountry = "Bangladesh", DestinationCountry = "United Kingdom", Description = "Community for Bangladeshis migrating to the UK.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 17, Name = "China → Australia", OriginCountry = "China", DestinationCountry = "Australia", Description = "Community for Chinese migrants to Australia.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 18, Name = "Vietnam → Australia", OriginCountry = "Vietnam", DestinationCountry = "Australia", Description = "Community for Vietnamese migrants to Australia.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 19, Name = "Brazil → Portugal", OriginCountry = "Brazil", DestinationCountry = "Portugal", Description = "Community for Brazilians migrating to Portugal.", CreatedAt = new DateTime(2024, 1, 1) },
            new Community { Id = 20, Name = "Mexico → United States", OriginCountry = "Mexico", DestinationCountry = "United States", Description = "Community for Mexicans migrating to the US.", CreatedAt = new DateTime(2024, 1, 1) }
        );
    }
}