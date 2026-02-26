using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PM_AdminApp.Server.Controllers;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;

namespace PM_AdminApp.Server.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="BrandController"/> class.
    /// </summary>
    [TestClass]
    public class BrandControllerTests
    {
        /// <summary>
        /// Tests that the BrandController constructor successfully initializes with all valid dependencies.
        /// Input: All required dependencies (IMapper, IAsyncRepository, ILogger, IConfiguration, BlobServiceClient).
        /// Expected: Constructor completes without throwing exceptions and instance is created successfully.
        /// </summary>
        [TestMethod]
        public void Constructor_WithAllValidDependencies_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();
            var mockLogger = new Mock<ILogger<BrandController>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();

            // Act
            var controller = new BrandController(
                mockMapper.Object,
                mockBrandRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockBlobServiceClient.Object);

            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that GetBrands returns OkObjectResult with empty list when repository returns no brands.
        /// Input: Valid BrandFilterDto.
        /// Expected: Returns OkObjectResult containing an empty list.
        /// </summary>
        [TestMethod]
        public async Task GetBrands_WithNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();
            var mockLogger = new Mock<ILogger<BrandController>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();

            var controller = new BrandController(
                mockMapper.Object,
                mockBrandRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockBlobServiceClient.Object);

            var filterDto = new BrandFilterDto { Id = 999 };
            var brandFilter = new BrandFilter { Id = 999 };
            var emptyBrands = new List<Brand>();

            mockMapper.Setup(m => m.Map<BrandFilter>(filterDto)).Returns(brandFilter);
            mockBrandRepository.Setup(r => r.ListAsync(It.IsAny<BrandSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyBrands);

            // Act
            var result = await controller.GetBrands(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var brands = okResult.Value as IReadOnlyList<Brand>;
            Assert.IsNotNull(brands);
            Assert.AreEqual(0, brands.Count);
        }

        /// <summary>
        /// Tests that GetBrands returns 500 status code when mapper throws an exception.
        /// Input: BrandFilterDto that causes mapper to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetBrands_WhenMapperThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();
            var mockLogger = new Mock<ILogger<BrandController>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();

            var controller = new BrandController(
                mockMapper.Object,
                mockBrandRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockBlobServiceClient.Object);

            var filterDto = new BrandFilterDto { Id = 1 };
            var exceptionMessage = "Mapping failed";

            mockMapper.Setup(m => m.Map<BrandFilter>(filterDto)).Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetBrands(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetBrands action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetBrands returns 500 status code when repository throws an exception.
        /// Input: Valid BrandFilterDto, but repository fails.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetBrands_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();
            var mockLogger = new Mock<ILogger<BrandController>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();

            var controller = new BrandController(
                mockMapper.Object,
                mockBrandRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockBlobServiceClient.Object);

            var filterDto = new BrandFilterDto { Id = 1 };
            var brandFilter = new BrandFilter { Id = 1 };
            var exceptionMessage = "Database connection failed";

            mockMapper.Setup(m => m.Map<BrandFilter>(filterDto)).Returns(brandFilter);
            mockBrandRepository.Setup(r => r.ListAsync(It.IsAny<BrandSpecification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetBrands(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetBrands action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetBrands handles different exception types correctly.
        /// Input: Various exception types thrown from dependencies.
        /// Expected: Returns 500 status code and logs warning for each exception type.
        /// </summary>
        [TestMethod]
        [DataRow(typeof(ArgumentNullException), "Argument was null")]
        [DataRow(typeof(InvalidOperationException), "Invalid operation")]
        [DataRow(typeof(NullReferenceException), "Null reference")]
        public async Task GetBrands_WithDifferentExceptionTypes_ReturnsInternalServerError(Type exceptionType, string message)
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();
            var mockLogger = new Mock<ILogger<BrandController>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();

            var controller = new BrandController(
                mockMapper.Object,
                mockBrandRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockBlobServiceClient.Object);

            var filterDto = new BrandFilterDto { Id = 1 };
            var exception = (Exception)Activator.CreateInstance(exceptionType, message)!;

            mockMapper.Setup(m => m.Map<BrandFilter>(filterDto)).Throws(exception);

            // Act
            var result = await controller.GetBrands(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
        }

        /// <summary>
        /// Tests that GetBrands handles extreme Id values correctly.
        /// Input: BrandFilterDto with int.MaxValue as Id.
        /// Expected: Returns OkObjectResult with empty list (no brand with that Id exists).
        /// </summary>
        [TestMethod]
        public async Task GetBrands_WithMaxIntFilterId_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();
            var mockLogger = new Mock<ILogger<BrandController>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();

            var controller = new BrandController(
                mockMapper.Object,
                mockBrandRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockBlobServiceClient.Object);

            var filterDto = new BrandFilterDto { Id = int.MaxValue };
            var brandFilter = new BrandFilter { Id = int.MaxValue };
            var emptyBrands = new List<Brand>();

            mockMapper.Setup(m => m.Map<BrandFilter>(filterDto)).Returns(brandFilter);
            mockBrandRepository.Setup(r => r.ListAsync(It.IsAny<BrandSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyBrands);

            // Act
            var result = await controller.GetBrands(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var brands = okResult.Value as IReadOnlyList<Brand>;
            Assert.IsNotNull(brands);
            Assert.AreEqual(0, brands.Count);
        }

        /// <summary>
        /// Tests that GetBrands handles negative Id values correctly.
        /// Input: BrandFilterDto with negative Id value.
        /// Expected: Returns OkObjectResult (possibly with empty list or filtered results based on business logic).
        /// </summary>
        [TestMethod]
        public async Task GetBrands_WithNegativeFilterId_ReturnsOk()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();
            var mockLogger = new Mock<ILogger<BrandController>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();

            var controller = new BrandController(
                mockMapper.Object,
                mockBrandRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockBlobServiceClient.Object);

            var filterDto = new BrandFilterDto { Id = -1 };
            var brandFilter = new BrandFilter { Id = -1 };
            var emptyBrands = new List<Brand>();

            mockMapper.Setup(m => m.Map<BrandFilter>(filterDto)).Returns(brandFilter);
            mockBrandRepository.Setup(r => r.ListAsync(It.IsAny<BrandSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyBrands);

            // Act
            var result = await controller.GetBrands(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }


        /// <summary>
        /// Tests that UpdateBrand returns Accepted when brand.Id is null.
        /// Input: BrandUploadDto with null Id.
        /// Expected: Returns AcceptedResult without attempting any repository operations.
        /// </summary>
        [TestMethod]
        public async Task UpdateBrand_WithNullId_ReturnsAccepted()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();
            var mockLogger = new Mock<ILogger<BrandController>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();

            var controller = new BrandController(
                mockMapper.Object,
                mockBrandRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockBlobServiceClient.Object);

            var brandUploadDto = new BrandUploadDto
            {
                Id = null,
                Name = "Test Brand"
            };

            // Act
            var result = await controller.UpdateBrand(brandUploadDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AcceptedResult));
            mockBrandRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            mockBrandRepository.Verify(r => r.UpdateAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Tests that UpdateBrand returns Accepted when brand does not exist in repository.
        /// Input: BrandUploadDto with Id that doesn't exist in repository.
        /// Expected: Returns AcceptedResult without updating anything.
        /// </summary>
        [TestMethod]
        public async Task UpdateBrand_WithNonExistentBrand_ReturnsAccepted()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();
            var mockLogger = new Mock<ILogger<BrandController>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();

            var controller = new BrandController(
                mockMapper.Object,
                mockBrandRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockBlobServiceClient.Object);

            var brandUploadDto = new BrandUploadDto
            {
                Id = 999,
                Name = "Test Brand"
            };

            mockBrandRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Brand?)null);

            // Act
            var result = await controller.UpdateBrand(brandUploadDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AcceptedResult));
            mockBrandRepository.Verify(r => r.UpdateAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Tests that UpdateBrand returns 500 when GetByIdAsync throws an exception.
        /// Input: BrandUploadDto that causes repository GetByIdAsync to throw.
        /// Expected: Returns ObjectResult with status code 500 and logs warning.
        /// </summary>
        [TestMethod]
        public async Task UpdateBrand_WhenGetByIdThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();
            var mockLogger = new Mock<ILogger<BrandController>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();

            var controller = new BrandController(
                mockMapper.Object,
                mockBrandRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockBlobServiceClient.Object);

            var brandUploadDto = new BrandUploadDto
            {
                Id = 1,
                Name = "Test Brand"
            };

            var exceptionMessage = "Database connection failed";
            mockBrandRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.UpdateBrand(brandUploadDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside UpdateBrand action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that UpdateBrand handles extreme Id values correctly.
        /// Input: BrandUploadDto with various extreme Id values (int.MaxValue, int.MinValue, 0, negative).
        /// Expected: Returns AcceptedResult after attempting to get brand from repository.
        /// </summary>
        [TestMethod]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        [DataRow(0)]
        [DataRow(-1)]
        public async Task UpdateBrand_WithExtremeIdValues_ReturnsAccepted(int id)
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();
            var mockLogger = new Mock<ILogger<BrandController>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();

            var controller = new BrandController(
                mockMapper.Object,
                mockBrandRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockBlobServiceClient.Object);

            var brandUploadDto = new BrandUploadDto
            {
                Id = id,
                Name = "Test Brand"
            };

            mockBrandRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Brand?)null);

            // Act
            var result = await controller.UpdateBrand(brandUploadDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(AcceptedResult));
            mockBrandRepository.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateBrand handles different exception types correctly.
        /// Input: Various exception types thrown from dependencies.
        /// Expected: Returns 500 status code and logs warning for each exception type.
        /// </summary>
        [TestMethod]
        [DataRow(typeof(ArgumentNullException), "Argument was null")]
        [DataRow(typeof(InvalidOperationException), "Invalid operation")]
        [DataRow(typeof(NullReferenceException), "Null reference")]
        public async Task UpdateBrand_WithDifferentExceptionTypes_ReturnsInternalServerError(Type exceptionType, string message)
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockBrandRepository = new Mock<IAsyncRepository<Brand>>();
            var mockLogger = new Mock<ILogger<BrandController>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockBlobServiceClient = new Mock<BlobServiceClient>();

            var controller = new BrandController(
                mockMapper.Object,
                mockBrandRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object,
                mockBlobServiceClient.Object);

            var brandUploadDto = new BrandUploadDto
            {
                Id = 1,
                Name = "Test Brand"
            };

            var exception = (Exception)Activator.CreateInstance(exceptionType, message)!;
            mockBrandRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            var result = await controller.UpdateBrand(brandUploadDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside UpdateBrand action")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

    }
}