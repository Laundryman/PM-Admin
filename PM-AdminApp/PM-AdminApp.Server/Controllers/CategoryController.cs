using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using PMInfrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMXApi.Controllers
{
    [Authorize]
    [Route("api/categories/[action]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        //private ILoggerManager _logger;
        //private IRepositoryWrapper _repository;
        //private ILoggerManager _logger;
        private readonly IAppLogger<CategoryController> _logger;
        private readonly IMapper _mapper;
        //private readonly IReadRepository<Part> _partReadRepository;
        //private readonly IAsyncRepository<PartType> _partTypeRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;


        public CategoryController(IMapper mapper, IAsyncRepository<Category> categoryRepository, IAppLogger<CategoryController> logger)
        {
            _logger = logger;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpGet(Name = "Categories")]
        public async Task<IActionResult> GetCategories([FromQuery] CategoryFilterDto filterDto)
        {
            try
            {
                var spec = new CategorySpecification(_mapper.Map<CategoryFilter>(filterDto));
                var categories = await _categoryRepository.ListAsync(spec);

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetPartTypes action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet(Name = "CategorySelectList")]
        public async Task<IActionResult> GetCategorySelectList([FromQuery] CategoryFilterDto filterDto)
        {
            try
            {
                var spec = new CategorySpecification(_mapper.Map<CategoryFilter>(filterDto));
                var categories = await _categoryRepository.ListAsync(spec);

                var categoriesSelectList = CreateCategorySelectList(categories);
                return Ok(categoriesSelectList);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetCountries action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        private List<CategoryDto> CreateCategorySelectList(IReadOnlyList<Category> list)
        {
            var selectList = new List<CategoryDto>();
            var categorySelect = new CategoryDto("Select Category");
            categorySelect.Id = 0;

            selectList.Add(categorySelect);

            for (int i = 0; i < list.Count; i++)
            {
                selectList.Add(_mapper.Map<CategoryDto>(list[i]));
            }

            return selectList;
        }

    }
}
