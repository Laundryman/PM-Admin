using AutoMapper;
using Microsoft.Graph.Models;
using PlanMatr_API.Mappings.Resolvers;
using PMApplication.Dtos;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities.JobsAggregate;
using PMApplication.Entities.PlanogramAggregate;
using System.Globalization;
using Microsoft.Kiota.Abstractions;

namespace PM_AdminApp.Server.Mappings
{
    public class JobProfile : Profile
    {
        public JobProfile()
        {
            CreateMap<Job, JobDto>();
            //.ForMember(j => j.DateFrom, opt => opt.MapFrom(x => x.DateFrom != null ? x.DateFrom.Value.ToBinary() : (long?)null))
            //.ForMember(j => j.DateTo,
            //    opt => opt.MapFrom(x => x.DateTo != null ? x.DateTo.Value.ToBinary() : (long?)null))
            //.ForMember(j => j.UploadedOn,
            //    opt => opt.MapFrom(x => x.UploadedOn.ToBinary()));





        }

    }
}
