using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PM_AdminApp.Server.Extensions;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.PlanModels;
using PMApplication.Dtos.StandTypes;
using PMApplication.Entities;
using PMApplication.Entities.StandAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Interfaces.ServiceInterfaces;
using PMApplication.Services;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;

namespace PM_AdminApp.Server.Controllers.Planner
{
    [Authorize]
    [Route("api/clusters/create/[action]")]
    [ApiController]
    public class CreateClusterApiController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<CreateClusterApiController> _logger;
        private readonly IPlanogramService _planogramService;
        private readonly IAuditService _auditService;
        private readonly IAsyncRepository<StandType> _asyncStandTypeRepository;
        private readonly IAsyncRepository<Stand> _asyncStandRepository;
        private readonly IClusterRepository _clusterRepository;
        private readonly IClusterService _clusterService;


        public CreateClusterApiController(IMapper mapper, ILogger<CreateClusterApiController> logger, IPlanogramService planogramService, IAuditService auditService, IAsyncRepository<StandType> asyncStandTypeRepository, IAsyncRepository<Stand> asyncStandRepository, IClusterRepository clusterRepository, IClusterService clusterService)
        {
            _mapper = mapper;
            _logger = logger;
            _planogramService = planogramService;
            _auditService = auditService;
            _asyncStandTypeRepository = asyncStandTypeRepository;
            _asyncStandRepository = asyncStandRepository;
            _clusterRepository = clusterRepository;
            _clusterService = clusterService;
        }

        #region API

        [HttpPost]
        public async Task<IActionResult> GetStands(StandFilterDto filterDto)
        {
            try
            {
                var spec = new StandSpecification(_mapper.Map<StandFilter>(filterDto));
                var stands = await _asyncStandRepository.ListAsync(spec);

                var mappedStands = _mapper.Map<List<StandDto>>(stands);
                return Ok(mappedStands);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetStands action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }   

        [HttpPost]
        public async Task<IActionResult> GetStandTypes(StandTypeFilterDto filterDto)
        {
            try
            {
                var spec = new StandTypeSpecification(_mapper.Map<StandTypeFilter>(filterDto));
                var standTypes = await _asyncStandTypeRepository.ListAsync(spec);

                if (filterDto.GetParents)
                {
                    var mappedPTypes = _mapper.Map<List<ParentStandTypeDto>>(standTypes);
                    return Ok(mappedPTypes);
                }

                var mappedTypes = _mapper.Map<List<StandTypeDto>>(standTypes);
                return Ok(mappedTypes.Where(st => st.StandCount > 0));
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetStandTypes action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetLayouts(LayoutFilterDto filterDto)
        {
            //need to update the cluster table/cluster nomenclature to change the name to layout 
            try
            {
                var spec = new ClusterSpecification(_mapper.Map<ClusterFilter>(filterDto));
                var clusters = await _clusterRepository.ListAsync(spec);

                var mappedTypes = _mapper.Map<List<PlanmClusterDto>>(clusters);
                return Ok(mappedTypes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetLayouts action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateLayout(CreateLayoutDto newLayoutDetails)
        {
            try
            {
                var userProfile = await this.MappedUser();

                if (newLayoutDetails.BrandId == 0 ||
                    newLayoutDetails.StandId == 0 || newLayoutDetails.StandTypeId == 0)
                {
                    throw new Exception("Layout information incomplete");
                }


                string? userId = userProfile.Id;

                var clusterId = await _clusterService.CreateCluster(newLayoutDetails, userProfile);


                var cluster = await _clusterService.GetCluster(clusterId);

                //Audit the action
                var audit = new AuditLog
                {
                    UserId = userId,
                    Date = DateTime.Now,
                    BrandId = newLayoutDetails.BrandId,
                    Roles = userProfile.RoleIds,
                    UserName = userProfile.DisplayName,
                    Action = (int)LogActionEnum.CreateLayout,
                    Message = userProfile.DisplayName + " created layout " + cluster.Name,
                    PlanoId = clusterId
                };
                await _auditService.AuditEvent(audit);

                return Ok(clusterId);
            }
            catch (Exception ex)
            {
                return BadRequest("Could not create Layout");
            }

        }

        #endregion
    }
}
