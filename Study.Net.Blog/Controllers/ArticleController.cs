using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Study.Net.BaseService;
using Study.Net.IBaseService;
using Study.Net.Model;
using Study.Net.Model.DTO;
using Study.Net.Utility;

namespace Study.Net.Blog.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly IMapper _mapper;
        public ArticleController(IArticleService articleService,IMapper mapper)
        {
            _mapper = mapper;
            _articleService = articleService;
        }

        [HttpGet("Articles")]
        public async Task<ActionResult<ApiResult>> GetArticles()
        {
            var data = await _articleService.FindAllAsync();
            if (data.Count == 0)
            {
                return ApiResultHelper.Error("没有更多文章");
            }

            List<ArticleDTO>articleDTOs = new List<ArticleDTO>();
            foreach (var article in data)
            {
                articleDTOs.Add(_mapper.Map<ArticleDTO>(article));
            }
            return ApiResultHelper.Success(articleDTOs);
        }

        [HttpPost("Create")]
        public async Task<ActionResult<ApiResult>> CreateArticle(string title, string content, Guid TypeId)
        {
            Article article = new Article()
            {
                Title = title,
                Content = content,
                TypeId = TypeId,
                IsDeleted = false,
                ViewCount = 0,
                LikeCount = 0
            };
            var result = await _articleService.CreateAsync(article);
            if (!result)
            {
                return ApiResultHelper.Error($"{title}添加失败！");
            }
            return ApiResultHelper.Success(result);
        }
        [HttpDelete("Delete")]
        public async Task<ActionResult<ApiResult>> Delete(Guid id)
        {
            Article article = await _articleService.FindOneAsync(id);
            if(article == null)
            {
                return ApiResultHelper.Error("删除失败");
            }

            article.IsDeleted = true;
            bool result = await _articleService.DeleteAsync(article);
            if (!result)
            {
                return ApiResultHelper.Error("删除失败");
            }
            return ApiResultHelper.Success(result);
        }

        [HttpPut("Edit")]
        public async Task<ActionResult<ApiResult>>Edit(Guid id,string title,string content,Guid typeid)
        {
            var article = await _articleService.FindOneAsync(id);
            if(article is null)
            {
                return ApiResultHelper.Error("所修改的文章不存在");
            }
            article.Title = title;
            article.Content = content;
            article.TypeId = typeid;
            var result = await _articleService.UpdateAsync(article);

            if(!result)
            {
                return ApiResultHelper.Error("修改失败");
            }
            return ApiResultHelper.Success(result);
        }
    }

   

}
