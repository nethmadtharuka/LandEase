using Microsoft.AspNetCore.Http;

namespace LandEase.Application.DTOs.Kyc;

public class KycSubmissionDto
{
    public IFormFile IdDocument { get; set; } = null!;
    public IFormFile Selfie { get; set; } = null!;
    public IFormFile? AddressProof { get; set; }
}