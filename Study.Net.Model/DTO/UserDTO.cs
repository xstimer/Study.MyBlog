namespace Study.Net.Model.DTO;

public class UserDTO
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public List<string> ArticleNames { get; set; } = new List<string>();

}
