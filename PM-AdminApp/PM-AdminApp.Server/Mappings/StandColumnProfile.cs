using AutoMapper;
using PM_AdminApp.Server.Mappings.Resolvers;
using PMApplication.Dtos.PlanModels;
using PMApplication.Dtos.StandTypes;
using PMApplication.Entities.StandAggregate;

namespace PM_AdminApp.Server.Mappings
{
    public class StandColumnProfile : Profile
    {


        public StandColumnProfile()
        {
            CreateMap<StandColumn, PlanmStandColumnDto>()
                .ForMember(p => p.ColumnUprightList, opt => opt.MapFrom(s => s.StandColumnUprights));


        }

    }
}
