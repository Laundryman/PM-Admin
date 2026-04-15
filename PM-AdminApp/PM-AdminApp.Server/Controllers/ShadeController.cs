using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.PagedLists;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.ProductAggregate;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using System;
using Microsoft.Graph.Models;
using Page = PMApplication.Dtos.Page;


namespace PM_AdminApp.Server.Controllers
{
    [Authorize]
    [Route("api/shades/[action]")]
    [ApiController]
    public class ShadeController : BaseController
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IMapper _mapper;
        private readonly IAsyncRepositoryLong<Shade> _shadeRepository;
        private readonly IAsyncRepository<Country> _countryRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;

        public ShadeController(IMapper mapper, IAsyncRepositoryLong<Shade> shadeRepository,
            IAsyncRepository<Country> countryRepository, IAsyncRepository<Category> categoryRepository,
                ILogger<ProductsController> logger)
        {
            _logger = logger;
            _shadeRepository = shadeRepository;
            _countryRepository = countryRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetShades([FromQuery] ShadeFilterDto filterDto)
        {
            try
            {
                if (filterDto.CountryList != null)
                {
                    var allCountries = await IsAllCountries(filterDto.CountryList, _countryRepository, _mapper);
                    if (allCountries)
                    {
                        filterDto.CountryList = null;
                    }
                }

                var spec = new ShadeSpecification(_mapper.Map<ShadeFilter>(filterDto));
                var shades = await _shadeRepository.ListAsync(spec);
                var countFilter = filterDto;
                countFilter.IsPagingEnabled = false;
                var countSpec = new ShadeSpecification(_mapper.Map<ShadeFilter>(countFilter));
                int totalItems = await _shadeRepository.CountAsync(countSpec);
                _logger.LogInformation($"Returned all products from database.");


                var response = new PagedShadesListDto();


                response.Data = _mapper.Map<List<ShadeDto>>(shades);
                response.Page = new Page();
                response.Page.PageNumber = filterDto.Page;
                response.Page.TotalItems = totalItems;
                response.Page.TotalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)filterDto.PageSize);
                response.Page.Size = filterDto.PageSize;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetAllProducts action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreateShade(ShadeDto shadeDto)
        {
            try
            {
                var newShade = _mapper.Map<Shade>(shadeDto);
                newShade.Countries = new List<Country>();
                newShade.DateCreated = DateTime.Now;
                newShade.DateUpdated = DateTime.Now;
                newShade.DateAvailable = DateTime.Now;
                var createdShade = await _shadeRepository.AddAsync(newShade);

                UpdateCountryCollection(createdShade, shadeDto);
                await _shadeRepository.UpdateAsync(createdShade);
                return Ok(_mapper.Map<ShadeDto>(createdShade));
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside CreateShade action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> UpdateShade(ShadeDto shadeDto)
        {
            try
            {
                var filter = new ShadeFilter();
                filter.Id = shadeDto.Id;
                filter.LoadChildren = true;
                var spec = new ShadeSpecification(filter);
                var shade = await _shadeRepository.FirstAsync(spec);
                if (shade == null)
                {
                    return NotFound();
                }

                shade.ShadeNumber = shadeDto.ShadeNumber;
                shade.ShadeDescription = shadeDto.ShadeDescription;
                shade.Published = shadeDto.Published;
                shade.DateUpdated = DateTime.UtcNow;
                UpdateCountryCollection(shade, shadeDto);

                await _shadeRepository.UpdateAsync(shade);
                return Ok(_mapper.Map<ShadeDto>(shade));
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside UpdateShade action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }



        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task UpdateCountryCollection(Shade origShade, ShadeDto updateShade)
        {
            //add new countries
            //var standCountries = JsonConvert.DeserializeObject<List<CountryDto>>(updateStand.Countries);
            foreach (var country in updateShade.Countries)
            {
                var origCountry = origShade.Countries.FirstOrDefault(c => c.Id == country.Id);
                if (origCountry == null)
                {
                    var dbCountry = await _countryRepository.GetByIdAsync(country.Id);
                    if (dbCountry != null)
                    {
                        origShade.Countries.Add(dbCountry);
                    }
                }
            }
            //remove deleted countries
            for (int i = origShade.Countries.Count - 1; i >= 0; i--)
            {
                var origCountry = origShade.Countries[i];
                var updatedCountry = updateShade.Countries.FirstOrDefault(c => c.Id == origCountry.Id);
                if (updatedCountry == null)
                {
                    var dbCountry = await _countryRepository.GetByIdAsync(origCountry.Id);
                    origShade.Countries.Remove(dbCountry);
                }
            }

            //update Part.CountryList string
            origShade.CountryList = string.Join(",", origShade.Countries.Select(c => c.Id));
        }
    }





}
