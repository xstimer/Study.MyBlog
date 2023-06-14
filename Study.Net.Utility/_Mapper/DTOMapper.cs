using AutoMapper;
using Study.Net.Model;
using Study.Net.Model.DTO;

namespace Study.Net.Utility._Mapper;

public class DTOMapper : Profile
{
    public DTOMapper()
    {
        base.CreateMap<Article, ArticleDTO>().ForMember(x => x.TypeName, opt =>
        {
            opt.MapFrom(src => src.Type.TypeName);
        });
        base.CreateMap<ArticleType, ArticleTypeDTO>().ForMember(x => x.ArticleNames, opt =>
        {
            opt.MapFrom(src => src.Articles.Select(x=>x.Title));
        });
    }
}
