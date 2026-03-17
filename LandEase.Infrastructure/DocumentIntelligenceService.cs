using UglyToad.PdfPig;
using LandEase.Application.DTOs.Immigration;
using LandEase.Infrastructure.ExternalServices;
using System.Text;
using System.Text.Json;

namespace LandEase.Infrastructure;

public class DocumentIntelligenceService
{
    private readonly GeminiAiService _gemini;

    public DocumentIntelligenceService(GeminiAiService gemini)
    {
        _gemini = gemini;
    }

    public async Task<DocumentAnalysisResultDto> AnalyzeDocumentAsync(
        byte[] fileBytes,
        string documentType,
        ProfileSubmitDto profile)
    {
        // Step 1: try extract text from PDF
        string extractedText = TryExtractText(fileBytes);

        string userMessage;

        if (extractedText.Length > 50)
        {
            // Digital PDF — send as plain text
            userMessage = $"Document text content:\n{extractedText}";
        }
        else
        {
            // Scanned PDF — send as base64 image
            string base64 = Convert.ToBase64String(fileBytes);
            userMessage = $"The document is a scanned PDF provided as base64:\n{base64}";
        }

 string systemPrompt =
    "You are a visa document analyst.\n\n" +
    "The applicant claims:\n" +
    $"- Visa type: {profile.VisaType}\n" +
    $"- Destination country: {profile.DestinationCountry}\n" +
    $"- Education level: {profile.EducationLevel}\n" +
    $"- Work experience: {profile.YearsOfWorkExperience} years\n" +
    $"- Job title: {profile.JobTitle}\n" +
    $"- Language test: {profile.LanguageTest}, score: {profile.LanguageScore}\n" +
    $"- Annual income: {profile.AnnualIncome}\n" +
    $"- Savings: {profile.SavingsAmount}\n" +
    $"- Age: {profile.Age}\n" +
    $"- Prior visa refusal: {profile.HasPriorVisaRefusal}\n\n" +
    $"Analyze the provided {documentType} document.\n\n" +
    "Return ONLY valid JSON with this exact structure, no extra text:\n" +
    "{\n" +
    "  \"isComplete\": true or false,\n" +
    "  \"isExpired\": true or false,\n" +
    "  \"expiryDate\": \"YYYY-MM or null\",\n" +
    "  \"missingFields\": [\"field1\", \"field2\"],\n" +
    "  \"inconsistencies\": [\"description of any mismatch with applicant claims\"],\n" +
    "  \"extractedData\": { \"key\": \"value\" }\n" +
    "}";


        // Your GeminiAiService signature:
        // GenerateResponseAsync(systemPrompt, conversationHistory, newMessage)
        // Pass empty history since this is a single-turn extraction
        var response = await _gemini.GenerateResponseAsync(
            systemPrompt,
            new List<(string role, string content)>(),
            userMessage
        );

        return ParseResult(response, documentType);
    }

    private string TryExtractText(byte[] fileBytes)
    {
        try
        {
            using var doc = PdfDocument.Open(fileBytes);
            var sb = new StringBuilder();
            foreach (var page in doc.GetPages())
                sb.Append(page.Text);
            return sb.ToString().Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    private DocumentAnalysisResultDto ParseResult(string json, string docType)
    {
        try
        {
            var clean = json
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var parsed = JsonSerializer.Deserialize<JsonElement>(clean);

            return new DocumentAnalysisResultDto
            {
                DocumentType  = docType,
                IsComplete    = parsed.GetProperty("isComplete").GetBoolean(),
                IsExpired     = parsed.GetProperty("isExpired").GetBoolean(),
                ExpiryDate    = parsed.TryGetProperty("expiryDate", out var exp)
                                ? exp.GetString() : null,
                MissingFields = parsed.GetProperty("missingFields")
                                .EnumerateArray()
                                .Select(x => x.GetString()!)
                                .ToList(),
                Inconsistencies = parsed.GetProperty("inconsistencies")
                                  .EnumerateArray()
                                  .Select(x => x.GetString()!)
                                  .ToList(),
                ExtractedData = parsed.GetProperty("extractedData")
                                .EnumerateObject()
                                .ToDictionary(p => p.Name, p => p.Value.GetString()!),
                ExtractionMethod = json.Length > 50 ? "TextExtraction" : "Vision"
            };
        }
        catch
        {
            return new DocumentAnalysisResultDto
            {
                DocumentType  = docType,
                IsComplete    = false,
                MissingFields = new List<string> { "Could not parse document" }
            };
        }
    }
}