using LandEase.Application.DTOs.Sos;

namespace LandEase.Application.Interfaces;

public interface ISosService
{
    Task<SosEventDto> TriggerAsync(int userId, TriggerSosDto dto);
    Task<List<SosEventDto>> GetActiveAsync(int userId);
    Task<List<SosEventDto>> GetMyHistoryAsync(int userId);
    Task<SosEventDto> AcknowledgeAsync(int sosEventId, int userId);
    Task<SosEventDto> ResolveAsync(int sosEventId, int userId);
}