using LandEase.Application.DTOs;
using LandEase.Application.DTOs.Community;
using LandEase.Application.Interfaces;
using LandEase.Domain.Entities;
using LandEase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LandEase.Infrastructure;

public class CommunityService : ICommunityService
{
    private readonly AppDbContext _context;

    public CommunityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<CommunityDto>> GetAllAsync(
        int? userId, int page, int pageSize)
    {
        var query = _context.Communities
            .Include(c => c.Members)
            .Include(c => c.Posts)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<CommunityDto>
        {
            Items = items.Select(c => MapToDto(c, userId)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CommunityDto> GetByIdAsync(int communityId, int? userId)
    {
        var community = await _context.Communities
            .Include(c => c.Members)
            .Include(c => c.Posts)
            .FirstOrDefaultAsync(c => c.Id == communityId)
            ?? throw new Exception("Community not found.");

        return MapToDto(community, userId);
    }

    public async Task JoinAsync(int communityId, int userId)
    {
        var community = await _context.Communities.FindAsync(communityId)
            ?? throw new Exception("Community not found.");

        var alreadyMember = await _context.CommunityMembers
            .AnyAsync(cm => cm.CommunityId == communityId && cm.UserId == userId);

        if (alreadyMember)
            throw new Exception("You are already a member of this community.");

        _context.CommunityMembers.Add(new CommunityMember
        {
            CommunityId = communityId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task LeaveAsync(int communityId, int userId)
    {
        var membership = await _context.CommunityMembers
            .FirstOrDefaultAsync(
                cm => cm.CommunityId == communityId && cm.UserId == userId)
            ?? throw new Exception("You are not a member of this community.");

        _context.CommunityMembers.Remove(membership);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResultDto<CommunityMemberDto>> GetMembersAsync(
        int communityId, int page, int pageSize)
    {
        var community = await _context.Communities.FindAsync(communityId)
            ?? throw new Exception("Community not found.");

        var query = _context.CommunityMembers
            .Include(cm => cm.User)
            .Where(cm => cm.CommunityId == communityId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(cm => cm.JoinedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cm => new CommunityMemberDto
            {
                UserId = cm.UserId,
                FullName = cm.User.FullName,
                OriginCountry = cm.User.OriginCountry ?? string.Empty,
                IsKycVerified = cm.User.IsKycVerified,
                AverageRating = cm.User.AverageRating,
                JoinedAt = cm.JoinedAt
            })
            .ToListAsync();

        return new PagedResultDto<CommunityMemberDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResultDto<CommunityPostDto>> GetPostsAsync(
        int communityId, int page, int pageSize)
    {
        var community = await _context.Communities.FindAsync(communityId)
            ?? throw new Exception("Community not found.");

        var query = _context.CommunityPosts
            .Include(cp => cp.Author)
            .Where(cp => cp.CommunityId == communityId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(cp => cp.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cp => new CommunityPostDto
            {
                Id = cp.Id,
                CommunityId = cp.CommunityId,
                AuthorId = cp.AuthorId,
                AuthorName = cp.Author.FullName,
                AuthorIsKycVerified = cp.Author.IsKycVerified,
                Content = cp.Content,
                CreatedAt = cp.CreatedAt,
                UpdatedAt = cp.UpdatedAt
            })
            .ToListAsync();

        return new PagedResultDto<CommunityPostDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CommunityPostDto> CreatePostAsync(
        int communityId, int userId, CreatePostDto dto)
    {
        var community = await _context.Communities.FindAsync(communityId)
            ?? throw new Exception("Community not found.");

        var isMember = await _context.CommunityMembers
            .AnyAsync(cm => cm.CommunityId == communityId && cm.UserId == userId);

        if (!isMember)
            throw new Exception(
                "You must be a member of this community to post.");

        var user = await _context.Users.FindAsync(userId)!;

        var post = new CommunityPost
        {
            CommunityId = communityId,
            AuthorId = userId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.CommunityPosts.Add(post);
        await _context.SaveChangesAsync();

        return new CommunityPostDto
        {
            Id = post.Id,
            CommunityId = post.CommunityId,
            AuthorId = post.AuthorId,
            AuthorName = user!.FullName,
            AuthorIsKycVerified = user.IsKycVerified,
            Content = post.Content,
            CreatedAt = post.CreatedAt
        };
    }

    private static CommunityDto MapToDto(Community community, int? userId) => new()
    {
        Id = community.Id,
        Name = community.Name,
        Description = community.Description,
        OriginCountry = community.OriginCountry,
        DestinationCountry = community.DestinationCountry,
        MemberCount = community.Members.Count,
        PostCount = community.Posts.Count,
        IsMember = userId.HasValue &&
                   community.Members.Any(m => m.UserId == userId.Value),
        CreatedAt = community.CreatedAt
    };
}