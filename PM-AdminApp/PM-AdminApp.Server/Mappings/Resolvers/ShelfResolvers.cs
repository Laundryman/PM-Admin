using AutoMapper;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Entities.ClusterAggregate;
namespace PlanMatr_API.Mappings.Resolvers
{
    public class ShelfPositionResolver : IValueResolver<PlanogramShelf, PlanmPartInfo, PlanmPosition>
    {
        public PlanmPosition Resolve(PlanogramShelf source, PlanmPartInfo destination, PlanmPosition destMember, ResolutionContext context)
        {
            return new PlanmPosition
            {
                x = source.PositionX,
                y = source.PositionY
            };
        }
    }

    public class ShelfStatusEnumResolver : IValueResolver<PlanogramShelf, PlanmPartInfo, String>
    {
        public string Resolve(PlanogramShelf source, PlanmPartInfo destination, string destMember, ResolutionContext context)
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

    public class ClusterShelfPositionResolver : IValueResolver<ClusterShelf, PlanmPartInfo, PlanmPosition>
    {
        public PlanmPosition Resolve(ClusterShelf source, PlanmPartInfo destination, PlanmPosition destMember, ResolutionContext context)
        {
            return new PlanmPosition
            {
                x = source.PositionX,
                y = source.PositionY
            };
        }
    }

    public class ClusterShelfStatusEnumResolver : IValueResolver<ClusterShelf, PlanmPartInfo, String>
    {
        public string Resolve(ClusterShelf source, PlanmPartInfo destination, string destMember, ResolutionContext context)
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
}
