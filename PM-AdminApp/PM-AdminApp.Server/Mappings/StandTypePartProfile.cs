using AutoMapper;
using PlanMatr_API.Mappings.Resolvers;
using PMApplication.Dtos;
using PMApplication.Dtos.PlanModels;
using PMApplication.Dtos.StandTypes;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Entities.StandAggregate;

namespace PlanMatr_API.Mappings
{
    public class StandTypePartProfile : Profile
    {


        public StandTypePartProfile()
        {
            CreateMap<StandTypePart, StandTypeDto>()
                .ForMember(p => p.Id, opt => opt.MapFrom(s => s.StandType.Id))
                .ForMember(p => p.BrandId, opt => opt.MapFrom(s => s.StandType.BrandId))
                .ForMember(p => p.ParentStandTypeId, opt => opt.MapFrom(s => s.StandType.ParentStandTypeId))
                .ForMember(p => p.Name, opt => opt.MapFrom(s => s.StandType.Name))
                .ForMember(p => p.Description, opt => opt.MapFrom(s => s.StandType.Description))
                .ForMember(p => p.Lock, opt => opt.MapFrom(s => s.StandType.Lock))
                .ForMember(p => p.StandImage, opt => opt.MapFrom(s => s.StandType.StandImage))
                .ForMember(p => p.HidePrices, opt => opt.MapFrom(s => s.StandType.HidePrices));

        }
    }
}
