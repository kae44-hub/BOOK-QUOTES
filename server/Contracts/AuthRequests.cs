using System.ComponentModel.DataAnnotations;
namespace BookQuotes.Api.Contracts;
public class Credentials
{
    [Required, StringLength(30, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Use letters, numbers and underscores only.")]
    public string Username { get; set; } = "";
    [Required, StringLength(128, MinimumLength = 10)]
    public string Password { get; set; } = "";
}
public class QuoteRequest
{
    [Required, StringLength(1000)]
    public string Text { get; set; } = "";
    [Required, StringLength(200)]
    public string Author { get; set; } = "";
}
