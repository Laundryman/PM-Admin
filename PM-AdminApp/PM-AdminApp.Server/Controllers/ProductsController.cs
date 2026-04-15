using AutoMapper;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
using PMApplication.Services;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using PMInfrastructure.Repositories;
using Page = PMApplication.Dtos.Page;

namespace PM_AdminApp.Server.Controllers
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
        private readonly IAsyncRepository<Region> _regionRepository;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;


        public ProductsController(IMapper mapper, IAsyncRepositoryLong<Product> asyncProductRepository,
            IAsyncRepository<Country> countryRepository, IAsyncRepository<Category> categoryRepository,
            ILogger<ProductsController> logger, IProductRepository productRepository, IAsyncRepositoryLong<Shade> shadeRepository, IAsyncRepository<Region> regionRepository, BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _logger = logger;
            _productRepository = productRepository;
            _asyncProductRepository = asyncProductRepository;
            _countryRepository = countryRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _shadeRepository = shadeRepository;
            _regionRepository = regionRepository;
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
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

        [HttpPost]
        public async Task<IActionResult> SaveProduct([FromForm] ProductUpdateDto updateProduct)
        {
            try
            {
                if (updateProduct == null)
                {
                    _logger.LogError("Product object sent from client is null.");
                    return BadRequest("Product object is null");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid part object sent from client.");
                    return BadRequest("Invalid model object");
                }
                //var stand = _mapper.Map<Stand>(updateProduct);

                var id = updateProduct.Id;
                var productFilter = new ProductFilter() { Id = id, LoadChildren = true};

                var spec = new ProductSpecification(productFilter);
                var productEdit = await _productRepository.FirstAsync(spec);
                if (productEdit == null)
                {
                    _logger.LogError($"Stand with id: {id}, hasn't been found in db.");
                    return NotFound();
                }

                _mapper.Map(updateProduct, productEdit);
                var category = await _categoryRepository.GetByIdAsync(updateProduct.CategoryId);
                if (category != null)
                {
                    productEdit.CategoryName = category.Name;
                    productEdit.ParentCategoryName = category.ParentCategoryName;
                }
                if (updateProduct.DateCreated == null)
                {
                    productEdit.DateCreated = DateTime.Now;
                }
                productEdit.DateUpdated = DateTime.Now;
                productEdit.DateAvailable = DateTime.Now;

                await _productRepository.UpdateAsync(productEdit);

                //Now manage relationships.
                await UpdateCountryCollection(productEdit, updateProduct);
                await UpdateRegionsCollection(productEdit, updateProduct);
                await UpdateProductImage(productEdit, updateProduct);
                await _productRepository.UpdateAsync(productEdit);
                return Ok(_mapper.Map<ProductDto>(productEdit));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside UpdateStand action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductUpdateDto productDto)
        {
            try
            {
                var newProduct = _mapper.Map<Product>(productDto);
                newProduct.Regions = new List<Region>();

                var category = await _categoryRepository.GetByIdAsync(productDto.CategoryId);
                if (category != null)
                {
                    newProduct.CategoryName = category.Name;
                    newProduct.ParentCategoryName = category.ParentCategoryName;
                }
                newProduct.DateCreated = DateTime.Now;
                newProduct.DateUpdated = DateTime.Now;
                newProduct.DateAvailable = DateTime.Now;
                var createdProduct = await _productRepository.AddAsync(newProduct);

                await UpdateRegionsCollection(createdProduct, productDto);
                await UpdateCountryCollection(createdProduct, productDto);
                await UpdateProductImage(createdProduct, productDto);

                await _productRepository.UpdateAsync(createdProduct);
                return Ok(_mapper.Map<ProductDto>(createdProduct));
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside CreateProduct action: {ex.Message}");
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

        private async Task UpdateRegionsCollection(Product origProduct, ProductUpdateDto updateProduct)
        {
            //var regionDtos = JsonConvert.DeserializeObject<List<RegionDto>>(updateProduct.Regions);
            var regionDtos = JsonSerializer.Deserialize<List<RegionDto>>(updateProduct.Regions);
            foreach (var region in regionDtos)
            {
                var origRegion = origProduct.Regions.FirstOrDefault(r => r.Id == region.Id);
                if (origRegion == null)
                {

                    var dbRegion = await _regionRepository.GetByIdAsync(region.Id);
                    origProduct.Regions.Add(dbRegion);
                }
            }
            for (int i = origProduct.Regions.Count - 1; i >= 0; i--)
            {
                var origRegion = origProduct.Regions[i];
                var updatedRegion = regionDtos.FirstOrDefault(r => r.Id == origRegion.Id);
                if (updatedRegion == null)
                {
                    var dbRegion = await _regionRepository.GetByIdAsync(origRegion.Id);
                    origProduct.Regions.Remove(dbRegion);
                }
            }

            //update Part.RegionList string
            origProduct.RegionsList = string.Join(",", origProduct.Regions.Select(r => r.Id));
        }

        private async Task UpdateCountryCollection(Product origProduct, ProductUpdateDto updateProduct)
        {
            //add new countries
            var productountries = JsonSerializer.Deserialize<List<CountryDto>>(updateProduct.Countries);
            foreach (var country in productountries)
            {
                var origCountry = origProduct.Countries.FirstOrDefault(c => c.Id == country.Id);
                if (origCountry == null)
                {
                    var dbCountry = await _countryRepository.GetByIdAsync(country.Id);
                    if (dbCountry != null)
                    {
                        origProduct.Countries.Add(dbCountry);
                    }
                }
            }
            //remove deleted countries
            for (int i = 0; i < origProduct.Countries.Count; i++)
            {
                var origCountry = origProduct.Countries[i];
                var updatedCountry = productountries.FirstOrDefault(c => c.Id == origCountry.Id);
                if (updatedCountry == null)
                {
                    //var dbCountry = await _countryRepository.GetByIdAsync(origCountry.Id);
                    origProduct.Countries.Remove(origCountry);
                }
            }

            //update Part.CountryList string
            origProduct.CountriesList = string.Join(",", origProduct.Countries.Select(c => c.Id));
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task UpdateProductImage(Product origProduct, ProductUpdateDto updateProduct)
        {
            //handle file upload if a new file is provided
            var blobService = new AzureBlobService(_blobServiceClient);
            var storeName = _configuration["AzureBlob:StoreName"];
            var blobServiceClient = blobService.GetBlobServiceClient(storeName);

            var fileName = origProduct.Id + "_" + origProduct.Name.Replace(" ", "");


            if (updateProduct.ImageFile != null && updateProduct.ImageFile.Length > 0)
            {
                var fileType = updateProduct.ImageFile.FileName.Split('.')[1];
                origProduct.ProductImage = fileName + "." + fileType;
                var containerName = _configuration["AzureBlob:ProductStoreContainer"];
                var containerClient = blobService.GetBlobContainerClient(blobServiceClient, containerName);
                await blobService.UploadFormFileAsync(containerClient, updateProduct.ImageFile, origProduct.ProductImage);
            }
        }

    }
}
