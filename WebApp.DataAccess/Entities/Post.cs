namespace WebApp.DataAccess.Entities;
public class Post
{
    public int Id { get; set; }

    public string? Content { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public int ThreadId { get; set; }

    public string? UserId { get; set; }

    public ForumThread? Thread { get; set; }

    public ApplicationUser? User { get; set; }
}
