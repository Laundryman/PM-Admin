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
    public class PlanmStandDtoProfile : Profile
    {


        public PlanmStandDtoProfile()
        {
            CreateMap<Stand, PlanmStandDto>()
                .ForMember(s => s.StandId, opt => opt.MapFrom(x => x.Id))
                .ForMember(s => s.StandTypeId, opt => opt.MapFrom(x => x.StandTypeId))
                .ForMember(s => s.ParentStandTypeId, opt => opt.MapFrom(x => x.ParentStandTypeId))
                .ForMember(s => s.StandTypeName, opt => opt.MapFrom(x => x.StandTypeName))

                //.ForMember(s => s.ParentStandTypeName, opt => opt.MapFrom(x => x.StandType.ParentStandTypeName))
                .ForMember(s => s.BrandId, opt => opt.MapFrom(x => x.BrandId))
                //.ForMember(s => s.IsUsed, opt => opt.MapFrom(x => x.IsUsed))
                //.ForMember(s => s.IsUsed, opt => opt.MapFrom<StandIsUsedResolver>())
                .ForMember(s => s.ShelfLock, opt => opt.MapFrom(x => x.StandType.Lock))
                .ForMember(s => s.Name, opt => opt.MapFrom(x => x.Name))
                .ForMember(s => s.StandAssemblyNumber, opt => opt.MapFrom(x => x.StandAssemblyNumber))
                .ForMember(s => s.LayoutStyle, opt => opt.MapFrom<StandLayoutResolver>())
                .ForMember(s => s.Height, opt => opt.MapFrom(x => x.Height))
                .ForMember(s => s.Width, opt => opt.MapFrom(x => x.Width))
                .ForMember(s => s.MerchHeight, opt => opt.MapFrom(x => x.MerchHeight))
                .ForMember(s => s.MerchWidth, opt => opt.MapFrom(x => x.MerchWidth))
                .ForMember(s => s.HeaderHeight, opt => opt.MapFrom(x => x.HeaderHeight))
                .ForMember(s => s.HeaderWidth, opt => opt.MapFrom(x => x.HeaderWidth))
                .ForMember(s => s.FooterHeight, opt => opt.MapFrom(x => x.FooterHeight))
                .ForMember(s => s.FooterWidth, opt => opt.MapFrom(x => x.FooterWidth))
                .ForMember(s => s.Cols, opt => opt.MapFrom(x => x.Cols))
                .ForMember(s => s.EqualCols, opt => opt.MapFrom(x => x.EqualCols))
                .ForMember(s => s.DefaultColWidth, opt => opt.MapFrom(x => x.DefaultColWidth))
                .ForMember(s => s.HorizontalPitchCount, opt => opt.MapFrom(x => x.HorizontalPitchCount))
                .ForMember(s => s.HorizontalPitchSize, opt => opt.MapFrom(x => x.HorizontalPitchSize))
                .ForMember(s => s.Rows, opt => opt.MapFrom(x => x.Rows))
                .ForMember(s => s.EqualRows, opt => opt.MapFrom(x => x.EqualRows))
                .ForMember(s => s.DefaultRowHeight, opt => opt.MapFrom(x => x.DefaultRowHeight))
                .ForMember(s => s.shelfIncrement, opt => opt.MapFrom(x => x.ShelfIncrement))
                .ForMember(s => s.HeaderGraphic, opt => opt.MapFrom(x => x.HeaderGraphic))
                //.ForMember(s => s.HeaderGraphicLocation, opt => opt.MapFrom(x => x.HeaderGraphicLocation))
                .ForMember(s => s.StandCost, opt => opt.MapFrom(x => x.StandCost))
                .ForMember(s => s.DateCreated, opt => opt.MapFrom(x => x.DateCreated))
                .ForMember(s => s.DateUpdated, opt => opt.MapFrom(x => x.DateUpdated))
                .ForMember(s => s.DateAvailable, opt => opt.MapFrom(x => x.DateAvailable))
                .ForMember(s => s.Published, opt => opt.MapFrom(x => x.Published))
                .ForMember(s => s.Discontinued, opt => opt.MapFrom(x => x.Discontinued))
                //.ForMember(s => s.CountryId, opt => opt.MapFrom(x => x.CountryId))
                //.ForMember(s => s.CountryIds, opt => opt.MapFrom<StandCountryIdsResolver>())
                .ForMember(s => s.SpanShelves, opt => opt.MapFrom(x => x.SpanShelves))
                .ForMember(s => s.AllowOverHang, opt => opt.MapFrom(x => x.AllowOverHang))

                .ForMember(s => s.ColumnList, opt => opt.MapFrom(x => x.ColumnList))
                .ForMember(s => s.RowList, opt => opt.MapFrom<StandRowResolver>());



        }
    }
}
