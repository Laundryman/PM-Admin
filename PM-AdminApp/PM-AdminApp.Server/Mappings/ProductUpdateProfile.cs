using AutoMapper;
using PMApplication.Dtos;
using PMApplication.Entities.ProductAggregate;

namespace PM_AdminApp.Server.Mappings
{
    public class ProductUpdateProfile : Profile
    {
        public ProductUpdateProfile()
        {
            CreateMap<ProductUpdateDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.FullDescription, opt => opt.MapFrom(src => src.FullDescription))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))

                .ForMember(dest => dest.Countries, opt => opt.Ignore())
                .ForMember(dest => dest.Regions, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Parts, opt => opt.Ignore())
                .ForMember(dest => dest.Shades, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());

        }
    
    }
}
