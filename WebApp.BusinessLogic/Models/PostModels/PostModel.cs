namespace WebApp.BusinessLogic.Models.PostModels;
public class PostModel
{
    public int Id { get; set; }

    public string? Content { get; set; }

    public DateTime Created { get; set; }

    public string? UserName { get; set; }

    public int ThreadId { get; set; }
}
