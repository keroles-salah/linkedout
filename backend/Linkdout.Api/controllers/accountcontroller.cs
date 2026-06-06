using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Linkdout.Api.Data;
using Linkdout.Api.DTOs;
using Linkdout.Api.Models;

namespace Linkdout.Api.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet("/Account/Login")]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        return View();
    }

    [AllowAnonymous]
    [HttpPost("/Account/Login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
        {
            ViewBag.Error = "برجاء إدخال البريد الإلكتروني وكلمة المرور";
            return View();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        {
            ViewBag.Error = "البريد الإلكتروني أو كلمة المرور غير صحيحة";
            return View();
        }

        await SignInUser(user);
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpPost("/Account/Register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
        {
            ViewBag.Error = "برجاء إدخال جميع البيانات المطلوبة";
            ViewBag.ActiveTab = "register";
            return View("Login");
        }

        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
        {
            ViewBag.Error = "البريد الإلكتروني مسجل بالفعل — جرب تسجيل الدخول";
            ViewBag.ActiveTab = "register";
            return View("Login");
        }

        var user = new User
        {
            FullName = req.FullName,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Status = "open",
            Skills = JsonSerializer.Serialize(new List<string>())
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await SignInUser(user);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("/Account/Logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    private async Task SignInUser(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}
