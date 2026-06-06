using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Linkdout.Api.Data;
using Linkdout.Api.DTOs;
using Linkdout.Api.Models;

namespace Linkdout.Api.Controllers;

[ApiController, Route("api/connections")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ConnectionsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ConnectionsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetConnections([FromQuery] string? circle)
    {
        var userId = GetUserId();
        var query = _db.Connections
            .Include(c => c.Requester).Include(c => c.Recipient)
            .Where(c => (c.RequesterId == userId || c.RecipientId == userId) && c.Status == "accepted");

        if (!string.IsNullOrEmpty(circle))
            query = query.Where(c => c.Circle == circle);

        var connections = await query.Select(c => new ConnectionDto
        {
            Id = c.Id,
            Status = c.Status,
            Circle = c.Circle,
            CreatedAt = c.CreatedAt,
            User = c.RequesterId == userId
                ? new UserBriefDto { Id = c.Recipient.Id, FullName = c.Recipient.FullName, Headline = c.Recipient.Headline, AvatarUrl = c.Recipient.AvatarUrl }
                : new UserBriefDto { Id = c.Requester.Id, FullName = c.Requester.FullName, Headline = c.Requester.Headline, AvatarUrl = c.Requester.AvatarUrl }
        }).ToListAsync();

        return Ok(connections);
    }

    [HttpPost("request/{recipientId}")]
    public async Task<IActionResult> SendRequest(int recipientId)
    {
        var userId = GetUserId();
        if (userId == recipientId) return BadRequest(new { message = "لا يمكنك إرسال طلب لنفسك" });

        var existing = await _db.Connections.FirstOrDefaultAsync(c =>
            (c.RequesterId == userId && c.RecipientId == recipientId) ||
            (c.RequesterId == recipientId && c.RecipientId == userId));

        if (existing != null)
            return BadRequest(new { message = "طلب التواصل موجود بالفعل" });

        _db.Connections.Add(new Connection { RequesterId = userId, RecipientId = recipientId });
        await _db.SaveChangesAsync();
        return Ok(new { message = "تم إرسال طلب التواصل" });
    }

    [HttpPut("{id}/accept")]
    public async Task<IActionResult> Accept(int id)
    {
        var conn = await _db.Connections.FindAsync(id);
        if (conn == null || conn.RecipientId != GetUserId())
            return NotFound(new { message = "الطلب غير موجود" });

        conn.Status = "accepted";
        conn.AcceptedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "تم قبول طلب التواصل" });
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        var conn = await _db.Connections.FindAsync(id);
        if (conn == null || conn.RecipientId != GetUserId())
            return NotFound();

        conn.Status = "rejected";
        await _db.SaveChangesAsync();
        return Ok(new { message = "تم رفض الطلب" });
    }

    [HttpPut("{id}/circle")]
    public async Task<IActionResult> SetCircle(int id, [FromBody] string circle)
    {
        var conn = await _db.Connections.FindAsync(id);
        if (conn == null) return NotFound();
        conn.Circle = circle;
        await _db.SaveChangesAsync();
        return Ok(new { message = "تم تحديث الدائرة" });
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var userId = GetUserId();
        var pending = await _db.Connections
            .Include(c => c.Requester)
            .Where(c => c.RecipientId == userId && c.Status == "pending")
            .Select(c => new ConnectionDto
            {
                Id = c.Id, Status = c.Status, CreatedAt = c.CreatedAt,
                User = new UserBriefDto { Id = c.Requester.Id, FullName = c.Requester.FullName, Headline = c.Requester.Headline, AvatarUrl = c.Requester.AvatarUrl }
            }).ToListAsync();

        return Ok(pending);
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
