using AutoMapper;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities.ClusterAggregate;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;

namespace PlanMatr_API.Mappings.Resolvers
{
    public class PartPositionResolver : IValueResolver<PlanogramPart, PlanmPartInfo, PlanmPosition>
    {
        public PlanmPosition Resolve(PlanogramPart source, PlanmPartInfo destination, PlanmPosition destMember, ResolutionContext context)
        {
            return new PlanmPosition
            {
                x = source.PositionX,
                y = source.PositionY
            };
        }
    }

    public class PartStatusEnumResolver : IValueResolver<PlanogramPart, PlanmPartInfo, String>
    {
        public string Resolve(PlanogramPart source, PlanmPartInfo destination, string destMember, ResolutionContext context)
        {
            if (source.PartStatusId != null)
            {
                // Map the PartStatusId to a status string or enum as needed
                var statusEnum = (PlanoItemStatusEnum)source.PartStatusId;
                return statusEnum.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public class PartProductsResolver : IValueResolver<PlanogramPart, PlanmPartInfo, String>
    {
        public string Resolve(PlanogramPart source, PlanmPartInfo destination, string destMember, ResolutionContext context)
        {
            if (source.PartStatusId != null)
            {
                // Map the PartStatusId to a status string or enum as needed
                var statusEnum = (PlanoItemStatusEnum)source.PartStatusId;
                return nameof(statusEnum).ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public class ClusterPartPositionResolver : IValueResolver<ClusterPart, PlanmPartInfo, PlanmPosition>
    {
        public PlanmPosition Resolve(ClusterPart source, PlanmPartInfo destination, PlanmPosition destMember, ResolutionContext context)
        {
            return new PlanmPosition
            {
                x = source.PositionX,
                y = source.PositionY
            };
        }
    }

    public class ClusterPartStatusEnumResolver : IValueResolver<ClusterPart, PlanmPartInfo, String>
    {
        public string Resolve(ClusterPart source, PlanmPartInfo destination, string destMember, ResolutionContext context)
        {
            if (source.PartStatusId != null)
            {
                // Map the PartStatusId to a status string or enum as needed
                var statusEnum = (PlanoItemStatusEnum)source.PartStatusId;
                return statusEnum.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }

}
