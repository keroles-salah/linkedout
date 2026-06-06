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

[ApiController, Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PostsController(AppDbContext db) => _db = db;

    [HttpGet("feed")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        var connectedIds = await _db.Connections
            .Where(c => (c.RequesterId == userId || c.RecipientId == userId) && c.Status == "accepted")
            .Select(c => c.RequesterId == userId ? c.RecipientId : c.RequesterId)
            .ToListAsync();
        connectedIds.Add(userId);

        var posts = await _db.Posts
            .Include(p => p.User)
            .Where(p => connectedIds.Contains(p.UserId) && p.GroupId == null)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var likedPostIds = await _db.Likes
            .Where(l => l.UserId == userId && posts.Select(p => p.Id).Contains(l.PostId))
            .Select(l => l.PostId)
            .ToListAsync();

        var totalCount = await _db.Posts
            .CountAsync(p => connectedIds.Contains(p.UserId) && p.GroupId == null);

        return Ok(new PagedResponse<PostDto>
        {
            Items = posts.Select(p => MapPost(p, likedPostIds.Contains(p.Id))).ToList(),
            TotalCount = totalCount, Page = page, PageSize = pageSize
        });
    }

    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        var post = await _db.Posts.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) return NotFound();

        var isLiked = await _db.Likes.AnyAsync(l => l.PostId == id && l.UserId == userId);
        return Ok(MapPost(post, isLiked));
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest req)
    {
        var post = new Post
        {
            UserId = GetUserId(),
            Body = req.Body,
            Images = req.Images != null ? JsonSerializer.Serialize(req.Images) : null,
            Tags = req.Tags != null ? JsonSerializer.Serialize(req.Tags) : null,
            GroupId = req.GroupId
        };

        _db.Posts.Add(post);

        if (req.GroupId.HasValue)
        {
            var group = await _db.Groups.FindAsync(req.GroupId);
            if (group != null) group.PostCount++;
        }

        await _db.SaveChangesAsync();

        await _db.Entry(post).Reference(p => p.User).LoadAsync();
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, MapPost(post, false));
    }

    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();
        if (post.UserId != userId) return Forbid();

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        return Ok(new { message = "تم حذف المنشور" });
    }

    [HttpPost("{id}/like")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ToggleLike(int id)
    {
        var userId = GetUserId();
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();

        var existingLike = await _db.Likes.FirstOrDefaultAsync(l => l.PostId == id && l.UserId == userId);

        if (existingLike != null)
        {
            _db.Likes.Remove(existingLike);
            post.LikeCount--;
        }
        else
        {
            _db.Likes.Add(new Like { PostId = id, UserId = userId });
            post.LikeCount++;
        }

        await _db.SaveChangesAsync();
        return Ok(new { isLiked = existingLike == null, likeCount = post.LikeCount });
    }

    [HttpPost("{id}/comment")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentRequest req)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();

        var comment = new Comment { PostId = id, UserId = GetUserId(), Body = req.Body };
        _db.Comments.Add(comment);
        post.CommentCount++;
        await _db.SaveChangesAsync();

        await _db.Entry(comment).Reference(c => c.User).LoadAsync();
        return Ok(new CommentDto
        {
            Id = comment.Id,
            User = MapBrief(comment.User),
            Body = comment.Body,
            CreatedAt = comment.CreatedAt
        });
    }

    [HttpGet("{id}/comments")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetComments(int id)
    {
        var comments = await _db.Comments
            .Include(c => c.User)
            .Where(c => c.PostId == id)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                User = new UserBriefDto { Id = c.User.Id, FullName = c.User.FullName, Headline = c.User.Headline, AvatarUrl = c.User.AvatarUrl },
                Body = c.Body,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(comments);
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static PostDto MapPost(Post p, bool isLiked) => new()
    {
        Id = p.Id,
        Author = MapBrief(p.User),
        Body = p.Body,
        Images = string.IsNullOrEmpty(p.Images) ? null : JsonSerializer.Deserialize<List<string>>(p.Images),
        Tags = string.IsNullOrEmpty(p.Tags) ? null : JsonSerializer.Deserialize<List<string>>(p.Tags),
        LikeCount = p.LikeCount,
        CommentCount = p.CommentCount,
        ShareCount = p.ShareCount,
        IsLiked = isLiked,
        CreatedAt = p.CreatedAt
    };

    private static UserBriefDto MapBrief(User u) => new()
    {
        Id = u.Id, FullName = u.FullName, Headline = u.Headline, AvatarUrl = u.AvatarUrl
    };
}
