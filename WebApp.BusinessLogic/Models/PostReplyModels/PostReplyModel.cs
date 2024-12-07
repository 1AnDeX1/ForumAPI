namespace WebApp.BusinessLogic.Models.PostReplyModels;
public class PostReplyModel
{
    public int Id { get; set; }

    public string? Content { get; set; }

    public DateTime Created { get; set; }

    public string? UserName { get; set; }

    public int PostId { get; set; }
}
