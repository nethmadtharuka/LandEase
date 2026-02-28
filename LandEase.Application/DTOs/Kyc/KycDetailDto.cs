using LandEase.Domain.Enums;

namespace LandEase.Application.DTOs.Kyc;

public class KycDetailDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? IdDocumentUrl { get; set; }
    public string? SelfieUrl { get; set; }
    public string? AddressProofUrl { get; set; }
    public KycStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}