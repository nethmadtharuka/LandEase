using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace LandEase.Infrastructure.ExternalServices;

public class GeminiAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _baseUrl;

    public GeminiAiService(IConfiguration config)
    {
        _httpClient = new HttpClient();
        _apiKey = config["GeminiApi:ApiKey"]!;
        _model = config["GeminiApi:Model"]!;
        _baseUrl = config["GeminiApi:BaseUrl"]!;
    }

    public async Task<string> GenerateResponseAsync(
        string systemPrompt,
        List<(string role, string content)> conversationHistory,
        string newMessage)
    {
        var url = $"{_baseUrl}/{_model}:generateContent?key={_apiKey}";

        // Build contents array with full conversation history
        var contents = new List<object>();

        foreach (var (role, content) in conversationHistory)
        {
            contents.Add(new
            {
                role = role == "assistant" ? "model" : "user",
                parts = new[] { new { text = content } }
            });
        }

        // Add new user message
        contents.Add(new
        {
            role = "user",
            parts = new[] { new { text = newMessage } }
        });

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = contents,
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 1000
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content2 = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content2);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Gemini API error: {error}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseBody);

        var text = jsonDoc
            .RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return text ?? "I could not generate a response. Please try again.";
    }
}