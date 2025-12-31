using AutoMapper;
using PM_AdminApp.Server.Mappings.Resolvers;
using PMApplication.Dtos.StandTypes;
using PMApplication.Entities.StandAggregate;

namespace PM_AdminApp.Server.Mappings
{
    public class StandTypeProfile : Profile
    {


        public StandTypeProfile()
        {
            CreateMap<StandType, StandTypeDto>()
                .ForMember(p => p.StandCount, opt => opt.MapFrom<StandCountResolver>());


        }
    }
}
