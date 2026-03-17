using LandEase.Domain.Enums;

namespace LandEase.Application.DTOs.Kyc;

public class KycStatusDto
{
    public int Id { get; set; }
    public KycStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}