using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BookQuotes.Api.Contracts;
using BookQuotes.Api.Data;
using BookQuotes.Api.Models;
namespace BookQuotes.Api.Controllers;

[ApiController, Authorize, Route("api/books")]
public class BooksController(AppDbContext database) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Book>>> GetBooks() =>
        Ok(await database.Books.AsNoTracking().OrderByDescending(b => b.Id).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        var book = await database.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        return book is null ? NotFound() : Ok(book);
    }
    [HttpPost]
    public async Task<ActionResult<Book>> CreateBook(CreateBookRequest request)
    {
        var book = new Book { Title = request.Title.Trim(), Author = request.Author.Trim(), PublicationDate = request.PublicationDate!.Value };
        database.Books.Add(book); await database.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBook(int id, CreateBookRequest request)
    {
        var book = await database.Books.FindAsync(id);
        if (book is null) return NotFound();
        book.Title = request.Title.Trim(); book.Author = request.Author.Trim();
        book.PublicationDate = request.PublicationDate!.Value;
        await database.SaveChangesAsync(); return NoContent();
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await database.Books.FindAsync(id);
        if (book is null) return NotFound();
        database.Books.Remove(book); await database.SaveChangesAsync(); return NoContent();
    }
}
