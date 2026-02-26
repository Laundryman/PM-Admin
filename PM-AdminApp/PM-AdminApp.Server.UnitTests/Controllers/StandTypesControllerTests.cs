using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PM_AdminApp.Server.Controllers;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.StandTypes;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.StandAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;

namespace PM_AdminApp.Server.Controllers.UnitTests
{
    [TestClass]
    public class StandTypesControllerTests
    {
        /// <summary>
        /// Tests that the StandTypesController constructor successfully initializes with all valid dependencies.
        /// Input: All required dependencies (IMapper, IAsyncRepository instances, ILogger, IStandTypeRepository).
        /// Expected: Constructor completes without throwing exceptions and instance is created successfully.
        /// </summary>
        [TestMethod]
        public void Constructor_WithAllValidDependencies_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandTypesController>>();
            var mockStandTypeRepository = new Mock<IStandTypeRepository>();

            // Act
            var controller = new StandTypesController(
                mockMapper.Object,
                mockAsyncStandTypeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandTypeRepository.Object);

            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that GetStandTypes returns OkObjectResult with ParentStandTypeDto list when GetParents is true.
        /// Input: Valid StandTypeFilterDto with GetParents set to true.
        /// Expected: Returns OkObjectResult containing List of ParentStandTypeDto.
        /// </summary>
        [TestMethod]
        public async Task GetStandTypes_WithGetParentsTrue_ReturnsOkWithParentStandTypeDtoList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandTypesController>>();
            var mockStandTypeRepository = new Mock<IStandTypeRepository>();

            var controller = new StandTypesController(
                mockMapper.Object,
                mockAsyncStandTypeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandTypeRepository.Object);

            var filterDto = new StandTypeFilterDto { GetParents = true, Id = 1 };
            var standTypeFilter = new StandTypeFilter { Id = 1 };
            var standTypes = new List<StandType> { new StandType(), new StandType() };
            var parentStandTypeDtos = new List<ParentStandTypeDto>
            {
                new ParentStandTypeDto(),
                new ParentStandTypeDto()
            };

            mockMapper.Setup(m => m.Map<StandTypeFilter>(filterDto)).Returns(standTypeFilter);
            mockAsyncStandTypeRepository.Setup(r => r.ListAsync(It.IsAny<StandTypeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(standTypes);
            mockMapper.Setup(m => m.Map<List<ParentStandTypeDto>>(standTypes)).Returns(parentStandTypeDtos);

            // Act
            var result = await controller.GetStandTypes(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Assert.AreSame(parentStandTypeDtos, okResult.Value);
        }

        /// <summary>
        /// Tests that GetStandTypes returns OkObjectResult with StandTypeDto list when GetParents is false.
        /// Input: Valid StandTypeFilterDto with GetParents set to false.
        /// Expected: Returns OkObjectResult containing List of StandTypeDto.
        /// </summary>
        [TestMethod]
        public async Task GetStandTypes_WithGetParentsFalse_ReturnsOkWithStandTypeDtoList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandTypesController>>();
            var mockStandTypeRepository = new Mock<IStandTypeRepository>();

            var controller = new StandTypesController(
                mockMapper.Object,
                mockAsyncStandTypeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandTypeRepository.Object);

            var filterDto = new StandTypeFilterDto { GetParents = false, Id = 1 };
            var standTypeFilter = new StandTypeFilter { Id = 1 };
            var standTypes = new List<StandType> { new StandType(), new StandType() };
            var standTypeDtos = new List<StandTypeDto>
            {
                new StandTypeDto(),
                new StandTypeDto()
            };

            mockMapper.Setup(m => m.Map<StandTypeFilter>(filterDto)).Returns(standTypeFilter);
            mockAsyncStandTypeRepository.Setup(r => r.ListAsync(It.IsAny<StandTypeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(standTypes);
            mockMapper.Setup(m => m.Map<List<StandTypeDto>>(standTypes)).Returns(standTypeDtos);

            // Act
            var result = await controller.GetStandTypes(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Assert.AreSame(standTypeDtos, okResult.Value);
        }

        /// <summary>
        /// Tests that GetStandTypes returns OkObjectResult with empty list when repository returns no results and GetParents is true.
        /// Input: Valid StandTypeFilterDto with GetParents set to true.
        /// Expected: Returns OkObjectResult containing an empty list of ParentStandTypeDto.
        /// </summary>
        [TestMethod]
        public async Task GetStandTypes_WithGetParentsTrueAndNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandTypesController>>();
            var mockStandTypeRepository = new Mock<IStandTypeRepository>();

            var controller = new StandTypesController(
                mockMapper.Object,
                mockAsyncStandTypeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandTypeRepository.Object);

            var filterDto = new StandTypeFilterDto { GetParents = true };
            var standTypeFilter = new StandTypeFilter();
            var emptyStandTypes = new List<StandType>();
            var emptyParentStandTypeDtos = new List<ParentStandTypeDto>();

            mockMapper.Setup(m => m.Map<StandTypeFilter>(filterDto)).Returns(standTypeFilter);
            mockAsyncStandTypeRepository.Setup(r => r.ListAsync(It.IsAny<StandTypeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyStandTypes);
            mockMapper.Setup(m => m.Map<List<ParentStandTypeDto>>(emptyStandTypes)).Returns(emptyParentStandTypeDtos);

            // Act
            var result = await controller.GetStandTypes(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var dtos = okResult.Value as List<ParentStandTypeDto>;
            Assert.IsNotNull(dtos);
            Assert.AreEqual(0, dtos.Count);
        }

        /// <summary>
        /// Tests that GetStandTypes returns OkObjectResult with empty list when repository returns no results and GetParents is false.
        /// Input: Valid StandTypeFilterDto with GetParents set to false.
        /// Expected: Returns OkObjectResult containing an empty list of StandTypeDto.
        /// </summary>
        [TestMethod]
        public async Task GetStandTypes_WithGetParentsFalseAndNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandTypesController>>();
            var mockStandTypeRepository = new Mock<IStandTypeRepository>();

            var controller = new StandTypesController(
                mockMapper.Object,
                mockAsyncStandTypeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandTypeRepository.Object);

            var filterDto = new StandTypeFilterDto { GetParents = false };
            var standTypeFilter = new StandTypeFilter();
            var emptyStandTypes = new List<StandType>();
            var emptyStandTypeDtos = new List<StandTypeDto>();

            mockMapper.Setup(m => m.Map<StandTypeFilter>(filterDto)).Returns(standTypeFilter);
            mockAsyncStandTypeRepository.Setup(r => r.ListAsync(It.IsAny<StandTypeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyStandTypes);
            mockMapper.Setup(m => m.Map<List<StandTypeDto>>(emptyStandTypes)).Returns(emptyStandTypeDtos);

            // Act
            var result = await controller.GetStandTypes(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var dtos = okResult.Value as List<StandTypeDto>;
            Assert.IsNotNull(dtos);
            Assert.AreEqual(0, dtos.Count);
        }

        /// <summary>
        /// Tests that GetStandTypes returns 500 status code when mapper throws exception on first mapping.
        /// Input: StandTypeFilterDto that causes mapper to throw when mapping to StandTypeFilter.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetStandTypes_WhenMapperThrowsExceptionOnFirstMapping_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandTypesController>>();
            var mockStandTypeRepository = new Mock<IStandTypeRepository>();

            var controller = new StandTypesController(
                mockMapper.Object,
                mockAsyncStandTypeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandTypeRepository.Object);

            var filterDto = new StandTypeFilterDto { GetParents = true };
            var exceptionMessage = "Mapping failed";

            mockMapper.Setup(m => m.Map<StandTypeFilter>(filterDto)).Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetStandTypes(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetStandTypes action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetStandTypes returns 500 status code when repository throws exception.
        /// Input: Valid StandTypeFilterDto, but repository ListAsync throws exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetStandTypes_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandTypesController>>();
            var mockStandTypeRepository = new Mock<IStandTypeRepository>();

            var controller = new StandTypesController(
                mockMapper.Object,
                mockAsyncStandTypeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandTypeRepository.Object);

            var filterDto = new StandTypeFilterDto { GetParents = false };
            var standTypeFilter = new StandTypeFilter();
            var exceptionMessage = "Database connection failed";

            mockMapper.Setup(m => m.Map<StandTypeFilter>(filterDto)).Returns(standTypeFilter);
            mockAsyncStandTypeRepository.Setup(r => r.ListAsync(It.IsAny<StandTypeSpecification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetStandTypes(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetStandTypes action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetStandTypes returns 500 status code when mapper throws exception on second mapping to ParentStandTypeDto.
        /// Input: Valid StandTypeFilterDto with GetParents set to true, mapper throws on List mapping.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetStandTypes_WhenMapperThrowsExceptionOnParentStandTypeDtoMapping_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandTypesController>>();
            var mockStandTypeRepository = new Mock<IStandTypeRepository>();

            var controller = new StandTypesController(
                mockMapper.Object,
                mockAsyncStandTypeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandTypeRepository.Object);

            var filterDto = new StandTypeFilterDto { GetParents = true };
            var standTypeFilter = new StandTypeFilter();
            var standTypes = new List<StandType> { new StandType() };
            var exceptionMessage = "Mapping to ParentStandTypeDto failed";

            mockMapper.Setup(m => m.Map<StandTypeFilter>(filterDto)).Returns(standTypeFilter);
            mockAsyncStandTypeRepository.Setup(r => r.ListAsync(It.IsAny<StandTypeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(standTypes);
            mockMapper.Setup(m => m.Map<List<ParentStandTypeDto>>(standTypes)).Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetStandTypes(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetStandTypes action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetStandTypes returns 500 status code when mapper throws exception on second mapping to StandTypeDto.
        /// Input: Valid StandTypeFilterDto with GetParents set to false, mapper throws on List mapping.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetStandTypes_WhenMapperThrowsExceptionOnStandTypeDtoMapping_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandTypeRepository = new Mock<IAsyncRepository<StandType>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<StandTypesController>>();
            var mockStandTypeRepository = new Mock<IStandTypeRepository>();

            var controller = new StandTypesController(
                mockMapper.Object,
                mockAsyncStandTypeRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockStandTypeRepository.Object);

            var filterDto = new StandTypeFilterDto { GetParents = false };
            var standTypeFilter = new StandTypeFilter();
            var standTypes = new List<StandType> { new StandType() };
            var exceptionMessage = "Mapping to StandTypeDto failed";

            mockMapper.Setup(m => m.Map<StandTypeFilter>(filterDto)).Returns(standTypeFilter);
            mockAsyncStandTypeRepository.Setup(r => r.ListAsync(It.IsAny<StandTypeSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(standTypes);
            mockMapper.Setup(m => m.Map<List<StandTypeDto>>(standTypes)).Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetStandTypes(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetStandTypes action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}