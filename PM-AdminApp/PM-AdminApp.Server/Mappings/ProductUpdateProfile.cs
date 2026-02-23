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
                .ForMember(dest => dest.Countries, opt => opt.Ignore())
                .ForMember(dest => dest.Regions, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Parts, opt => opt.Ignore())
                .ForMember(dest => dest.Shades, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());

        }
    
    }
}
