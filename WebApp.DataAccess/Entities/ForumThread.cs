namespace WebApp.DataAccess.Entities;
public class ForumThread
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public string? UserId { get; set; }

    public ApplicationUser? User { get; set; }

    public IEnumerable<Post>? Posts { get; set; }
}
