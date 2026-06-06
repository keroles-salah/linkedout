using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Linkdout.Api.Data;
using Linkdout.Api.DTOs;
using Linkdout.Api.Models;

namespace Linkdout.Api.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db) => _db = db;

    private async Task<bool> IsAdmin()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return false;
        var user = await _db.Users.FindAsync(int.Parse(userIdClaim));
        return user?.Role == "admin";
    }

    private IActionResult AdminOnly()
    {
        if (!IsAdmin().Result)
        {
            TempData["Error"] = "غير مصرح بالدخول";
            return RedirectToAction("Index", "Home");
        }
        return null!;
    }

    // ═══ DASHBOARD ═══
    [HttpGet("/Admin")]
    public async Task<IActionResult> Index()
    {
        var check = await IsAdmin();
        if (!check) return RedirectToAction("Index", "Home");

        ViewBag.TotalUsers = await _db.Users.CountAsync();
        ViewBag.ActiveUsers = await _db.Users.CountAsync(u => u.IsActive);
        ViewBag.TotalPosts = await _db.Posts.CountAsync();
        ViewBag.TotalComments = await _db.Comments.CountAsync();
        ViewBag.TotalConnections = await _db.Connections.CountAsync(c => c.Status == "accepted");
        ViewBag.TotalGroups = await _db.Groups.CountAsync();
        ViewBag.TotalCompanies = await _db.Companies.CountAsync();
        ViewBag.TotalJobs = await _db.Jobs.CountAsync();

        // New today
        var today = DateTime.UtcNow.Date;
        ViewBag.NewUsersToday = await _db.Users.CountAsync(u => u.CreatedAt >= today);
        ViewBag.NewPostsToday = await _db.Posts.CountAsync(p => p.CreatedAt >= today);

        // Recent users
        ViewBag.RecentUsers = await _db.Users.OrderByDescending(u => u.CreatedAt).Take(5)
            .Select(u => new { u.Id, u.FullName, u.Email, u.CreatedAt, u.Role, u.IsActive })
            .ToListAsync();

        // Recent posts  
        ViewBag.RecentPosts = await _db.Posts.Include(p => p.User).OrderByDescending(p => p.CreatedAt).Take(5)
            .Select(p => new { p.Id, p.Body, p.UserId, Author = p.User.FullName, p.CreatedAt, p.LikeCount, p.CommentCount })
            .ToListAsync();

        return View();
    }

    // ═══ USERS MANAGEMENT ═══
    [HttpGet("/Admin/Users")]
    public async Task<IActionResult> Users([FromQuery] string? search, [FromQuery] string? role, [FromQuery] string? status, [FromQuery] int page = 1)
    {
        var check = await IsAdmin();
        if (!check) return RedirectToAction("Index", "Home");

        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);
        if (status == "active")
            query = query.Where(u => u.IsActive);
        else if (status == "inactive")
            query = query.Where(u => !u.IsActive);

        ViewBag.TotalCount = await query.CountAsync();
        ViewBag.Search = search;
        ViewBag.Role = role;
        ViewBag.Status = status;
        ViewBag.Page = page;

        ViewBag.Users = await query.OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * 20).Take(20)
            .Select(u => new UserDto
            {
                Id = u.Id, FullName = u.FullName, Email = u.Email,
                Headline = u.Headline, Location = u.Location, Status = u.Status,
                ProfileViews = u.ProfileViews, XP = u.XP, CreatedAt = u.CreatedAt,
                Role = u.Role, IsActive = u.IsActive
            })
            .ToListAsync();

        return View();
    }

    [HttpPost("/Admin/Users/ToggleActive/{id}")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var check = await IsAdmin();
        if (!check) return Json(new { error = "غير مصرح" });

        var user = await _db.Users.FindAsync(id);
        if (user == null) return Json(new { error = "المستخدم غير موجود" });
        if (user.Role == "admin") return Json(new { error = "لا يمكن تعطيل الأدمن" });

        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();
        return Json(new { ok = true, isActive = user.IsActive });
    }

    [HttpPost("/Admin/Users/MakeAdmin/{id}")]
    public async Task<IActionResult> MakeAdmin(int id)
    {
        var check = await IsAdmin();
        if (!check) return Json(new { error = "غير مصرح" });

        var user = await _db.Users.FindAsync(id);
        if (user == null) return Json(new { error = "المستخدم غير موجود" });

        user.Role = user.Role == "admin" ? "user" : "admin";
        await _db.SaveChangesAsync();
        return Json(new { ok = true, role = user.Role });
    }

    [HttpDelete("/Admin/Users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var check = await IsAdmin();
        if (!check) return Json(new { error = "غير مصرح" });

        var user = await _db.Users.FindAsync(id);
        if (user == null) return Json(new { error = "غير موجود" });
        if (user.Role == "admin") return Json(new { error = "لا يمكن حذف الأدمن" });

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return Json(new { ok = true });
    }

    // ═══ POSTS MANAGEMENT ═══
    [HttpGet("/Admin/Posts")]
    public async Task<IActionResult> Posts([FromQuery] string? search, [FromQuery] int page = 1)
    {
        var check = await IsAdmin();
        if (!check) return RedirectToAction("Index", "Home");

        var query = _db.Posts.Include(p => p.User).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Body.Contains(search));

        ViewBag.TotalCount = await query.CountAsync();
        ViewBag.Search = search;
        ViewBag.Page = page;

        ViewBag.Posts = await query.OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * 20).Take(20)
            .Select(p => new PostDto
            {
                Id = p.Id, Body = p.Body,
                Tags = new List<string>(),
                Images = new List<string>(),
                LikeCount = p.LikeCount,
                CommentCount = p.CommentCount, ShareCount = p.ShareCount,
                ViewCount = p.ViewCount, CreatedAt = p.CreatedAt,
                UserId = p.UserId, AuthorName = p.User.FullName,
                AuthorAvatar = p.User.AvatarUrl, AuthorHeadline = p.User.Headline,
                GroupId = p.GroupId
            })
            .ToListAsync();

        return View();
    }

    [HttpPost("/Admin/Posts/Delete/{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        var check = await IsAdmin();
        if (!check) return Json(new { error = "غير مصرح" });

        var post = await _db.Posts.FindAsync(id);
        if (post == null) return Json(new { error = "غير موجود" });

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        return Json(new { ok = true });
    }

    // ═══ ANALYTICS ═══
    [HttpGet("/Admin/Analytics")]
    public async Task<IActionResult> Analytics()
    {
        var check = await IsAdmin();
        if (!check) return RedirectToAction("Index", "Home");
        return View();
    }

    // ═══ API: STATS ═══
    [HttpGet("/api/admin/stats")]
    public async Task<IActionResult> GetStats()
    {
        var check = await IsAdmin();
        if (!check) return Json(new { error = "غير مصرح" });

        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddDays(-30);

        return Json(new
        {
            totalUsers = await _db.Users.CountAsync(),
            activeUsers = await _db.Users.CountAsync(u => u.IsActive),
            totalPosts = await _db.Posts.CountAsync(),
            totalComments = await _db.Comments.CountAsync(),
            totalConnections = await _db.Connections.CountAsync(c => c.Status == "accepted"),
            totalGroups = await _db.Groups.CountAsync(),
            totalCompanies = await _db.Companies.CountAsync(),
            totalJobs = await _db.Jobs.CountAsync(j => j.IsActive),
            usersToday = await _db.Users.CountAsync(u => u.CreatedAt >= today),
            usersThisWeek = await _db.Users.CountAsync(u => u.CreatedAt >= weekAgo),
            usersThisMonth = await _db.Users.CountAsync(u => u.CreatedAt >= monthAgo),
            postsToday = await _db.Posts.CountAsync(p => p.CreatedAt >= today),
            postsThisWeek = await _db.Posts.CountAsync(p => p.CreatedAt >= weekAgo),
            totalLikes = await _db.Likes.CountAsync(),
            avgPostsPerUser = await _db.Users.CountAsync() > 0
                ? (double)await _db.Posts.CountAsync() / await _db.Users.CountAsync() : 0
        });
    }

    // ═══ API: CHARTS ═══
    [HttpGet("/api/admin/charts")]
    public async Task<IActionResult> GetCharts([FromQuery] string type = "growth")
    {
        var check = await IsAdmin();
        if (!check) return Json(new { error = "غير مصرح" });

        var result = new Dictionary<string, object>();

        // User growth: last 30 days
        var growthLabels = new List<string>();
        var growthData = new List<int>();
        for (int i = 29; i >= 0; i--)
        {
            var day = DateTime.UtcNow.Date.AddDays(-i);
            growthLabels.Add(day.ToString("MMM dd"));
            growthData.Add(await _db.Users.CountAsync(u => u.CreatedAt <= day.AddDays(1)));
        }
        result["growth"] = new { labels = growthLabels, data = growthData };

        // Posts per day: last 14 days
        var postLabels = new List<string>();
        var postData = new List<int>();
        for (int i = 13; i >= 0; i--)
        {
            var day = DateTime.UtcNow.Date.AddDays(-i);
            postLabels.Add(day.ToString("MMM dd"));
            postData.Add(await _db.Posts.CountAsync(p => p.CreatedAt >= day && p.CreatedAt < day.AddDays(1)));
        }
        result["posts"] = new { labels = postLabels, data = postData };

        // XP distribution
        var levels = new[] { (0, 100, "0-100"), (100, 500, "100-500"), (500, 1500, "500-1500"), (1500, 5000, "1500-5000"), (5000, 1000000, "5000+") };
        var xpLabels = new List<string>();
        var xpData = new List<int>();
        foreach (var (min, max, label) in levels)
        {
            xpLabels.Add(label);
            xpData.Add(await _db.Users.CountAsync(u => u.XP >= min && u.XP < max));
        }
        result["xp"] = new { labels = xpLabels, data = xpData };

        // Engagement: likes vs comments ratio
        var totalLikes = await _db.Likes.CountAsync();
        var totalComments = await _db.Comments.CountAsync();
        result["engagement"] = new
        {
            labels = new[] { "إعجابات", "تعليقات", "مشاركات" },
            data = new[] { totalLikes, totalComments, 0 }
        };

        // Top users by XP
        var topUsers = await _db.Users.OrderByDescending(u => u.XP).Take(10)
            .Select(u => new { u.FullName, u.XP })
            .ToListAsync();
        result["topUsers"] = new
        {
            labels = topUsers.Select(u => u.FullName),
            data = topUsers.Select(u => u.XP)
        };

        return Json(result);
    }
}
