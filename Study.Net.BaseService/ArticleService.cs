using Study.Net.IBaseRepository;
using Study.Net.IBaseService;
using Study.Net.Model;

namespace Study.Net.BaseService;

public class ArticleService : BaseService<Article>, IArticleService
{
    private readonly IArticleRepository articleRepository;

    public ArticleService(IArticleRepository articleRepository)
    {
        base._repository = articleRepository;
        this.articleRepository = articleRepository;
    }
}
