using Microsoft.AspNetCore.Mvc;
using BookQuotes.Api.Models;

namespace BookQuotes.Api.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Book>> GetBooks()
    {
        var books = new List<Book>
        {
            new Book
            {
                Id = 1,
                Title = "Pride and Prejudice",
                Author = "Jane Austen",
                PublicationDate = new DateOnly(1813, 1, 28)
            },
            new Book
            {
                Id = 2,
                Title = "The Hobbit",
                Author = "J. R. R. Tolkien",
                PublicationDate = new DateOnly(1937, 9, 21)
            },
            new Book
            {
                Id = 3,
                Title = "Nineteen Eighty-Four",
                Author = "George Orwell",
                PublicationDate = new DateOnly(1949, 6, 8)
            }
        };

        return Ok(books);
    }
}