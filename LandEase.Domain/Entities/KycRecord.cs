using LandEase.Domain.Enums;

namespace LandEase.Domain.Entities;

public class KycRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string? IdDocumentUrl { get; set; }
    public string? SelfieUrl { get; set; }
    public string? AddressProofUrl { get; set; }

    public KycStatus Status { get; set; } = KycStatus.Pending;
    public string? RejectionReason { get; set; }

    public int? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}