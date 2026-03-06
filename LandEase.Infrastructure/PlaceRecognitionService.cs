using LandEase.Application.DTOs.Ai;
using LandEase.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace LandEase.Infrastructure;

public class PlaceRecognitionService : IPlaceRecognitionService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _baseUrl;

    public PlaceRecognitionService(IConfiguration config)
    {
        _httpClient = new HttpClient();
        _apiKey = config["GeminiApi:ApiKey"]!;
        _model = config["GeminiApi:Model"]!;
        _baseUrl = config["GeminiApi:BaseUrl"]!;
    }

    public async Task<PlaceRecognitionResponseDto> RecognizePlaceAsync(
        PlaceRecognitionRequestDto dto)
    {
        var url = $"{_baseUrl}/{_model}:generateContent?key={_apiKey}";

        var prompt = @"You are a travel guide AI assistant helping migrants 
explore their new destination country.

Analyze this image and identify the place, landmark, building, or location shown.

Respond ONLY in this exact JSON format with no markdown or code blocks:
{
  ""placeName"": ""Name of the place or landmark"",
  ""country"": ""Country where this place is located"",
  ""city"": ""City or region"",
  ""description"": ""2-3 sentence overview of what this place is"",
  ""history"": ""Brief interesting history of this place in 2-3 sentences"",
  ""famousFor"": ""What this place is most famous for"",
  ""travelTips"": ""Practical tips for visiting this place""
}

If you cannot identify a specific landmark, describe what you see 
(e.g. a street, park, building type) and provide general information 
about that type of place. Always respond with the JSON format.";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new
                        {
                            inline_data = new
                            {
                                mime_type = dto.MimeType,
                                data = dto.ImageBase64
                            }
                        },
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.4,
                maxOutputTokens = 1000
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Gemini Vision API error: {error}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseBody);

        var text = jsonDoc
            .RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "{}";

        // Clean and parse JSON response
        var clean = text
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        try
        {
            var parsed = JsonSerializer.Deserialize<JsonElement>(clean);

            return new PlaceRecognitionResponseDto
            {
                PlaceName = GetString(parsed, "placeName"),
                Country = GetString(parsed, "country"),
                City = GetString(parsed, "city"),
                Description = GetString(parsed, "description"),
                History = GetString(parsed, "history"),
                FamousFor = GetString(parsed, "famousFor"),
                TravelTips = GetString(parsed, "travelTips"),
                RecognizedAt = DateTime.UtcNow
            };
        }
        catch
        {
            // If JSON parsing fails return raw text as description
            return new PlaceRecognitionResponseDto
            {
                PlaceName = "Place Detected",
                Description = clean,
                RecognizedAt = DateTime.UtcNow
            };
        }
    }

    private static string GetString(JsonElement element, string key)
    {
        try
        {
            return element.GetProperty(key).GetString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}