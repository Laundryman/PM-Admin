using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using PMInfrastructure.Repositories;

namespace PM_AdminApp.Server.Controllers
{
    [Authorize]
    [Route("api/planograms/[action]")]
    [ApiController]
    public class PlanogramsController : BaseController
    {
        private readonly ILogger<ProductsController> _logger;

        private readonly IMapper _mapper;
        private readonly IAsyncRepositoryLong<Planogram> _asyncPlanogramRepository;
        private readonly IPlanogramRepository _planogramRepository;
        private readonly IAsyncRepositoryLong<PlanogramLock> _planogramLockRepository;
        private readonly IAsyncRepository<Country> _countryRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;
        private readonly IAsyncRepository<Region> _regionRepository;

        public PlanogramsController(ILogger<ProductsController> logger, IMapper mapper,
            IAsyncRepositoryLong<Planogram> asyncPlanogramRepository, IAsyncRepository<Country> countryRepository,
            IPlanogramRepository planogramRepository, IAsyncRepository<Category> categoryRepository,
            IAsyncRepository<Region> regionRepository, IAsyncRepositoryLong<PlanogramLock> planogramLockRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _asyncPlanogramRepository = asyncPlanogramRepository;
            _countryRepository = countryRepository;
            _planogramRepository = planogramRepository;
            _categoryRepository = categoryRepository;
            _regionRepository = regionRepository;
            _planogramLockRepository = planogramLockRepository;
        }



        [HttpPost]
        public async Task<IActionResult> SearchPlanograms(PlanogramFilterDto filterDto)
        {
            try
            {
                //var spec = new ProductSpecification(_mapper.Map<ProductFilter>(filterDto));
                var planograms = await _planogramRepository.SearchPlanograms(filterDto);

                return Ok(planograms);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside SearchProducts action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Unlock([FromQuery] long id)
        {
            try
            {
                var filter = new PlanogramLockFilter { PlanogramId = id };
                var spec = new PlanogramLockSpecification(filter);
                var planogramLocks = await _planogramLockRepository.ListAsync(spec);
                foreach (var lockItem in planogramLocks)
                {
                    await _planogramLockRepository.DeleteAsync(lockItem);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside Unlock action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
