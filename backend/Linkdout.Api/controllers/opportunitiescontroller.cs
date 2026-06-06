using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Linkdout.Api.Data;
using Linkdout.Api.DTOs;

namespace Linkdout.Api.Controllers;

[Authorize]
public class OpportunitiesController : Controller
{
    private readonly AppDbContext _db;

    public OpportunitiesController(AppDbContext db) => _db = db;

    [HttpGet("/Opportunities")]
    public async Task<IActionResult> Index()
    {
        var jobs = await _db.Jobs
            .Include(j => j.Company)
            .Where(j => j.IsActive)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new JobDto
            {
                Id = j.Id,
                CompanyId = j.CompanyId,
                CompanyName = j.Company.Name,
                Title = j.Title,
                Type = j.Type,
                Location = j.Location,
                Description = j.Description,
                
                CreatedAt = j.CreatedAt
            })
            .ToListAsync();

        ViewBag.Jobs = jobs;

        return View();
    }
}
