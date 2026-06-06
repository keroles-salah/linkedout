using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Linkdout.Api.Data;
using Linkdout.Api.DTOs;

namespace Linkdout.Api.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;

    public ProfileController(AppDbContext db) => _db = db;

    [HttpGet("/Profile")]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        return await BuildProfileView(userId);
    }

    [HttpGet("/Profile/{id}")]
    public async Task<IActionResult> Index(int id)
    {
        return await BuildProfileView(id);
    }

    private async Task<IActionResult> BuildProfileView(int userId)
    {
        var currentUserId = GetUserId();

        var user = await _db.Users
            .Include(u => u.Experiences)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound();

        // Increment views if viewing someone else
        if (userId != currentUserId)
        {
            user.ProfileViews++;
            await _db.SaveChangesAsync();
        }

        var connectionCount = await _db.Connections.CountAsync(c =>
            (c.RequesterId == userId || c.RecipientId == userId) && c.Status == "accepted");

        var skills = string.IsNullOrEmpty(user.Skills)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(user.Skills) ?? new List<string>();

        ViewBag.Profile = new UserDto
        {
            Id = user.Id, FullName = user.FullName, Email = user.Email,
            Headline = user.Headline, Bio = user.Bio, Location = user.Location,
            Website = user.Website, AvatarUrl = user.AvatarUrl, CoverUrl = user.CoverUrl,
            Status = user.Status, Skills = skills, ProfileViews = user.ProfileViews,
            ConnectionCount = connectionCount, XP = user.XP,
            Level = GamificationController.LevelFromXp(user.XP),
            Badges = string.IsNullOrEmpty(user.Badges) ? new() : JsonSerializer.Deserialize<List<string>>(user.Badges) ?? new(),
            Experiences = user.Experiences.OrderByDescending(e => e.IsCurrent ? DateTime.MaxValue : e.StartDate).Select(e => new ExperienceDto
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
        };

        ViewBag.IsOwnProfile = userId == currentUserId;

        return View();
    }

    [HttpGet("/Profile/Edit")]
    public async Task<IActionResult> Edit()
    {
        var userId = GetUserId();
        var user = await _db.Users
            .Include(u => u.Experiences)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();

        var skills = string.IsNullOrEmpty(user.Skills)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(user.Skills) ?? new List<string>();

        ViewBag.Profile = new UserDto
        {
            Id = user.Id, FullName = user.FullName, Headline = user.Headline,
            Bio = user.Bio, Location = user.Location, Website = user.Website,
            Status = user.Status, Skills = skills
        };
        return View();
    }

    [HttpPost("/Profile/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] UpdateProfileRequest req)
    {
        var userId = GetUserId();
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Error = "برجاء إدخال البيانات المطلوبة";
            ViewBag.Profile = new UserDto { Id = userId, FullName = req.FullName, Headline = req.Headline, Bio = req.Bio, Location = req.Location, Website = req.Website, Status = req.Status, Skills = new() };
            return View();
        }

        user.FullName = req.FullName;
        user.Headline = req.Headline;
        user.Bio = req.Bio;
        user.Location = req.Location;
        user.Website = req.Website;
        user.Status = req.Status ?? "open";
        user.Skills = !string.IsNullOrEmpty(req.Skills)
            ? JsonSerializer.Serialize(req.Skills.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList())
            : "[]";
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        ViewBag.Success = "تم تحديث الملف الشخصي بنجاح! ✓";
        
        var skills = string.IsNullOrEmpty(user.Skills) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(user.Skills) ?? new();
        ViewBag.Profile = new UserDto { Id = userId, FullName = user.FullName, Headline = user.Headline, Bio = user.Bio, Location = user.Location, Website = user.Website, Status = user.Status, Skills = skills };
        return View();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
