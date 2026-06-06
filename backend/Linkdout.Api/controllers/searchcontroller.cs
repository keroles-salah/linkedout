using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Linkdout.Api.Data;
using Linkdout.Api.DTOs;

namespace Linkdout.Api.Controllers;

[Authorize]
public class SearchController : Controller
{
    private readonly AppDbContext _db;

    public SearchController(AppDbContext db) => _db = db;

    [HttpGet("/Search")]
    public async Task<IActionResult> Index([FromQuery] string? q)
    {
        ViewBag.Query = q;

        if (string.IsNullOrWhiteSpace(q))
        {
            ViewBag.People = new List<UserDto>();
            ViewBag.Jobs = new List<JobDto>();
            ViewBag.Groups = new List<GroupDto>();
            ViewBag.Companies = new List<CompanyDto>();
            return View();
        }

        var query = q.Trim().ToLower();

        // Search people
        var people = await _db.Users
            .Where(u => u.FullName.ToLower().Contains(query)
                        || (u.Headline != null && u.Headline.ToLower().Contains(query))
                        || (u.Bio != null && u.Bio.ToLower().Contains(query))
                        || (u.Skills != null && u.Skills.ToLower().Contains(query)))
            .Take(10)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Headline = u.Headline,
                Bio = u.Bio,
                Location = u.Location,
                AvatarUrl = u.AvatarUrl,
                Status = u.Status,
                
                ProfileViews = u.ProfileViews,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        ViewBag.People = people;

        // Search jobs
        var jobs = await _db.Jobs
            .Include(j => j.Company)
            .Where(j => j.IsActive && (j.Title.ToLower().Contains(query)
                        || (j.Description != null && j.Description.ToLower().Contains(query))
                        || (j.RequiredSkills != null && j.RequiredSkills.ToLower().Contains(query))))
            .Take(10)
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

        // Search groups
        var groups = await _db.Groups
            .Where(g => g.Name.ToLower().Contains(query)
                        || (g.Description != null && g.Description.ToLower().Contains(query)))
            .Take(8)
            .Select(g => new GroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                CoverColor = g.CoverColor,
                Icon = g.Icon,
                Privacy = g.Privacy,
                MemberCount = g.MemberCount,
                PostCount = g.PostCount,
                CreatedAt = g.CreatedAt
            })
            .ToListAsync();

        ViewBag.Groups = groups;

        // Search companies
        var companies = await _db.Companies
            .Where(c => c.Name.ToLower().Contains(query)
                        || (c.Industry != null && c.Industry.ToLower().Contains(query))
                        || (c.Description != null && c.Description.ToLower().Contains(query)))
            .Take(8)
            .Select(c => new CompanyDto
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
                FollowerCount = c.FollowerCount
            })
            .ToListAsync();

        ViewBag.Companies = companies;
        ViewBag.HasResults = people.Any() || jobs.Any() || groups.Any() || companies.Any();

        return View();
    }

    [HttpGet("/api/search/autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2) return Ok(new List<object>());

        var qLower = q.ToLower().Trim();
        var results = new List<object>();

        var users = await _db.Users
            .Where(u => u.FullName.ToLower().Contains(qLower) || (u.Headline != null && u.Headline.ToLower().Contains(qLower)))
            .Take(3)
            .Select(u => new { icon = "👤", name = u.FullName, type = "شخص", url = "/Profile/" + u.Id })
            .ToListAsync();
        results.AddRange(users);

        var jobs = await _db.Jobs
            .Where(j => j.Title.ToLower().Contains(qLower) && j.IsActive)
            .Take(2)
            .Select(j => new { icon = "💼", name = j.Title, type = "وظيفة", url = "/Opportunities" })
            .ToListAsync();
        results.AddRange(jobs);

        var groups = await _db.Groups
            .Where(g => g.Name.ToLower().Contains(qLower))
            .Take(2)
            .Select(g => new { icon = g.Icon ?? "👥", name = g.Name, type = "مجموعة", url = "/Groups/" + g.Id })
            .ToListAsync();
        results.AddRange(groups);

        return Ok(results);
    }

}