using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos.Categories;
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

namespace PM_AdminApp.Server.Controllers
{
    [Authorize]
    [Route("api/categories/[action]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        //private ILoggerManager _logger;
        //private IRepositoryWrapper _repository;
        //private ILoggerManager _logger;
        private readonly ILogger<CategoryController> _logger;
        private readonly IMapper _mapper;
        //private readonly IReadRepository<Part> _partReadRepository;
        //private readonly IAsyncRepository<PartType> _partTypeRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;


        public CategoryController(IMapper mapper, IAsyncRepository<Category> categoryRepository, ILogger<CategoryController> logger)
        {
            _logger = logger;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpPost(Name = "Categories")]
        public async Task<IActionResult> GetCategories(CategoryFilterDto filterDto)
        {
            try
            {
                var spec = new CategorySpecification(_mapper.Map<CategoryFilter>(filterDto));
                var categories = await _categoryRepository.ListAsync(spec);

                foreach (var cat in categories)
                {
                    // Do something with each category
                    if (cat.ParentCategoryId == 0)
                    {
                        cat.ParentCategoryName = "1 PARENT CATEGORIES";
                    }
                }
                if (filterDto.GetParents)
                {
                    var mappedPCats = _mapper.Map<List<ParentCategoryDto>>(categories);
                    return Ok(mappedPCats);
                }
                var mappedCats = _mapper.Map<List<CategoryDto>>(categories);
                
                return Ok(mappedCats);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetCategories action: {ex.Message}");
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
