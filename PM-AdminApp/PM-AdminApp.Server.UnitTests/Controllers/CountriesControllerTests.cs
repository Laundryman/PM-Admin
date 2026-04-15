using Ardalis.Specification;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PM_AdminApp.Server.Controllers;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PM_AdminApp.Server.Controllers.UnitTests
{
    [TestClass]
    public partial class CountriesControllerTests
    {
        /// <summary>
        /// Tests that GetCountrySelectList returns 500 status code when mapper throws exception during filter mapping.
        /// Input: CountriesFilterDto that causes mapper to throw exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetCountrySelectList_WhenMapperThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var filterDto = new CountriesFilterDto
            {
                RegionId = 1
            };
            var exceptionMessage = "Mapping failed";
            mockMapper.Setup(m => m.Map<CountryFilter>(filterDto)).Throws(new Exception(exceptionMessage));
            // Act
            var result = await controller.GetCountrySelectList(filterDto);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetCountries action") && v.ToString()!.Contains(exceptionMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetCountrySelectList returns 500 status code when repository throws exception.
        /// Input: Valid CountriesFilterDto but repository.ListAsync throws exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetCountrySelectList_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var filterDto = new CountriesFilterDto
            {
                RegionId = 1
            };
            var countryFilter = new CountryFilter
            {
                RegionId = 1
            };
            var exceptionMessage = "Database connection failed";
            mockMapper.Setup(m => m.Map<CountryFilter>(filterDto)).Returns(countryFilter);
            mockCountryRepository.Setup(r => r.ListAsync(It.IsAny<CountrySpecification>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception(exceptionMessage));
            // Act
            var result = await controller.GetCountrySelectList(filterDto);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetCountries action") && v.ToString()!.Contains(exceptionMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetRegionById returns OkObjectResult with mapped RegionDto when a valid region is found.
        /// Input: Valid positive region ID that exists in the repository.
        /// Expected: Returns OkObjectResult containing the mapped RegionDto, logs information message.
        /// </summary>
        [TestMethod]
        [DataRow(1)]
        [DataRow(100)]
        [DataRow(int.MaxValue)]
        public async Task GetRegionById_WithValidIdAndRegionFound_ReturnsOkResultWithRegionDto(int id)
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var region = new Region
            {
                Name = "Test Region"
            };
            var regionDto = new RegionDto("Test Region")
            {
                Id = id
            };
            mockRegionRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(region);
            mockMapper.Setup(m => m.Map<RegionDto>(region)).Returns(regionDto);
            // Act
            var result = await controller.GetRegionById(id);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Assert.IsInstanceOfType(okResult.Value, typeof(RegionDto));
            var returnedDto = (RegionDto)okResult.Value;
            Assert.AreEqual(regionDto.Id, returnedDto.Id);
            Assert.AreEqual(regionDto.Name, returnedDto.Name);
            mockRegionRepository.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            mockMapper.Verify(m => m.Map<RegionDto>(region), Times.Once);
            mockLogger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Returned region with id: {id}")), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetRegionById returns NotFoundResult when region is not found in the repository.
        /// Input: Valid ID that does not exist in the repository (GetByIdAsync returns null).
        /// Expected: Returns NotFoundResult and logs warning message.
        /// </summary>
        [TestMethod]
        [DataRow(999)]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(int.MinValue)]
        public async Task GetRegionById_WithNonExistentId_ReturnsNotFoundResult(int id)
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            mockRegionRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Region? )null);
            // Act
            var result = await controller.GetRegionById(id);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockRegionRepository.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            mockMapper.Verify(m => m.Map<RegionDto>(It.IsAny<Region>()), Times.Never);
            mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Region with id: {id}, hasn't been found in db.")), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetRegionById returns 500 status code when repository throws an exception.
        /// Input: Valid ID, but GetByIdAsync throws an exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs error.
        /// </summary>
        [TestMethod]
        public async Task GetRegionById_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var exceptionMessage = "Database connection failed";
            var exception = new Exception(exceptionMessage);
            mockRegionRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);
            // Act
            var result = await controller.GetRegionById(1);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockMapper.Verify(m => m.Map<RegionDto>(It.IsAny<Region>()), Times.Never);
            mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetRegionById action") && v.ToString()!.Contains(exceptionMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetRegionById returns 500 status code when mapper throws an exception.
        /// Input: Valid ID with existing region, but mapper throws during mapping.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs error.
        /// </summary>
        [TestMethod]
        public async Task GetRegionById_WhenMapperThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var region = new Region
            {
                Name = "Test Region"
            };
            var exceptionMessage = "Mapping configuration error";
            var exception = new Exception(exceptionMessage);
            mockRegionRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(region);
            mockMapper.Setup(m => m.Map<RegionDto>(region)).Throws(exception);
            // Act
            var result = await controller.GetRegionById(1);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockRegionRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMapper.Verify(m => m.Map<RegionDto>(region), Times.Once);
            mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetRegionById action") && v.ToString()!.Contains(exceptionMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetRegionById handles edge case boundary values for ID parameter.
        /// Input: Edge case integer values including zero and negative values.
        /// Expected: Returns appropriate result based on whether region exists with that ID.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(-100)]
        [DataRow(int.MinValue)]
        public async Task GetRegionById_WithEdgeCaseIds_HandlesCorrectly(int id)
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var region = new Region
            {
                Name = "Edge Case Region"
            };
            var regionDto = new RegionDto("Edge Case Region")
            {
                Id = id
            };
            mockRegionRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(region);
            mockMapper.Setup(m => m.Map<RegionDto>(region)).Returns(regionDto);
            // Act
            var result = await controller.GetRegionById(id);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Assert.IsInstanceOfType(okResult.Value, typeof(RegionDto));
            mockRegionRepository.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that the CountriesController constructor successfully initializes with all valid dependencies.
        /// Input: All required dependencies (IMapper, IAsyncRepository for Country and Region, ILogger).
        /// Expected: Constructor completes without throwing exceptions and instance is created successfully.
        /// </summary>
        [TestMethod]
        public void Constructor_WithAllValidDependencies_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            // Act
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that GetAllCountries returns OkObjectResult with empty list when repository returns no results.
        /// Input: Valid regionId with no matching countries.
        /// Expected: Returns OkObjectResult with empty list, logs information message.
        /// </summary>
        [TestMethod]
        public async Task GetAllCountries_WithNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var emptyCountries = new List<Country>();
            var emptyDtos = new List<CountryDto>();
            mockCountryRepository.Setup(r => r.ListAsync(It.IsAny<ISpecification<Country>>(), It.IsAny<CancellationToken>())).ReturnsAsync(emptyCountries);
            mockMapper.Setup(m => m.Map<List<CountryDto>>(emptyCountries)).Returns(emptyDtos);
            // Act
            var result = await controller.GetAllCountries(1, "");
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var response = okResult.Value as List<CountryDto>;
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count);
            mockLogger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Returned all parts from database")), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetAllCountries returns 500 status code when repository throws an exception.
        /// Input: regionId that causes repository to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning with exception details.
        /// </summary>
        [TestMethod]
        public async Task GetAllCountries_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var exceptionMessage = "Database connection failed";
            mockCountryRepository.Setup(r => r.ListAsync(It.IsAny<ISpecification<Country>>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception(exceptionMessage));
            // Act
            var result = await controller.GetAllCountries(1, "");
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetCountries action") && v.ToString()!.Contains(exceptionMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetRegions returns OkObjectResult with empty list when repository returns no regions.
        /// Input: Valid RegionsFilterDto that results in no matches.
        /// Expected: Returns OkObjectResult containing an empty list.
        /// </summary>
        [TestMethod]
        public async Task GetRegions_WithNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var filterDto = new RegionsFilterDto
            {
                BrandId = 999
            };
            var regionFilter = new RegionFilter
            {
                BrandId = 999
            };
            var emptyRegions = new List<Region>();
            var emptyRegionDtos = new List<RegionDto>();
            mockMapper.Setup(m => m.Map<RegionFilter>(filterDto)).Returns(regionFilter);
            mockMapper.Setup(m => m.Map<List<RegionDto>>(emptyRegions)).Returns(emptyRegionDtos);
            mockRegionRepository.Setup(r => r.ListAsync(It.IsAny<RegionSpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(emptyRegions);
            // Act
            var result = await controller.GetRegions(filterDto);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var regions = okResult.Value as List<RegionDto>;
            Assert.IsNotNull(regions);
            Assert.AreEqual(0, regions.Count);
        }

        /// <summary>
        /// Tests that GetRegions returns 500 status code when mapper throws an exception.
        /// Input: RegionsFilterDto that causes mapper to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetRegions_WhenMapperThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var filterDto = new RegionsFilterDto
            {
                BrandId = 1
            };
            var exceptionMessage = "Mapping failed";
            mockMapper.Setup(m => m.Map<RegionFilter>(filterDto)).Throws(new Exception(exceptionMessage));
            // Act
            var result = await controller.GetRegions(filterDto);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetRegions action") && v.ToString()!.Contains(exceptionMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetRegions returns 500 status code when repository throws an exception.
        /// Input: Valid RegionsFilterDto but repository fails.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetRegions_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var filterDto = new RegionsFilterDto
            {
                BrandId = 1
            };
            var regionFilter = new RegionFilter
            {
                BrandId = 1
            };
            var exceptionMessage = "Database connection failed";
            mockMapper.Setup(m => m.Map<RegionFilter>(filterDto)).Returns(regionFilter);
            mockRegionRepository.Setup(r => r.ListAsync(It.IsAny<RegionSpecification>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception(exceptionMessage));
            // Act
            var result = await controller.GetRegions(filterDto);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetRegions action") && v.ToString()!.Contains(exceptionMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetRegions handles various filter properties correctly.
        /// Input: RegionsFilterDto with various property combinations.
        /// Expected: Returns OkObjectResult and mapper is called with correct filter values.
        /// </summary>
        [TestMethod]
        [DataRow(null, true, false, 0, 0, DisplayName = "Null BrandId with LoadChildren")]
        [DataRow(1, false, true, 1, 10, DisplayName = "With BrandId and paging enabled")]
        [DataRow(0, true, false, 0, 0, DisplayName = "Zero BrandId")]
        [DataRow(int.MaxValue, false, false, 0, 0, DisplayName = "Maximum BrandId value")]
        public async Task GetRegions_WithVariousFilterProperties_ReturnsOk(int? brandId, bool loadChildren, bool isPagingEnabled, int page, int pageSize)
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var filterDto = new RegionsFilterDto
            {
                BrandId = brandId,
                LoadChildren = loadChildren,
                IsPagingEnabled = isPagingEnabled,
                Page = page,
                PageSize = pageSize
            };
            var regionFilter = new RegionFilter
            {
                BrandId = brandId,
                LoadChildren = loadChildren
            };
            var regions = new List<Region>();
            mockMapper.Setup(m => m.Map<RegionFilter>(filterDto)).Returns(regionFilter);
            mockRegionRepository.Setup(r => r.ListAsync(It.IsAny<RegionSpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(regions);
            // Act
            var result = await controller.GetRegions(filterDto);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockMapper.Verify(m => m.Map<RegionFilter>(filterDto), Times.Once);
        }

        /// <summary>
        /// Tests that GetRegions handles ArgumentNullException from specification constructor.
        /// Input: Valid RegionsFilterDto but specification constructor throws ArgumentNullException.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetRegions_WhenSpecificationThrowsArgumentNullException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var filterDto = new RegionsFilterDto
            {
                BrandId = 1
            };
            var exceptionMessage = "Filter cannot be null";
            mockMapper.Setup(m => m.Map<RegionFilter>(filterDto)).Throws(new ArgumentNullException("filter", exceptionMessage));
            // Act
            var result = await controller.GetRegions(filterDto);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
        }

        /// <summary>
        /// Tests that GetRegions handles InvalidOperationException correctly.
        /// Input: Valid RegionsFilterDto but repository throws InvalidOperationException.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetRegions_WhenRepositoryThrowsInvalidOperationException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var filterDto = new RegionsFilterDto
            {
                BrandId = 1
            };
            var regionFilter = new RegionFilter
            {
                BrandId = 1
            };
            var exceptionMessage = "Invalid operation";
            mockMapper.Setup(m => m.Map<RegionFilter>(filterDto)).Returns(regionFilter);
            mockRegionRepository.Setup(r => r.ListAsync(It.IsAny<RegionSpecification>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException(exceptionMessage));
            // Act
            var result = await controller.GetRegions(filterDto);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetRegions action") && v.ToString()!.Contains(exceptionMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetAllRegions returns OkObjectResult with empty list when repository returns no data.
        /// Input: Valid brandId that yields no results.
        /// Expected: Returns OkObjectResult containing empty list, logs information message.
        /// </summary>
        [TestMethod]
        public async Task GetAllRegions_WithNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            int brandId = 999;
            var regionFilter = new RegionFilter
            {
                BrandId = brandId
            };
            var emptyRegions = new List<Region>();
            mockMapper.Setup(m => m.Map<RegionFilter>(It.IsAny<RegionsFilterDto>())).Returns(regionFilter);
            mockRegionRepository.Setup(r => r.ListAsync(It.IsAny<RegionSpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(emptyRegions);
            // Act
            var result = await controller.GetAllRegions(brandId);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var returnedRegions = okResult.Value as IReadOnlyList<Region>;
            Assert.IsNotNull(returnedRegions);
            Assert.AreEqual(0, returnedRegions.Count);
        }

        /// <summary>
        /// Tests that GetAllRegions correctly handles boundary values for brandId parameter.
        /// Input: Various boundary values including 0, int.MinValue, int.MaxValue, and negative values.
        /// Expected: Returns OkObjectResult with regions for all boundary values.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        [DataRow(-1)]
        [DataRow(-999)]
        public async Task GetAllRegions_WithBoundaryBrandIdValues_ReturnsOk(int brandId)
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var regionFilter = new RegionFilter
            {
                BrandId = brandId
            };
            var regions = new List<Region>();
            mockMapper.Setup(m => m.Map<RegionFilter>(It.IsAny<RegionsFilterDto>())).Returns(regionFilter);
            mockRegionRepository.Setup(r => r.ListAsync(It.IsAny<RegionSpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(regions);
            // Act
            var result = await controller.GetAllRegions(brandId);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that GetAllRegions handles various searchText parameter values correctly.
        /// Input: Valid brandId with different searchText values (null, empty, whitespace, normal string).
        /// Expected: Returns OkObjectResult regardless of searchText value (parameter is not used in implementation).
        /// </summary>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("search term")]
        [DataRow("very long search text that exceeds normal length expectations")]
        public async Task GetAllRegions_WithVariousSearchTextValues_ReturnsOk(string? searchText)
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            int brandId = 123;
            var regionFilter = new RegionFilter
            {
                BrandId = brandId
            };
            var regions = new List<Region>();
            mockMapper.Setup(m => m.Map<RegionFilter>(It.IsAny<RegionsFilterDto>())).Returns(regionFilter);
            mockRegionRepository.Setup(r => r.ListAsync(It.IsAny<RegionSpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(regions);
            // Act
            var result = await controller.GetAllRegions(brandId, searchText);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that GetAllRegions returns 500 status code when mapper throws an exception.
        /// Input: brandId that causes mapper to throw exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning with exception message.
        /// </summary>
        [TestMethod]
        public async Task GetAllRegions_WhenMapperThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            int brandId = 123;
            var exceptionMessage = "Mapping failed";
            mockMapper.Setup(m => m.Map<RegionFilter>(It.IsAny<RegionsFilterDto>())).Throws(new Exception(exceptionMessage));
            // Act
            var result = await controller.GetAllRegions(brandId);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetRegions action") && v.ToString()!.Contains(exceptionMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetAllRegions returns 500 status code when repository throws an exception.
        /// Input: Valid brandId but repository ListAsync throws exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning with exception message.
        /// </summary>
        [TestMethod]
        public async Task GetAllRegions_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            int brandId = 123;
            var regionFilter = new RegionFilter
            {
                BrandId = brandId
            };
            var exceptionMessage = "Database connection failed";
            mockMapper.Setup(m => m.Map<RegionFilter>(It.IsAny<RegionsFilterDto>())).Returns(regionFilter);
            mockRegionRepository.Setup(r => r.ListAsync(It.IsAny<RegionSpecification>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception(exceptionMessage));
            // Act
            var result = await controller.GetAllRegions(brandId);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetRegions action") && v.ToString()!.Contains(exceptionMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetAllRegions returns 500 status code when RegionSpecification constructor throws an exception.
        /// Input: Valid brandId but RegionSpecification creation fails.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning with exception message.
        /// </summary>
        [TestMethod]
        public async Task GetAllRegions_WhenSpecificationCreationFails_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            int brandId = 123;
            var exceptionMessage = "Specification creation failed";
            mockMapper.Setup(m => m.Map<RegionFilter>(It.IsAny<RegionsFilterDto>())).Throws(new InvalidOperationException(exceptionMessage));
            // Act
            var result = await controller.GetAllRegions(brandId);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetRegions action") && v.ToString()!.Contains(exceptionMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetAllRegions verifies the BrandId is correctly set on RegionsFilterDto.
        /// Input: Specific brandId value.
        /// Expected: Mapper receives RegionsFilterDto with BrandId property set to the provided brandId.
        /// </summary>
        [TestMethod]
        public async Task GetAllRegions_SetsCorrectBrandIdOnFilterDto()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            int brandId = 456;
            var regionFilter = new RegionFilter
            {
                BrandId = brandId
            };
            var regions = new List<Region>();
            RegionsFilterDto? capturedFilterDto = null;
            mockMapper.Setup(m => m.Map<RegionFilter>(It.IsAny<RegionsFilterDto>())).Callback<object>(dto => capturedFilterDto = dto as RegionsFilterDto).Returns(regionFilter);
            mockRegionRepository.Setup(r => r.ListAsync(It.IsAny<RegionSpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(regions);
            // Act
            var result = await controller.GetAllRegions(brandId);
            // Assert
            Assert.IsNotNull(capturedFilterDto);
            Assert.AreEqual(brandId, capturedFilterDto.BrandId);
        }

        /// <summary>
        /// Tests that GetCountriesFromList returns OkObjectResult with only default item when repository returns empty list.
        /// Input: Valid countryList string "999".
        /// Expected: Returns OkObjectResult containing only the default "Select Country" item.
        /// </summary>
        [TestMethod]
        public async Task GetCountriesFromList_WithNoResults_ReturnsOkWithDefaultItemOnly()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var countryList = "999";
            var countryFilter = new CountryFilter
            {
                CountryList = countryList
            };
            var emptyCountries = new List<Country>();
            mockMapper.Setup(m => m.Map<CountryFilter>(It.IsAny<CountryFilter>())).Returns(countryFilter);
            mockCountryRepository.Setup(r => r.ListAsync(It.IsAny<CountrySpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(emptyCountries);
            // Act
            var result = await controller.GetCountriesFromList(countryList);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var resultList = okResult.Value as List<CountryDto>;
            Assert.IsNotNull(resultList);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual(0, resultList[0].Id);
            Assert.AreEqual("Select Country", resultList[0].Name);
        }

        /// <summary>
        /// Tests that GetCountriesFromList returns 500 status code when mapper throws exception during filter mapping.
        /// Input: Valid countryList that causes mapper to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetCountriesFromList_WhenMapperThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var countryList = "1,2,3";
            var exceptionMessage = "Mapping failed";
            mockMapper.Setup(m => m.Map<CountryFilter>(It.IsAny<CountryFilter>())).Throws(new Exception(exceptionMessage));
            // Act
            var result = await controller.GetCountriesFromList(countryList);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetCountries action") && v.ToString()!.Contains(exceptionMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetCountriesFromList returns 500 status code when repository throws exception.
        /// Input: Valid countryList that causes repository to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetCountriesFromList_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var countryList = "1,2,3";
            var countryFilter = new CountryFilter
            {
                CountryList = countryList
            };
            var exceptionMessage = "Repository failed";
            mockMapper.Setup(m => m.Map<CountryFilter>(It.IsAny<CountryFilter>())).Returns(countryFilter);
            mockCountryRepository.Setup(r => r.ListAsync(It.IsAny<CountrySpecification>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception(exceptionMessage));
            // Act
            var result = await controller.GetCountriesFromList(countryList);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetCountries action") && v.ToString()!.Contains(exceptionMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetCountriesFromList returns 500 status code when null countryList is provided.
        /// Input: Null countryList parameter.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message due to NullReferenceException, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetCountriesFromList_WithNullCountryList_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            string? countryList = null;
            // Act
            var result = await controller.GetCountriesFromList(countryList!);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
            mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetCountries action")), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetCountriesFromList handles empty string countryList parameter correctly.
        /// Input: Empty string countryList "".
        /// Expected: Returns OkObjectResult with only default "Select Country" item.
        /// </summary>
        [TestMethod]
        public async Task GetCountriesFromList_WithEmptyStringCountryList_ReturnsOkWithDefaultItemOnly()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var countryList = "";
            var countryFilter = new CountryFilter
            {
                CountryList = countryList
            };
            var emptyCountries = new List<Country>();
            mockMapper.Setup(m => m.Map<CountryFilter>(It.IsAny<CountryFilter>())).Returns(countryFilter);
            mockCountryRepository.Setup(r => r.ListAsync(It.IsAny<CountrySpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(emptyCountries);
            // Act
            var result = await controller.GetCountriesFromList(countryList);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var resultList = okResult.Value as List<CountryDto>;
            Assert.IsNotNull(resultList);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual(0, resultList[0].Id);
        }

        /// <summary>
        /// Tests that GetCountriesFromList handles whitespace-only countryList parameter.
        /// Input: Whitespace-only countryList "   ".
        /// Expected: Returns OkObjectResult with only default "Select Country" item.
        /// </summary>
        [TestMethod]
        public async Task GetCountriesFromList_WithWhitespaceCountryList_ReturnsOkWithDefaultItemOnly()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var countryList = "   ";
            var countryFilter = new CountryFilter
            {
                CountryList = countryList
            };
            var emptyCountries = new List<Country>();
            mockMapper.Setup(m => m.Map<CountryFilter>(It.IsAny<CountryFilter>())).Returns(countryFilter);
            mockCountryRepository.Setup(r => r.ListAsync(It.IsAny<CountrySpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(emptyCountries);
            // Act
            var result = await controller.GetCountriesFromList(countryList);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var resultList = okResult.Value as List<CountryDto>;
            Assert.IsNotNull(resultList);
            Assert.AreEqual(1, resultList.Count);
        }

        /// <summary>
        /// Tests that GetCountriesFromList handles countryList with special characters.
        /// Input: CountryList with special characters "1,@,#".
        /// Expected: Returns OkObjectResult with countries from repository.
        /// </summary>
        [TestMethod]
        public async Task GetCountriesFromList_WithSpecialCharactersInCountryList_ReturnsOkWithCountries()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockLogger = new Mock<ILogger<CountriesController>>();
            var controller = new CountriesController(mockMapper.Object, mockCountryRepository.Object, mockRegionRepository.Object, mockLogger.Object);
            var countryList = "1,@,#";
            var countryFilter = new CountryFilter
            {
                CountryList = countryList
            };
            var emptyCountries = new List<Country>();
            mockMapper.Setup(m => m.Map<CountryFilter>(It.IsAny<CountryFilter>())).Returns(countryFilter);
            mockCountryRepository.Setup(r => r.ListAsync(It.IsAny<CountrySpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(emptyCountries);
            // Act
            var result = await controller.GetCountriesFromList(countryList);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var resultList = okResult.Value as List<CountryDto>;
            Assert.IsNotNull(resultList);
            Assert.AreEqual(1, resultList.Count);
        }
    }
}