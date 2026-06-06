using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Linkdout.Api.Data;
using Linkdout.Api.DTOs;

namespace Linkdout.Api.Controllers;

[Authorize]
public class CompaniesController : Controller
{
    private readonly AppDbContext _db;

    public CompaniesController(AppDbContext db) => _db = db;

    [HttpGet("/Companies")]
    public async Task<IActionResult> Index()
    {
        var companies = await _db.Companies
            .Include(c => c.Jobs)
            .OrderByDescending(c => c.FollowerCount)
            .ToListAsync();

        ViewBag.Companies = companies.Select(c => new CompanyDto
        {
            Id = c.Id,
            Name = c.Name,
            Industry = c.Industry,
            Description = c.Description,
            Location = c.Location,
            Size = c.Size,
            Website = c.Website,
            LogoUrl = c.LogoUrl,
            CoverColor = c.CoverColor,
            FollowerCount = c.FollowerCount,
            JobCount = c.Jobs.Count(j => j.IsActive)
        }).ToList();

        return View();
    }

    [HttpGet("/Companies/{id}")]
    public async Task<IActionResult> Detail(int id)
    {
        var company = await _db.Companies
            .Include(c => c.Jobs)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null) return NotFound();

        ViewBag.Company = new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Industry = company.Industry,
            Description = company.Description,
            Location = company.Location,
            Size = company.Size,
            Website = company.Website,
            LogoUrl = company.LogoUrl,
            CoverColor = company.CoverColor,
            FollowerCount = company.FollowerCount,
            JobCount = company.Jobs.Count(j => j.IsActive)
        };

        ViewBag.Jobs = company.Jobs
            .Where(j => j.IsActive)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new JobDto
            {
                Id = j.Id,
                CompanyId = j.CompanyId,
                CompanyName = company.Name,
                Title = j.Title,
                Type = j.Type,
                Location = j.Location,
                Description = j.Description,
                RequiredSkills = string.IsNullOrEmpty(j.RequiredSkills) ? null
                    : System.Text.Json.JsonSerializer.Deserialize<List<string>>(j.RequiredSkills),
                CreatedAt = j.CreatedAt
            }).ToList();

        return View();
    }
}
