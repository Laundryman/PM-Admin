using AutoMapper;
using PMApplication.Dtos.PlanModels;
using PMApplication.Dtos.StandTypes;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Entities.StandAggregate;

namespace PM_AdminApp.Server.Mappings.Resolvers
{
    public class StandCountResolver : IValueResolver<StandType, StandTypeDto, int>
    {
        public int Resolve(StandType source, StandTypeDto destination, int destMember, ResolutionContext context)
        {
            if (source.Stands != null)
            {
                // Map the PartStatusId to a status string or enum as needed
                
                return source.Stands.Count;
            }
            else
            {
                return 0;
            }
        }
    }
}
