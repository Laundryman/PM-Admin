using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.PagedLists;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using System;
using Page = PMApplication.Dtos.Page;


namespace LMXApi.Controllers
{
    [Authorize]
    [Route("api/shades/[action]")]
    [ApiController]
    public class ShadeController : BaseController
    {
        private readonly IAppLogger<ProductsController> _logger;
        private readonly IMapper _mapper;
        private readonly IAsyncRepositoryLong<Shade> _shadeRepository;
        private readonly IAsyncRepository<Country> _countryRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;

        public ShadeController(IMapper mapper, IAsyncRepositoryLong<Shade> shadeRepository,
            IAsyncRepository<Country> countryRepository, IAsyncRepository<Category> categoryRepository,
            IAppLogger<ProductsController> logger)
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


    }



}
