namespace LandEase.Application.DTOs.Immigration;

public class AgentVerdictDto
{
    public string AgentName { get; set; } = string.Empty;
    public string Verdict { get; set; } = string.Empty;  // APPROVE / BORDERLINE / REJECT
    public int Confidence { get; set; }                  // 0-100
    public List<string> CriticalWeaknesses { get; set; } = new();
    public string Reasoning { get; set; } = string.Empty;
    public List<string> QuestionsForDebate { get; set; } = new();
}

public class DebateSummaryDto
{
    public List<AgentVerdictDto> Round1Verdicts { get; set; } = new();
    public List<AgentVerdictDto> Round2Verdicts { get; set; } = new();
    public string ConsensusVerdict { get; set; } = string.Empty;
    public int ConsensusConfidence { get; set; }
    public int JudgeQualityScore { get; set; }
    public int DebateRoundsUsed { get; set; }  // 1 or 2 (Reflexion triggered on 2)
}
