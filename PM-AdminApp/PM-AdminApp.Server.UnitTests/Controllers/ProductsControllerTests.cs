using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Ardalis.Specification;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PM_AdminApp.Server.Controllers;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.ProductAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;

namespace PM_AdminApp.Server.Controllers.UnitTests
{
    [TestClass]
    public class ProductsControllerTests
    {
        /// <summary>
        /// Tests that SearchProducts returns OkObjectResult with products when repository returns results.
        /// Input: Valid ProductFilterDto.
        /// Expected: Returns OkObjectResult containing the list of SearchProductInfo.
        /// </summary>
        [TestMethod]
        public async Task SearchProducts_WithValidFilter_ReturnsOkWithResults()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto { BrandId = 1, CategoryId = 2 };
            var expectedProducts = new List<SearchProductInfo>
            {
                new SearchProductInfo(),
                new SearchProductInfo()
            };

            mockProductRepository.Setup(r => r.SearchProducts(filterDto))
                .ReturnsAsync(expectedProducts);

            // Act
            var result = await controller.SearchProducts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var products = okResult.Value as IReadOnlyList<SearchProductInfo>;
            Assert.IsNotNull(products);
            Assert.AreEqual(2, products.Count);
        }

        /// <summary>
        /// Tests that SearchProducts returns OkObjectResult with empty list when repository returns no results.
        /// Input: Valid ProductFilterDto that yields no results.
        /// Expected: Returns OkObjectResult containing an empty list.
        /// </summary>
        [TestMethod]
        public async Task SearchProducts_WithNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto { BrandId = 999 };
            var emptyProducts = new List<SearchProductInfo>();

            mockProductRepository.Setup(r => r.SearchProducts(filterDto))
                .ReturnsAsync(emptyProducts);

            // Act
            var result = await controller.SearchProducts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var products = okResult.Value as IReadOnlyList<SearchProductInfo>;
            Assert.IsNotNull(products);
            Assert.AreEqual(0, products.Count);
        }

        /// <summary>
        /// Tests that SearchProducts returns 500 status code when repository throws an exception.
        /// Input: ProductFilterDto that causes repository to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task SearchProducts_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto { BrandId = 1 };
            var exceptionMessage = "Database connection failed";

            mockProductRepository.Setup(r => r.SearchProducts(filterDto))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.SearchProducts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside SearchProducts action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SearchProducts returns OkObjectResult with results when filter has all properties set.
        /// Input: ProductFilterDto with all properties populated.
        /// Expected: Returns OkObjectResult containing the list of SearchProductInfo.
        /// </summary>
        [TestMethod]
        public async Task SearchProducts_WithCompleteFilter_ReturnsOkWithResults()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto
            {
                BrandId = 1,
                PartId = 2,
                RegionId = 3,
                CountryId = 4,
                CategoryId = 5,
                CountriesList = "US,CA,MX"
            };
            var expectedProducts = new List<SearchProductInfo> { new SearchProductInfo() };

            mockProductRepository.Setup(r => r.SearchProducts(filterDto))
                .ReturnsAsync(expectedProducts);

            // Act
            var result = await controller.SearchProducts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var products = okResult.Value as IReadOnlyList<SearchProductInfo>;
            Assert.IsNotNull(products);
            Assert.AreEqual(1, products.Count);
        }

        /// <summary>
        /// Tests that SearchProducts returns OkObjectResult with results when filter has default values.
        /// Input: ProductFilterDto with default/zero values.
        /// Expected: Returns OkObjectResult containing the list of SearchProductInfo.
        /// </summary>
        [TestMethod]
        public async Task SearchProducts_WithDefaultFilter_ReturnsOkWithResults()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto();
            var expectedProducts = new List<SearchProductInfo> { new SearchProductInfo() };

            mockProductRepository.Setup(r => r.SearchProducts(filterDto))
                .ReturnsAsync(expectedProducts);

            // Act
            var result = await controller.SearchProducts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
        }

        /// <summary>
        /// Tests that SearchProducts returns 500 status code when repository throws ArgumentException.
        /// Input: ProductFilterDto that causes repository to throw ArgumentException.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task SearchProducts_WhenRepositoryThrowsArgumentException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto { BrandId = -1 };
            var exceptionMessage = "Invalid brand ID";

            mockProductRepository.Setup(r => r.SearchProducts(filterDto))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await controller.SearchProducts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside SearchProducts action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SearchProducts returns 500 status code when repository throws InvalidOperationException.
        /// Input: ProductFilterDto that causes repository to throw InvalidOperationException.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task SearchProducts_WhenRepositoryThrowsInvalidOperationException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto { CategoryId = 100 };
            var exceptionMessage = "Invalid operation state";

            mockProductRepository.Setup(r => r.SearchProducts(filterDto))
                .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            // Act
            var result = await controller.SearchProducts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside SearchProducts action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetProductsByCategory returns OkObjectResult with empty list when repository returns no products.
        /// Input: ProductFilterDto with null CountriesList.
        /// Expected: Returns OkObjectResult with empty mapped product list.
        /// </summary>
        [TestMethod]
        public async Task GetProductsByCategory_WithNoProducts_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto { CountriesList = null };
            var productFilter = new ProductFilter();
            var products = new List<Product>();
            var productDtos = new List<ProductDto>();

            mockMapper.Setup(m => m.Map<ProductFilter>(filterDto)).Returns(productFilter);
            mockAsyncProductRepository.Setup(r => r.ListAsync(It.IsAny<ProductSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);
            mockMapper.Setup(m => m.Map<IReadOnlyList<ProductDto>>(products)).Returns(productDtos);

            // Act
            var result = await controller.GetProductsByCategory(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var resultList = okResult.Value as IReadOnlyList<ProductDto>;
            Assert.IsNotNull(resultList);
            Assert.AreEqual(0, resultList.Count);
        }

        /// <summary>
        /// Tests that GetProductsByCategory returns 500 status code when mapper throws exception during ProductFilter mapping.
        /// Input: ProductFilterDto that causes mapper to throw during ProductFilter mapping.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetProductsByCategory_WhenMapperThrowsExceptionOnProductFilterMapping_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto { CountriesList = null };
            var exceptionMessage = "Mapping to ProductFilter failed";

            mockMapper.Setup(m => m.Map<ProductFilter>(filterDto)).Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetProductsByCategory(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetProduct action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetProductsByCategory returns 500 status code when repository throws exception during ListAsync.
        /// Input: Valid ProductFilterDto.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetProductsByCategory_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto { CountriesList = null };
            var productFilter = new ProductFilter();
            var exceptionMessage = "Database connection failed";

            mockMapper.Setup(m => m.Map<ProductFilter>(filterDto)).Returns(productFilter);
            mockAsyncProductRepository.Setup(r => r.ListAsync(It.IsAny<ProductSpecification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetProductsByCategory(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetProduct action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetProductsByCategory returns 500 status code when IsAllCountries throws exception.
        /// Input: ProductFilterDto with non-null CountriesList that causes IsAllCountries to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetProductsByCategory_WhenIsAllCountriesThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto { CountriesList = "1,2,3" };
            var exceptionMessage = "Country repository failed";
            var countryFilter = new CountryFilter();

            mockMapper.Setup(m => m.Map<CountryFilter>(It.IsAny<CountryFilter>())).Returns(countryFilter);
            mockCountryRepository.Setup(r => r.CountAsync(It.IsAny<CountrySpecification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetProductsByCategory(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetProduct action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetProduct returns OkObjectResult with ProductDto when product exists.
        /// Input: Valid product id = 1.
        /// Expected: Returns OkObjectResult containing mapped ProductDto and logs information message.
        /// </summary>
        [TestMethod]
        public async Task GetProduct_WithValidIdAndProductExists_ReturnsOkWithProductDto()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var productId = 1;
            var product = new Product();
            var productDto = new ProductDto { Id = productId };

            mockAsyncProductRepository
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ProductByIdSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            mockMapper.Setup(m => m.Map<ProductDto>(product)).Returns(productDto);

            // Act
            var result = await controller.GetProduct(productId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(productDto, okResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Returned product with id: {productId}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetProduct returns NotFoundResult when product does not exist.
        /// Input: Valid product id = 999.
        /// Expected: Returns NotFoundResult and logs warning message.
        /// </summary>
        [TestMethod]
        public async Task GetProduct_WithValidIdAndProductNotFound_ReturnsNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var productId = 999;

            mockAsyncProductRepository
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ProductByIdSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await controller.GetProduct(productId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Product with id: {productId}, hasn't been found in db.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetProduct returns 500 status code when repository throws exception.
        /// Input: Valid product id = 1 with repository throwing exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs error.
        /// </summary>
        [TestMethod]
        public async Task GetProduct_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var productId = 1;
            var exceptionMessage = "Database connection failed";

            mockAsyncProductRepository
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ProductByIdSpecification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetProduct(productId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetProductById action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetProduct returns 500 status code when mapper throws exception.
        /// Input: Valid product id = 1 with mapper throwing exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs error.
        /// </summary>
        [TestMethod]
        public async Task GetProduct_WhenMapperThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var productId = 1;
            var product = new Product();
            var exceptionMessage = "Mapping failed";

            mockAsyncProductRepository
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ProductByIdSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            mockMapper.Setup(m => m.Map<ProductDto>(product)).Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetProduct(productId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetProductById action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetProduct handles edge case product id values correctly.
        /// Input: Edge case id values (0, negative, int.MaxValue, int.MinValue).
        /// Expected: Returns NotFoundResult when product is not found for edge case ids.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(-999)]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        public async Task GetProduct_WithEdgeCaseIds_ReturnsNotFoundWhenProductNotExists(int productId)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            mockAsyncProductRepository
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ProductByIdSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await controller.GetProduct(productId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Product with id: {productId}, hasn't been found in db.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetProduct returns OkObjectResult with ProductDto for edge case ids when product exists.
        /// Input: Edge case id values (0, int.MaxValue) where product exists.
        /// Expected: Returns OkObjectResult with ProductDto and logs information message.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(int.MaxValue)]
        public async Task GetProduct_WithEdgeCaseIdsAndProductExists_ReturnsOkWithProductDto(int productId)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var product = new Product();
            var productDto = new ProductDto { Id = productId };

            mockAsyncProductRepository
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ProductByIdSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            mockMapper.Setup(m => m.Map<ProductDto>(product)).Returns(productDto);

            // Act
            var result = await controller.GetProduct(productId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(productDto, okResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Returned product with id: {productId}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetProducts returns 500 status code with valid ProductFilterDto when NotImplementedException is thrown.
        /// Input: Valid ProductFilterDto object.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning with exception details.
        /// </summary>
        [TestMethod]
        public async Task GetProducts_WithValidFilterDto_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto
            {
                BrandId = 1,
                PartId = 2,
                RegionId = 3,
                CountryId = 4,
                CategoryId = 5,
                CountriesList = "1,2,3"
            };

            // Act
            var result = await controller.GetProducts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetAllProducts action")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetProducts returns 500 status code with null ProductFilterDto when NotImplementedException is thrown.
        /// Input: Null ProductFilterDto.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning with exception details.
        /// </summary>
        [TestMethod]
        public async Task GetProducts_WithNullFilterDto_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            // Act
            var result = await controller.GetProducts(null!);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetAllProducts action")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetProducts returns 500 status code with empty ProductFilterDto when NotImplementedException is thrown.
        /// Input: ProductFilterDto with default values.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning with exception details.
        /// </summary>
        [TestMethod]
        public async Task GetProducts_WithEmptyFilterDto_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto();

            // Act
            var result = await controller.GetProducts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetAllProducts action")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetProducts logs the complete exception message including NotImplementedException details.
        /// Input: Valid ProductFilterDto.
        /// Expected: Logger receives warning message containing exception message "The method or operation is not implemented."
        /// </summary>
        [TestMethod]
        public async Task GetProducts_LogsCompleteExceptionMessage_WithNotImplementedExceptionDetails()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto { BrandId = 1 };

            // Act
            var result = await controller.GetProducts(filterDto);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()!.Contains("Something went wrong inside GetAllProducts action") &&
                        v.ToString()!.Contains("not implemented")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetProducts with boundary value BrandId int.MaxValue returns 500 status code.
        /// Input: ProductFilterDto with BrandId set to int.MaxValue.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message.
        /// </summary>
        [TestMethod]
        public async Task GetProducts_WithMaxIntBrandId_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto { BrandId = int.MaxValue };

            // Act
            var result = await controller.GetProducts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
        }

        /// <summary>
        /// Tests that GetProducts with boundary value BrandId int.MinValue returns 500 status code.
        /// Input: ProductFilterDto with BrandId set to int.MinValue.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message.
        /// </summary>
        [TestMethod]
        public async Task GetProducts_WithMinIntBrandId_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto { BrandId = int.MinValue };

            // Act
            var result = await controller.GetProducts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
        }

        /// <summary>
        /// Tests that GetProducts with null nullable properties in ProductFilterDto returns 500 status code.
        /// Input: ProductFilterDto with all nullable properties set to null.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message.
        /// </summary>
        [TestMethod]
        public async Task GetProducts_WithNullablePropertiesNull_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto
            {
                RegionId = null,
                CountryId = null,
                CategoryId = null,
                CountriesList = null
            };

            // Act
            var result = await controller.GetProducts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
        }

        /// <summary>
        /// Tests that GetProducts with empty string CountriesList returns 500 status code.
        /// Input: ProductFilterDto with empty string CountriesList.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message.
        /// </summary>
        [TestMethod]
        public async Task GetProducts_WithEmptyCountriesList_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto { CountriesList = string.Empty };

            // Act
            var result = await controller.GetProducts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
        }

        /// <summary>
        /// Tests that GetProducts with whitespace-only CountriesList returns 500 status code.
        /// Input: ProductFilterDto with whitespace-only CountriesList.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message.
        /// </summary>
        [TestMethod]
        public async Task GetProducts_WithWhitespaceCountriesList_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new ProductFilterDto { CountriesList = "   " };

            // Act
            var result = await controller.GetProducts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
        }

        /// <summary>
        /// Tests that SaveProduct returns BadRequest when updateProduct parameter is null.
        /// Input: null ProductUpdateDto.
        /// Expected: Returns BadRequestObjectResult with message "Part object is null" and logs error.
        /// </summary>
        [TestMethod]
        public async Task SaveProduct_WithNullUpdateProduct_ReturnsBadRequest()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            // Act
            var result = await controller.SaveProduct(null!);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual("Part object is null", badRequestResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Part object sent from client is null.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SaveProduct returns BadRequest when ModelState is invalid.
        /// Input: Valid ProductUpdateDto with invalid ModelState.
        /// Expected: Returns BadRequestObjectResult with message "Invalid model object" and logs error.
        /// </summary>
        [TestMethod]
        public async Task SaveProduct_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            controller.ModelState.AddModelError("Name", "Name is required");

            var updateProduct = new ProductUpdateDto { Id = 1 };

            // Act
            var result = await controller.SaveProduct(updateProduct);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual("Invalid model object", badRequestResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid part object sent from client.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SaveProduct returns NotFound when product does not exist in repository.
        /// Input: ProductUpdateDto with Id that doesn't exist.
        /// Expected: Returns NotFoundResult and logs error.
        /// </summary>
        [TestMethod]
        [DataRow(1L)]
        [DataRow(999L)]
        [DataRow(long.MaxValue)]
        public async Task SaveProduct_WithNonExistentProduct_ReturnsNotFound(long productId)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var updateProduct = new ProductUpdateDto
            {
                Id = productId,
                Regions = new List<RegionDto>(),
                Countries = new List<CountryDto>()
            };

            mockProductRepository.Setup(r => r.FirstAsync(
                It.IsAny<ProductSpecification>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await controller.SaveProduct(updateProduct);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Stand with id: {productId}, hasn't been found in db.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SaveProduct handles exceptions from FirstAsync and returns 500 error.
        /// Input: ProductUpdateDto where FirstAsync throws exception.
        /// Expected: Returns ObjectResult with status code 500, message "Internal server error", and logs error.
        /// </summary>
        [TestMethod]
        public async Task SaveProduct_WhenFirstAsyncThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var updateProduct = new ProductUpdateDto
            {
                Id = 1,
                Regions = new List<RegionDto>(),
                Countries = new List<CountryDto>()
            };

            var exceptionMessage = "Database connection failed";
            mockProductRepository.Setup(r => r.FirstAsync(
                It.IsAny<ProductSpecification>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.SaveProduct(updateProduct);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside UpdateStand action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SaveProduct handles edge case with Id = 0.
        /// Input: ProductUpdateDto with Id = 0.
        /// Expected: Returns NotFoundResult when product doesn't exist and logs error.
        /// </summary>
        [TestMethod]
        public async Task SaveProduct_WithIdZero_ReturnsNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var updateProduct = new ProductUpdateDto
            {
                Id = 0,
                Regions = new List<RegionDto>(),
                Countries = new List<CountryDto>()
            };

            mockProductRepository.Setup(r => r.FirstAsync(
                It.IsAny<ProductSpecification>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await controller.SaveProduct(updateProduct);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        /// <summary>
        /// Tests that SaveProduct handles edge case with negative Id.
        /// Input: ProductUpdateDto with Id = -1.
        /// Expected: Returns NotFoundResult when product doesn't exist and logs error.
        /// </summary>
        [TestMethod]
        public async Task SaveProduct_WithNegativeId_ReturnsNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var updateProduct = new ProductUpdateDto
            {
                Id = -1,
                Regions = new List<RegionDto>(),
                Countries = new List<CountryDto>()
            };

            mockProductRepository.Setup(r => r.FirstAsync(
                It.IsAny<ProductSpecification>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await controller.SaveProduct(updateProduct);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        /// <summary>
        /// Tests that GetShades returns OkObjectResult with empty list when repository returns no shades.
        /// Input: Valid productId with no matching shades.
        /// Expected: Returns OkObjectResult containing an empty list.
        /// </summary>
        [TestMethod]
        public async Task GetShades_WithNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var emptyShades = new List<Shade>();
            var emptyShadeDtos = new List<ShadeDto>();

            mockShadeRepository.Setup(r => r.ListAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyShades);
            mockMapper.Setup(m => m.Map<IReadOnlyList<ShadeDto>>(emptyShades))
                .Returns(emptyShadeDtos);

            // Act
            var result = await controller.GetShades(999);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var response = okResult.Value as IReadOnlyList<ShadeDto>;
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count);
        }

        /// <summary>
        /// Tests that GetShades returns 500 status code when repository throws an exception.
        /// Input: ProductId that causes repository to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs error.
        /// </summary>
        [TestMethod]
        public async Task GetShades_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            var exceptionMessage = "Database connection failed";
            mockShadeRepository.Setup(r => r.ListAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetShades(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetShades action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that the ProductsController constructor successfully initializes with all valid dependencies.
        /// Input: All required dependencies (IMapper, IAsyncRepositoryLong for Product and Shade, IAsyncRepository for Country, Category, and Region, ILogger, IProductRepository).
        /// Expected: Constructor completes without throwing exceptions and instance is created successfully.
        /// </summary>
        [TestMethod]
        public void Constructor_WithAllValidDependencies_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            // Act
            var controller = new ProductsController(
                mockMapper.Object,
                mockAsyncProductRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockProductRepository.Object,
                mockShadeRepository.Object,
                mockRegionRepository.Object);

            // Assert
            Assert.IsNotNull(controller);
        }
    }
}