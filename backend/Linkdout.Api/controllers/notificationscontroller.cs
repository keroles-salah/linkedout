using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Linkdout.Api.Data;
using Linkdout.Api.DTOs;
using Linkdout.Api.Models;

namespace Linkdout.Api.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly AppDbContext _db;
    public NotificationsController(AppDbContext db) => _db = db;
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ===== NOTIFICATIONS API =====
    [HttpGet("/api/notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = GetUserId();

        // Recent likes on user's posts
        var recentLikes = await _db.Likes
            .Include(l => l.User)
            .Include(l => l.Post)
            .Where(l => l.Post.UserId == userId && l.UserId != userId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(5)
            .Select(l => new {
                id = l.Id,
                type = "like",
                userName = l.User.FullName,
                userId = l.User.Id,
                postId = l.PostId,
                text = l.User.FullName + " أعجب بمنشورك",
                time = l.CreatedAt.ToString("dd MMM"),
                createdAt = l.CreatedAt
            })
            .ToListAsync();

        // Recent comments on user's posts
        var recentComments = await _db.Comments
            .Include(c => c.User)
            .Include(c => c.Post)
            .Where(c => c.Post.UserId == userId && c.UserId != userId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new {
                id = c.Id,
                type = "comment",
                userName = c.User.FullName,
                userId = c.User.Id,
                postId = c.PostId,
                text = c.User.FullName + " علق على منشورك: \"" + (c.Body.Length > 40 ? c.Body.Substring(0, 40) + "..." : c.Body) + "\"",
                time = c.CreatedAt.ToString("dd MMM"),
                createdAt = c.CreatedAt
            })
            .ToListAsync();

        // Pending connection requests
        var pendingRequests = await _db.Connections
            .Include(c => c.Requester)
            .Where(c => c.RecipientId == userId && c.Status == "pending")
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new {
                id = c.Id,
                type = "connection",
                userName = c.Requester.FullName,
                userId = c.Requester.Id,
                text = c.Requester.FullName + " أرسل لك طلب علاقة",
                time = c.CreatedAt.ToString("dd MMM"),
                createdAt = c.CreatedAt,
                circle = c.Circle
            })
            .ToListAsync();

        var all = recentLikes.Cast<object>()
            .Concat(recentComments.Cast<object>())
            .Concat(pendingRequests.Cast<object>())
            .OrderByDescending(n => ((dynamic)n).createdAt)
            .Take(15)
            .ToList();

        var unreadCount = recentLikes.Count + recentComments.Count + pendingRequests.Count;

        return Ok(new { notifications = all, unreadCount });
    }

    [HttpGet("/api/notifications/count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        var likes = await _db.Likes.CountAsync(l => l.Post.UserId == userId && l.UserId != userId);
        var comments = await _db.Comments.CountAsync(c => c.Post.UserId == userId && c.UserId != userId);
        var pending = await _db.Connections.CountAsync(c => c.RecipientId == userId && c.Status == "pending");
        return Ok(new { count = likes + comments + pending });
    }

    // ===== CONNECTION REQUESTS PAGE =====
    [HttpGet("/Notifications")]
    public async Task<IActionResult> NotificationPage()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Redirect("/Account/Login");
        var notifications = await _db.Notifications
            .Where(n => n.UserId == int.Parse(userId))
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();
        return View("NotificationsPage", notifications);
    }

    [HttpGet("/Connections")]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();

        var pending = await _db.Connections
            .Include(c => c.Requester)
            .Where(c => c.RecipientId == userId && c.Status == "pending")
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ConnectionDto {
                Id = c.Id,
                User = new UserBriefDto {
                    Id = c.Requester.Id,
                    FullName = c.Requester.FullName,
                    Headline = c.Requester.Headline,
                    AvatarUrl = c.Requester.AvatarUrl
                },
                Status = c.Status,
                Circle = c.Circle,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        var sent = await _db.Connections
            .Include(c => c.Recipient)
            .Where(c => c.RequesterId == userId && c.Status == "pending")
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ConnectionDto {
                Id = c.Id,
                User = new UserBriefDto {
                    Id = c.Recipient.Id,
                    FullName = c.Recipient.FullName,
                    Headline = c.Recipient.Headline,
                    AvatarUrl = c.Recipient.AvatarUrl
                },
                Status = c.Status,
                Circle = c.Circle,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        ViewBag.Pending = pending;
        ViewBag.Sent = sent;
        return View();
    }
}
