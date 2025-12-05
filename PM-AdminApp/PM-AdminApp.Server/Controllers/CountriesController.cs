using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.PagedLists;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;

namespace LMXApi.Controllers
{
    [Authorize]
    [Route("api/countries/[action]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        //private ILoggerManager _logger;
        //private IRepositoryWrapper _repository;
        //private ILoggerManager _logger;
        private readonly ILogger<CountriesController> _logger;
        private readonly IMapper _mapper;
        //private readonly IReadRepository<Part> _partReadRepository;
        private readonly IAsyncRepository<Region> _regionRepository;
        private readonly IAsyncRepository<Country> _countryRepository;


        public CountriesController(IMapper mapper, IAsyncRepository<Country> countryRepository, IAsyncRepository<Region> regionRepository, ILogger<CountriesController> logger)
        {
            _logger = logger;
            _countryRepository = countryRepository;
            _regionRepository = regionRepository;
            _mapper = mapper;
        }

        #region Countries

        [HttpGet(Name = "Countries")]
        public async Task<IActionResult> GetAllCountries(int regionId, string? searchText = "")
        {
            try
            {
                var filter = new CountryFilter();
                filter.RegionId = regionId;
                var spec = new CountrySpecification(filter);
                var countries = await _countryRepository.ListAsync(spec);
                //var countFilter = filterDto;
                //countFilter.IsPagingEnabled = false;
                //var countSpec = new CountrySpecification(_mapper.Map<CountryFilter>(countFilter));
                //int totalItems = await _countryRepository.CountAsync(countSpec);
                _logger.LogInformation($"Returned all parts from database.");

                //var response = new PagedCountriesListDto();
                //response.Data = _mapper.Map<List<CountryDto>>(countries);
                //update partTypenames here
                //response.Page = new Page();
                //response.Page.PageNumber = filterDto.Page;
                //response.Page.TotalItems = totalItems;
                //response.Page.TotalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)filterDto.PageSize);
                //response.Page.Size = filterDto.PageSize;
                var response = _mapper.Map<List<CountryDto>>(countries);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetCountries action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet( "{countryList}", Name = "CountriesFromList")]
        public async Task<IActionResult> GetCountriesFromList(string countryList)
        {
            try
            {
                var countryIds = countryList.Split(',').ToList();
                var filterDto = new CountryFilter();
                filterDto.CountryList = countryList;
                var spec = new CountrySpecification(_mapper.Map<CountryFilter>(filterDto));
                var countries = await _countryRepository.ListAsync(spec);

                var countriesSelectList = CreateCountrySelectList(countries);
                return Ok(countriesSelectList);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetCountries action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet(Name = "CountriesSelectList")]
        public async Task<IActionResult> GetCountrySelectList([FromQuery] CountriesFilterDto filterDto)
        {
            try
            {
                var spec = new CountrySpecification(_mapper.Map<CountryFilter>(filterDto));
                var countries = await _countryRepository.ListAsync(spec);

                var countriesSelectList = CreateCountrySelectList(countries);
                return Ok(countriesSelectList);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetCountries action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        private List<CountryDto> CreateCountrySelectList(IReadOnlyList<Country> list)
        {
            var selectList = new List<CountryDto>();
            var countrySelect = new CountryDto("Select Country");
            countrySelect.Id = 0;

            selectList.Add(countrySelect);

            for (int i = 0; i < list.Count; i++)
            {
                selectList.Add(_mapper.Map<CountryDto>(list[i]));
            }

            return selectList;
        }

        #endregion

        #region Regions

        [HttpGet(Name = "Regions")]
        public async Task<IActionResult> GetAllRegions(int brandId, string? searchText = "")
        {
            try
            {
                var filterDto = new RegionsFilterDto();
                filterDto.BrandId = brandId;
                var spec = new RegionSpecification(_mapper.Map<RegionFilter>(filterDto));
                var regions = await _regionRepository.ListAsync(spec);
                //var countFilter = filterDto;
                //countFilter.IsPagingEnabled = false;
                //var countSpec = new RegionSpecification(_mapper.Map<RegionFilter>(countFilter));
                //int totalItems = await _regionRepository.CountAsync(countSpec);
                _logger.LogInformation($"Returned all parts from database.");

                //var response = new PagedRegionListDto();
                //response.Data = _mapper.Map<List<RegionDto>>(regions);
                //update partTypenames here
                //response.Page = new Page();
                //response.Page.PageNumber = filterDto.Page;
                //response.Page.TotalItems = totalItems;
                //response.Page.TotalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)filterDto.PageSize);
                //response.Page.Size = filterDto.PageSize;
                return Ok(regions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetRegions action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet(Name = "RegionsSelectList")]
        public async Task<IActionResult> GetRegions([FromQuery] RegionsFilterDto filterDto)
        {
            try
            {
                var spec = new RegionSpecification(_mapper.Map<RegionFilter>(filterDto));
                var regions = await _regionRepository.ListAsync(spec);

                var regionsSelectList = CreateSelectList(regions);
                return Ok(regionsSelectList);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetRegions action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("{id}", Name = "RegionById")]
        public async Task<IActionResult> GetRegionById(int id)
        {
            try
            {
                var region = await _regionRepository.GetByIdAsync(id);

                if (region == null)
                {
                    _logger.LogWarning($"Region with id: {id}, hasn't been found in db.");
                    return NotFound();
                }
                else
                {
                    _logger.LogInformation($"Returned region with id: {id}");
                    var response = _mapper.Map<RegionDto>(region);
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetRegionById action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        private List<RegionDto> CreateSelectList(IReadOnlyList<Region> list)
        {
            var selectList = new List<RegionDto>();
            var regionSelect = new RegionDto("Select Region");
            regionSelect.Id = 0;

            selectList.Add(regionSelect);

            for (int i = 0; i < list.Count; i++)
            {
                selectList.Add(_mapper.Map<RegionDto>(list[i]));
            }

            return selectList;
        }
#endregion
    }
}
