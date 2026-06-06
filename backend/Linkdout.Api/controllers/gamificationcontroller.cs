using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Linkdout.Api.Data;
using Linkdout.Api.DTOs;
using Linkdout.Api.Models;

namespace Linkdout.Api.Controllers;

[Authorize]
public class GamificationController : Controller
{
    private readonly AppDbContext _db;
    public GamificationController(AppDbContext db) => _db = db;
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static int XpForLevel(int level) => level <= 1 ? 0 : (level - 1) * 100 + (level - 2) * 50;
    public static int LevelFromXp(int xp)
    {
        for (int i = 1; i <= 20; i++)
            if (xp < XpForLevel(i + 1)) return i;
        return 20;
    }
    public static string LevelTitle(int level) => level switch
    {
        <= 1 => "🌱 مبتدئ", <= 3 => "🌿 متعلم", <= 6 => "🌳 محترف", <= 10 => "⭐ خبير", <= 15 => "👑 قائد", _ => "🏆 أسطورة"
    };
    public static string LevelColor(int level) => level switch
    {
        <= 1 => "#A8988A", <= 3 => "#5B8C5A", <= 6 => "#1A1A2E", <= 10 => "#C27B4F", <= 15 => "#E8B960", _ => "linear-gradient(135deg,#E8B960,#C27B4F)"
    };

    // ===== LEADERBOARD =====
    [HttpGet("/Leaderboard")]
    public async Task<IActionResult> Index([FromQuery] string? period)
    {
        period ??= "all";
        ViewBag.Period = period;

        List<LeaderboardEntry> users;

        if (period == "weekly")
        {
            var weekAgo = DateTime.UtcNow.AddDays(-7);
            users = await _db.XpTransactions
                .Where(t => t.CreatedAt >= weekAgo)
                .GroupBy(t => t.UserId)
                .Select(g => new { UserId = g.Key, TotalXp = g.Sum(t => t.Amount) })
                .OrderByDescending(g => g.TotalXp)
                .Take(20)
                .Join(_db.Users, g => g.UserId, u => u.Id, (g, u) => new LeaderboardEntry {
                    Id = u.Id, FullName = u.FullName, Headline = u.Headline,
                    XP = g.TotalXp, Badges = u.Badges, ProfileViews = u.ProfileViews
                })
                .ToListAsync();
        }
        else if (period == "monthly")
        {
            var monthAgo = DateTime.UtcNow.AddDays(-30);
            users = await _db.XpTransactions
                .Where(t => t.CreatedAt >= monthAgo)
                .GroupBy(t => t.UserId)
                .Select(g => new { UserId = g.Key, TotalXp = g.Sum(t => t.Amount) })
                .OrderByDescending(g => g.TotalXp)
                .Take(30)
                .Join(_db.Users, g => g.UserId, u => u.Id, (g, u) => new LeaderboardEntry {
                    Id = u.Id, FullName = u.FullName, Headline = u.Headline,
                    XP = g.TotalXp, Badges = u.Badges, ProfileViews = u.ProfileViews
                })
                .ToListAsync();
        }
        else // all-time
        {
            users = await _db.Users
                .OrderByDescending(u => u.XP)
                .Take(50)
                .Select(u => new LeaderboardEntry {
                    Id = u.Id, FullName = u.FullName, Headline = u.Headline,
                    XP = u.XP, Badges = u.Badges, ProfileViews = u.ProfileViews
                })
                .ToListAsync();
        }

        for (int i = 0; i < users.Count; i++)
        {
            users[i].Rank = i + 1;
            users[i].Level = LevelFromXp(users[i].XP);
        }

        ViewBag.Leaders = users;
        return View();
    }

    // ===== JOB MATCHER =====
    [HttpGet("/Jobs/Match")]
    public async Task<IActionResult> JobMatch()
    {
        var userId = GetUserId();
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return RedirectToAction("Index", "Opportunities");

        var userSkills = string.IsNullOrEmpty(user.Skills) ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(user.Skills) ?? new();

        var userSkillsNormalized = userSkills.Select(s => s.Trim().ToLowerInvariant()).ToHashSet();

        var jobs = await _db.Jobs
            .Include(j => j.Company)
            .Where(j => j.IsActive)
            .ToListAsync();

        var matched = jobs.Select(j =>
        {
            var jobSkills = string.IsNullOrEmpty(j.RequiredSkills) ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(j.RequiredSkills) ?? new();

            var jobSkillsNormalized = jobSkills.Select(s => s.Trim().ToLowerInvariant()).ToList();

            var matchingSkills = jobSkillsNormalized
                .Where(js => userSkillsNormalized.Any(us => us == js || us.Contains(js) || js.Contains(us)))
                .ToList();

            var missingSkills = jobSkillsNormalized
                .Where(js => !userSkillsNormalized.Any(us => us == js || us.Contains(js) || js.Contains(us)))
                .ToList();

            var matchCount = matchingSkills.Count;
            var score = jobSkills.Count > 0 ? (int)Math.Round(100.0 * matchCount / jobSkills.Count) : 50;

            // Bonus for location/headline keyword match
            if (!string.IsNullOrEmpty(user.Location) &&
                (j.Title.Contains(user.Location, StringComparison.OrdinalIgnoreCase) ||
                 (j.Location != null && j.Location.Contains(user.Location, StringComparison.OrdinalIgnoreCase))))
                score += 10;
            if (!string.IsNullOrEmpty(user.Headline) &&
                j.Title.Contains(user.Headline, StringComparison.OrdinalIgnoreCase))
                score += 5;
            score = Math.Min(100, score);

            return new {
                Job = new JobDto {
                    Id = j.Id, CompanyId = j.CompanyId, CompanyName = j.Company.Name,
                    Title = j.Title, Type = j.Type, Location = j.Location,
                    Description = j.Description, RequiredSkills = jobSkills, CreatedAt = j.CreatedAt
                },
                MatchScore = score,
                MatchingSkills = matchingSkills,
                MissingSkills = missingSkills
            };
        })
        .OrderByDescending(m => m.MatchScore)
        .ToList();

        ViewBag.Matches = matched;
        ViewBag.UserSkills = userSkills;
        return View();
    }

    // ===== AWARD XP (called by other controllers) =====
    public static async Task AwardXp(AppDbContext db, int userId, int amount, string reason = "")
    {
        var user = await db.Users.FindAsync(userId);
        if (user == null) return;
        user.XP += amount;

        // Log transaction for time-based leaderboards
        db.XpTransactions.Add(new XpTransaction
        {
            UserId = userId,
            Amount = amount,
            Reason = string.IsNullOrEmpty(reason) ? "other" : reason
        });

        await AutoAwardBadges(db, user);
        await db.SaveChangesAsync();
    }

    private static async Task AutoAwardBadges(AppDbContext db, User user)
    {
        var badges = string.IsNullOrEmpty(user.Badges) ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(user.Badges) ?? new();

        var postCount = await db.Posts.CountAsync(p => p.UserId == user.Id);
        var likeCount = await db.Likes.CountAsync(l => l.Post.UserId == user.Id);
        var commentCount = await db.Comments.CountAsync(c => c.UserId == user.Id);
        var connCount = await db.Connections.CountAsync(c => (c.RequesterId == user.Id || c.RecipientId == user.Id) && c.Status == "accepted");

        void AddIfMissing(string badge) { if (!badges.Contains(badge)) badges.Add(badge); }

        if (postCount >= 1) AddIfMissing("🖊️ أول منشور");
        if (postCount >= 10) AddIfMissing("✍️ كاتب محتوى");
        if (postCount >= 50) AddIfMissing("📝 صانع محتوى");
        if (likeCount >= 10) AddIfMissing("❤️ محبوب");
        if (likeCount >= 100) AddIfMissing("🌟 نجم المنصة");
        if (commentCount >= 5) AddIfMissing("💬 متفاعل");
        if (commentCount >= 50) AddIfMissing("🗣️ الأكثر تفاعلاً");
        if (connCount >= 10) AddIfMissing("🔗 شبكة قوية");
        if (connCount >= 50) AddIfMissing("🌐 الأكثر تواصلاً");
        if (user.XP >= 100) AddIfMissing("⚡ نشيط");
        if (user.XP >= 500) AddIfMissing("🔥 ملتهب");
        if (user.XP >= 1000) AddIfMissing("💎 أسطورة");
        if (user.ProfileViews >= 100) AddIfMissing("👁️ ملف مميز");
        if (user.ProfileViews >= 1000) AddIfMissing("🔍 الأكثر بحثاً");

        user.Badges = JsonSerializer.Serialize(badges);
    }

    // XP award amounts
    public const int XpPost = 10;
    public const int XpComment = 3;
    public const int XpLike = 2;
    public const int XpShare = 5;
    public const int XpConnect = 7;
    public const int XpJoinGroup = 5;
    public const int XpProfileEdit = 15;
}

public class LeaderboardEntry
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string? Headline { get; set; }
    public int XP { get; set; }
    public string? Badges { get; set; }
    public int ProfileViews { get; set; }
    public int Rank { get; set; }
    public int Level { get; set; }
}
