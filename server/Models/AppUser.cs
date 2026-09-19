namespace BookQuotes.Api.Models;
public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = "";
    public string NormalizedUsername { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class Quote
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public string Author { get; set; } = "";
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
}
public class UserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}
