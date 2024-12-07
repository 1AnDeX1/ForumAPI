namespace WebApp.BusinessLogic.Models.ThreadModels;
public class ForumThreadModel
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public DateTime Created { get; set; }

    public string? UserName { get; set; }
}
