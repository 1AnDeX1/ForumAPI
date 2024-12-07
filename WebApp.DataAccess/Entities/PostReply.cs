namespace WebApp.DataAccess.Entities;
public class PostReply
{
    public int Id { get; set; }

    public string? Content { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public string? UserId { get; set; }

    public int PostId { get; set; }

    public ApplicationUser? User { get; set; }

    public Post? Post { get; set; }
}
