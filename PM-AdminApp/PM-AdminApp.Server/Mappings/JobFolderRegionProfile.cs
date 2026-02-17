using AutoMapper;
using PlanMatr_API.Mappings.Resolvers;
using PMApplication.Dtos;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities.JobsAggregate;
using PMApplication.Entities.CountriesAggregate;

namespace PM_AdminApp.Server.Mappings
{
    public class JobFolderRegionProfile : Profile
    {
        public JobFolderRegionProfile()
        {
            CreateMap<Region, JobFolderRegionDto>()
                .ForMember(Dest => Dest.JobFolders, opt => opt.Ignore())
                .ForMember(Dest => Dest.Countries, opt => opt.Ignore());






        }

    }
}
