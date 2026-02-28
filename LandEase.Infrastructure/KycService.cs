using LandEase.Application.DTOs.Kyc;
using LandEase.Application.Interfaces;
using LandEase.Domain.Entities;
using LandEase.Domain.Enums;
using LandEase.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LandEase.Infrastructure;

public class KycService : IKycService
{
    private readonly AppDbContext _context;
    private readonly IBlobStorageService _blobStorage;
    private readonly IEmailService _emailService;

    public KycService(
        AppDbContext context,
        IBlobStorageService blobStorage,
        IEmailService emailService)
    {
        _context = context;
        _blobStorage = blobStorage;
        _emailService = emailService;
    }

    public async Task<KycStatusDto> SubmitKycAsync(int userId, KycSubmissionDto dto)
    {
        var existing = await _context.KycRecords
            .FirstOrDefaultAsync(k => k.UserId == userId);

        if (existing != null && existing.Status != KycStatus.Rejected)
            throw new Exception("KYC already submitted or currently under review.");

        var idDocUrl = await UploadKycFile(dto.IdDocument, userId, "id-document");
        var selfieUrl = await UploadKycFile(dto.Selfie, userId, "selfie");
        string? addressUrl = null;

        if (dto.AddressProof != null)
            addressUrl = await UploadKycFile(dto.AddressProof, userId, "address-proof");

        if (existing != null)
        {
            existing.IdDocumentUrl = idDocUrl;
            existing.SelfieUrl = selfieUrl;
            existing.AddressProofUrl = addressUrl;
            existing.Status = KycStatus.Pending;
            existing.RejectionReason = null;
            existing.SubmittedAt = DateTime.UtcNow;
            existing.ReviewedAt = null;
            existing.ReviewedByUserId = null;
        }
        else
        {
            _context.KycRecords.Add(new KycRecord
            {
                UserId = userId,
                IdDocumentUrl = idDocUrl,
                SelfieUrl = selfieUrl,
                AddressProofUrl = addressUrl,
                Status = KycStatus.Pending
            });
        }

        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            await _emailService.SendEmailAsync(
                user.Email,
                user.FullName,
                "LandEase — KYC Submission Received",
                $@"<div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;'>
                    <h2 style='color:#1B2A4A;'>Hi {user.FullName},</h2>
                    <p>Your KYC documents have been received and are currently under review.</p>
                    <p>We will notify you once the review is complete. This usually takes 1–2 business days.</p>
                    <br/>
                    <p style='color:#2E86AB;font-weight:bold;'>The LandEase Team</p>
                   </div>"
            );
        }

        return await GetMyKycStatusAsync(userId);
    }

    public async Task<KycStatusDto> GetMyKycStatusAsync(int userId)
    {
        var record = await _context.KycRecords
            .FirstOrDefaultAsync(k => k.UserId == userId);

        if (record == null)
            throw new Exception("No KYC submission found for this user.");

        return MapToStatusDto(record);
    }

    public async Task<List<KycDetailDto>> GetPendingKycRecordsAsync()
    {
        return await _context.KycRecords
            .Include(k => k.User)
            .Where(k => k.Status == KycStatus.Pending ||
                        k.Status == KycStatus.UnderReview)
            .OrderBy(k => k.SubmittedAt)
            .Select(k => new KycDetailDto
            {
                Id = k.Id,
                UserId = k.UserId,
                UserFullName = k.User.FullName,
                UserEmail = k.User.Email,
                IdDocumentUrl = k.IdDocumentUrl,
                SelfieUrl = k.SelfieUrl,
                AddressProofUrl = k.AddressProofUrl,
                Status = k.Status,
                RejectionReason = k.RejectionReason,
                SubmittedAt = k.SubmittedAt,
                ReviewedAt = k.ReviewedAt
            })
            .ToListAsync();
    }

    public async Task<KycStatusDto> ReviewKycAsync(
        int kycId, int reviewerUserId, KycReviewDto dto)
    {
        var record = await _context.KycRecords
            .Include(k => k.User)
            .FirstOrDefaultAsync(k => k.Id == kycId);

        if (record == null)
            throw new Exception("KYC record not found.");

        if (record.Status == KycStatus.Approved)
            throw new Exception("This KYC record is already approved.");

        record.Status = dto.IsApproved ? KycStatus.Approved : KycStatus.Rejected;
        record.RejectionReason = dto.IsApproved ? null : dto.RejectionReason;
        record.ReviewedByUserId = reviewerUserId;
        record.ReviewedAt = DateTime.UtcNow;

        if (dto.IsApproved)
        {
            var user = await _context.Users.FindAsync(record.UserId);
            if (user != null)
                user.IsKycVerified = true;
        }

        await _context.SaveChangesAsync();

        var emailSubject = dto.IsApproved
            ? "LandEase — KYC Approved!"
            : "LandEase — KYC Review Update";

        var emailBody = dto.IsApproved
            ? $@"<div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;'>
                  <h2 style='color:#1B2A4A;'>Hi {record.User.FullName},</h2>
                  <p>🎉 Congratulations! Your identity has been verified successfully.</p>
                  <p>You now have full access to all LandEase features.</p>
                  <br/>
                  <p style='color:#2E86AB;font-weight:bold;'>The LandEase Team</p>
                 </div>"
            : $@"<div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;'>
                  <h2 style='color:#1B2A4A;'>Hi {record.User.FullName},</h2>
                  <p>Unfortunately your KYC submission was not approved.</p>
                  <p><strong>Reason:</strong> {dto.RejectionReason}</p>
                  <p>You may resubmit your documents at any time from your profile.</p>
                  <br/>
                  <p style='color:#2E86AB;font-weight:bold;'>The LandEase Team</p>
                 </div>";

        await _emailService.SendEmailAsync(
            record.User.Email,
            record.User.FullName,
            emailSubject,
            emailBody);

        return MapToStatusDto(record);
    }

    private async Task<string> UploadKycFile(
        IFormFile file, int userId, string documentType)
    {
        var extension = Path.GetExtension(file.FileName);
        var fileName =
            $"{userId}/{DateTime.UtcNow:yyyyMMddHHmmss}_{documentType}{extension}";
        using var stream = file.OpenReadStream();
        return await _blobStorage.UploadFileAsync(stream, fileName, file.ContentType);
    }

    private static KycStatusDto MapToStatusDto(KycRecord record) => new()
    {
        Id = record.Id,
        Status = record.Status,
        RejectionReason = record.RejectionReason,
        SubmittedAt = record.SubmittedAt,
        ReviewedAt = record.ReviewedAt
    };
}