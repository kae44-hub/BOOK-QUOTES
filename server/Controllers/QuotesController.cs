using System.Security.Claims;
using BookQuotes.Api.Contracts;
using BookQuotes.Api.Data;
using BookQuotes.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BookQuotes.Api.Controllers;

[ApiController, Authorize, Route("api/quotes")]
public class QuotesController(AppDbContext db) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue("sub")!);
    private static object View(Quote q) => new { q.Id, q.Text, q.Author };
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await db.Quotes.AsNoTracking()
        .Where(q => q.UserId == UserId).OrderByDescending(q => q.Id)
        .Select(q => new { q.Id, q.Text, q.Author }).ToListAsync());
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var quote = await db.Quotes.AsNoTracking().SingleOrDefaultAsync(q => q.Id == id && q.UserId == UserId);
        return quote is null ? NotFound() : Ok(View(quote));
    }
    [HttpPost]
    public async Task<IActionResult> Create(QuoteRequest request)
    {
        var quote = new Quote { Text = request.Text.Trim(), Author = request.Author.Trim(), UserId = UserId };
        db.Quotes.Add(quote); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = quote.Id }, View(quote));
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, QuoteRequest request)
    {
        var quote = await db.Quotes.SingleOrDefaultAsync(q => q.Id == id && q.UserId == UserId);
        if (quote is null) return NotFound();
        quote.Text = request.Text.Trim(); quote.Author = request.Author.Trim();
        await db.SaveChangesAsync(); return NoContent();
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var quote = await db.Quotes.SingleOrDefaultAsync(q => q.Id == id && q.UserId == UserId);
        if (quote is null) return NotFound();
        db.Quotes.Remove(quote); await db.SaveChangesAsync(); return NoContent();
    }
}
