using LandEase.Application.DTOs;
using LandEase.Application.DTOs.Ai;
using LandEase.Application.Interfaces;
using LandEase.Domain.Entities;
using LandEase.Infrastructure.Data;
using LandEase.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;

namespace LandEase.Infrastructure;

public class AiChatService : IAiChatService
{
    private readonly AppDbContext _context;
    private readonly GeminiAiService _gemini;

    public AiChatService(AppDbContext context, GeminiAiService gemini)
    {
        _context = context;
        _gemini = gemini;
    }

    public async Task<ChatResponseDto> ChatAsync(int userId, ChatMessageDto dto)
    {
        // Get user profile for context-aware prompt
        var user = await _context.Users.FindAsync(userId)
            ?? throw new Exception("User not found.");

        // Generate or reuse session ID
        var sessionId = string.IsNullOrWhiteSpace(dto.SessionId)
            ? Guid.NewGuid().ToString()
            : dto.SessionId;

        // Build context-aware system prompt
        var systemPrompt = BuildSystemPrompt(user);

        // Load last 10 messages from this session as conversation history
        var history = await _context.ChatHistories
            .Where(ch => ch.UserId == userId && ch.SessionId == sessionId)
            .OrderByDescending(ch => ch.CreatedAt)
            .Take(10)
            .OrderBy(ch => ch.CreatedAt)
            .Select(ch => new { ch.Role, ch.Content })
            .ToListAsync();

        var conversationHistory = history
            .Select(h => (h.Role, h.Content))
            .ToList();

        // Call Gemini API
        var reply = await _gemini.GenerateResponseAsync(
            systemPrompt,
            conversationHistory,
            dto.Message);

        // Save user message to history
        _context.ChatHistories.Add(new ChatHistory
        {
            UserId = userId,
            SessionId = sessionId,
            Role = "user",
            Content = dto.Message,
            CreatedAt = DateTime.UtcNow
        });

        // Save assistant reply to history
        _context.ChatHistories.Add(new ChatHistory
        {
            UserId = userId,
            SessionId = sessionId,
            Role = "assistant",
            Content = reply,
            CreatedAt = DateTime.UtcNow.AddMilliseconds(1)
        });

        await _context.SaveChangesAsync();

        return new ChatResponseDto
        {
            SessionId = sessionId,
            Reply = reply,
            Timestamp = DateTime.UtcNow
        };
    }

    // ── Translation ───────────────────────────────────────────

    public async Task<string> TranslateAsync(TranslateDto dto)
    {
        var systemPrompt = "You are a professional translator. Return ONLY the translated text, nothing else. No explanations, no quotes, no preamble.";

        var reply = await _gemini.GenerateResponseAsync(
            systemPrompt,
            new List<(string Role, string Content)>(),
            $"Translate the following text from {dto.FromLanguage} to {dto.ToLanguage}:\n\n{dto.Text}"
        );

        return reply;
    }

    // ── History ───────────────────────────────────────────────

    public async Task<PagedResultDto<ChatHistoryDto>> GetHistoryAsync(
        int userId, string? sessionId, int page, int pageSize)
    {
        var query = _context.ChatHistories
            .Where(ch => ch.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(sessionId))
            query = query.Where(ch => ch.SessionId == sessionId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(ch => ch.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ch => new ChatHistoryDto
            {
                Id = ch.Id,
                SessionId = ch.SessionId,
                Role = ch.Role,
                Content = ch.Content,
                CreatedAt = ch.CreatedAt
            })
            .ToListAsync();

        return new PagedResultDto<ChatHistoryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task ClearHistoryAsync(int userId)
    {
        var history = await _context.ChatHistories
            .Where(ch => ch.UserId == userId)
            .ToListAsync();

        _context.ChatHistories.RemoveRange(history);
        await _context.SaveChangesAsync();
    }

    // ── System Prompt ─────────────────────────────────────────

    private static string BuildSystemPrompt(Domain.Entities.User user)
    {
        return $@"You are LandEase AI, a friendly and knowledgeable migration support assistant.

User Profile:
- Name: {user.FullName}
- Migrating from: {user.OriginCountry ?? "unknown"}
- Migrating to: {user.DestinationCountry ?? "unknown"}
- Current status: {user.MigrationStatus}
- KYC Verified: {(user.IsKycVerified ? "Yes" : "No")}

Your role is to help this user with their migration journey. You can help with:
- Visa and immigration questions for {user.DestinationCountry}
- Banking and financial setup in {user.DestinationCountry}
- Housing and accommodation advice
- Employment and job searching tips
- Education and schooling information
- Local transport and orientation
- Healthcare and insurance guidance
- Cultural tips for settling in {user.DestinationCountry}
- Emergency contacts and resources

Guidelines:
- Always be warm, supportive and encouraging
- Give practical, actionable advice
- If you don't know something specific, recommend official government sources
- Keep responses concise and easy to understand
- Focus only on migration-related topics
- If asked about unrelated topics, politely redirect to migration support";
    }
}