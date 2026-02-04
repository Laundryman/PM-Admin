using AutoMapper;
using PM_AdminApp.Server.Mappings.Resolvers;
using PMApplication.Dtos;
using PMApplication.Entities.PartAggregate;

namespace PM_AdminApp.Server.Mappings
{
    public class PartUploadDtoPartProfile : Profile
    {
        public PartUploadDtoPartProfile()
        {
            CreateMap<PartUploadDto, Part>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.PartNumber, opt => opt.MapFrom(src => src.PartNumber))
                .ForMember(dest => dest.AltPartNumber, opt => opt.MapFrom(src => src.AltPartNumber))
                .ForMember(dest => dest.CustomerRefNo, opt => opt.MapFrom(src => src.CustomerRefNo))
                .ForMember(dest => dest.Published, opt => opt.MapFrom(src => src.Published))
                .ForMember(dest => dest.Discontinued, opt => opt.MapFrom(src => src.Discontinued))
                .ForMember(dest => dest.Facings, opt => opt.MapFrom(src => src.Facings))
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
                .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Width))
                .ForMember(dest => dest.Depth, opt => opt.MapFrom(src => src.Depth))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.Stock))
                .ForMember(dest => dest.CountriesList, opt => opt.MapFrom(src => src.CountriesList))
                .ForMember(dest => dest.PartTypeId, opt => opt.MapFrom(src => src.PartTypeId))
                .ForMember(dest => dest.ShoppingHeight, opt => opt.MapFrom(src => src.ShoppingHeight))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.DateCreated))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => src.DateUpdated))
                .ForMember(dest => dest.Shoppable, opt => opt.MapFrom(src => src.Shoppable))
                .ForMember(dest => dest.PackShotImageSrc, opt => opt.MapFrom(src => src.PackShotImageSrc))
                .ForMember(dest => dest.Render2dImage, opt => opt.MapFrom(src => src.Render2dImage))
                .ForMember(dest => dest.SvgLineGraphic, opt => opt.MapFrom(src => src.SvgLineGraphic))
                .ForMember(dest => dest.UnitCost, opt => opt.MapFrom(src => src.UnitCost))
                .ForMember(dest => dest.LaunchPrice, opt => opt.MapFrom(src => src.LaunchPrice))
                .ForMember(dest => dest.LaunchDate, opt => opt.MapFrom(src => src.LaunchDate))
                .ForMember(dest => dest.Presentation, opt => opt.MapFrom(src => src.Presentation))
                .ForMember(dest => dest.CassetteBio, opt => opt.MapFrom(src => src.CassetteBio))
                .ForMember(dest => dest.ManufacturingProcess, opt => opt.MapFrom(src => src.ManufacturingProcess))
                .ForMember(dest => dest.TestingType, opt => opt.MapFrom(src => src.TestingType))
                .ForMember(dest => dest.InternationalPart, opt => opt.MapFrom(src => src.internationalPart))
                .ForMember(dest => dest.DmiReco, opt => opt.MapFrom(src => src.DmiReco))
                .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.ParentCategoryId))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategoryName))
                .ForMember(dest => dest.PartTypeName, opt => opt.MapFrom(src => src.PartTypeName))
                .ForMember(dest => dest.RegionsList, opt => opt.MapFrom(src => src.RegionList))
                .ForMember(dest => dest.StandTypeList, opt => opt.MapFrom(src => src.StandTypeList))
                .ForMember(dest => dest.ProductList, opt => opt.MapFrom(src => src.ProductList))
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.BrandId))
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.ClusterParts, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore())
                .ForMember(dest => dest.Countries, opt => opt.Ignore())
                .ForMember(dest => dest.PartType, opt => opt.Ignore())
                .ForMember(dest => dest.Regions, opt => opt.Ignore())
                .ForMember(dest => dest.StandTypes, opt => opt.Ignore());

            //.ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.DateCreated ?? DateTime.UtcNow))
            //.ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.UtcNow))
            //.ForMember(dest => dest.Products, opt => opt.MapFrom<ProductDtoProductResolver>())
            //.ForMember(dest => dest.Countries, opt => opt.MapFrom<CountryPartResolver>())
            //.ForMember(dest => dest.StandTypes, opt => opt.MapFrom<StandTypePartResolver>())
            //.ForMember(dest => dest.PartType, opt => opt.MapFrom<PartTypePartResolver>())
            //.ForMember(dest => dest.Regions, opt => opt.MapFrom<RegionPartResolver>());


            //.ForMember(dest => dest.HidePrices, opt => opt.MapFrom(src => src.HidePrices))





        }
    }
}
