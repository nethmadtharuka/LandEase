using FluentAssertions;
using LandEase.Application.DTOs.Kyc;
using LandEase.Application.Interfaces;
using LandEase.Domain.Enums;
using LandEase.Infrastructure;
using LandEase.Tests.Helpers;
using Moq;

namespace LandEase.Tests.Unit;

public class KycServiceTests
{
    private readonly Mock<IBlobStorageService> _blobStorageMock;
    private readonly Mock<IEmailService> _emailServiceMock;

    public KycServiceTests()
    {
        _blobStorageMock = new Mock<IBlobStorageService>();
        _emailServiceMock = new Mock<IEmailService>();

        // Default blob upload returns a fake URL
        _blobStorageMock
            .Setup(b => b.UploadFileAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync("https://storage.blob.core.windows.net/kyc/test-file.jpg");

        // Default email send does nothing
        _emailServiceMock
            .Setup(e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    // ── GetMyKycStatusAsync Tests ─────────────────────────────

    [Fact]
    public async Task GetMyKycStatusAsync_UserHasNoRecord_ThrowsException()
    {
        // Arrange
        var context = TestDbContextFactory.Create("kyc_no_record");
        var service = new KycService(context, _blobStorageMock.Object,
            _emailServiceMock.Object);

        // Act
        var act = async () => await service.GetMyKycStatusAsync(999);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*No KYC submission found*");
    }

    [Fact]
    public async Task GetMyKycStatusAsync_UserHasRecord_ReturnsCorrectStatus()
    {
        // Arrange
        var context = TestDbContextFactory.Create("kyc_has_record");
        var migrant = TestDataBuilder.CreateMigrant();
        var kycRecord = TestDataBuilder.CreateKycRecord(migrant.Id, KycStatus.Pending);
        context.Users.Add(migrant);
        context.KycRecords.Add(kycRecord);
        await context.SaveChangesAsync();

        var service = new KycService(context, _blobStorageMock.Object,
            _emailServiceMock.Object);

        // Act
        var result = await service.GetMyKycStatusAsync(migrant.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(KycStatus.Pending);
    }

    // ── GetPendingKycRecordsAsync Tests ───────────────────────

    [Fact]
    public async Task GetPendingKycRecordsAsync_NoPendingRecords_ReturnsEmptyList()
    {
        // Arrange
        var context = TestDbContextFactory.Create("kyc_no_pending");
        var service = new KycService(context, _blobStorageMock.Object,
            _emailServiceMock.Object);

        // Act
        var result = await service.GetPendingKycRecordsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPendingKycRecordsAsync_HasPendingRecords_ReturnsCorrectCount()
    {
        // Arrange
        var context = TestDbContextFactory.Create("kyc_has_pending");
        var migrant1 = TestDataBuilder.CreateMigrant(1);
        var migrant2 = TestDataBuilder.CreateMigrant(2);
        var kyc1 = TestDataBuilder.CreateKycRecord(1, KycStatus.Pending);
        var kyc2 = TestDataBuilder.CreateKycRecord(2, KycStatus.UnderReview);

        context.Users.AddRange(migrant1, migrant2);
        context.KycRecords.AddRange(kyc1, kyc2);
        await context.SaveChangesAsync();

        var service = new KycService(context, _blobStorageMock.Object,
            _emailServiceMock.Object);

        // Act
        var result = await service.GetPendingKycRecordsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPendingKycRecordsAsync_ApprovedRecordsExcluded()
    {
        // Arrange
        var context = TestDbContextFactory.Create("kyc_approved_excluded");
        var migrant1 = TestDataBuilder.CreateMigrant(1);
        var migrant2 = TestDataBuilder.CreateMigrant(2);
        var pendingKyc = TestDataBuilder.CreateKycRecord(1, KycStatus.Pending);
        var approvedKyc = TestDataBuilder.CreateKycRecord(2, KycStatus.Approved);

        context.Users.AddRange(migrant1, migrant2);
        context.KycRecords.AddRange(pendingKyc, approvedKyc);
        await context.SaveChangesAsync();

        var service = new KycService(context, _blobStorageMock.Object,
            _emailServiceMock.Object);

        // Act
        var result = await service.GetPendingKycRecordsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().UserId.Should().Be(1);
    }

    // ── ReviewKycAsync Tests ──────────────────────────────────

    [Fact]
    public async Task ReviewKycAsync_ApproveKyc_UpdatesUserIsKycVerified()
    {
        // Arrange
        var context = TestDbContextFactory.Create("kyc_approve_sets_verified");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var agency = TestDataBuilder.CreateAgency(3);
        var kycRecord = TestDataBuilder.CreateKycRecord(1, KycStatus.Pending);

        context.Users.AddRange(migrant, agency);
        context.KycRecords.Add(kycRecord);
        await context.SaveChangesAsync();

        var service = new KycService(context, _blobStorageMock.Object,
            _emailServiceMock.Object);

        var reviewDto = new KycReviewDto { IsApproved = true };

        // Act
        var result = await service.ReviewKycAsync(kycRecord.Id, agency.Id, reviewDto);

        // Assert
        result.Status.Should().Be(KycStatus.Approved);
        result.ReviewedAt.Should().NotBeNull();

        var updatedUser = await context.Users.FindAsync(migrant.Id);
        updatedUser!.IsKycVerified.Should().BeTrue();
    }

    [Fact]
    public async Task ReviewKycAsync_RejectKyc_SetsRejectionReason()
    {
        // Arrange
        var context = TestDbContextFactory.Create("kyc_reject_sets_reason");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var agency = TestDataBuilder.CreateAgency(3);
        var kycRecord = TestDataBuilder.CreateKycRecord(1, KycStatus.Pending);

        context.Users.AddRange(migrant, agency);
        context.KycRecords.Add(kycRecord);
        await context.SaveChangesAsync();

        var service = new KycService(context, _blobStorageMock.Object,
            _emailServiceMock.Object);

        var reviewDto = new KycReviewDto
        {
            IsApproved = false,
            RejectionReason = "ID document is blurry and unreadable."
        };

        // Act
        var result = await service.ReviewKycAsync(kycRecord.Id, agency.Id, reviewDto);

        // Assert
        result.Status.Should().Be(KycStatus.Rejected);
        result.RejectionReason.Should().Be("ID document is blurry and unreadable.");

        var updatedUser = await context.Users.FindAsync(migrant.Id);
        updatedUser!.IsKycVerified.Should().BeFalse();
    }

    [Fact]
    public async Task ReviewKycAsync_AlreadyApproved_ThrowsException()
    {
        // Arrange
        var context = TestDbContextFactory.Create("kyc_already_approved");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var agency = TestDataBuilder.CreateAgency(3);
        var kycRecord = TestDataBuilder.CreateKycRecord(1, KycStatus.Approved);

        context.Users.AddRange(migrant, agency);
        context.KycRecords.Add(kycRecord);
        await context.SaveChangesAsync();

        var service = new KycService(context, _blobStorageMock.Object,
            _emailServiceMock.Object);

        var reviewDto = new KycReviewDto { IsApproved = true };

        // Act
        var act = async () =>
            await service.ReviewKycAsync(kycRecord.Id, agency.Id, reviewDto);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*already approved*");
    }

    [Fact]
    public async Task ReviewKycAsync_RecordNotFound_ThrowsException()
    {
        // Arrange
        var context = TestDbContextFactory.Create("kyc_record_not_found");
        var service = new KycService(context, _blobStorageMock.Object,
            _emailServiceMock.Object);

        var reviewDto = new KycReviewDto { IsApproved = true };

        // Act
        var act = async () => await service.ReviewKycAsync(999, 1, reviewDto);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*KYC record not found*");
    }

    [Fact]
    public async Task ReviewKycAsync_Approve_SendsApprovalEmail()
    {
        // Arrange
        var context = TestDbContextFactory.Create("kyc_sends_approval_email");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var agency = TestDataBuilder.CreateAgency(3);
        var kycRecord = TestDataBuilder.CreateKycRecord(1, KycStatus.Pending);

        context.Users.AddRange(migrant, agency);
        context.KycRecords.Add(kycRecord);
        await context.SaveChangesAsync();

        var service = new KycService(context, _blobStorageMock.Object,
            _emailServiceMock.Object);

        // Act
        await service.ReviewKycAsync(
            kycRecord.Id, agency.Id, new KycReviewDto { IsApproved = true });

        // Assert — verify email was sent exactly once with correct address
        _emailServiceMock.Verify(e => e.SendEmailAsync(
            migrant.Email,
            migrant.FullName,
            It.Is<string>(s => s.Contains("Approved")),
            It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task ReviewKycAsync_Reject_SendsRejectionEmail()
    {
        // Arrange
        var context = TestDbContextFactory.Create("kyc_sends_rejection_email");
        var migrant = TestDataBuilder.CreateMigrant(1);
        var agency = TestDataBuilder.CreateAgency(3);
        var kycRecord = TestDataBuilder.CreateKycRecord(1, KycStatus.Pending);

        context.Users.AddRange(migrant, agency);
        context.KycRecords.Add(kycRecord);
        await context.SaveChangesAsync();

        var service = new KycService(context, _blobStorageMock.Object,
            _emailServiceMock.Object);

        // Act
        await service.ReviewKycAsync(kycRecord.Id, agency.Id, new KycReviewDto
        {
            IsApproved = false,
            RejectionReason = "Documents unclear."
        });

        // Assert
        _emailServiceMock.Verify(e => e.SendEmailAsync(
            migrant.Email,
            migrant.FullName,
            It.Is<string>(s => s.Contains("Update")),
            It.IsAny<string>()),
            Times.Once);
    }
}