using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Linkdout.Api.Data;
using Linkdout.Api.DTOs;
using Linkdout.Api.Models;

namespace Linkdout.Api.Controllers;

[ApiController, Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db) => _db = db;

    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserId();
        var user = await _db.Users
            .Include(u => u.Experiences)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound();

        var connectionCount = await _db.Connections.CountAsync(c =>
            (c.RequesterId == userId || c.RecipientId == userId) && c.Status == "accepted");

        return Ok(new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Headline = user.Headline,
            Bio = user.Bio,
            Location = user.Location,
            Website = user.Website,
            AvatarUrl = user.AvatarUrl,
            CoverUrl = user.CoverUrl,
            Status = user.Status,
            Skills = DeserializeList(user.Skills),
            ProfileViews = user.ProfileViews,
            ConnectionCount = connectionCount,
            Experiences = user.Experiences.Select(e => new ExperienceDto
            {
                Id = e.Id,
                Title = e.Title,
                Company = e.Company,
                Type = e.Type,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsCurrent = e.IsCurrent,
                Description = e.Description
            }).ToList(),
            CreatedAt = user.CreatedAt
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _db.Users
            .Include(u => u.Experiences)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound(new { message = "المستخدم غير موجود" });

        user.ProfileViews++;
        await _db.SaveChangesAsync();

        // Track profile view
        try
        {
            var viewerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(viewerId) && int.Parse(viewerId) != id)
            {
                _db.Database.ExecuteSqlRaw(
                    "INSERT INTO ProfileViews (ViewerId, ProfileId, CreatedAt) VALUES ({0}, {1}, NOW())",
                    int.Parse(viewerId), id);
            }
        }
        catch { /* non-critical */ }

        var connectionCount = await _db.Connections.CountAsync(c =>
            (c.RequesterId == id || c.RecipientId == id) && c.Status == "accepted");

        // Mutual connections
        int mutualCount = 0;
        try
        {
            var viewerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(viewerId) && int.Parse(viewerId) != id)
            {
                var vid = int.Parse(viewerId);
                var viewerConns = await _db.Connections
                    .Where(c => (c.RequesterId == vid || c.RecipientId == vid) && c.Status == "accepted")
                    .Select(c => c.RequesterId == vid ? c.RecipientId : c.RequesterId)
                    .ToListAsync();
                var profileConns = await _db.Connections
                    .Where(c => (c.RequesterId == id || c.RecipientId == id) && c.Status == "accepted")
                    .Select(c => c.RequesterId == id ? c.RecipientId : c.RequesterId)
                    .ToListAsync();
                mutualCount = viewerConns.Intersect(profileConns).Count();
            }
        }
        catch { }

        return Ok(new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Headline = user.Headline,
            Bio = user.Bio,
            Location = user.Location,
            Website = user.Website,
            AvatarUrl = user.AvatarUrl,
            CoverUrl = user.CoverUrl,
            Status = user.Status,
            Skills = DeserializeList(user.Skills),
            ProfileViews = user.ProfileViews,
            ConnectionCount = connectionCount,
            Experiences = user.Experiences.Select(e => new ExperienceDto
            {
                Id = e.Id, Title = e.Title, Company = e.Company,
                Type = e.Type, StartDate = e.StartDate, EndDate = e.EndDate,
                IsCurrent = e.IsCurrent, Description = e.Description
            }).ToList(),
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPut("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        var user = await _db.Users.FindAsync(GetUserId());
        if (user == null) return NotFound();

        if (req.FullName != null) user.FullName = req.FullName;
        if (req.Headline != null) user.Headline = req.Headline;
        if (req.Bio != null) user.Bio = req.Bio;
        if (req.Location != null) user.Location = req.Location;
        if (req.Website != null) user.Website = req.Website;
        if (req.Status != null) user.Status = req.Status;
        if (req.Skills != null) user.Skills = JsonSerializer.Serialize(req.Skills);

        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "تم تحديث الملف الشخصي" });
    }

    [HttpPost("me/experiences")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> AddExperience([FromBody] AddExperienceRequest req)
    {
        var exp = new Experience
        {
            UserId = GetUserId(),
            Title = req.Title,
            Company = req.Company,
            Type = req.Type,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            IsCurrent = req.IsCurrent,
            Description = req.Description
        };
        _db.Experiences.Add(exp);
        await _db.SaveChangesAsync();
        return Ok(new ExperienceDto
        {
            Id = exp.Id, Title = exp.Title, Company = exp.Company,
            Type = exp.Type, StartDate = exp.StartDate, EndDate = exp.EndDate,
            IsCurrent = exp.IsCurrent, Description = exp.Description
        });
    }

    [HttpGet("suggestions")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetSuggestions([FromQuery] int count = 5)
    {
        var userId = GetUserId();
        var connectedUserIds = await _db.Connections
            .Where(c => (c.RequesterId == userId || c.RecipientId == userId) && c.Status == "accepted")
            .Select(c => c.RequesterId == userId ? c.RecipientId : c.RequesterId)
            .ToListAsync();

        connectedUserIds.Add(userId);

        var suggestions = await _db.Users
            .Where(u => !connectedUserIds.Contains(u.Id))
            .OrderByDescending(u => u.ProfileViews)
            .Take(count)
            .Select(u => new UserBriefDto
            {
                Id = u.Id, FullName = u.FullName,
                Headline = u.Headline, AvatarUrl = u.AvatarUrl
            })
            .ToListAsync();

        return Ok(suggestions);
    }

    private int GetUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // @mentions search
    [HttpGet("mention-search")]
    public async Task<IActionResult> MentionSearch([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 1) return Ok(new List<object>());
        var users = await _db.Users
            .Where(u => u.FullName.Contains(q))
            .Take(6)
            .Select(u => new { u.Id, u.FullName, u.AvatarUrl })
            .ToListAsync();
        return Ok(users);
    }

    private static List<string> DeserializeList(string? json)
        => string.IsNullOrEmpty(json) ? new() : JsonSerializer.Deserialize<List<string>>(json) ?? new();
}
