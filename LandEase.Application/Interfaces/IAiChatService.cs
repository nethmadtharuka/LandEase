using LandEase.Application.DTOs;
using LandEase.Application.DTOs.Ai;

namespace LandEase.Application.Interfaces;

public interface IAiChatService
{
    Task<ChatResponseDto> ChatAsync(int userId, ChatMessageDto dto);
    Task<PagedResultDto<ChatHistoryDto>> GetHistoryAsync(
        int userId, string? sessionId, int page, int pageSize);
    Task ClearHistoryAsync(int userId);
    Task<string> TranslateAsync(TranslateDto dto);
}