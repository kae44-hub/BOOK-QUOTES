using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookQuotes.Api.Contracts;
using BookQuotes.Api.Data;
using BookQuotes.Api.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
namespace BookQuotes.Api.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(AppDbContext db, IPasswordHasher<AppUser> hasher,
    IConfiguration configuration, IWebHostEnvironment environment) : ControllerBase
{
    private CookieOptions AccessCookie(DateTimeOffset? expires = null) => new()
    {
        HttpOnly = true,
        Secure = !environment.IsDevelopment(),
        SameSite = SameSiteMode.Strict,
        Path = "/",
        Expires = expires
    };
    [HttpGet("csrf")]
    public IActionResult Csrf([FromServices] IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
        { HttpOnly = false, Secure = !environment.IsDevelopment(), SameSite = SameSiteMode.Strict, Path = "/" });
        return Ok(new { ready = true });
    }
    [HttpPost("register"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(Credentials request)
    {
        var normalized = request.Username.ToUpperInvariant();
        if (await db.Users.AnyAsync(u => u.NormalizedUsername == normalized))
            return Conflict(new { message = "That username is already in use." });
        var user = new AppUser { Username = request.Username, NormalizedUsername = normalized };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        string[] starters = ["Make room for a little reading every day.",
            "A good question can be as valuable as a good answer.",
            "Keep the words that make you pause.", "Small steps are still steps forward.",
            "There is always another page to turn."];
        db.Quotes.AddRange(starters.Select(text => new Quote { Text = text, Author = "Folio notes", UserId = user.Id }));
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return Conflict(new { message = "That username is already in use." }); }
        return StatusCode(201, new { message = "Account created. Sign in to your library." });
    }
    [HttpPost("login"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(Credentials request)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.NormalizedUsername == request.Username.ToUpperInvariant());
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Username or password is incorrect." });
        await db.Sessions.Where(s => s.ExpiresAt <= DateTime.UtcNow).ExecuteDeleteAsync();
        var session = new UserSession { UserId = user.Id, ExpiresAt = DateTime.UtcNow.AddHours(1) };
        db.Sessions.Add(session); await db.SaveChangesAsync();
        var token = new JwtSecurityToken("folio-api", "folio-client",
            [new Claim("sub", user.Id.ToString()), new Claim("name", user.Username), new Claim("jti", session.Id.ToString())],
            expires: session.ExpiresAt,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)), SecurityAlgorithms.HmacSha256));
        Response.Cookies.Append("folio.access", new JwtSecurityTokenHandler().WriteToken(token), AccessCookie(session.ExpiresAt));
        return Ok(new { user.Id, user.Username });
    }
    [Authorize, HttpGet("me")]
    public IActionResult Me() => Ok(new { id = User.FindFirstValue("sub"), username = User.Identity!.Name });
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (Guid.TryParse(User.FindFirstValue("jti"), out var id))
            await db.Sessions.Where(s => s.Id == id).ExecuteDeleteAsync();
        Response.Cookies.Delete("folio.access", AccessCookie());
        return NoContent();
    }
}
