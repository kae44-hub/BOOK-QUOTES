using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using BookQuotes.Api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace BookQuotes.Tests;

public sealed class ApiTests : IDisposable
{
    private readonly ApiFactory factory = new();
    private const string Password = "Test-only_password_2026!";
    private HttpClient Client() => factory.CreateClient(new WebApplicationFactoryClientOptions
    { BaseAddress = new Uri("http://localhost"), AllowAutoRedirect = false });

    private static async Task Csrf(HttpClient client)
    {
        var response = await client.GetAsync("/api/auth/csrf");
        response.EnsureSuccessStatusCode();
        var cookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("XSRF-TOKEN="));
        var token = Uri.UnescapeDataString(cookie.Split(';')[0]["XSRF-TOKEN=".Length..]);
        client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", token);
    }
    private static async Task<string> Login(HttpClient client, string username)
    {
        await Csrf(client);
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password = Password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var accessCookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("folio.access="));
        Assert.Contains("httponly", accessCookie.ToLowerInvariant());
        Assert.Contains("samesite=strict", accessCookie.ToLowerInvariant());
        await Csrf(client);
        return accessCookie.Split(';')[0]["folio.access=".Length..];
    }
    private static async Task<string> RegisterAndLogin(HttpClient client, string username)
    {
        await Csrf(client);
        var response = await client.PostAsJsonAsync("/api/auth/register", new { username, password = Password });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await Login(client, username);
    }

    [Theory]
    [InlineData("/api/books")]
    [InlineData("/api/quotes")]
    [InlineData("/api/auth/me")]
    public async Task Anonymous_requests_are_rejected(string path)
    {
        using var client = Client();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(path)).StatusCode);
    }

    [Fact]
    public async Task Registration_hashes_password_seeds_five_quotes_and_rejects_duplicate()
    {
        using var client = Client();
        await RegisterAndLogin(client, "reader");
        var quotes = await client.GetFromJsonAsync<JsonElement[]>("/api/quotes");
        Assert.Equal(5, quotes!.Length);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync();
        Assert.NotEqual(Password, user.PasswordHash);
        Assert.True(user.PasswordHash.Length > 40);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/auth/register",
            new { username = "READER", password = Password })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/auth/login",
            new { username = "reader", password = "incorrect_password" })).StatusCode);
    }

    [Fact]
    public async Task Book_crud_persists_across_sessions_and_validates_input()
    {
        using var client = Client();
        await RegisterAndLogin(client, "books_reader");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/books",
            new { title = " ", author = " ", publicationDate = "not-a-date" })).StatusCode);
        var created = await client.PostAsJsonAsync("/api/books",
            new { title = "The Hobbit", author = "J. R. R. Tolkien", publicationDate = "1937-09-21" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var book = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = book.GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync($"/api/books/{id}",
            new { title = "Edited title", author = "An author", publicationDate = "2000-02-29" })).StatusCode);
        using var second = Client();
        await Login(second, "books_reader");
        var persisted = await second.GetFromJsonAsync<JsonElement>($"/api/books/{id}");
        Assert.Equal("Edited title", persisted.GetProperty("title").GetString());
        Assert.Equal("2000-02-29", persisted.GetProperty("publicationDate").GetString());
        Assert.Equal(HttpStatusCode.NoContent, (await second.DeleteAsync($"/api/books/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/books/{id}")).StatusCode);
    }

    [Fact]
    public async Task Quotes_are_private_and_support_full_crud()
    {
        using var alice = Client();
        using var bob = Client();
        await RegisterAndLogin(alice, "alice");
        await RegisterAndLogin(bob, "bob");
        var created = await alice.PostAsJsonAsync("/api/quotes", new { text = "Private words", author = "Alice" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var quote = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = quote.GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.NotFound, (await bob.GetAsync($"/api/quotes/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.PutAsJsonAsync($"/api/quotes/{id}",
            new { text = "stolen", author = "Bob" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.DeleteAsync($"/api/quotes/{id}")).StatusCode);
        var bobQuotes = await bob.GetFromJsonAsync<JsonElement[]>("/api/quotes");
        Assert.DoesNotContain(bobQuotes!, q => q.GetProperty("id").GetInt32() == id);
        Assert.Equal(HttpStatusCode.NoContent, (await alice.PutAsJsonAsync($"/api/quotes/{id}",
            new { text = "Revised words", author = "Alice" })).StatusCode);
        var updated = await alice.GetFromJsonAsync<JsonElement>($"/api/quotes/{id}");
        Assert.Equal("Revised words", updated.GetProperty("text").GetString());
        Assert.Equal(HttpStatusCode.NoContent, (await alice.DeleteAsync($"/api/quotes/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await alice.GetAsync($"/api/quotes/{id}")).StatusCode);
    }

    [Fact]
    public async Task Mutation_without_csrf_is_rejected()
    {
        using var client = Client();
        await RegisterAndLogin(client, "csrf_reader");
        client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/books",
            new { title = "Untrusted", author = "Author", publicationDate = "2020-01-01" })).StatusCode);
    }

    [Fact]
    public async Task Logout_revokes_even_a_copied_token()
    {
        using var client = Client();
        var token = await RegisterAndLogin(client, "logout_reader");
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/api/auth/logout", new { })).StatusCode);
        using var replay = Client();
        replay.DefaultRequestHeaders.Add("Cookie", "folio.access=" + token);
        Assert.Equal(HttpStatusCode.Unauthorized, (await replay.GetAsync("/api/books")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Expired_or_incorrectly_signed_tokens_are_rejected(bool expired)
    {
        using var client = Client();
        var valid = new JwtSecurityTokenHandler().ReadJwtToken(await RegisterAndLogin(client, "jwt_reader"));
        var key = expired ? ApiFactory.SigningKey : "wrong-signing-key-at-least-thirty-two-characters-long";
        var token = new JwtSecurityToken("folio-api", "folio-client", valid.Claims,
            notBefore: DateTime.UtcNow.AddHours(-2),
            expires: expired ? DateTime.UtcNow.AddMinutes(-2) : DateTime.UtcNow.AddMinutes(30),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256));
        using var bad = Client();
        bad.DefaultRequestHeaders.Add("Cookie", "folio.access=" + new JwtSecurityTokenHandler().WriteToken(token));
        Assert.Equal(HttpStatusCode.Unauthorized, (await bad.GetAsync("/api/books")).StatusCode);
    }
    [Fact]
    public async Task Render_https_proxy_allows_secure_antiforgery_cookie()
    {
        using var production = new ApiFactory("Production", render: true);
        using var client = production.CreateClient(new WebApplicationFactoryClientOptions
        { BaseAddress = new Uri("http://localhost"), AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Forwarded-Proto", "https");
        var response = await client.GetAsync("/api/auth/csrf");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("folio.csrf="));
        Assert.Contains("secure", cookie.ToLowerInvariant());
        Assert.Contains("httponly", cookie.ToLowerInvariant());
    }

    public void Dispose() => factory.Dispose();
}

