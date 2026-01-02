using AutoMapper;
using PlanMatr_API.Mappings.Resolvers;
using PMApplication.Dtos;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;

namespace PlanMatr_API.Mappings
{
    public class CategoryMenuProfile : Profile
    {


        public CategoryMenuProfile()
        {
            CreateMap<Category, CategoryMenuDto>()
                .ForMember(C => C.ParentCategoryId, opt => opt.MapFrom(x => x.Id))
                .ForMember(p => p.ParentCategoryName, opt => opt.MapFrom(x => x.ParentCategoryName))
                .ForMember(C => C.Name, opt => opt.MapFrom(x => x.Name))
                .ForMember(p => p.DisplayOrder, opt => opt.MapFrom(x => x.DisplayOrder))
                .ForMember(p => p.HeroProductId, opt => opt.MapFrom(x => x.HeroProductId))
                //.ForMember(p => p.HeroImageUrl, opt => opt.MapFrom(x => x.))
                .ForMember(p => p.Parts, opt => opt.MapFrom(x => x.Parts));

        }
    }
}
