using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Ardalis.Specification;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PM_AdminApp.Server.Controllers;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.PagedLists;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.ProductAggregate;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;

namespace PM_AdminApp.Server.Controllers.UnitTests
{
    [TestClass]
    public class ShadeControllerTests
    {
        /// <summary>
        /// Tests that the ShadeController constructor successfully initializes with all valid dependencies.
        /// Input: All required dependencies (IMapper, IAsyncRepositoryLong for Shade, IAsyncRepository for Country, IAsyncRepository for Category, ILogger).
        /// Expected: Constructor completes without throwing exceptions and instance is created successfully.
        /// </summary>
        [TestMethod]
        public void Constructor_WithAllValidDependencies_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();

            // Act
            var controller = new ShadeController(
                mockMapper.Object,
                mockShadeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that GetShades returns OkObjectResult with correct paged data when CountryList is null.
        /// Input: Valid ShadeFilterDto with null CountryList.
        /// Expected: Returns OkObjectResult with PagedShadesListDto containing correct data and pagination info.
        /// </summary>
        [TestMethod]
        public async Task GetShades_WithNullCountryList_ReturnsOkWithPagedData()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();

            var controller = new ShadeController(
                mockMapper.Object,
                mockShadeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new ShadeFilterDto
            {
                CountryList = null,
                IsPagingEnabled = true,
                Page = 1,
                PageSize = 10
            };
            var shadeFilter = new ShadeFilter();
            var shades = new List<Shade> { new Shade(), new Shade() };
            var shadeDtos = new List<ShadeDto> { new ShadeDto(), new ShadeDto() };

            mockMapper.Setup(m => m.Map<ShadeFilter>(filterDto)).Returns(shadeFilter);
            mockMapper.Setup(m => m.Map<List<ShadeDto>>(It.IsAny<IReadOnlyList<Shade>>())).Returns(shadeDtos);
            mockShadeRepository.Setup(r => r.ListAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(shades);
            mockShadeRepository.Setup(r => r.CountAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(25);

            // Act
            var result = await controller.GetShades(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var response = okResult.Value as PagedShadesListDto;
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(2, response.Data.Count);
            Assert.IsNotNull(response.Page);
            Assert.AreEqual(1, response.Page.PageNumber);
            Assert.AreEqual(25, response.Page.TotalItems);
            Assert.AreEqual(3, response.Page.TotalPages);
            Assert.AreEqual(10, response.Page.Size);
        }

        /// <summary>
        /// Tests that GetShades returns OkObjectResult with empty list when repository returns no shades.
        /// Input: Valid ShadeFilterDto.
        /// Expected: Returns OkObjectResult with empty data list and correct pagination.
        /// </summary>
        [TestMethod]
        public async Task GetShades_WithNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();

            var controller = new ShadeController(
                mockMapper.Object,
                mockShadeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new ShadeFilterDto
            {
                CountryList = null,
                IsPagingEnabled = true,
                Page = 1,
                PageSize = 10
            };
            var shadeFilter = new ShadeFilter();
            var emptyShades = new List<Shade>();
            var emptyShadeDtos = new List<ShadeDto>();

            mockMapper.Setup(m => m.Map<ShadeFilter>(filterDto)).Returns(shadeFilter);
            mockMapper.Setup(m => m.Map<List<ShadeDto>>(It.IsAny<IReadOnlyList<Shade>>())).Returns(emptyShadeDtos);
            mockShadeRepository.Setup(r => r.ListAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyShades);
            mockShadeRepository.Setup(r => r.CountAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act
            var result = await controller.GetShades(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var response = okResult.Value as PagedShadesListDto;
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(0, response.Data.Count);
            Assert.AreEqual(0, response.Page.TotalItems);
            Assert.AreEqual(0, response.Page.TotalPages);
        }

        /// <summary>
        /// Tests that GetShades correctly calculates total pages with various pagination scenarios.
        /// Input: Different combinations of total items and page sizes.
        /// Expected: Correctly calculates ceiling of division (total items / page size).
        /// </summary>
        [TestMethod]
        [DataRow(100, 10, 10, DisplayName = "Even division")]
        [DataRow(101, 10, 11, DisplayName = "Remainder division")]
        [DataRow(1, 10, 1, DisplayName = "Single item")]
        [DataRow(99, 10, 10, DisplayName = "Just below even")]
        [DataRow(50, 25, 2, DisplayName = "Exact half")]
        [DataRow(51, 25, 3, DisplayName = "Half plus one")]
        public async Task GetShades_WithVariousPaginationScenarios_CalculatesCorrectTotalPages(int totalItems, int pageSize, int expectedTotalPages)
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();

            var controller = new ShadeController(
                mockMapper.Object,
                mockShadeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new ShadeFilterDto
            {
                CountryList = null,
                IsPagingEnabled = true,
                Page = 1,
                PageSize = pageSize
            };
            var shadeFilter = new ShadeFilter();
            var shades = new List<Shade>();
            var shadeDtos = new List<ShadeDto>();

            mockMapper.Setup(m => m.Map<ShadeFilter>(filterDto)).Returns(shadeFilter);
            mockMapper.Setup(m => m.Map<List<ShadeDto>>(It.IsAny<IReadOnlyList<Shade>>())).Returns(shadeDtos);
            mockShadeRepository.Setup(r => r.ListAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(shades);
            mockShadeRepository.Setup(r => r.CountAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(totalItems);

            // Act
            var result = await controller.GetShades(filterDto);

            // Assert
            var okResult = (OkObjectResult)result;
            var response = okResult.Value as PagedShadesListDto;
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedTotalPages, response.Page.TotalPages);
        }

        /// <summary>
        /// Tests that GetShades returns 500 status code when mapper throws exception during ShadeFilter mapping.
        /// Input: ShadeFilterDto that causes mapper to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetShades_WhenMapperThrowsExceptionOnShadeFilter_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();

            var controller = new ShadeController(
                mockMapper.Object,
                mockShadeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new ShadeFilterDto { Page = 1, PageSize = 10 };
            var exceptionMessage = "Mapping to ShadeFilter failed";

            mockMapper.Setup(m => m.Map<ShadeFilter>(filterDto)).Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetShades(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetAllProducts action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetShades returns 500 status code when repository ListAsync throws exception.
        /// Input: Valid ShadeFilterDto but repository throws.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetShades_WhenRepositoryListAsyncThrows_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();

            var controller = new ShadeController(
                mockMapper.Object,
                mockShadeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new ShadeFilterDto { Page = 1, PageSize = 10 };
            var shadeFilter = new ShadeFilter();
            var exceptionMessage = "Database connection failed";

            mockMapper.Setup(m => m.Map<ShadeFilter>(filterDto)).Returns(shadeFilter);
            mockShadeRepository.Setup(r => r.ListAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetShades(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetAllProducts action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetShades returns 500 status code when repository CountAsync throws exception.
        /// Input: Valid ShadeFilterDto but CountAsync throws.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetShades_WhenRepositoryCountAsyncThrows_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();

            var controller = new ShadeController(
                mockMapper.Object,
                mockShadeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new ShadeFilterDto { Page = 1, PageSize = 10 };
            var shadeFilter = new ShadeFilter();
            var shades = new List<Shade>();
            var exceptionMessage = "Count query failed";

            mockMapper.Setup(m => m.Map<ShadeFilter>(filterDto)).Returns(shadeFilter);
            mockShadeRepository.Setup(r => r.ListAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(shades);
            mockShadeRepository.Setup(r => r.CountAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetShades(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetAllProducts action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetShades returns 500 status code when mapper throws exception during ShadeDto list mapping.
        /// Input: Valid ShadeFilterDto but mapper throws when mapping shades to DTOs.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetShades_WhenMapperThrowsExceptionOnShadeDtoList_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();

            var controller = new ShadeController(
                mockMapper.Object,
                mockShadeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new ShadeFilterDto { Page = 1, PageSize = 10 };
            var shadeFilter = new ShadeFilter();
            var shades = new List<Shade> { new Shade() };
            var exceptionMessage = "Mapping to ShadeDto list failed";

            mockMapper.Setup(m => m.Map<ShadeFilter>(filterDto)).Returns(shadeFilter);
            mockShadeRepository.Setup(r => r.ListAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(shades);
            mockShadeRepository.Setup(r => r.CountAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            mockMapper.Setup(m => m.Map<List<ShadeDto>>(It.IsAny<IReadOnlyList<Shade>>()))
                .Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetShades(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetAllProducts action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetShades logs information message on successful execution.
        /// Input: Valid ShadeFilterDto.
        /// Expected: Logs information message with "Returned all products from database."
        /// </summary>
        [TestMethod]
        public async Task GetShades_OnSuccess_LogsInformationMessage()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();

            var controller = new ShadeController(
                mockMapper.Object,
                mockShadeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new ShadeFilterDto { Page = 1, PageSize = 10 };
            var shadeFilter = new ShadeFilter();
            var shades = new List<Shade>();
            var shadeDtos = new List<ShadeDto>();

            mockMapper.Setup(m => m.Map<ShadeFilter>(filterDto)).Returns(shadeFilter);
            mockMapper.Setup(m => m.Map<List<ShadeDto>>(It.IsAny<IReadOnlyList<Shade>>())).Returns(shadeDtos);
            mockShadeRepository.Setup(r => r.ListAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(shades);
            mockShadeRepository.Setup(r => r.CountAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act
            await controller.GetShades(filterDto);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Returned all products from database.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetShades correctly handles CountryList with empty string.
        /// Input: ShadeFilterDto with empty string CountryList.
        /// Expected: Returns OkObjectResult with paged data without attempting to check all countries.
        /// </summary>
        [TestMethod]
        public async Task GetShades_WithEmptyStringCountryList_ReturnsOkWithPagedData()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();

            var controller = new ShadeController(
                mockMapper.Object,
                mockShadeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new ShadeFilterDto
            {
                CountryList = null,
                IsPagingEnabled = true,
                Page = 1,
                PageSize = 10
            };
            var shadeFilter = new ShadeFilter();
            var shades = new List<Shade>();
            var shadeDtos = new List<ShadeDto>();

            mockMapper.Setup(m => m.Map<ShadeFilter>(filterDto)).Returns(shadeFilter);
            mockMapper.Setup(m => m.Map<List<ShadeDto>>(It.IsAny<IReadOnlyList<Shade>>())).Returns(shadeDtos);
            mockShadeRepository.Setup(r => r.ListAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(shades);
            mockShadeRepository.Setup(r => r.CountAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act
            var result = await controller.GetShades(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that GetShades correctly handles CountryList that equals all countries.
        /// Input: ShadeFilterDto with CountryList that matches all countries.
        /// Expected: Returns OkObjectResult with paged data and CountryList set to null in filter.
        /// </summary>
        [TestMethod]
        public async Task GetShades_WithCountryListMatchingAllCountries_SetsCountryListToNull()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();

            var controller = new TestableShadeController(
                mockMapper.Object,
                mockShadeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                isAllCountriesResult: true);

            var filterDto = new ShadeFilterDto
            {
                CountryList = "US,UK,CA",
                IsPagingEnabled = true,
                Page = 1,
                PageSize = 10
            };
            var shadeFilter = new ShadeFilter();
            var shades = new List<Shade>();
            var shadeDtos = new List<ShadeDto>();

            mockMapper.Setup(m => m.Map<ShadeFilter>(filterDto)).Returns(shadeFilter);
            mockMapper.Setup(m => m.Map<CountryFilter>(It.IsAny<CountryFilter>())).Returns(new CountryFilter());
            mockMapper.Setup(m => m.Map<List<ShadeDto>>(It.IsAny<IReadOnlyList<Shade>>())).Returns(shadeDtos);
            mockShadeRepository.Setup(r => r.ListAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(shades);
            mockShadeRepository.Setup(r => r.CountAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);
            mockCountryRepository.Setup(r => r.CountAsync(It.IsAny<CountrySpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            // Act
            var result = await controller.GetShades(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.IsNull(filterDto.CountryList);
        }

        /// <summary>
        /// Tests that GetShades correctly handles CountryList that does not match all countries.
        /// Input: ShadeFilterDto with CountryList that doesn't match all countries.
        /// Expected: Returns OkObjectResult with paged data and CountryList remains unchanged.
        /// </summary>
        [TestMethod]
        public async Task GetShades_WithCountryListNotMatchingAllCountries_KeepsCountryListUnchanged()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockShadeRepository = new Mock<IAsyncRepositoryLong<Shade>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ProductsController>>();

            var controller = new TestableShadeController(
                mockMapper.Object,
                mockShadeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                isAllCountriesResult: false);

            var filterDto = new ShadeFilterDto
            {
                CountryList = "US,UK",
                IsPagingEnabled = true,
                Page = 1,
                PageSize = 10
            };
            var shadeFilter = new ShadeFilter();
            var shades = new List<Shade>();
            var shadeDtos = new List<ShadeDto>();

            mockMapper.Setup(m => m.Map<ShadeFilter>(filterDto)).Returns(shadeFilter);
            mockMapper.Setup(m => m.Map<List<ShadeDto>>(It.IsAny<IReadOnlyList<Shade>>())).Returns(shadeDtos);
            mockMapper.Setup(m => m.Map<CountryFilter>(It.IsAny<CountryFilter>())).Returns(new CountryFilter());
            mockShadeRepository.Setup(r => r.ListAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(shades);
            mockShadeRepository.Setup(r => r.CountAsync(It.IsAny<ShadeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);
            mockCountryRepository.Setup(r => r.CountAsync(It.IsAny<CountrySpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);

            // Act
            var result = await controller.GetShades(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual("US,UK", filterDto.CountryList);
        }

        /// <summary>
        /// Helper class to test IsAllCountries behavior without mocking protected methods.
        /// </summary>
        private class TestableShadeController : ShadeController
        {
            private readonly bool _isAllCountriesResult;

            public TestableShadeController(
                IMapper mapper,
                IAsyncRepositoryLong<Shade> shadeRepository,
                IAsyncRepository<Country> countryRepository,
                IAsyncRepository<Category> categoryRepository,
                ILogger<ProductsController> logger,
                bool isAllCountriesResult)
                : base(mapper, shadeRepository, countryRepository, categoryRepository, logger)
            {
                _isAllCountriesResult = isAllCountriesResult;
            }

            internal new Task<bool> IsAllCountries(string countryList, IAsyncRepository<Country> countryRepository, IMapper mapper)
            {
                return Task.FromResult(_isAllCountriesResult);
            }
        }
    }
}