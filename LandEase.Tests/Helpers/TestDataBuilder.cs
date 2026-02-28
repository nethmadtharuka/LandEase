using LandEase.Domain.Entities;
using LandEase.Domain.Enums;

namespace LandEase.Tests.Helpers;

public static class TestDataBuilder
{
    public static User CreateMigrant(int id = 1) => new()
    {
        Id = id,
        FullName = "Test Migrant",
        Email = $"migrant{id}@test.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@1234"),
        Role = UserRole.Migrant,
        OriginCountry = "Sri Lanka",
        DestinationCountry = "Australia",
        MigrationStatus = MigrationStatus.NewlyArrived,
        IsEmailVerified = true,
        IsKycVerified = false,
        CreatedAt = DateTime.UtcNow
    };

    public static User CreateHelper(int id = 2) => new()
    {
        Id = id,
        FullName = "Test Helper",
        Email = $"helper{id}@test.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@1234"),
        Role = UserRole.Helper,
        OriginCountry = "Sri Lanka",
        DestinationCountry = "Australia",
        IsEmailVerified = true,
        IsKycVerified = true,
        CreatedAt = DateTime.UtcNow
    };

    public static User CreateAgency(int id = 3) => new()
    {
        Id = id,
        FullName = "Test Agency",
        Email = $"agency{id}@test.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@1234"),
        Role = UserRole.Agency,
        IsEmailVerified = true,
        IsKycVerified = true,
        CreatedAt = DateTime.UtcNow
    };

    public static KycRecord CreateKycRecord(int userId, KycStatus status = KycStatus.Pending) => new()
    {
        UserId = userId,
        IdDocumentUrl = "https://storage.blob.core.windows.net/kyc/test-id.jpg",
        SelfieUrl = "https://storage.blob.core.windows.net/kyc/test-selfie.jpg",
        Status = status,
        SubmittedAt = DateTime.UtcNow
    };
}