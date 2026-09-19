using System.ComponentModel.DataAnnotations;

namespace BookQuotes.Api.Contracts;

public class CreateBookRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Author { get; set; } = string.Empty;

    [Required]
    public DateOnly? PublicationDate { get; set; }
}
