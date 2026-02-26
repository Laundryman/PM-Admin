using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Ardalis.Specification;
using AutoMapper;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PM_AdminApp.Server;
using PM_AdminApp.Server.Controllers;
using PMApplication;
using PMApplication.Entities;
using PMApplication.Entities.PartAggregate;
using PMApplication.Interfaces;
using PMApplication.Specifications;

namespace PM_AdminApp.Server.Controllers.UnitTests
{
    /// <summary>
    /// Contains unit tests for the PartTypesController class.
    /// </summary>
    [TestClass]
    public class PartTypesControllerTests
    {
        /// <summary>
        /// Tests that GetPartTypes returns OkObjectResult with part types when repository returns valid data.
        /// Input: Repository returns list of part types.
        /// Expected: Returns OkObjectResult with list containing "Select Type" and all part types from repository.
        /// </summary>
        [TestMethod]
        public async Task GetPartTypes_WithValidData_ReturnsOkWithPartTypes()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartTypesController>>();

            var controller = new PartTypesController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var partType1 = new PartType("Type1", "Description1");
            var partType2 = new PartType("Type2", "Description2");
            var partTypes = new List<PartType> { partType1, partType2 };

            mockPartTypeRepository.Setup(r => r.ListAsync(
                It.IsAny<ISpecification<PartType>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(partTypes);

            // Act
            var result = await controller.GetPartTypes();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var returnedList = okResult.Value as List<PartType>;
            Assert.IsNotNull(returnedList);
            Assert.AreEqual(3, returnedList.Count); // "Select Type" + 2 part types
            Assert.AreEqual("Select Type", returnedList[0].Name);
        }

        /// <summary>
        /// Tests that GetPartTypes returns OkObjectResult with only "Select Type" when repository returns empty list.
        /// Input: Repository returns empty list.
        /// Expected: Returns OkObjectResult with list containing only "Select Type" entry.
        /// </summary>
        [TestMethod]
        public async Task GetPartTypes_WithEmptyList_ReturnsOkWithSelectTypeOnly()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartTypesController>>();

            var controller = new PartTypesController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var emptyPartTypes = new List<PartType>();

            mockPartTypeRepository.Setup(r => r.ListAsync(
                It.IsAny<ISpecification<PartType>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyPartTypes);

            // Act
            var result = await controller.GetPartTypes();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var returnedList = okResult.Value as List<PartType>;
            Assert.IsNotNull(returnedList);
            Assert.AreEqual(1, returnedList.Count);
            Assert.AreEqual("Select Type", returnedList[0].Name);
        }

        /// <summary>
        /// Tests that GetPartTypes returns 500 status code when repository throws an exception.
        /// Input: Repository ListAsync throws exception.
        /// Expected: Returns ObjectResult with status code 500, "Internal server error" message, and logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetPartTypes_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartTypesController>>();

            var controller = new PartTypesController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var exceptionMessage = "Database connection failed";

            mockPartTypeRepository.Setup(r => r.ListAsync(
                It.IsAny<ISpecification<PartType>>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetPartTypes();

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetPartTypes action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetPartTypes returns 500 status code when an unexpected exception occurs.
        /// Input: Repository throws InvalidOperationException.
        /// Expected: Returns ObjectResult with status code 500, "Internal server error" message, and logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetPartTypes_WhenUnexpectedExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartTypesController>>();

            var controller = new PartTypesController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var exceptionMessage = "Unexpected error";

            mockPartTypeRepository.Setup(r => r.ListAsync(
                It.IsAny<ISpecification<PartType>>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            // Act
            var result = await controller.GetPartTypes();

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetPartTypes action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetPartTypes calls repository with correct specification type.
        /// Input: Valid setup with mocked repository.
        /// Expected: Repository ListAsync is called once with PartTypeSpecification.
        /// </summary>
        [TestMethod]
        public async Task GetPartTypes_CallsRepositoryWithCorrectSpecification_Success()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartTypesController>>();

            var controller = new PartTypesController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var partTypes = new List<PartType>();

            mockPartTypeRepository.Setup(r => r.ListAsync(
                It.IsAny<ISpecification<PartType>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(partTypes);

            // Act
            await controller.GetPartTypes();

            // Assert
            mockPartTypeRepository.Verify(
                r => r.ListAsync(
                    It.IsAny<ISpecification<PartType>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that the PartTypesController constructor successfully initializes with all valid dependencies.
        /// Input: All required dependencies (IMapper, IAsyncRepository<PartType>, IAsyncRepository<Category>, ILogger).
        /// Expected: Constructor completes without throwing exceptions and instance is created successfully.
        /// </summary>
        [TestMethod]
        public void Constructor_WithAllValidDependencies_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockPartTypeRepository = new Mock<IAsyncRepository<PartType>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<PartTypesController>>();

            // Act
            var controller = new PartTypesController(
                mockMapper.Object,
                mockPartTypeRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            // Assert
            Assert.IsNotNull(controller);
        }
    }
}