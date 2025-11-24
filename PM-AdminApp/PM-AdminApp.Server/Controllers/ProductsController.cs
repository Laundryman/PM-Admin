using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using Page = PMApplication.Dtos.Page;

namespace LMXApi.Controllers
{
    [Authorize]
    [Route("api/products/[action]")]
    [ApiController]
    public class ProductsController : BaseController
    {
        private readonly IAppLogger<ProductsController> _logger;
        private readonly IMapper _mapper;
        private readonly IAsyncRepositoryLong<Product> _productRepository;
        private readonly IAsyncRepository<Country> _countryRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;



        public ProductsController(IMapper mapper, IAsyncRepositoryLong<Product> productRepository,
            IAsyncRepository<Country> countryRepository, IAsyncRepository<Category> categoryRepository,
            IAppLogger<ProductsController> logger)
        {
            _logger = logger;
            _productRepository = productRepository;
            _countryRepository = countryRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filterDto)
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

                var spec = new ProductSpecification(_mapper.Map<ProductFilter>(filterDto));
                var products = await _productRepository.ListAsync(spec);
                var countFilter = filterDto;
                countFilter.IsPagingEnabled = false;
                var countSpec = new ProductSpecification(_mapper.Map<ProductFilter>(countFilter));
                int totalItems = await _productRepository.CountAsync(countSpec);
                _logger.LogInformation($"Returned all products from database.");

                
                var response = new PagedProductsListDto();


                response.Data = _mapper.Map<List<ProductDto>>(products);
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

        [HttpGet(Name = "ProductSelectList")]
        public async Task<IActionResult> GetProductSelectList([FromQuery] ProductFilterDto filterDto)
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

                var spec = new ProductSpecification(_mapper.Map<ProductFilter>(filterDto));
                var products = await _productRepository.ListAsync(spec);

                var ProductSelectList = CreateSelectList(products);
                return Ok(ProductSelectList);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetProduct action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}", Name = "ProductById")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);

                if (product == null)
                {
                    _logger.LogWarning($"Product with id: {id}, hasn't been found in db.");
                    return NotFound();
                }
                else
                {
                    _logger.LogInformation($"Returned product with id: {id}");
                    var response = _mapper.Map<FullProductDto>(product);

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetProductById action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        private List<ProductListDto> CreateSelectList(IReadOnlyList<Product> list)
        {
            var selectList = new List<ProductListDto>();
            var productSelect = new ProductListDto("Select Product");
            productSelect.Id = 0;

            selectList.Add(productSelect);

            for (int i = 0; i < list.Count; i++)
            {
                selectList.Add(_mapper.Map<ProductListDto>(list[i]));
            }

            return selectList;
        }

        private async Task<List<Product>> GetProductsFromCountryList(string countryList, IReadOnlyList<Product> products)
        {
            //we need to filter only products that have the at least one of the countries that are required
            var requiredCountryList = countryList.Split(',');var filteredProducts = new List<Product>();

            foreach (var product in products)
            {
                if (product.CountryList != null)
                {
                    var productCountryList = product.CountryList.Split(",");

                    foreach (var country in productCountryList)
                    {
                        if (requiredCountryList.Contains(country))
                        {
                            filteredProducts.Add(product);
                            break;
                        }
                    }
                }
            }

            return filteredProducts;

        }
    }
}
