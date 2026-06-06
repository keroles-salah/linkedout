using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Linkdout.Api.Data;
using Linkdout.Api.DTOs;
using Linkdout.Api.Models;

namespace Linkdout.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/interactions")]
public class InteractionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public InteractionsController(AppDbContext db) => _db = db;

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ===== LIKE =====
    [HttpPost("like/{postId}")]
    public async Task<IActionResult> ToggleLike(int postId)
    {
        var userId = GetUserId();
        var post = await _db.Posts.FindAsync(postId);
        if (post == null) return NotFound(new { error = "المنشور غير موجود" });

        var existing = await _db.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
        bool liked;
        if (existing != null)
        {
            _db.Likes.Remove(existing);
            post.LikeCount = Math.Max(0, post.LikeCount - 1);
            liked = false;
        }
        else
        {
            _db.Likes.Add(new Like { PostId = postId, UserId = userId });
            post.LikeCount++;
            liked = true;
        }
        await _db.SaveChangesAsync();
        // Only award XP for the like action itself
        if (liked) _ = GamificationController.AwardXp(_db, userId, GamificationController.XpLike, "like");
        return Ok(new { liked, likeCount = post.LikeCount });
    }

    // ===== CREATE POST =====
    [HttpPost("posts")]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest req)
    {
        var userId = GetUserId();
        var post = new Post
        {
            UserId = userId,
            Body = req.Body,
            Tags = req.Tags != null ? JsonSerializer.Serialize(req.Tags) : null,
            Images = req.Images != null ? JsonSerializer.Serialize(req.Images) : null,
            GroupId = req.GroupId,
            Poll = req.Poll != null ? JsonSerializer.Serialize(new { req.Poll.Question, req.Poll.Options, votes = new int[req.Poll.Options.Count] }) : null,
            LikeCount = 0,
            CommentCount = 0,
            ShareCount = 0
        };
        _db.Posts.Add(post);

        if (req.GroupId.HasValue)
        {
            var group = await _db.Groups.FindAsync(req.GroupId.Value);
            if (group != null) group.PostCount++;
        }

        await _db.SaveChangesAsync();
        _ = GamificationController.AwardXp(_db, userId, GamificationController.XpPost, "post");

        var user = await _db.Users.FindAsync(userId);
        return Ok(new
        {
            post.Id,
            Author = new { user!.Id, user.FullName, user.Headline, user.AvatarUrl },
            post.Body,
            post.LikeCount,
            post.CommentCount,
            post.ShareCount,
            Tags = req.Tags,
            post.CreatedAt
        });
    }

    // ===== COMMENT =====
    [HttpPost("comment/{postId}")]
    public async Task<IActionResult> AddComment(int postId, [FromBody] CommentRequest req)
    {
        var userId = GetUserId();
        var post = await _db.Posts.FindAsync(postId);
        if (post == null) return NotFound(new { error = "المنشور غير موجود" });

        var comment = new Comment
        {
            PostId = postId,
            UserId = userId,
            Body = req.Body
        };
        _db.Comments.Add(comment);
        post.CommentCount++;
        await _db.SaveChangesAsync();
        _ = GamificationController.AwardXp(_db, userId, GamificationController.XpComment, "comment");

        var user = await _db.Users.FindAsync(userId);
        return Ok(new { comment.Id, Author = user!.FullName, comment.Body, comment.CreatedAt });
    }

    // ===== GET COMMENTS =====
    [HttpGet("comments/{postId}")]
    public async Task<IActionResult> GetComments(int postId)
    {
        var comments = await _db.Comments
            .Include(c => c.User)
            .Where(c => c.PostId == postId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(20)
            .Select(c => new
            {
                c.Id,
                AuthorName = c.User.FullName,
                AuthorId = c.User.Id,
                c.Body,
                c.CreatedAt
            })
            .ToListAsync();
        return Ok(comments);
    }

    // ===== CONNECTION REQUEST =====
    [HttpPost("connect/{userId}")]
    public async Task<IActionResult> SendConnectionRequest(int userId)
    {
        var myId = GetUserId();
        if (userId == myId) return BadRequest(new { error = "لا يمكنك إرسال طلب لنفسك" });

        var existing = await _db.Connections.FirstOrDefaultAsync(c =>
            (c.RequesterId == myId && c.RecipientId == userId) ||
            (c.RequesterId == userId && c.RecipientId == myId));

        if (existing != null)
        {
            if (existing.Status == "accepted") return BadRequest(new { error = "أنتم متصلون بالفعل" });
            if (existing.Status == "pending") return BadRequest(new { error = "طلب معلق بالفعل" });
            // Re-send rejected
            existing.Status = "pending";
            existing.RequesterId = myId;
            existing.RecipientId = userId;
            await _db.SaveChangesAsync();
            return Ok(new { status = "pending" });
        }

        _db.Connections.Add(new Connection
        {
            RequesterId = myId,
            RecipientId = userId,
            Status = "pending",
            Circle = "professional"
        });
        await _db.SaveChangesAsync();
        return Ok(new { status = "pending" });
    }

    // ===== ACCEPT CONNECTION =====
    [HttpPost("accept/{connectionId}")]
    public async Task<IActionResult> AcceptConnection(int connectionId)
    {
        var myId = GetUserId();
        var conn = await _db.Connections.FindAsync(connectionId);
        if (conn == null) return NotFound(new { error = "الطلب غير موجود" });
        if (conn.RecipientId != myId) return Forbid();
        if (conn.Status != "pending") return BadRequest(new { error = "الطلب ليس معلقاً" });

        conn.Status = "accepted";
        conn.AcceptedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        // Award XP to both parties
        _ = GamificationController.AwardXp(_db, myId, GamificationController.XpConnect, "connect");
        _ = GamificationController.AwardXp(_db, conn.RequesterId, GamificationController.XpConnect, "connect");
        return Ok(new { status = "accepted" });
    }

    // ===== REJECT CONNECTION =====
    [HttpPost("reject/{connectionId}")]
    public async Task<IActionResult> RejectConnection(int connectionId)
    {
        var myId = GetUserId();
        var conn = await _db.Connections.FindAsync(connectionId);
        if (conn == null) return NotFound();
        if (conn.RecipientId != myId) return Forbid();

        _db.Connections.Remove(conn);
        await _db.SaveChangesAsync();
        return Ok(new { status = "rejected" });
    }

    // ===== JOIN GROUP =====
    [HttpPost("groups/join/{groupId}")]
    public async Task<IActionResult> JoinGroup(int groupId)
    {
        var userId = GetUserId();
        var group = await _db.Groups.FindAsync(groupId);
        if (group == null) return NotFound(new { error = "المجموعة غير موجودة" });

        var existing = await _db.GroupMembers.FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
        if (existing != null)
        {
            _db.GroupMembers.Remove(existing);
            group.MemberCount = Math.Max(0, group.MemberCount - 1);
            await _db.SaveChangesAsync();
            return Ok(new { isMember = false, memberCount = group.MemberCount });
        }

        _db.GroupMembers.Add(new GroupMember { GroupId = groupId, UserId = userId, Role = "member" });
        group.MemberCount++;
        await _db.SaveChangesAsync();
        _ = GamificationController.AwardXp(_db, userId, GamificationController.XpJoinGroup, "join_group");
        return Ok(new { isMember = true, memberCount = group.MemberCount });
    }


    // ===== REACT TO POST (like/love/insightful/awesome) =====
    [HttpPost("react/{postId}")]
    public async Task<IActionResult> ReactToPost(int postId, [FromBody] ReactRequest req)
    {
        var userId = GetUserId();
        var post = await _db.Posts.FindAsync(postId);
        if (post == null) return NotFound(new { error = "المنشور غير موجود" });

        var existing = await _db.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
        if (existing != null)
        {
            _db.Likes.Remove(existing);
            post.LikeCount = Math.Max(0, post.LikeCount - 1);
            await _db.SaveChangesAsync();
            return Ok(new { count = post.LikeCount, type = req.Type ?? "like", liked = false });
        }
        _db.Likes.Add(new Like { PostId = postId, UserId = userId });
        post.LikeCount++;
        await _db.SaveChangesAsync();
        _ = GamificationController.AwardXp(_db, userId, GamificationController.XpLike, req.Type ?? "like");
        return Ok(new { count = post.LikeCount, type = req.Type ?? "like", liked = true });
    }


    // ===== BOOKMARK POST =====
    [HttpPost("bookmark/{postId}")]
    public async Task<IActionResult> ToggleBookmark(int postId)
    {
        var userId = GetUserId();
        var existing = await _db.PostBookmarks.FirstOrDefaultAsync(b => b.PostId == postId && b.UserId == userId);
        var post = await _db.Posts.FindAsync(postId);
        if (existing != null)
        {
            _db.PostBookmarks.Remove(existing);
            if (post != null && post.SaveCount > 0) post.SaveCount--;
            await _db.SaveChangesAsync();
            return Ok(new { bookmarked = false, saveCount = post?.SaveCount ?? 0 });
        }
        _db.PostBookmarks.Add(new PostBookmark { PostId = postId, UserId = userId });
        if (post != null) post.SaveCount++;
        await _db.SaveChangesAsync();
        return Ok(new { bookmarked = true, saveCount = post?.SaveCount ?? 0 });
    }

    [HttpGet("bookmarks")]
    public async Task<IActionResult> GetBookmarks()
    {
        var userId = GetUserId();
        var bookmarks = await _db.PostBookmarks
            .Where(b => b.UserId == userId)
            .Select(b => b.PostId)
            .ToListAsync();
        return Ok(bookmarks);
    }


    // ===== VIEW POST =====
    [HttpPost("view/{postId}")]
    public async Task<IActionResult> ViewPost(int postId)
    {
        var post = await _db.Posts.FindAsync(postId);
        if (post == null) return NotFound();
        post.ViewCount++;
        await _db.SaveChangesAsync();
        return Ok(new { views = post.ViewCount });
    }

    // ===== SHARE POST =====
    [HttpPost("share/{postId}")]
    public async Task<IActionResult> SharePost(int postId)
    {
        var post = await _db.Posts.FindAsync(postId);
        if (post == null) return NotFound();
        post.ShareCount++;
        await _db.SaveChangesAsync();
        _ = GamificationController.AwardXp(_db, GetUserId(), GamificationController.XpShare, "share");
        return Ok(new { shareCount = post.ShareCount });
    }

    // ===== TRENDING HASHTAGS =====
    [AllowAnonymous]
    [HttpGet("trending")]
    public async Task<IActionResult> Trending()
    {
        var posts = await _db.Posts
            .Where(p => p.Tags != null && p.Tags != "" && p.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .Select(p => p.Tags)
            .ToListAsync();

        var tagCounts = new Dictionary<string, int>();
        foreach (var tagsJson in posts)
        {
            try
            {
                var tags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(tagsJson);
                if (tags != null)
                    foreach (var tag in tags)
                    {
                        var clean = tag.Trim().ToLower();
                        if (tagCounts.ContainsKey(clean)) tagCounts[clean]++;
                        else tagCounts[clean] = 1;
                    }
            }
            catch { }
        }

        var trending = tagCounts.OrderByDescending(kv => kv.Value).Take(10)
            .Select(kv => new { tag = kv.Key, count = kv.Value });
        return Ok(trending);
    }

    // ===== POLL VOTE =====
    [HttpPost("poll/{postId}/vote")]
    public async Task<IActionResult> VotePoll(int postId, [FromBody] PollVoteRequest req)
    {
        var post = await _db.Posts.FindAsync(postId);
        if (post == null || post.Poll == null) return NotFound();
        try
        {
            var poll = JsonSerializer.Deserialize<JsonElement>(post.Poll);
            var votes = poll.GetProperty("votes").EnumerateArray().Select(v => v.GetInt32()).ToArray();
            if (req.OptionIndex < 0 || req.OptionIndex >= votes.Length) return BadRequest(new { error = "خيار غير صالح" });
            votes[req.OptionIndex]++;
            var options = poll.GetProperty("options").EnumerateArray().Select(o => o.GetString()).ToList();
            var total = votes.Sum();
            var results = options.Select((o, i) => new { option = o, count = votes[i], percent = total > 0 ? Math.Round(100.0 * votes[i] / total, 1) : 0.0 }).ToList();
            var updated = new { question = poll.GetProperty("question").GetString(), options, votes, results };
            post.Poll = JsonSerializer.Serialize(updated);
            await _db.SaveChangesAsync();
            return Ok(new { results });
        }
        catch { return BadRequest(new { error = "خطأ في التصويت" }); }
    }

    // ===== EDIT POST =====
    [HttpPut("posts/{postId}")]
    public async Task<IActionResult> EditPost(int postId, [FromBody] UpdatePostRequest req)
    {
        var userId = GetUserId();
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId && p.UserId == userId);
        if (post == null) return NotFound();
        if (!string.IsNullOrWhiteSpace(req.Body)) post.Body = req.Body;
        if (req.Tags != null) post.Tags = JsonSerializer.Serialize(req.Tags);
        post.IsEdited = true;
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { ok = true, body = post.Body });
    }

    // ===== LINK PREVIEW =====
    [AllowAnonymous]
    [HttpGet("link-preview")]
    public async Task<IActionResult> LinkPreview([FromQuery] string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return Ok(new { });
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            http.DefaultRequestHeaders.Add("User-Agent", "LinkdoutBot/1.0");
            var html = await http.GetStringAsync(url);
            var title = System.Text.RegularExpressions.Regex.Match(html, @"<meta\s+property=""og:title""\s+content=""([^""]+)""", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Groups[1].Value;
            if (string.IsNullOrEmpty(title))
                title = System.Text.RegularExpressions.Regex.Match(html, @"<title>([^<]+)</title>", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Groups[1].Value;
            var desc = System.Text.RegularExpressions.Regex.Match(html, @"<meta\s+property=""og:description""\s+content=""([^""]+)""", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Groups[1].Value;
            var img = System.Text.RegularExpressions.Regex.Match(html, @"<meta\s+property=""og:image""\s+content=""([^""]+)""", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Groups[1].Value;
            return Ok(new { title = title?.Trim(), description = desc?.Trim(), image = img?.Trim() });
        }
        catch { return Ok(new { }); }
    }

    // ===== COMMENT REPLY =====
    [HttpPost("comment/{postId}/reply/{parentCommentId}")]
    public async Task<IActionResult> ReplyToComment(int postId, int parentCommentId, [FromBody] CommentRequest req)
    {
        var userId = GetUserId();
        var comment = new Comment
        {
            PostId = postId, UserId = userId, Body = req.Body, ParentCommentId = parentCommentId
        };
        _db.Comments.Add(comment);
        var post = await _db.Posts.FindAsync(postId);
        if (post != null) post.CommentCount++;
        await _db.SaveChangesAsync();
        var user = await _db.Users.FindAsync(userId);
        return Ok(new { id = comment.Id, body = comment.Body, author = user?.FullName, createdAt = comment.CreatedAt });
    }

    // ===== STORIES =====
    [AllowAnonymous]
    [HttpGet("stories")]
    public async Task<IActionResult> GetStories()
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var stories = await _db.Set<UserStory>().FromSqlRaw(
            "SELECT s.*, u.FullName, u.CoverColor FROM UserStories s JOIN Users u ON s.UserId = u.Id WHERE s.ExpiresAt > NOW() ORDER BY s.CreatedAt DESC LIMIT 15"
        ).ToListAsync();
        // Fallback: show recent active users
        if (stories.Count == 0)
        {
            var users = await _db.Users.OrderByDescending(u => u.LastActiveAt).Take(6)
                .Select(u => new { u.Id, u.FullName, u.CoverColor })
                .ToListAsync();
            return Ok(users.Select(u => new { id = u.Id, name = u.FullName, color = u.CoverColor ?? "#7C3AED" }));
        }
        return Ok(stories);
    }
}

public class UserStory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Body { get; set; }
    public string? ImageUrl { get; set; }
    public string? Color { get; set; }
    public string? FullName { get; set; }
    public string? CoverColor { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class PollVoteRequest { public int OptionIndex { get; set; } }
public class CreatePostRequest
{
    public string Body { get; set; } = "";
    public List<string>? Tags { get; set; }
    public List<string>? Images { get; set; }
    public int? GroupId { get; set; }
    public PollData? Poll { get; set; }
}

public class CommentRequest
{
    public string Body { get; set; } = "";
}


public class ReactRequest
{
    public string? Type { get; set; }
}
