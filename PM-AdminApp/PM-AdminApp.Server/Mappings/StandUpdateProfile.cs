using AutoMapper;
using PlanMatr_API.Mappings.Resolvers;
using PM_AdminApp.Server.Mappings.Resolvers;
using PMApplication.Dtos;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Entities.StandAggregate;

namespace PlanMatr_API.Mappings
{
    public class StandUpdateDtoProfile : Profile
    {


        public StandUpdateDtoProfile()
        {
            CreateMap<StandUpdateDto, Stand>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.Id))
                .ForMember(dest => dest.StandTypeId, opt => opt.MapFrom(x => x.StandTypeId))
                .ForMember(dest => dest.ParentStandTypeId, opt => opt.MapFrom(x => x.ParentStandTypeId))
                .ForMember(dest => dest.StandTypeName, opt => opt.MapFrom(x => x.StandTypeName))
                //.ForMember(dest => dest.ParentStandTypeName, opt => opt.MapFrom(x => x.StandType.ParentStandTypeName))
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(x => x.BrandId))
                //.ForMember(dest => dest.IsUsed, opt => opt.MapFrom(x => x.IsUsed))
                //.ForMember(dest => dest.IsUsed, opt => opt.MapFrom<StandIsUsedResolver>())
                //.ForMember(dest => dest.ShelfLock, opt => opt.MapFrom(x => x.StandType.Lock))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(x => x.Name))
                .ForMember(dest => dest.StandAssemblyNumber, opt => opt.MapFrom(x => x.StandAssemblyNumber))
                //.ForMember(dest => dest.LayoutStyle, opt => opt.MapFrom<StandUpdateLayoutResolver>())
                .ForMember(dest => dest.Height, opt => opt.MapFrom(x => x.Height))
                .ForMember(dest => dest.Width, opt => opt.MapFrom(x => x.Width))
                .ForMember(dest => dest.MerchHeight, opt => opt.MapFrom(x => x.MerchHeight))
                .ForMember(dest => dest.MerchWidth, opt => opt.MapFrom(x => x.MerchWidth))
                .ForMember(dest => dest.HeaderHeight, opt => opt.MapFrom(x => x.HeaderHeight))
                .ForMember(dest => dest.HeaderWidth, opt => opt.MapFrom(x => x.HeaderWidth))
                .ForMember(dest => dest.FooterHeight, opt => opt.MapFrom(x => x.FooterHeight))
                .ForMember(dest => dest.FooterWidth, opt => opt.MapFrom(x => x.FooterWidth))
                .ForMember(dest => dest.Cols, opt => opt.MapFrom(x => x.Cols))
                .ForMember(dest => dest.EqualCols, opt => opt.MapFrom(x => x.EqualCols))
                .ForMember(dest => dest.DefaultColWidth, opt => opt.MapFrom(x => x.DefaultColWidth))
                .ForMember(dest => dest.HorizontalPitchCount, opt => opt.MapFrom(x => x.HorizontalPitchCount))
                .ForMember(dest => dest.HorizontalPitchSize, opt => opt.MapFrom(x => x.HorizontalPitchSize))
                .ForMember(dest => dest.Rows, opt => opt.MapFrom(x => x.Rows))
                .ForMember(dest => dest.EqualRows, opt => opt.MapFrom(x => x.EqualRows))
                .ForMember(dest => dest.DefaultRowHeight, opt => opt.MapFrom(x => x.DefaultRowHeight))
                //.ForMember(dest => dest.ShelfIncrement, opt => opt.MapFrom(x => x.shelfIncrement))
                .ForMember(dest => dest.HeaderGraphic, opt => opt.MapFrom(x => x.HeaderGraphic))
                //.ForMember(dest => dest.HeaderGraphicLocation, opt => opt.MapFrom(x => x.HeaderGraphicLocation))
                .ForMember(dest => dest.StandCost, opt => opt.MapFrom(x => x.StandCost))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(x => x.DateCreated))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(x => x.DateUpdated))
                .ForMember(dest => dest.DateAvailable, opt => opt.MapFrom(x => x.DateAvailable))
                .ForMember(dest => dest.Published, opt => opt.MapFrom(x => x.Published))
                .ForMember(dest => dest.Discontinued, opt => opt.MapFrom(x => x.Discontinued))
                //.ForMember(dest => dest.CountryId, opt => opt.MapFrom(x => x.CountryId))
                //.ForMember(dest => dest.CountryIds, opt => opt.MapFrom<StandCountryIdsResolver>())
                .ForMember(dest => dest.SpanShelves, opt => opt.MapFrom(x => x.SpanShelves))
                .ForMember(dest => dest.AllowOverHang, opt => opt.MapFrom(x => x.AllowOverHang))

                //.ForMember(dest => dest.ColumnList, opt => opt.MapFrom(x => x.ColumnList))
                //.ForMember(dest => dest.RowList, opt => opt.MapFrom<StandRowResolver>());
                .ForMember(dest => dest.ColumnList, opt => opt.Ignore())
                .ForMember(dest => dest.RowList, opt => opt.Ignore())
                .ForMember(dest => dest.Countries, opt => opt.Ignore())
                .ForMember(dest => dest.Regions, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.StandType, opt => opt.Ignore());







        }
    }
}
