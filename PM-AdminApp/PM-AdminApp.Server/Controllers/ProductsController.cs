using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.ProductAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
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
        private readonly ILogger<ProductsController> _logger;
        private readonly IMapper _mapper;
        private readonly IAsyncRepositoryLong<Product> _asyncProductRepository;
        private readonly IProductRepository _productRepository;
        private readonly IAsyncRepository<Country> _countryRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;
        private readonly IAsyncRepositoryLong<Shade> _shadeRepository;



        public ProductsController(IMapper mapper, IAsyncRepositoryLong<Product> asyncProductRepository,
            IAsyncRepository<Country> countryRepository, IAsyncRepository<Category> categoryRepository,
            ILogger<ProductsController> logger, IProductRepository productRepository, IAsyncRepositoryLong<Shade> shadeRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
            _asyncProductRepository = asyncProductRepository;
            _countryRepository = countryRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _shadeRepository = shadeRepository;
        }


        [HttpPost]
        public async Task<IActionResult> SearchProducts(ProductFilterDto filterDto)
        {
            try
            {
                //var spec = new ProductSpecification(_mapper.Map<ProductFilter>(filterDto));
                var products = await _productRepository.SearchProducts(filterDto);
                
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside SearchProducts action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filterDto)
        {
            try
            {
                //if (filterDto.CountriesList != null)
                //{
                //    var allCountries = await IsAllCountries(filterDto.CountriesList, _countryRepository, _mapper);
                //    if (allCountries)
                //    {
                //        filterDto.CountriesList = null;
                //    }
                //}

                //var spec = new ProductSpecification(_mapper.Map<ProductFilter>(filterDto));
                //var products = await _asyncProductRepository.ListAsync(spec);
                //_logger.LogInformation($"GetProducts returned successfully.");


                //var response = new PagedProductsListDto();


                //response.Data = _mapper.Map<List<ProductDto>>(products);
                //response.Page = new Page();
                //response.Page.PageNumber = filterDto.Page;
                //response.Page.TotalItems = totalItems;
                //response.Page.TotalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)filterDto.PageSize);
                //response.Page.Size = filterDto.PageSize;
                throw new NotImplementedException();
            }

            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetAllProducts action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpPost(Name = "ProductSelectList")]
        public async Task<IActionResult> GetProductsByCategory(ProductFilterDto filterDto)
        {
            try
            {
                if (filterDto.CountriesList != null)
                {
                    var allCountries = await IsAllCountries(filterDto.CountriesList, _countryRepository, _mapper);
                    if (allCountries)
                    {
                        filterDto.CountriesList = null;
                    }
                }

                var spec = new ProductSpecification(_mapper.Map<ProductFilter>(filterDto));
                var products = await _asyncProductRepository.ListAsync(spec);

                //var ProductSelectList = CreateSelectList(products);
                var response = _mapper.Map<IReadOnlyList<ProductDto>>(products);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetProduct action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet()]
        public async Task<IActionResult> GetProduct([FromQuery] int id)
        {
            try
            {
                var spec = new ProductByIdSpecification(id);

                var product = await _asyncProductRepository.FirstOrDefaultAsync(spec);

                if (product == null)
                {
                    _logger.LogWarning($"Product with id: {id}, hasn't been found in db.");
                    return NotFound();
                }
                else
                {
                    _logger.LogInformation($"Returned product with id: {id}");
                    var response = _mapper.Map<ProductDto>(product);

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetProductById action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet()]
        public async Task<IActionResult> GetShades([FromQuery] int productId)
        {
            try
            {
                var spec = new ShadeSpecification(new ShadeFilter { ProductId = productId });
                var shades = await _shadeRepository.ListAsync(spec);
                var response = _mapper.Map<IReadOnlyList<ShadeDto>>(shades);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetShades action: {ex.Message}");
                
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
                if (product.CountriesList != null)
                {
                    var productCountryList = product.CountriesList.Split(",");

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
