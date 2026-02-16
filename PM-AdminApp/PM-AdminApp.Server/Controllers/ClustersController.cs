using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.ProductAggregate;
using PMApplication.Entities.StandAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using Page = PMApplication.Dtos.Page;

namespace PM_AdminApp.Server.Controllers
{
    [Authorize]
    [Route("api/clusters/[action]")]
    [ApiController]
    public class ClustersController : BaseController
    {
        private readonly ILogger<ClustersController> _logger;
        private readonly IMapper _mapper;
        private readonly IAsyncRepository<Stand> _asyncStandRepository;
        private readonly IClusterRepository _clusterRepository;
        private readonly IAsyncRepository<Country> _countryRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;



        public ClustersController(IMapper mapper, IAsyncRepository<Stand> asyncStandRepository,
            IAsyncRepository<Country> countryRepository, IAsyncRepository<Category> categoryRepository,
            ILogger<ClustersController> logger, IClusterRepository clusterRepository)
        {
            _logger = logger;
            _clusterRepository = clusterRepository;
            _asyncStandRepository = asyncStandRepository;
            _countryRepository = countryRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }


        [HttpPost]
        public async Task<IActionResult> SearchClusters(ClusterFilterDto filterDto)
        {
            try
            {
                //var spec = new ProductSpecification(_mapper.Map<ProductFilter>(filterDto));
                var clusters = await _clusterRepository.SearchClusters(filterDto);
                
                return Ok(clusters);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside SearchClusters action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        //[HttpGet(Name = "ProductSelectList")]
        //public async Task<IActionResult> GetStandSelectList([FromQuery] ProductFilterDto filterDto)
        //{
        //    try
        //    {
        //        //if (filterDto.CountriesList != null)
        //        //{
        //        //    var allCountries = await IsAllCountries(filterDto.CountriesList, _countryRepository, _mapper);
        //        //    if (allCountries)
        //        //    {
        //        //        filterDto.CountriesList = null;
        //        //    }
        //        //}

        //        var spec = new StandSpecification(_mapper.Map<StandFilter>(filterDto));
        //        var stands = await _asyncStandRepository.ListAsync(spec);

        //        var StandSelectList = CreateSelectList(stands);
        //        return Ok(StandSelectList);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogWarning($"Something went wrong inside GetStand action: {ex.Message}");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        //[HttpGet("{id}", Name = "StandById")]
        //public async Task<IActionResult> GetStandById(int id)
        //{
        //    try
        //    {
        //        var stand = await _asyncStandRepository.GetByIdAsync(id);

        //        if (stand == null)
        //        {
        //            _logger.LogWarning($"Stand with id: {id}, hasn't been found in db.");
        //            return NotFound();
        //        }
        //        else
        //        {
        //            _logger.LogInformation($"Returned stand with id: {id}");
        //            var response = _mapper.Map<FullStandDto>(stand);

        //            return Ok(response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Something went wrong inside GetProductById action: {ex.Message}");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        //private List<StandListDto> CreateSelectList(IReadOnlyList<Stand> list)
        //{
        //    var selectList = new List<StandListDto>();
        //    var productSelect = new StandListDto("Select Product");
        //    productSelect.Id = 0;

        //    selectList.Add(productSelect);

        //    for (int i = 0; i < list.Count; i++)
        //    {
        //        selectList.Add(_mapper.Map<StandListDto>(list[i]));
        //    }

        //    return selectList;
        //}

        //private async Task<List<Stand>> GetStandsFromCountryList(string countryList, IReadOnlyList<Stand> products)
        //{
        //    //we need to filter only products that have the at least one of the countries that are required
        //    var requiredCountryList = countryList.Split(',');var filteredStands = new List<Stand>();

        //    foreach (var product in products)
        //    {
        //        if (stand.CountriesList != null)
        //        {
        //            var productCountryList = stand.CountriesList.Split(",");

        //            foreach (var country in productCountryList)
        //            {
        //                if (requiredCountryList.Contains(country))
        //                {
        //                    filteredProducts.Add(stand);
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    return filteredProducts;

        //}
    }
}
