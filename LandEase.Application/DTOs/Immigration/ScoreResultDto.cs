namespace LandEase.Application.DTOs.Immigration;

public class ScoreResultDto
{
    public int OverallScore { get; set; }          // 0-100
    public string StrengthLevel { get; set; } = string.Empty; // Weak/Moderate/Strong/Excellent
    public List<CategoryScoreDto> Categories { get; set; } = new();
    public string AiAnalysis { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public string PublishedApprovalRate { get; set; } = string.Empty;
    public List<DocumentAnalysisResultDto> DocumentResults { get; set; } = new();
public DebateSummaryDto? DebateSummary { get; set; }
public bool DocumentsVerified { get; set; }
public List<string> DocumentFlags { get; set; } = new();
// e.g. ["Bank statement covers only 4 months — 12 required"]


    
    public string Disclaimer { get; set; } =
        "This tool provides estimated analysis based on publicly available " +
        "immigration criteria. Results are NOT guaranteed and do NOT constitute " +
        "legal advice. Consult a licensed immigration lawyer before submitting.";
}

public class CategoryScoreDto
{
    public string Category { get; set; } = string.Empty;
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public string Status { get; set; } = string.Empty; // Strong/Moderate/Weak
    public string Note { get; set; } = string.Empty;
}
