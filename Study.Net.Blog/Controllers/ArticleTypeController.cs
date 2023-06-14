using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Study.Net.IBaseService;
using Study.Net.Model;
using Study.Net.Model.DTO;
using Study.Net.Utility;

namespace Study.Net.Blog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleTypeController : ControllerBase
    {
        private readonly IArticleTypeService _articleTypeService;
        private readonly IMapper _mapper;

        public ArticleTypeController(IArticleTypeService articleTypeService,IMapper mapper)
        {
            _mapper = mapper;
            _articleTypeService = articleTypeService;
        }

        [HttpGet("GetTypes")]
        public async Task<ActionResult<ApiResult>> GetTypes()
        {
            var data = await _articleTypeService.FindAllAsync();
            if (data.Count == 0)
            {
                return ApiResultHelper.Error("没有更多的文章类型");
            }

            List<ArticleTypeDTO> articleTypeDTOs = new List<ArticleTypeDTO>();

            foreach (var articleType in data)
            {
                articleTypeDTOs.Add(_mapper.Map<ArticleTypeDTO>(articleType));
            }

            return ApiResultHelper.Success(articleTypeDTOs);
        }

        [HttpPost("Create")]
        public async Task<ActionResult<ApiResult>> Create(string typeName)
        {
            ArticleType type = new ArticleType()
            {
                TypeName = typeName,
                IsDeleted = false
            };
            var result = await _articleTypeService.CreateAsync(type);
            if (!result)
            {
                return ApiResultHelper.Error($"{typeName}类型创建失败");
            }
            return ApiResultHelper.Success(result);
        }

        [HttpDelete("Deleted")]
        public async Task<ActionResult<ApiResult>> Delete(Guid id)
        {
            var data = await _articleTypeService.FindOneAsync(id);
            if (data == null)
            {
                return ApiResultHelper.Error("删除失败");
            }
            data.IsDeleted = true;
            var result = await _articleTypeService.DeleteAsync(data);
            if (!result)
            {
                return ApiResultHelper.Error("删除失败");
            }
            return ApiResultHelper.Success(result);
        }

        [HttpPut("Edit")]
        public async Task<ActionResult<ApiResult>> Edit(Guid id, string TypeName)
        {
            var data = await _articleTypeService.FindOneAsync(id);
            if (data is null)
            {
                return ApiResultHelper.Error("修改失败");
            }

            data.TypeName = TypeName;

            var result = await _articleTypeService.UpdateAsync(data);
            if (!result)
            {
                return ApiResultHelper.Error("修改失败");
            }
            return ApiResultHelper.Success(result);
        }
    }
}
