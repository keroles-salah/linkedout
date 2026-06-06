using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Linkdout.Api.Data;
using Linkdout.Api.DTOs;

namespace Linkdout.Api.Controllers;

[Authorize]
public class CirclesController : Controller
{
    private readonly AppDbContext _db;

    public CirclesController(AppDbContext db) => _db = db;

    [HttpGet("/Circles")]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();

        var connections = await _db.Connections
            .Include(c => c.Requester)
            .Include(c => c.Recipient)
            .Where(c => (c.RequesterId == userId || c.RecipientId == userId) && c.Status == "accepted")
            .ToListAsync();

        var grouped = connections
            .GroupBy(c => c.Circle ?? "بدون دائرة")
            .Select(g => new
            {
                Circle = g.Key,
                Connections = g.Select(c =>
                {
                    var otherUser = c.RequesterId == userId ? c.Recipient : c.Requester;
                    return new ConnectionDto
                    {
                        Id = c.Id,
                        User = new UserBriefDto
                        {
                            Id = otherUser.Id,
                            FullName = otherUser.FullName,
                            Headline = otherUser.Headline,
                            AvatarUrl = otherUser.AvatarUrl
                        },
                        Status = c.Status,
                        Circle = c.Circle,
                        CreatedAt = c.CreatedAt
                    };
                }).ToList()
            })
            .ToList();

        ViewBag.Circles = grouped;
        ViewBag.TotalConnections = connections.Count;

        return View();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
