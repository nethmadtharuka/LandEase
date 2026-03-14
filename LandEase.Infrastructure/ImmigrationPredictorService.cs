using LandEase.Application.DTOs.Immigration;
using LandEase.Application.Interfaces;
using LandEase.Infrastructure.ExternalServices;
using System.Text.Json;

namespace LandEase.Infrastructure;

public class ImmigrationPredictorService : IImmigrationPredictorService
{
    private readonly GeminiAiService _gemini;

    // FIX 2: StringComparer.OrdinalIgnoreCase so "australia-skilledworker" matches too
    private static readonly Dictionary<string, string> _approvalRates =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "Australia-SkilledWorker",  "Published approval range: 65-80% (DIBP annual report)" },
            { "Canada-SkilledWorker",     "Published approval range: 70-85% (IRCC annual report)" },
            { "UK-SkilledWorker",         "Published approval range: 75-88% (Home Office data)" },
            { "Australia-StudentVisa",    "Published approval range: 72-83% (DIBP annual report)" },
            { "Default",                  "Approval rates vary; consult official government sources" },
        };

    public ImmigrationPredictorService(GeminiAiService gemini) => _gemini = gemini;

    public async Task<ScoreResultDto> AnalyzeProfileAsync(ProfileSubmitDto dto)
    {
        // Rule-based scoring
        var (eduScore,  eduNote)  = ScoringEngine.ScoreEducation(dto.EducationLevel);
        var (workScore, workNote) = ScoringEngine.ScoreWorkExperience(dto.YearsOfWorkExperience, dto.HasJobOffer);
        var (langScore, langNote) = ScoringEngine.ScoreLanguage(dto.LanguageTest, dto.LanguageScore);
        var (finScore,  finNote)  = ScoringEngine.ScoreFinancials(dto.SavingsAmount);
        var (perScore,  perNote)  = ScoringEngine.ScorePersonal(
                                        dto.Age,
                                        dto.MaritalStatus,
                                        dto.HasFamilyInDestination,
                                        dto.HasPriorVisaRefusal,
                                        dto.HasCriminalRecord);

        var total = eduScore + workScore + langScore + finScore + perScore;

        var strength = total switch
        {
            >= 80 => "Excellent",
            >= 65 => "Strong",
            >= 45 => "Moderate",
            >= 25 => "Weak",
            _     => "Very Weak"
        };

        var categories = new List<CategoryScoreDto>
        {
            new() { Category = "Education",        Score = eduScore,  MaxScore = 25, Note = eduNote  },
            new() { Category = "Work Experience",  Score = workScore, MaxScore = 25, Note = workNote },
            new() { Category = "Language",         Score = langScore, MaxScore = 20, Note = langNote },
            new() { Category = "Financial",        Score = finScore,  MaxScore = 15, Note = finNote  },
            new() { Category = "Personal Profile", Score = perScore,  MaxScore = 15, Note = perNote  },
        };

        foreach (var c in categories)
            c.Status = (c.Score * 100 / c.MaxScore) switch
            {
                >= 80 => "Strong",
                >= 50 => "Moderate",
                _     => "Weak"
            };

        // Gemini qualitative analysis
        var aiAnalysis = await GetGeminiAnalysisAsync(dto, total, strength, categories);

        // FIX 2: Normalise key — strip spaces, use TryGetValue for safe fallback
        var rateKey    = $"{dto.DestinationCountry}-{dto.VisaType}".Replace(" ", "");
        var approvalRate = _approvalRates.TryGetValue(rateKey, out var rate)
            ? rate
            : _approvalRates["Default"];   // "Default" always exists so this is safe

        return new ScoreResultDto
        {
            OverallScore          = total,
            StrengthLevel         = strength,
            Categories            = categories,
            AiAnalysis            = aiAnalysis.analysis,
            Strengths             = aiAnalysis.strengths,
            Weaknesses            = aiAnalysis.weaknesses,
            Suggestions           = aiAnalysis.suggestions,
            PublishedApprovalRate = approvalRate,
        };
    }

    private async Task<(string analysis, List<string> strengths,
                        List<string> weaknesses, List<string> suggestions)>
        GetGeminiAnalysisAsync(ProfileSubmitDto dto, int score,
                               string strength, List<CategoryScoreDto> categories)
    {
        // FIX 1: Use $@"..." verbatim string correctly.
        // In verbatim strings \" is a literal backslash + quote — Gemini sees it and
        // gets confused. Use "" for a literal double-quote inside a verbatim string.
        // Use {{ }} for literal braces in interpolated strings.
        var prompt = $@"You are an immigration profile advisor using only
public, general knowledge about {dto.DestinationCountry} {dto.VisaType} requirements.

Profile summary:
- Education: {dto.EducationLevel}
- Work Experience: {dto.YearsOfWorkExperience} years, Job Offer: {dto.HasJobOffer}
- Language: {dto.LanguageTest} score {dto.LanguageScore}
- Savings: ${dto.SavingsAmount:N0}
- Age: {dto.Age}, Prior Refusal: {dto.HasPriorVisaRefusal}
- Rule-based score: {score}/100 ({strength})

Return ONLY valid JSON with no markdown:
{{
  ""analysis"": ""2-3 sentence qualitative overview"",
  ""strengths"": [""strength1"", ""strength2""],
  ""weaknesses"": [""weakness1""],
  ""suggestions"": [""actionable suggestion 1"", ""actionable suggestion 2""]
}}

IMPORTANT: State clearly this is general guidance only, not legal advice.";

        try
        {
            var raw = await _gemini.GenerateResponseAsync(
                "You are an immigration profile analysis assistant. Return only valid JSON.",
                new List<(string, string)>(),
                prompt);

            var clean  = raw.Replace("```json", "").Replace("```", "").Trim();
            var parsed = JsonSerializer.Deserialize<JsonElement>(clean);

            return (
                parsed.GetProperty("analysis").GetString() ?? "",
                parsed.GetProperty("strengths").EnumerateArray()
                      .Select(x => x.GetString() ?? "").ToList(),
                parsed.GetProperty("weaknesses").EnumerateArray()
                      .Select(x => x.GetString() ?? "").ToList(),
                parsed.GetProperty("suggestions").EnumerateArray()
                      .Select(x => x.GetString() ?? "").ToList()
            );
        }
        catch
        {
            return ("Analysis unavailable. Please try again.", new(), new(), new());
        }
    }

    public List<string> GetSupportedVisaTypes() =>
        new() { "SkilledWorker", "StudentVisa", "FamilyReunification",
                "InvestorVisa", "HumanitarianVisa" };

    public List<string> GetSupportedCountries() =>
        new() { "Australia", "Canada", "UK", "NewZealand", "Germany" };
}