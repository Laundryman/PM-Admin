using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;

namespace PM_AdminApp.Server.Controllers
{
    //[Authorize]
    [Route("api/brands/[action]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly ILogger<BrandController> _logger;

        private readonly IMapper _mapper;
        private readonly IAsyncRepository<Brand> _brandRepository;


        public BrandController(IMapper mapper, IAsyncRepository<Brand> brandRepository, ILogger<BrandController> logger)
        {
            _logger = logger;
            _brandRepository = brandRepository;
            _mapper = mapper;
        }

        [HttpGet(Name = "Brands")]
        public async Task<IActionResult> GetBrands([FromQuery] BrandFilterDto filterDto)
        {
            try
            {
                var spec = new BrandSpecification(_mapper.Map<BrandFilter>(filterDto));
                var brands = await _brandRepository.ListAsync(spec);

                return Ok(brands);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetPartTypes action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet(Name = "BrandSelectList")]
        public async Task<IActionResult> GetBrandSelectList([FromQuery] BrandFilterDto filterDto)
        {
            try
            {
                var spec = new BrandSpecification(_mapper.Map<BrandFilter>(filterDto));
                var brands = await _brandRepository.ListAsync(spec);

                var brandsSelectList = CreateBrandSelectList(brands);
                return Ok(brandsSelectList);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetCountries action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        private List<BrandDto> CreateBrandSelectList(IReadOnlyList<Brand> list)
        {
            var selectList = new List<BrandDto>();
            var brandSelect = new BrandDto { Name = "Select Brand" };
            brandSelect.Id = 0;

            selectList.Add(brandSelect);

            for (int i = 0; i < list.Count; i++)
            {
                selectList.Add(_mapper.Map<BrandDto>(list[i]));
            }

            return selectList;
        }

    }
}
