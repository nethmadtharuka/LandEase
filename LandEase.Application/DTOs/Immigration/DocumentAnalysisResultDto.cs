namespace LandEase.Application.DTOs.Immigration;

public class DocumentAnalysisResultDto
{
    public string DocumentType { get; set; } = string.Empty;
    // e.g. "Passport", "EmploymentLetter", "BankStatement", "LanguageCertificate"

    public bool IsComplete { get; set; }
    public bool IsExpired { get; set; }
    public string? ExpiryDate { get; set; }

    public List<string> MissingFields { get; set; } = new();
    // e.g. ["Employer signature", "Salary figure"]

    public List<string> Inconsistencies { get; set; } = new();
    // e.g. ["Name on passport differs from employment letter"]

    public Dictionary<string, string> ExtractedData { get; set; } = new();
    // e.g. { "PassportNumber": "N1234567", "IssueCountry": "Sri Lanka" }

    public string ExtractionMethod { get; set; } = string.Empty;
    // "TextExtraction" or "Vision" — useful for debugging
}
