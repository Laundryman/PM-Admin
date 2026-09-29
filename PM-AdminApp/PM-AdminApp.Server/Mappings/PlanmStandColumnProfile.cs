using AutoMapper;
using PM_AdminApp.Server.Mappings.Resolvers;
using PMApplication.Dtos.PlanModels;
using PMApplication.Dtos.StandTypes;
using PMApplication.Entities.StandAggregate;

namespace PM_AdminApp.Server.Mappings
{
    public class PlanmStandColumnProfile : Profile
    {

        public PlanmStandColumnProfile()
        {
            CreateMap<PlanmStandColumnDto, StandColumn>()
                .ForMember(p => p.StandColumnUprights, opt => opt.MapFrom(s => s.ColumnUprightList));


        }
    }
}
