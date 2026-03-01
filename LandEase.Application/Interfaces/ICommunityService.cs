using LandEase.Application.DTOs;
using LandEase.Application.DTOs.Community;

namespace LandEase.Application.Interfaces;

public interface ICommunityService
{
    Task<PagedResultDto<CommunityDto>> GetAllAsync(int? userId, int page, int pageSize);
    Task<CommunityDto> GetByIdAsync(int communityId, int? userId);
    Task JoinAsync(int communityId, int userId);
    Task LeaveAsync(int communityId, int userId);
    Task<PagedResultDto<CommunityMemberDto>> GetMembersAsync(
        int communityId, int page, int pageSize);
    Task<PagedResultDto<CommunityPostDto>> GetPostsAsync(
        int communityId, int page, int pageSize);
    Task<CommunityPostDto> CreatePostAsync(
        int communityId, int userId, CreatePostDto dto);
}