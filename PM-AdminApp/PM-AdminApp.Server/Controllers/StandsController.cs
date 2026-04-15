using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using System.Text.Json;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.ProductAggregate;
using PMApplication.Entities.StandAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using PMInfrastructure.Repositories;
using Page = PMApplication.Dtos.Page;

namespace PM_AdminApp.Server.Controllers
{
    [Authorize]
    [Route("api/stands/[action]")]
    [ApiController]
    public class StandsController : BaseController
    {
        private readonly ILogger<StandsController> _logger;
        private readonly IMapper _mapper;
        private readonly IAsyncRepository<Stand> _asyncStandRepository;
        private readonly IStandRepository _standRepository;
        private readonly IAsyncRepository<Region> _regionRepository;
        private readonly IAsyncRepository<Country> _countryRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;



        public StandsController(IMapper mapper, IAsyncRepository<Stand> asyncStandRepository,
            IAsyncRepository<Country> countryRepository, IAsyncRepository<Category> categoryRepository,
            ILogger<StandsController> logger, IStandRepository standRepository, IAsyncRepository<Region> regionRepository)
        {
            _logger = logger;
            _standRepository = standRepository;
            _asyncStandRepository = asyncStandRepository;
            _countryRepository = countryRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _regionRepository = regionRepository;
        }


        [HttpPost]
        public async Task<IActionResult> SearchStands(StandFilterDto filterDto)
        {
            try
            {
                //var spec = new ProductSpecification(_mapper.Map<ProductFilter>(filterDto));
                var stands = await _standRepository.SearchStands(filterDto);

                return Ok(stands);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside SearchStands action: {ex.Message}");
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

        [HttpGet()]
        public async Task<IActionResult> GetStand([FromQuery] int id)
        {
            try
            {
                var spec = new StandByIdSpecification(id);
                var stand = await _asyncStandRepository.FirstOrDefaultAsync(spec);

                if (stand == null)
                {
                    _logger.LogWarning($"Stand with id: {id}, hasn't been found in db.");
                    return NotFound();
                }
                else
                {
                    _logger.LogInformation($"Returned stand with id: {id}");
                    var response = _mapper.Map<StandDto>(stand);

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetProductById action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveStand(StandUpdateDto updateStand)
        {
            try
            {
                if (updateStand == null)
                {
                    _logger.LogError("Part object sent from client is null.");
                    return BadRequest("Part object is null");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid part object sent from client.");
                    return BadRequest("Invalid model object");
                }
                //var stand = _mapper.Map<Stand>(updateStand);

                var id = updateStand.Id;
                var standFilter = new StandFilter() { Id = id };
                var spec = new StandSpecification(standFilter);
                var standEdit = await _standRepository.FirstAsync(spec);
                if (standEdit == null)
                {
                    _logger.LogError($"Stand with id: {id}, hasn't been found in db.");
                    return NotFound();
                }

                _mapper.Map(updateStand, standEdit );
                await _standRepository.UpdateAsync(standEdit);

                //Now manage relationships.
                await UpdateStandCountryCollection(standEdit, updateStand);
                await UpdateRegionsCollection(standEdit, updateStand);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside UpdateStand action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateStand(StandDto createDto)
        {
            try
            {
                var stand = _mapper.Map<Stand>(createDto);
                var createdStand = await _standRepository.AddAsync(stand);
                var response = _mapper.Map<StandDto>(createdStand);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside CreateStand action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task UpdateRegionsCollection(Stand origStand, StandUpdateDto updateStand)
        {
            //var regionDtos = JsonConvert.DeserializeObject<List<RegionDto>>(updateStand.Regions);
            foreach (var region in updateStand.Regions)
            {
                var origRegion = origStand.Regions.FirstOrDefault(r => r.Id == region.Id);
                if (origRegion == null)
                {

                    var dbRegion = await _regionRepository.GetByIdAsync(region.Id);
                    origStand.Regions.Add(dbRegion);
                }
            }
            for (int i = origStand.Regions.Count - 1; i >= 0; i--)
            {
                var origRegion = origStand.Regions[i];
                var updatedRegion = updateStand.Regions.FirstOrDefault(r => r.Id == origRegion.Id);
                if (updatedRegion == null)
                {
                    var dbRegion = await _regionRepository.GetByIdAsync(origRegion.Id);
                    origStand.Regions.Remove(dbRegion);
                }
            }

            //update Part.RegionList string
            origStand.RegionsList = string.Join(",", origStand.Regions.Select(r => r.Id));
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task UpdateStandCountryCollection(Stand origStand, StandUpdateDto updateStand)
        {
            //add new countries
            //var standCountries = JsonConvert.DeserializeObject<List<CountryDto>>(updateStand.Countries);
            foreach (var country in updateStand.Countries)
            {
                var origCountry = origStand.Countries.FirstOrDefault(c => c.Id == country.Id);
                if (origCountry == null)
                {
                    var dbCountry = await _countryRepository.GetByIdAsync(country.Id);
                    if (dbCountry != null)
                    {
                        origStand.Countries.Add(dbCountry);
                    }
                }
            }
            //remove deleted countries
            for (int i = origStand.Countries.Count - 1; i >= 0; i--)
            {
                var origCountry = origStand.Countries[i];
                var updatedCountry = updateStand.Countries.FirstOrDefault(c => c.Id == origCountry.Id);
                if (updatedCountry == null)
                {
                    var dbCountry = await _countryRepository.GetByIdAsync(origCountry.Id);
                    origStand.Countries.Remove(dbCountry);
                }
            }

            //update Part.CountryList string
            origStand.CountriesList = string.Join(",", origStand.Countries.Select(c => c.Id));
        }
    }
}
