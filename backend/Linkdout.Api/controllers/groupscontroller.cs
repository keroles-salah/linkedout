using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Linkdout.Api.Data;
using Linkdout.Api.DTOs;

namespace Linkdout.Api.Controllers;

[Authorize]
public class GroupsController : Controller
{
    private readonly AppDbContext _db;

    public GroupsController(AppDbContext db) => _db = db;

    [HttpGet("/Groups")]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();

        var myGroupIds = await _db.GroupMembers
            .Where(gm => gm.UserId == userId)
            .Select(gm => gm.GroupId)
            .ToListAsync();

        var groups = await _db.Groups
            .OrderByDescending(g => g.MemberCount)
            .ToListAsync();

        ViewBag.Groups = groups.Select(g => new GroupDto
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            CoverColor = g.CoverColor,
            Icon = g.Icon,
            Privacy = g.Privacy,
            MemberCount = g.MemberCount,
            PostCount = g.PostCount,
            IsMember = myGroupIds.Contains(g.Id),
            CreatedAt = g.CreatedAt
        }).ToList();

        return View();
    }

    [HttpGet("/Groups/{id}")]
    public async Task<IActionResult> Detail(int id)
    {
        var userId = GetUserId();

        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == id);
        if (group == null) return NotFound();

        var isMember = await _db.GroupMembers.AnyAsync(gm => gm.GroupId == id && gm.UserId == userId);

        var posts = await _db.Posts
            .Include(p => p.User)
            .Where(p => p.GroupId == id)
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .ToListAsync();

        var likedPostIds = await _db.Likes
            .Where(l => l.UserId == userId && posts.Select(p => p.Id).Contains(l.PostId))
            .Select(l => l.PostId)
            .ToListAsync();

        ViewBag.Group = new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            CoverColor = group.CoverColor,
            Icon = group.Icon,
            Privacy = group.Privacy,
            MemberCount = group.MemberCount,
            PostCount = group.PostCount,
            IsMember = isMember,
            CreatedAt = group.CreatedAt
        };

        ViewBag.Posts = posts.Select(p => new PostDto
        {
            Id = p.Id,
            Author = new UserBriefDto
            {
                Id = p.User.Id,
                FullName = p.User.FullName,
                Headline = p.User.Headline,
                AvatarUrl = p.User.AvatarUrl
            },
            Body = p.Body,
            Images = string.IsNullOrEmpty(p.Images) ? null : JsonSerializer.Deserialize<List<string>>(p.Images),
            Tags = string.IsNullOrEmpty(p.Tags) ? null : JsonSerializer.Deserialize<List<string>>(p.Tags),
            LikeCount = p.LikeCount,
            CommentCount = p.CommentCount,
            ShareCount = p.ShareCount,
            IsLiked = likedPostIds.Contains(p.Id),
            CreatedAt = p.CreatedAt
        }).ToList();

        ViewBag.IsMember = isMember;

        return View();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
