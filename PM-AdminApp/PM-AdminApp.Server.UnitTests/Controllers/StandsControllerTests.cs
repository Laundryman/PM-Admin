using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PM_AdminApp.Server.Controllers;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.StandAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;

namespace PM_AdminApp.Server.Controllers.UnitTests
{
    /// <summary>
    /// Contains unit tests for the StandsController class.
    /// </summary>
    [TestClass]
    public class StandsControllerTests
    {
        /// <summary>
        /// Tests that SaveStand returns BadRequest when updateStand parameter is null.
        /// Input: null updateStand.
        /// Expected: Returns BadRequestObjectResult with message "Part object is null", logs error.
        /// </summary>
        [TestMethod]
        public async Task SaveStand_WithNullUpdateStand_ReturnsBadRequest()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            // Act
            var result = await controller.SaveStand(null!);

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
        /// Tests that SaveStand returns BadRequest when ModelState is invalid.
        /// Input: Valid updateStand with invalid ModelState.
        /// Expected: Returns BadRequestObjectResult with message "Invalid model object", logs error.
        /// </summary>
        [TestMethod]
        public async Task SaveStand_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            controller.ModelState.AddModelError("Name", "Name is required");

            var updateStand = new StandUpdateDto
            {
                Id = 1,
                Regions = new List<RegionDto>(),
                Countries = new List<CountryDto>()
            };

            // Act
            var result = await controller.SaveStand(updateStand);

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
        /// Tests that SaveStand returns NotFound when stand does not exist in database.
        /// Input: Valid updateStand with Id that doesn't exist in database.
        /// Expected: Returns NotFoundResult, logs error with stand id.
        /// </summary>
        [TestMethod]
        [DataRow(1)]
        [DataRow(999)]
        [DataRow(int.MaxValue)]
        public async Task SaveStand_WhenStandNotFoundInDatabase_ReturnsNotFound(int standId)
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            var updateStand = new StandUpdateDto
            {
                Id = standId,
                Regions = new List<RegionDto>(),
                Countries = new List<CountryDto>()
            };

            mockStandRepository
                .Setup(r => r.FirstAsync(It.IsAny<StandSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Stand)null!);

            // Act
            var result = await controller.SaveStand(updateStand);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Stand with id: {standId}, hasn't been found in db.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SaveStand returns 500 when repository FirstAsync throws exception.
        /// Input: Valid updateStand with repository throwing exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error", logs error.
        /// </summary>
        [TestMethod]
        public async Task SaveStand_WhenRepositoryFirstAsyncThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            var updateStand = new StandUpdateDto
            {
                Id = 1,
                Regions = new List<RegionDto>(),
                Countries = new List<CountryDto>()
            };

            var exceptionMessage = "Database connection failed";

            mockStandRepository
                .Setup(r => r.FirstAsync(It.IsAny<StandSpecification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.SaveStand(updateStand);

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
        /// Tests that SaveStand handles boundary value for stand Id = 0.
        /// Input: updateStand with Id = 0 that doesn't exist in database.
        /// Expected: Returns NotFoundResult, logs error.
        /// </summary>
        [TestMethod]
        public async Task SaveStand_WithStandIdZero_ReturnsNotFound()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            var updateStand = new StandUpdateDto
            {
                Id = 0,
                Regions = new List<RegionDto>(),
                Countries = new List<CountryDto>()
            };

            mockStandRepository
                .Setup(r => r.FirstAsync(It.IsAny<StandSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Stand)null!);

            // Act
            var result = await controller.SaveStand(updateStand);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        /// <summary>
        /// Tests that the StandsController constructor successfully initializes with all valid dependencies.
        /// Input: All required dependencies (IMapper, repositories, ILogger).
        /// Expected: Constructor completes without throwing exceptions and instance is created successfully.
        /// </summary>
        [TestMethod]
        public void Constructor_WithAllValidDependencies_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            // Act
            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that CreateStand returns 500 status code when mapper throws exception during DTO to entity mapping.
        /// Input: StandDto that causes mapper to throw during DTO to entity mapping.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs error.
        /// </summary>
        [TestMethod]
        public async Task CreateStand_WhenMapperThrowsExceptionOnDtoToEntity_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            var createDto = new StandDto { Id = 1 };
            var exceptionMessage = "Mapping from DTO to entity failed";

            mockMapper.Setup(m => m.Map<Stand>(createDto)).Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.CreateStand(createDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside CreateStand action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetStand returns NotFoundResult when stand is not found.
        /// Input: Non-existent stand id.
        /// Expected: Returns NotFoundResult, logs warning message.
        /// </summary>
        [TestMethod]
        public async Task GetStand_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<PMApplication.Entities.Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<PMApplication.Interfaces.RepositoryInterfaces.IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Region>>();

            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            int standId = 999;

            mockAsyncStandRepository.Setup(r => r.FirstOrDefaultAsync(It.IsAny<StandByIdSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Stand)null!);

            // Act
            var result = await controller.GetStand(standId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Stand with id: {standId}, hasn't been found in db.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetStand handles various edge case id values appropriately.
        /// Input: Edge case id values (zero, negative, int.MinValue, int.MaxValue).
        /// Expected: Returns NotFoundResult when stand is not found, no exceptions thrown.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(-100)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public async Task GetStand_WithEdgeCaseIdValues_HandlesAppropriately(int standId)
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<PMApplication.Entities.Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<PMApplication.Interfaces.RepositoryInterfaces.IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Region>>();

            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            mockAsyncStandRepository.Setup(r => r.FirstOrDefaultAsync(It.IsAny<StandByIdSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Stand)null!);

            // Act
            var result = await controller.GetStand(standId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Stand with id: {standId}, hasn't been found in db.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetStand returns 500 status code when repository throws an exception.
        /// Input: Stand id that causes repository to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs error.
        /// </summary>
        [TestMethod]
        public async Task GetStand_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<PMApplication.Entities.Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<PMApplication.Interfaces.RepositoryInterfaces.IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Region>>();

            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            int standId = 1;
            var exceptionMessage = "Database connection failed";

            mockAsyncStandRepository.Setup(r => r.FirstOrDefaultAsync(It.IsAny<StandByIdSpecification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetStand(standId);

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
        /// Tests that SearchStands returns OkObjectResult with stands when repository returns results.
        /// Input: Valid StandFilterDto.
        /// Expected: Returns OkObjectResult containing a list of SearchStandInfo.
        /// </summary>
        [TestMethod]
        public async Task SearchStands_WithValidFilter_ReturnsOkWithResults()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new StandFilterDto { BrandId = 1, RegionId = 5, CountryId = 10 };
            var expectedStands = new List<SearchStandInfo>
            {
                new SearchStandInfo(),
                new SearchStandInfo()
            };

            mockStandRepository.Setup(r => r.SearchStands(filterDto))
                .ReturnsAsync(expectedStands);

            // Act
            var result = await controller.SearchStands(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var stands = okResult.Value as IReadOnlyList<SearchStandInfo>;
            Assert.IsNotNull(stands);
            Assert.AreEqual(2, stands.Count);
        }

        /// <summary>
        /// Tests that SearchStands returns OkObjectResult with empty list when repository returns no results.
        /// Input: Valid StandFilterDto.
        /// Expected: Returns OkObjectResult containing an empty list.
        /// </summary>
        [TestMethod]
        public async Task SearchStands_WithNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new StandFilterDto { BrandId = 999, RegionId = null, CountryId = null };
            var emptyStands = new List<SearchStandInfo>();

            mockStandRepository.Setup(r => r.SearchStands(filterDto))
                .ReturnsAsync(emptyStands);

            // Act
            var result = await controller.SearchStands(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var stands = okResult.Value as IReadOnlyList<SearchStandInfo>;
            Assert.IsNotNull(stands);
            Assert.AreEqual(0, stands.Count);
        }

        /// <summary>
        /// Tests that SearchStands returns 500 status code when repository throws an exception.
        /// Input: StandFilterDto that causes repository to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task SearchStands_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new StandFilterDto { BrandId = 1 };
            var exceptionMessage = "Database connection failed";

            mockStandRepository.Setup(r => r.SearchStands(filterDto))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.SearchStands(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside SearchStands action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SearchStands returns 500 status code when repository throws InvalidOperationException.
        /// Input: StandFilterDto that causes repository to throw InvalidOperationException.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task SearchStands_WhenRepositoryThrowsInvalidOperationException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new StandFilterDto { BrandId = 0, RegionId = -1 };
            var exceptionMessage = "Invalid operation on repository";

            mockStandRepository.Setup(r => r.SearchStands(filterDto))
                .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            // Act
            var result = await controller.SearchStands(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside SearchStands action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SearchStands handles boundary value for BrandId (int.MinValue).
        /// Input: StandFilterDto with BrandId = int.MinValue.
        /// Expected: Returns OkObjectResult with empty list.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        [DataRow(0)]
        public async Task SearchStands_WithBoundaryBrandIdValues_ReturnsOk(int brandId)
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new StandFilterDto { BrandId = brandId };
            var emptyStands = new List<SearchStandInfo>();

            mockStandRepository.Setup(r => r.SearchStands(It.IsAny<StandFilterDto>()))
                .ReturnsAsync(emptyStands);

            // Act
            var result = await controller.SearchStands(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that SearchStands handles nullable RegionId and CountryId values correctly.
        /// Input: StandFilterDto with null RegionId and CountryId.
        /// Expected: Returns OkObjectResult with results from repository.
        /// </summary>
        [TestMethod]
        public async Task SearchStands_WithNullableRegionAndCountryIds_ReturnsOk()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandsController>>();
            var mockStandRepository = new Mock<IStandRepository>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();

            var controller = new StandsController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandRepository.Object,
                mockRegionRepository.Object);

            var filterDto = new StandFilterDto { BrandId = 1, RegionId = null, CountryId = null };
            var expectedStands = new List<SearchStandInfo> { new SearchStandInfo() };

            mockStandRepository.Setup(r => r.SearchStands(filterDto))
                .ReturnsAsync(expectedStands);

            // Act
            var result = await controller.SearchStands(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var stands = okResult.Value as IReadOnlyList<SearchStandInfo>;
            Assert.IsNotNull(stands);
            Assert.AreEqual(1, stands.Count);
        }
    }
}