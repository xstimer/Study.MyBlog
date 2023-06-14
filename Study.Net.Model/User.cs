using Microsoft.AspNetCore.Identity;

namespace Study.Net.Model;

public class User:IdentityUser<Guid>
{
    public List<Article> Articles { get; set; } = new List<Article>();

    public bool isDeleted { get; set; }
}
