using LandEase.Application.DTOs.Immigration;
using LandEase.Infrastructure.ExternalServices;
using System.Text.Json;

namespace LandEase.Infrastructure;

public class DebateAgentService
{
    private readonly GeminiAiService _gemini;

    public DebateAgentService(GeminiAiService gemini)
    {
        _gemini = gemini;
    }

    public async Task<DebateSummaryDto> RunDebateAsync(
        ProfileSubmitDto profile,
        int ruleBasedScore,
        List<DocumentAnalysisResultDto> docResults)
    {
        // Round 1: all 3 agents assess independently
        var r1Strict   = await GetVerdictAsync("strict",   profile, ruleBasedScore, docResults, null, null);
        var r1Lenient  = await GetVerdictAsync("lenient",  profile, ruleBasedScore, docResults, null, null);
        var r1Advocate = await GetVerdictAsync("advocate", profile, ruleBasedScore, docResults, null, null);

        var round1 = new List<AgentVerdictDto> { r1Strict, r1Lenient, r1Advocate };

        // Round 2: each agent reads the other two verdicts
        var r2Strict   = await GetVerdictAsync("strict",   profile, ruleBasedScore, docResults,
                             new[] { r1Lenient, r1Advocate }, null);
        var r2Lenient  = await GetVerdictAsync("lenient",  profile, ruleBasedScore, docResults,
                             new[] { r1Strict, r1Advocate }, null);
        var r2Advocate = await GetVerdictAsync("advocate", profile, ruleBasedScore, docResults,
                             new[] { r1Strict, r1Lenient }, null);

        var round2 = new List<AgentVerdictDto> { r2Strict, r2Lenient, r2Advocate };

        // LLM-as-Judge
        int judgeScore = await RunJudgeAsync(round1, round2);

        var finalRound = round2;
        int roundsUsed = 2;

        // Reflexion: retry if judge quality is low
        if (judgeScore < 70)
        {
            string failureNote = BuildFailureNote(judgeScore);

            var r3Strict   = await GetVerdictAsync("strict",   profile, ruleBasedScore, docResults,
                                 new[] { r2Lenient, r2Advocate }, failureNote);
            var r3Lenient  = await GetVerdictAsync("lenient",  profile, ruleBasedScore, docResults,
                                 new[] { r2Strict, r2Advocate }, failureNote);
            var r3Advocate = await GetVerdictAsync("advocate", profile, ruleBasedScore, docResults,
                                 new[] { r2Strict, r2Lenient }, failureNote);

            finalRound = new List<AgentVerdictDto> { r3Strict, r3Lenient, r3Advocate };
            judgeScore = await RunJudgeAsync(round2, finalRound);
            roundsUsed = 3;
        }

        var (consensus, confidence) = DetermineConsensus(finalRound);

        return new DebateSummaryDto
        {
            Round1Verdicts      = round1,
            Round2Verdicts      = finalRound,
            ConsensusVerdict    = consensus,
            ConsensusConfidence = confidence,
            JudgeQualityScore   = judgeScore,
            DebateRoundsUsed    = roundsUsed
        };
    }

    private async Task<AgentVerdictDto> GetVerdictAsync(
        string agentType,
        ProfileSubmitDto profile,
        int score,
        List<DocumentAnalysisResultDto> docs,
        AgentVerdictDto[]? otherVerdicts,
        string? failureNote)
    {
        var (systemPrompt, userMessage) = BuildAgentPrompt(
            agentType, profile, score, docs, otherVerdicts, failureNote);

        var response = await _gemini.GenerateResponseAsync(
            systemPrompt,
            new List<(string role, string content)>(),
            userMessage
        );

        return ParseVerdictJson(response, agentType);
    }

    private (string systemPrompt, string userMessage) BuildAgentPrompt(
        string agentType,
        ProfileSubmitDto profile,
        int score,
        List<DocumentAnalysisResultDto> docs,
        AgentVerdictDto[]? otherVerdicts,
        string? failureNote)
    {
        string systemPrompt = agentType switch
        {
            "strict" =>
                "You are Senior Visa Officer Chen Wei with 18 years of experience. " +
                "You are strict and skeptical. Assume gaps in information mean something is hidden. " +
                "Missing documentation is treated as absent. Prior visa refusals are serious red flags. " +
                "Do not give benefit of the doubt.",

            "lenient" =>
                "You are Visa Officer Sarah Mitchell, an integration specialist. " +
                "You consider long-term economic contribution and personal context. " +
                "Weigh the applicant's potential and intent, not just current credentials. " +
                "Look for reasons to approve where reasonable.",

            _ =>
                "You are Immigration Advocate David Osei, an applicant rights specialist. " +
                "Your job is to ensure the applicant gets fair consideration. " +
                "Highlight strengths and challenge overly harsh interpretations. " +
                "Push back on rejections that are not clearly justified by evidence."
        };

        string docSummary = docs.Any()
            ? string.Join("\n", docs.Select(d =>
                $"- {d.DocumentType}: Complete={d.IsComplete}, Expired={d.IsExpired}" +
                (d.Inconsistencies.Any()
                    ? $", Issues: {string.Join(", ", d.Inconsistencies)}"
                    : "")))
            : "- No documents uploaded";

        string debateContext = otherVerdicts == null ? "" :
            "\n\nOTHER OFFICERS HAVE STATED:\n" +
            string.Join("\n", otherVerdicts.Select(v =>
                $"- {v.AgentName}: {v.Verdict} ({v.Confidence}%) — {v.Reasoning}\n" +
                $"  Their questions: {string.Join("; ", v.QuestionsForDebate)}"));

        string reflexionNote = string.IsNullOrEmpty(failureNote) ? "" :
            $"\n\nLESSON FROM PREVIOUS ROUND:\n{failureNote}";

        string userMessage =
            "PROFILE TO ASSESS:\n" +
            $"- Visa type: {profile.VisaType}\n" +
            $"- Destination: {profile.DestinationCountry}\n" +
            $"- Education: {profile.EducationLevel}\n" +
            $"- Work experience: {profile.YearsOfWorkExperience} years\n" +
            $"- Job title: {profile.JobTitle}\n" +
            $"- Has job offer: {profile.HasJobOffer}\n" +
            $"- Language: {profile.LanguageTest} {profile.LanguageScore}\n" +
            $"- Annual income: {profile.AnnualIncome}\n" +
            $"- Savings: {profile.SavingsAmount}\n" +
            $"- Age: {profile.Age}\n" +
            $"- Marital status: {profile.MaritalStatus}\n" +
            $"- Family in destination: {profile.HasFamilyInDestination}\n" +
            $"- Prior visa refusal: {profile.HasPriorVisaRefusal}\n" +
            $"- Criminal record: {profile.HasCriminalRecord}\n" +
            $"- Rule-based score: {score}/100\n\n" +
            "DOCUMENT VERIFICATION:\n" +
            docSummary +
            debateContext +
            reflexionNote +
            "\n\nRespond in this EXACT JSON format only, no extra text:\n" +
            "{\n" +
            "  \"agentName\": \"your name\",\n" +
            "  \"verdict\": \"APPROVE or BORDERLINE or REJECT\",\n" +
            "  \"confidence\": 0-100,\n" +
            "  \"criticalWeaknesses\": [\"weakness 1\", \"weakness 2\"],\n" +
            "  \"reasoning\": \"2-3 sentences\",\n" +
            "  \"questionsForDebate\": [\"question 1\", \"question 2\"]\n" +
            "}";

        return (systemPrompt, userMessage);
    }

    private async Task<int> RunJudgeAsync(
        List<AgentVerdictDto> round1,
        List<AgentVerdictDto> round2)
    {
        string systemPrompt =
            "You are a quality evaluator for immigration assessment debates. " +
            "Rate the debate quality from 0-100 based on three criteria: " +
            "Did agents address each other's arguments? (30 points) " +
            "Is the reasoning evidence-based? (30 points) " +
            "Did the debate converge toward a reasoned consensus? (40 points) " +
            "Respond with ONLY a JSON object, no extra text.";

        string userMessage =
            "ROUND 1 VERDICTS:\n" +
            JsonSerializer.Serialize(round1) +
            "\n\nROUND 2 VERDICTS:\n" +
            JsonSerializer.Serialize(round2) +
            "\n\nRespond with ONLY this JSON: { \"qualityScore\": 0-100 }";

        var response = await _gemini.GenerateResponseAsync(
            systemPrompt,
            new List<(string role, string content)>(),
            userMessage
        );

        try
        {
            var clean = response.Replace("```json", "").Replace("```", "").Trim();
            var parsed = JsonSerializer.Deserialize<JsonElement>(clean);
            return parsed.GetProperty("qualityScore").GetInt32();
        }
        catch
        {
            return 75; // default if parsing fails
        }
    }

    private string BuildFailureNote(int judgeScore)
    {
        return $"Previous debate quality score was {judgeScore}/100. " +
               "Agents did not sufficiently address each other's arguments. " +
               "In this round: directly respond to specific points raised by other officers, " +
               "cite document evidence for each claim, and either concede or firmly rebut each point.";
    }

    private (string verdict, int confidence) DetermineConsensus(List<AgentVerdictDto> verdicts)
    {
        var counts   = verdicts.GroupBy(v => v.Verdict)
                               .ToDictionary(g => g.Key, g => g.Count());
        var majority = counts.OrderByDescending(x => x.Value).First();
        var avgConf  = (int)verdicts
                           .Where(v => v.Verdict == majority.Key)
                           .Average(v => v.Confidence);
        return (majority.Key, avgConf);
    }

    private AgentVerdictDto ParseVerdictJson(string json, string fallbackName)
    {
        try
        {
            var clean  = json.Replace("```json", "").Replace("```", "").Trim();
            var parsed = JsonSerializer.Deserialize<JsonElement>(clean);
            return new AgentVerdictDto
            {
                AgentName          = parsed.GetProperty("agentName").GetString()!,
                Verdict            = parsed.GetProperty("verdict").GetString()!,
                Confidence         = parsed.GetProperty("confidence").GetInt32(),
                Reasoning          = parsed.GetProperty("reasoning").GetString()!,
                CriticalWeaknesses = parsed.GetProperty("criticalWeaknesses")
                                          .EnumerateArray()
                                          .Select(x => x.GetString()!)
                                          .ToList(),
                QuestionsForDebate = parsed.GetProperty("questionsForDebate")
                                          .EnumerateArray()
                                          .Select(x => x.GetString()!)
                                          .ToList(),
            };
        }
        catch
        {
            return new AgentVerdictDto
            {
                AgentName  = fallbackName,
                Verdict    = "BORDERLINE",
                Confidence = 50,
                Reasoning  = "Could not parse agent response."
            };
        }
    }
}