using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using ApplicationCore.Entities;
using Ardalis.Specification;
using AutoMapper;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using PM_AdminApp.Server.Controllers;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.StandTypes;
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

namespace PM_AdminApp.Server.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the PartController class.
    /// </summary>
    [TestClass]
    public class PartControllerTests
    {
        /// <summary>
        /// Tests that the PartController constructor successfully initializes with all valid dependencies.
        /// Input: All required dependencies (IMapper, IAsyncRepository instances, ILogger, IPartRepository, BlobServiceClient, IConfiguration).
        /// Expected: Constructor completes without throwing exceptions and instance is created successfully.
        /// </summary>
        [TestMethod]
        public void Constructor_WithAllValidDependencies_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            // Act
            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that GetPart returns NotFoundResult when part is not found in repository.
        /// Input: Part id that does not exist (999).
        /// Expected: Returns NotFoundResult and logs warning message.
        /// </summary>
        [TestMethod]
        public async Task GetPart_WhenPartNotFound_ReturnsNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockMapper = new Mock<IMapper>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockCountryRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<PMApplication.Entities.ProductAggregate.Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<PMApplication.Entities.StandAggregate.StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            int partId = 999;

            mockPartAsyncRepository.Setup(r => r.FirstAsync(It.IsAny<GetPartSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Part?)null);

            // Act
            var result = await controller.GetPart(partId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Part with id: {partId}, hasn't been found in db.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetPart returns 500 status code when repository throws an exception.
        /// Input: Part id that causes repository to throw exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs error.
        /// </summary>
        [TestMethod]
        public async Task GetPart_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockMapper = new Mock<IMapper>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockCountryRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<PMApplication.Entities.ProductAggregate.Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<PMApplication.Entities.StandAggregate.StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            int partId = 1;
            var exceptionMessage = "Database connection failed";

            mockPartAsyncRepository.Setup(r => r.FirstAsync(It.IsAny<GetPartSpecification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetPart(partId);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetPartById action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetPart handles edge case of id = 0.
        /// Input: Part id = 0.
        /// Expected: Creates PartFilter with id = 0, retrieves part or returns NotFound appropriately.
        /// </summary>
        [TestMethod]
        public async Task GetPart_WithIdZero_HandlesCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockMapper = new Mock<IMapper>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockCountryRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<PMApplication.Entities.ProductAggregate.Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<PMApplication.Entities.StandAggregate.StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            int partId = 0;

            mockPartAsyncRepository.Setup(r => r.FirstAsync(It.IsAny<GetPartSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Part?)null);

            // Act
            var result = await controller.GetPart(partId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        /// <summary>
        /// Tests that GetPart handles edge case of negative id.
        /// Input: Part id = -1.
        /// Expected: Creates PartFilter with id = -1, retrieves part or returns NotFound appropriately.
        /// </summary>
        [TestMethod]
        public async Task GetPart_WithNegativeId_HandlesCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockMapper = new Mock<IMapper>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockCountryRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<PMApplication.Entities.ProductAggregate.Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<PMApplication.Entities.StandAggregate.StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            int partId = -1;

            mockPartAsyncRepository.Setup(r => r.FirstAsync(It.IsAny<GetPartSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Part?)null);

            // Act
            var result = await controller.GetPart(partId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        /// <summary>
        /// Tests that GetPart handles edge case of int.MinValue.
        /// Input: Part id = int.MinValue.
        /// Expected: Creates PartFilter with id = int.MinValue, retrieves part or returns NotFound appropriately.
        /// </summary>
        [TestMethod]
        public async Task GetPart_WithIntMinValue_HandlesCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockMapper = new Mock<IMapper>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockCountryRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<PMApplication.Entities.ProductAggregate.Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<PMApplication.Entities.StandAggregate.StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            int partId = int.MinValue;

            mockPartAsyncRepository.Setup(r => r.FirstAsync(It.IsAny<GetPartSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Part?)null);

            // Act
            var result = await controller.GetPart(partId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        /// <summary>
        /// Tests that GetPart handles edge case of int.MaxValue.
        /// Input: Part id = int.MaxValue.
        /// Expected: Creates PartFilter with id = int.MaxValue, retrieves part or returns NotFound appropriately.
        /// </summary>
        [TestMethod]
        public async Task GetPart_WithIntMaxValue_HandlesCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockMapper = new Mock<IMapper>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockCountryRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<PMApplication.Entities.ProductAggregate.Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<PMApplication.Entities.StandAggregate.StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<PMApplication.Entities.CountriesAggregate.Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            int partId = int.MaxValue;

            mockPartAsyncRepository.Setup(r => r.FirstAsync(It.IsAny<GetPartSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Part?)null);

            // Act
            var result = await controller.GetPart(partId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        /// <summary>
        /// Tests that GetFilteredParts returns OkObjectResult with empty list when repositories return no data.
        /// Input: Valid PartFilterDto.
        /// Expected: Returns OkObjectResult containing PagedPartsListDto with empty list.
        /// </summary>
        [TestMethod]
        public async Task GetFilteredParts_WithNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var filterDto = new PartFilterDto { Id = 999, BrandId = 999 };
            var partFilter = new PartFilter { Id = 999, BrandId = 999 };
            var emptyPartTypes = new List<PartType>();
            var emptyCategories = new List<Category>();
            var emptyParts = new List<Part>();
            var emptyPartListDtos = new List<PartListDto>();

            mockPartTypeRepository.Setup(r => r.ListAsync(It.IsAny<ISpecification<PartType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyPartTypes);
            mockCategoryRepository.Setup(r => r.ListAsync(It.IsAny<ISpecification<Category>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyCategories);
            mockMapper.Setup(m => m.Map<PartFilter>(filterDto)).Returns(partFilter);
            mockPartAsyncRepository.Setup(r => r.ListAsync(It.IsAny<ISpecification<Part>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyParts);
            mockMapper.Setup(m => m.Map<List<PartListDto>>(emptyParts)).Returns(emptyPartListDtos);

            // Act
            var result = await controller.GetFilteredParts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var response = okResult.Value as PagedPartsListDto;
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(0, response.Data.Count);
        }

        /// <summary>
        /// Tests that GetFilteredParts returns 500 status code when PartTypeRepository throws an exception.
        /// Input: Valid PartFilterDto that causes PartTypeRepository to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetFilteredParts_WhenPartTypeRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var filterDto = new PartFilterDto { Id = 1 };
            var exceptionMessage = "Database connection failed";

            mockPartTypeRepository.Setup(r => r.ListAsync(It.IsAny<ISpecification<PartType>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetFilteredParts(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetAllParts action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetFilteredParts handles null or default PartFilterDto correctly.
        /// Input: Empty PartFilterDto with default values.
        /// Expected: Returns OkObjectResult with PagedPartsListDto.
        /// </summary>
        [TestMethod]
        public async Task GetFilteredParts_WithDefaultFilter_ReturnsOkResult()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var filterDto = new PartFilterDto();
            var partFilter = new PartFilter();
            var partTypes = new List<PartType>();
            var categories = new List<Category>();
            var parts = new List<Part>();
            var partListDtos = new List<PartListDto>();

            mockPartTypeRepository.Setup(r => r.ListAsync(It.IsAny<ISpecification<PartType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(partTypes);
            mockCategoryRepository.Setup(r => r.ListAsync(It.IsAny<ISpecification<Category>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);
            mockMapper.Setup(m => m.Map<PartFilter>(filterDto)).Returns(partFilter);
            mockPartAsyncRepository.Setup(r => r.ListAsync(It.IsAny<ISpecification<Part>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parts);
            mockMapper.Setup(m => m.Map<List<PartListDto>>(parts)).Returns(partListDtos);

            // Act
            var result = await controller.GetFilteredParts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that CreatePart returns BadRequest when partFormData is null.
        /// Input: null partFormData.
        /// Expected: Returns BadRequestObjectResult with "Part object is null" message and logs error.
        /// </summary>
        [TestMethod]
        public async Task CreatePart_WithNullPartFormData_ReturnsBadRequestWithMessage()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            // Act
            var result = await controller.CreatePart(null!);

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
        /// Tests that CreatePart returns BadRequest when ModelState is invalid.
        /// Input: Valid partFormData but invalid ModelState.
        /// Expected: Returns BadRequestObjectResult with "Invalid model object" message and logs error.
        /// </summary>
        [TestMethod]
        public async Task CreatePart_WithInvalidModelState_ReturnsBadRequestWithMessage()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            controller.ModelState.AddModelError("Name", "Name is required");

            var partFormData = new PartUploadDto
            {
                Id = 1,
                Name = "Test Part"
            };

            // Act
            var result = await controller.CreatePart(partFormData);

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
        /// Tests that CreatePart returns 500 status code when AddAsync throws an exception.
        /// Input: Valid PartUploadDto, but AddAsync throws exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task CreatePart_WhenAddAsyncThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var partFormData = new PartUploadDto
            {
                Name = "Test Part",
                Description = "Test Description"
            };

            var exceptionMessage = "Database connection failed";

            mockMapper.Setup(m => m.Map(partFormData, It.IsAny<Part>()));

            mockPartAsyncRepository.Setup(r => r.AddAsync(It.IsAny<Part>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.CreatePart(partFormData);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside UpdatePart action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that CreatePart returns 500 status code when mapper throws an exception.
        /// Input: Valid PartUploadDto, but Map throws exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task CreatePart_WhenMapperThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var partFormData = new PartUploadDto
            {
                Name = "Test Part",
                Description = "Test Description"
            };

            var exceptionMessage = "Mapping configuration error";

            mockMapper.Setup(m => m.Map(partFormData, It.IsAny<Part>()))
                .Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.CreatePart(partFormData);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside UpdatePart action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that DeletePart successfully deletes an existing part and returns NoContent.
        /// Input: Valid id for an existing part.
        /// Expected: Part is deleted, NoContent (204) response is returned.
        /// </summary>
        [TestMethod]
        public async Task DeletePart_WithValidIdAndExistingPart_ReturnsNoContent()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockMapper = new Mock<IMapper>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var partId = 123;
            var existingPart = new Part();
            mockPartAsyncRepository.Setup(r => r.GetByIdAsync(partId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPart);
            mockPartAsyncRepository.Setup(r => r.DeleteAsync(existingPart, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.DeletePart(partId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            mockPartAsyncRepository.Verify(r => r.GetByIdAsync(partId, It.IsAny<CancellationToken>()), Times.Once);
            mockPartAsyncRepository.Verify(r => r.DeleteAsync(existingPart, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that DeletePart returns NotFound when part does not exist.
        /// Input: Valid id for a non-existent part (repository returns null).
        /// Expected: NotFound (404) response is returned, error is logged.
        /// </summary>
        [TestMethod]
        public async Task DeletePart_WithIdForNonExistentPart_ReturnsNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockMapper = new Mock<IMapper>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var partId = 999;
            mockPartAsyncRepository.Setup(r => r.GetByIdAsync(partId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Part?)null);

            // Act
            var result = await controller.DeletePart(partId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockPartAsyncRepository.Verify(r => r.GetByIdAsync(partId, It.IsAny<CancellationToken>()), Times.Once);
            mockPartAsyncRepository.Verify(r => r.DeleteAsync(It.IsAny<Part>(), It.IsAny<CancellationToken>()), Times.Never);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Part with id: {partId}, hasn't been found in db.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that DeletePart returns 500 status code when GetByIdAsync throws an exception.
        /// Input: Valid id that causes GetByIdAsync to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs error.
        /// </summary>
        [TestMethod]
        public async Task DeletePart_WhenGetByIdAsyncThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockMapper = new Mock<IMapper>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var partId = 123;
            var exceptionMessage = "Database connection failed";
            mockPartAsyncRepository.Setup(r => r.GetByIdAsync(partId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.DeletePart(partId);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside DeletePart action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that DeletePart returns 500 status code when DeleteAsync throws an exception.
        /// Input: Valid id for an existing part, but DeleteAsync throws an exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs error.
        /// </summary>
        [TestMethod]
        public async Task DeletePart_WhenDeleteAsyncThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockMapper = new Mock<IMapper>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var partId = 123;
            var existingPart = new Part();
            var exceptionMessage = "Foreign key constraint violation";
            mockPartAsyncRepository.Setup(r => r.GetByIdAsync(partId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPart);
            mockPartAsyncRepository.Setup(r => r.DeleteAsync(existingPart, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.DeletePart(partId);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside DeletePart action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that DeletePart handles edge case id values correctly.
        /// Input: Edge case id values (0, int.MinValue, int.MaxValue, negative).
        /// Expected: Method attempts to retrieve and handles according to whether part exists or not.
        /// </summary>
        [TestMethod]
        [DataRow(0, DisplayName = "Zero id")]
        [DataRow(int.MinValue, DisplayName = "Minimum int value")]
        [DataRow(int.MaxValue, DisplayName = "Maximum int value")]
        [DataRow(-1, DisplayName = "Negative id")]
        public async Task DeletePart_WithEdgeCaseIds_HandlesCorrectly(int edgeCaseId)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockMapper = new Mock<IMapper>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            mockPartAsyncRepository.Setup(r => r.GetByIdAsync(edgeCaseId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Part?)null);

            // Act
            var result = await controller.DeletePart(edgeCaseId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockPartAsyncRepository.Verify(r => r.GetByIdAsync(edgeCaseId, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that DeletePart with positive id successfully deletes when part exists.
        /// Input: Positive id value with existing part.
        /// Expected: Part is deleted, NoContent response is returned.
        /// </summary>
        [TestMethod]
        [DataRow(1, DisplayName = "Id equals 1")]
        [DataRow(100, DisplayName = "Id equals 100")]
        [DataRow(999999, DisplayName = "Large positive id")]
        public async Task DeletePart_WithPositiveIdAndExistingPart_ReturnsNoContent(int positiveId)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockMapper = new Mock<IMapper>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var existingPart = new Part();
            mockPartAsyncRepository.Setup(r => r.GetByIdAsync(positiveId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPart);
            mockPartAsyncRepository.Setup(r => r.DeleteAsync(existingPart, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.DeletePart(positiveId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            mockPartAsyncRepository.Verify(r => r.DeleteAsync(existingPart, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that SearchParts returns OkObjectResult with parts when repository returns data successfully.
        /// Input: Valid PartFilterDto.
        /// Expected: Returns OkObjectResult containing the list of SearchPartInfo from repository.
        /// </summary>
        [TestMethod]
        public async Task SearchParts_WithValidFilter_ReturnsOkWithParts()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var filterDto = new PartFilterDto { Id = 1, BrandId = 100, RegionId = 5 };
            var expectedParts = new List<SearchPartInfo>
            {
                new SearchPartInfo { Id = 1 },
                new SearchPartInfo { Id = 2 }
            };

            mockPartRepository.Setup(r => r.SearchParts(filterDto))
                .ReturnsAsync(expectedParts);

            // Act
            var result = await controller.SearchParts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var parts = okResult.Value as IReadOnlyList<SearchPartInfo>;
            Assert.IsNotNull(parts);
            Assert.AreEqual(2, parts.Count);
            mockPartRepository.Verify(r => r.SearchParts(filterDto), Times.Once);
        }

        /// <summary>
        /// Tests that SearchParts returns OkObjectResult with empty list when repository returns no results.
        /// Input: Valid PartFilterDto with no matching results.
        /// Expected: Returns OkObjectResult containing an empty list.
        /// </summary>
        [TestMethod]
        public async Task SearchParts_WithNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var filterDto = new PartFilterDto { Id = 999 };
            var emptyParts = new List<SearchPartInfo>();

            mockPartRepository.Setup(r => r.SearchParts(filterDto))
                .ReturnsAsync(emptyParts);

            // Act
            var result = await controller.SearchParts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var parts = okResult.Value as IReadOnlyList<SearchPartInfo>;
            Assert.IsNotNull(parts);
            Assert.AreEqual(0, parts.Count);
        }

        /// <summary>
        /// Tests that SearchParts returns 500 status code when repository throws an exception.
        /// Input: PartFilterDto that causes repository to throw an exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task SearchParts_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var filterDto = new PartFilterDto { Id = 1 };
            var exceptionMessage = "Database connection failed";

            mockPartRepository.Setup(r => r.SearchParts(filterDto))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.SearchParts(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside SearchParts action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SearchParts returns 500 status code when repository throws InvalidOperationException.
        /// Input: PartFilterDto that causes repository to throw InvalidOperationException.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task SearchParts_WhenRepositoryThrowsInvalidOperationException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var filterDto = new PartFilterDto { BrandId = 50 };
            var exceptionMessage = "Sequence contains no elements";

            mockPartRepository.Setup(r => r.SearchParts(filterDto))
                .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            // Act
            var result = await controller.SearchParts(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside SearchParts action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SearchParts handles various PartFilterDto property combinations correctly.
        /// Input: PartFilterDto with different property values.
        /// Expected: Returns OkObjectResult with appropriate results for each filter configuration.
        /// </summary>
        [TestMethod]
        [DataRow(null, 0, null, null, DisplayName = "Filter with Id null, BrandId 0, nulls")]
        [DataRow(1, 100, 5, 10, DisplayName = "Filter with all properties set")]
        [DataRow(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, DisplayName = "Filter with max integer values")]
        [DataRow(0, 0, 0, 0, DisplayName = "Filter with all zeros")]
        public async Task SearchParts_WithVariousFilterConfigurations_ReturnsOkResult(int? id, int brandId, int? regionId, int? countryId)
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var filterDto = new PartFilterDto
            {
                Id = id,
                BrandId = brandId,
                RegionId = regionId,
                CountryId = countryId
            };
            var expectedParts = new List<SearchPartInfo> { new SearchPartInfo { Id = 1 } };

            mockPartRepository.Setup(r => r.SearchParts(It.IsAny<PartFilterDto>()))
                .ReturnsAsync(expectedParts);

            // Act
            var result = await controller.SearchParts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
        }

        /// <summary>
        /// Tests that SearchParts logs information message when parts are successfully retrieved.
        /// Input: Valid PartFilterDto.
        /// Expected: Logs information message containing "Returned all parts from database."
        /// </summary>
        [TestMethod]
        public async Task SearchParts_OnSuccess_LogsInformationMessage()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var filterDto = new PartFilterDto { Id = 1 };
            var expectedParts = new List<SearchPartInfo> { new SearchPartInfo { Id = 1 } };

            mockPartRepository.Setup(r => r.SearchParts(filterDto))
                .ReturnsAsync(expectedParts);

            // Act
            var result = await controller.SearchParts(filterDto);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Returned all parts from database.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SearchParts returns 500 status code when repository throws ArgumentException.
        /// Input: PartFilterDto that causes repository to throw ArgumentException.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message.
        /// </summary>
        [TestMethod]
        public async Task SearchParts_WhenRepositoryThrowsArgumentException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockPartAsyncRepository = new Mock<IAsyncRepositoryLong<Part>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartController>>();
            var mockPartRepository = new Mock<IPartRepository>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockProductRepository = new Mock<IAsyncRepositoryLong<Product>>();
            var mockStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockRegionRepository = new Mock<IAsyncRepository<Region>>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();

            var controller = new PartController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockPartAsyncRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockPartRepository.Object,
                mockCountryRepository.Object,
                mockProductRepository.Object,
                mockStandTypeRepository.Object,
                mockRegionRepository.Object,
                mockBlobServiceClient.Object,
                mockConfiguration.Object,
                mockBrandRepository.Object);

            var filterDto = new PartFilterDto { Id = -1 };
            var exceptionMessage = "Invalid filter parameter";

            mockPartRepository.Setup(r => r.SearchParts(filterDto))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await controller.SearchParts(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
        }
    }
}