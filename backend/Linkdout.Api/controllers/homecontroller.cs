using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Linkdout.Api.Data;
using Linkdout.Api.DTOs;

namespace Linkdout.Api.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private const int FeedPageSize = 10;

    public HomeController(AppDbContext db) => _db = db;

    [HttpGet("/")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "feed")]
    public async Task<IActionResult> Index([FromQuery] int page = 1)
    {
        if (!User.Identity?.IsAuthenticated == true)
            return View("Landing");

        var userId = GetUserId();
        ViewBag.Page = page;

        // ── SINGLE QUERY: user + connection count ──
        var currentUser = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id, u.FullName, u.Headline, u.AvatarUrl, u.Status,
                u.ProfileViews, u.XP, u.Badges,
                ConnectionCount = _db.Connections.Count(c =>
                    (c.RequesterId == userId || c.RecipientId == userId) && c.Status == "accepted")
            })
            .FirstOrDefaultAsync();

        if (currentUser != null)
        {
            ViewBag.MyProfile = new UserDto
            {
                Id = currentUser.Id,
                FullName = currentUser.FullName,
                Headline = currentUser.Headline,
                AvatarUrl = currentUser.AvatarUrl,
                Status = currentUser.Status,
                ProfileViews = currentUser.ProfileViews,
                ConnectionCount = currentUser.ConnectionCount,
                XP = currentUser.XP,
                Level = GamificationController.LevelFromXp(currentUser.XP),
                Badges = string.IsNullOrEmpty(currentUser.Badges) ? new()
                    : JsonSerializer.Deserialize<List<string>>(currentUser.Badges) ?? new()
            };
        }

        // ── SINGLE QUERY: connected IDs ──
        var connectedIds = await _db.Connections
            .Where(c => (c.RequesterId == userId || c.RecipientId == userId) && c.Status == "accepted")
            .Select(c => c.RequesterId == userId ? c.RecipientId : c.RequesterId)
            .ToListAsync();
        connectedIds.Add(userId);

        // ── FEED: combined count + posts + projections (1 query with 2 parts) ──
        var postsQuery = _db.Posts
            .Where(p => connectedIds.Contains(p.UserId) && p.GroupId == null)
            .OrderByDescending(p => p.CreatedAt);

        var totalPosts = await postsQuery.CountAsync();

        var posts = await postsQuery
            .Skip((page - 1) * FeedPageSize)
            .Take(FeedPageSize)
            .Select(p => new
            {
                Post = p,
                Author = new UserBriefDto
                {
                    Id = p.User.Id, FullName = p.User.FullName,
                    Headline = p.User.Headline, AvatarUrl = p.User.AvatarUrl
                }
            })
            .ToListAsync();

        ViewBag.HasMore = (page * FeedPageSize) < totalPosts;

        var postIds = posts.Select(x => x.Post.Id).ToList();
        var likedPostIds = await _db.Likes
            .Where(l => l.UserId == userId && postIds.Contains(l.PostId))
            .Select(l => l.PostId)
            .ToListAsync();

        ViewBag.Posts = posts.Select(x => new PostDto
        {
            Id = x.Post.Id,
            Author = x.Author,
            Body = x.Post.Body,
            Images = string.IsNullOrEmpty(x.Post.Images) ? null : JsonSerializer.Deserialize<List<string>>(x.Post.Images),
            Tags = string.IsNullOrEmpty(x.Post.Tags) ? null : JsonSerializer.Deserialize<List<string>>(x.Post.Tags),
            LikeCount = x.Post.LikeCount,
            CommentCount = x.Post.CommentCount,
            ShareCount = x.Post.ShareCount,
            ViewCount = x.Post.ViewCount,
            IsLiked = likedPostIds.Contains(x.Post.Id),
            IsEdited = x.Post.IsEdited,
            Poll = string.IsNullOrEmpty(x.Post.Poll) ? null : JsonSerializer.Deserialize<object>(x.Post.Poll),
            CreatedAt = x.Post.CreatedAt
        }).ToList();

        // ── COMBINED: circles + jobs + suggestions (sequential — DbContext is not thread-safe) ──
        var circles = await _db.Connections
            .Where(c => (c.RequesterId == userId || c.RecipientId == userId) && c.Status == "accepted")
            .GroupBy(c => c.Circle ?? "بدون دائرة")
            .Select(g => new { Circle = g.Key, Count = g.Count() })
            .ToListAsync();

        var jobs = await _db.Jobs
            .Where(j => j.IsActive)
            .OrderByDescending(j => j.CreatedAt)
            .Take(5)
            .Select(j => new JobDto
            {
                Id = j.Id, CompanyId = j.CompanyId, CompanyName = j.Company.Name,
                Title = j.Title, Type = j.Type, Location = j.Location,
                Description = j.Description, CreatedAt = j.CreatedAt
            })
            .ToListAsync();

        var suggestions = await _db.Users
            .Where(u => !connectedIds.Contains(u.Id) && u.Id != userId)
            .OrderByDescending(u => u.ProfileViews)
            .Take(5)
            .Select(u => new UserBriefDto
            {
                Id = u.Id, FullName = u.FullName,
                Headline = u.Headline, AvatarUrl = u.AvatarUrl
            })
            .ToListAsync();

        ViewBag.Circles = circles;
        ViewBag.Opportunities = jobs;
        ViewBag.Suggestions = suggestions;

        return View();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("/Bookmarks")]
    public async Task<IActionResult> Bookmarks()
    {
        var userId = GetUserId();
        var bookmarkIds = await _db.PostBookmarks
            .Where(b => b.UserId == userId)
            .Select(b => b.PostId)
            .ToListAsync();

        var posts = await _db.Posts
            .Where(p => bookmarkIds.Contains(p.Id))
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostDto
            {
                Id = p.Id, Body = p.Body,
                AuthorName = p.User.FullName, AuthorAvatar = p.User.AvatarUrl,
                LikeCount = p.LikeCount, CommentCount = p.CommentCount,
                ViewCount = p.ViewCount, CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        ViewBag.Bookmarks = posts;
        return View();
    }
}
