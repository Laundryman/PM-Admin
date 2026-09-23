using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PM_AdminApp.Server.Extensions;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters.Widgets;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Interfaces.ServiceInterfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using PMInfrastructure.Repositories;
using System.Net;
using PMApplication.Specifications.Widgets;
using static PMApplication.Enums.StatusEnums;

namespace PM_AdminApp.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WidgetsController : ControllerBase
    {
        private readonly ILogger<WidgetsController> _logger;

        private readonly IMapper _mapper;
        private readonly IAsyncRepositoryLong<Planogram> _asyncPlanogramRepository;
        private readonly IPlanogramRepository _planogramRepository;
        private readonly IAsyncRepositoryLong<PlanogramLock> _planogramLockRepository;
        private readonly IAsyncRepository<Country> _countryRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;
        private readonly IAsyncRepository<Region> _regionRepository;

        private readonly IBrandService _brandService;
        private readonly IPartRepository _partRepository;
        private readonly IProductRepository _productRepository;
        private readonly IShadeRepository _shadeRepository;

        private readonly IPlanogramService _planogramService;
        private readonly IAsyncRepositoryLong<Planogram> _planogramAsyncRepository;

        private readonly ICountryService _countryService;
        private readonly IRegionService _regionService;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public WidgetsController(ILogger<WidgetsController> logger, IMapper mapper,
            IAsyncRepositoryLong<Planogram> asyncPlanogramRepository, IAsyncRepository<Country> countryRepository,
            IAsyncRepository<Category> categoryRepository, IAsyncRepository<Region> regionRepository, IAsyncRepositoryLong<PlanogramLock> planogramLockRepository, IPlanogramRepository planogramRepository, 
            IBrandService brandService, IPlanogramService planogramService, IAsyncRepositoryLong<Planogram> planogramAsyncRepository, 
            ICountryService countryService, IRegionService regionService, IAuditService auditService, IConfiguration config, IWebHostEnvironment env, IPartRepository partRepository, IProductRepository productRepository, IShadeRepository shadeRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _asyncPlanogramRepository = asyncPlanogramRepository;
            _countryRepository = countryRepository;
            _categoryRepository = categoryRepository;
            _regionRepository = regionRepository;
            _planogramLockRepository = planogramLockRepository;
            _planogramRepository = planogramRepository;
            _brandService = brandService;
            _planogramService = planogramService;
            _planogramAsyncRepository = planogramAsyncRepository;
            _countryService = countryService;
            _regionService = regionService;
            _auditService = auditService;
            _config = config;
            _env = env;
            _partRepository = partRepository;
            _productRepository = productRepository;
            _shadeRepository = shadeRepository;
        }


        [HttpPost]
        public async Task<ActionResult<IEnumerable<Planogram>>> GetRecentPlanograms([FromQuery] PlanoWidgetFilterDto filter)
        {
            try
            {
                var recentPlanograms = await _planogramService.GetRecentPlanogramsAsync(filter);
                var partCount = await _partRepository.CountAsync(new PartSpecification(new PartFilter { BrandId = filter.BrandId }));
                return Ok(recentPlanograms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching recent planograms.");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
        }
        [HttpGet]
        public async Task<StatsDto> GetStats()
        {
            var recentFromDate = DateTime.Now.AddDays(-30);
            var planogramCount = await _planogramService.GetPlanogramCount(new CountsFilterDto() { });
            var recentPlanograms = await _planogramService.GetPlanogramCount(new CountsFilterDto() { RecentFromDate = recentFromDate });
            var partCount = await _partRepository.CountAsync(new PartCountSpecification(new CountsFilterDto() { BrandId = null })); // Adjust as needed
            var recentPartCount = await _partRepository.CountAsync(new PartCountSpecification(new CountsFilterDto() { BrandId = null, RecentFromDate = recentFromDate })); // Adjust as needed
            var productCount = await _productRepository.CountAsync(new ProductCountSpecification(new CountsFilterDto() { BrandId = null })); // Adjust as needed
            var recentProductCount = await _productRepository.CountAsync(new ProductCountSpecification(new CountsFilterDto() { BrandId = null, RecentFromDate = recentFromDate })); // Adjust as needed
            var shadesCount = await _shadeRepository.CountAsync(new ShadeCountSpecification(new CountsFilterDto() { BrandId = null })); // Adjust as needed
            var recentShadesCount = await _shadeRepository.CountAsync(new ShadeCountSpecification(new CountsFilterDto() { BrandId = null, RecentFromDate = recentFromDate })); // Adjust as needed

            return new StatsDto
            {
                PlanogramsCount = planogramCount,
                RecentPlanogramsCount = recentPlanograms,
                PartsCount = partCount,
                RecentPartsCount = recentPartCount,
                ProductsCount = productCount,
                RecentProductsCount = recentProductCount,
                ShadesCount = shadesCount,
                RecentShadesCount = recentShadesCount
            };
        }

    }
}
