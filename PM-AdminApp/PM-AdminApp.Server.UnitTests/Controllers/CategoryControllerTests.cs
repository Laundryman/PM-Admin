using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PM_AdminApp.Server.Controllers;
using PMApplication.Dtos.Categories;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;

namespace PM_AdminApp.Server.Controllers.UnitTests
{
    [TestClass]
    public partial class CategoryControllerTests
    {
        /// <summary>
        /// Tests that GetCategorySelectList returns OkObjectResult with only default entry when repository returns no categories.
        /// Input: Valid CategoryFilterDto with repository returning empty list.
        /// Expected: Returns OkObjectResult containing list with only "Select Category" entry.
        /// </summary>
        [TestMethod]
        public async Task GetCategorySelectList_WithNoResults_ReturnsOkWithDefaultEntryOnly()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            var controller = new CategoryController(
                mockMapper.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new CategoryFilterDto { GetParents = false };
            var categoryFilter = new CategoryFilter { GetParents = false };
            var emptyCategories = new List<Category>();

            mockMapper.Setup(m => m.Map<CategoryFilter>(filterDto)).Returns(categoryFilter);
            mockCategoryRepository.Setup(r => r.ListAsync(It.IsAny<CategorySpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyCategories);

            // Act
            var result = await controller.GetCategorySelectList(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var returnedList = okResult.Value as List<CategoryDto>;
            Assert.IsNotNull(returnedList);
            Assert.AreEqual(1, returnedList.Count);
            Assert.AreEqual(0, returnedList[0].Id);
            Assert.AreEqual("Select Category", returnedList[0].Name);
        }

        /// <summary>
        /// Tests that GetCategorySelectList returns 500 status code when mapper throws an exception during filter mapping.
        /// Input: CategoryFilterDto that causes mapper to throw during filter mapping.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetCategorySelectList_WhenMapperThrowsExceptionOnFilterMapping_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            var controller = new CategoryController(
                mockMapper.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new CategoryFilterDto { GetParents = true };
            var exceptionMessage = "Mapping filter failed";

            mockMapper.Setup(m => m.Map<CategoryFilter>(filterDto)).Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetCategorySelectList(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetCountries action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetCategorySelectList returns 500 status code when repository throws an exception.
        /// Input: Valid CategoryFilterDto with repository throwing exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetCategorySelectList_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            var controller = new CategoryController(
                mockMapper.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new CategoryFilterDto { GetParents = true };
            var categoryFilter = new CategoryFilter { GetParents = true };
            var exceptionMessage = "Repository error";

            mockMapper.Setup(m => m.Map<CategoryFilter>(filterDto)).Returns(categoryFilter);
            mockCategoryRepository.Setup(r => r.ListAsync(It.IsAny<CategorySpecification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetCategorySelectList(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetCountries action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that the CategoryController constructor successfully initializes with all valid dependencies.
        /// Input: All required dependencies (IMapper, IAsyncRepository, ILogger).
        /// Expected: Constructor completes without throwing exceptions and instance is created successfully.
        /// </summary>
        [TestMethod]
        public void Constructor_WithAllValidDependencies_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            // Act
            var controller = new CategoryController(
                mockMapper.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that GetCategories returns OkObjectResult with CategoryDto list when GetParents is false.
        /// Input: CategoryFilterDto with GetParents=false and valid categories returned from repository.
        /// Expected: Returns OkObjectResult containing List of CategoryDto.
        /// </summary>
        [TestMethod]
        public async Task GetCategories_WithGetParentsFalse_ReturnsOkWithCategoryDtoList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            var controller = new CategoryController(
                mockMapper.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new CategoryFilterDto { Id = 1, GetParents = false };
            var categoryFilter = new CategoryFilter { Id = 1 };
            var categories = new List<Category>
            {
                new Category { Name = "Category1", ParentCategoryId = 1 },
                new Category { Name = "Category2", ParentCategoryId = 2 }
            };
            var mappedCategoryDtos = new List<CategoryDto>
            {
                new CategoryDto { Name = "Category1" },
                new CategoryDto { Name = "Category2" }
            };

            mockMapper.Setup(m => m.Map<CategoryFilter>(filterDto)).Returns(categoryFilter);
            mockCategoryRepository.Setup(r => r.ListAsync(It.IsAny<CategorySpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);
            mockMapper.Setup(m => m.Map<List<CategoryDto>>(categories)).Returns(mappedCategoryDtos);

            // Act
            var result = await controller.GetCategories(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var resultDtos = okResult.Value as List<CategoryDto>;
            Assert.IsNotNull(resultDtos);
            Assert.AreEqual(2, resultDtos.Count);
        }

        /// <summary>
        /// Tests that GetCategories returns OkObjectResult with ParentCategoryDto list when GetParents is true.
        /// Input: CategoryFilterDto with GetParents=true and valid categories returned from repository.
        /// Expected: Returns OkObjectResult containing List of ParentCategoryDto.
        /// </summary>
        [TestMethod]
        public async Task GetCategories_WithGetParentsTrue_ReturnsOkWithParentCategoryDtoList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            var controller = new CategoryController(
                mockMapper.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new CategoryFilterDto { Id = 1, GetParents = true };
            var categoryFilter = new CategoryFilter { Id = 1 };
            var categories = new List<Category>
            {
                new Category { Name = "ParentCategory1", ParentCategoryId = 1 },
                new Category { Name = "ParentCategory2", ParentCategoryId = 2 }
            };
            var mappedParentCategoryDtos = new List<ParentCategoryDto>
            {
                new ParentCategoryDto { Name = "ParentCategory1" },
                new ParentCategoryDto { Name = "ParentCategory2" }
            };

            mockMapper.Setup(m => m.Map<CategoryFilter>(filterDto)).Returns(categoryFilter);
            mockCategoryRepository.Setup(r => r.ListAsync(It.IsAny<CategorySpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);
            mockMapper.Setup(m => m.Map<List<ParentCategoryDto>>(categories)).Returns(mappedParentCategoryDtos);

            // Act
            var result = await controller.GetCategories(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var resultDtos = okResult.Value as List<ParentCategoryDto>;
            Assert.IsNotNull(resultDtos);
            Assert.AreEqual(2, resultDtos.Count);
        }

        /// <summary>
        /// Tests that GetCategories sets ParentCategoryName to "1 PARENT CATEGORIES" when ParentCategoryId is 0.
        /// Input: Categories with ParentCategoryId=0.
        /// Expected: ParentCategoryName is set to "1 PARENT CATEGORIES" for categories with ParentCategoryId=0.
        /// </summary>
        [TestMethod]
        public async Task GetCategories_WithParentCategoryIdZero_SetsParentCategoryName()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            var controller = new CategoryController(
                mockMapper.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new CategoryFilterDto { GetParents = false };
            var categoryFilter = new CategoryFilter();
            var categories = new List<Category>
            {
                new Category { Name = "RootCategory", ParentCategoryId = 0, ParentCategoryName = null },
                new Category { Name = "ChildCategory", ParentCategoryId = 5, ParentCategoryName = "Parent" }
            };
            var mappedCategoryDtos = new List<CategoryDto>
            {
                new CategoryDto { Name = "RootCategory" },
                new CategoryDto { Name = "ChildCategory" }
            };

            mockMapper.Setup(m => m.Map<CategoryFilter>(filterDto)).Returns(categoryFilter);
            mockCategoryRepository.Setup(r => r.ListAsync(It.IsAny<CategorySpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);
            mockMapper.Setup(m => m.Map<List<CategoryDto>>(categories)).Returns(mappedCategoryDtos);

            // Act
            var result = await controller.GetCategories(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("1 PARENT CATEGORIES", categories[0].ParentCategoryName);
            Assert.AreEqual("Parent", categories[1].ParentCategoryName);
        }

        /// <summary>
        /// Tests that GetCategories returns OkObjectResult with empty list when repository returns no categories.
        /// Input: Valid CategoryFilterDto.
        /// Expected: Returns OkObjectResult containing an empty list.
        /// </summary>
        [TestMethod]
        public async Task GetCategories_WithNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            var controller = new CategoryController(
                mockMapper.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new CategoryFilterDto { Id = 999, GetParents = false };
            var categoryFilter = new CategoryFilter { Id = 999 };
            var emptyCategories = new List<Category>();
            var emptyMappedDtos = new List<CategoryDto>();

            mockMapper.Setup(m => m.Map<CategoryFilter>(filterDto)).Returns(categoryFilter);
            mockCategoryRepository.Setup(r => r.ListAsync(It.IsAny<CategorySpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyCategories);
            mockMapper.Setup(m => m.Map<List<CategoryDto>>(emptyCategories)).Returns(emptyMappedDtos);

            // Act
            var result = await controller.GetCategories(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var resultDtos = okResult.Value as List<CategoryDto>;
            Assert.IsNotNull(resultDtos);
            Assert.AreEqual(0, resultDtos.Count);
        }

        /// <summary>
        /// Tests that GetCategories returns 500 status code when mapper throws an exception during CategoryFilter mapping.
        /// Input: CategoryFilterDto that causes mapper to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetCategories_WhenMapperThrowsExceptionOnFilterMapping_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            var controller = new CategoryController(
                mockMapper.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new CategoryFilterDto { Id = 1 };
            var exceptionMessage = "Mapping to CategoryFilter failed";

            mockMapper.Setup(m => m.Map<CategoryFilter>(filterDto)).Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetCategories(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetCategories action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetCategories returns 500 status code when repository throws an exception.
        /// Input: CategoryFilterDto and repository that throws exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetCategories_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            var controller = new CategoryController(
                mockMapper.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new CategoryFilterDto { Id = 1 };
            var categoryFilter = new CategoryFilter { Id = 1 };
            var exceptionMessage = "Repository access failed";

            mockMapper.Setup(m => m.Map<CategoryFilter>(filterDto)).Returns(categoryFilter);
            mockCategoryRepository.Setup(r => r.ListAsync(It.IsAny<CategorySpecification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetCategories(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetCategories action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetCategories returns 500 status code when mapper throws an exception during ParentCategoryDto mapping.
        /// Input: CategoryFilterDto with GetParents=true and mapper that throws on ParentCategoryDto mapping.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetCategories_WhenMapperThrowsExceptionOnParentCategoryDtoMapping_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            var controller = new CategoryController(
                mockMapper.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new CategoryFilterDto { Id = 1, GetParents = true };
            var categoryFilter = new CategoryFilter { Id = 1 };
            var categories = new List<Category>
            {
                new Category { Name = "Category1", ParentCategoryId = 1 }
            };
            var exceptionMessage = "Mapping to ParentCategoryDto failed";

            mockMapper.Setup(m => m.Map<CategoryFilter>(filterDto)).Returns(categoryFilter);
            mockCategoryRepository.Setup(r => r.ListAsync(It.IsAny<CategorySpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);
            mockMapper.Setup(m => m.Map<List<ParentCategoryDto>>(categories)).Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetCategories(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetCategories action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetCategories returns 500 status code when mapper throws an exception during CategoryDto mapping.
        /// Input: CategoryFilterDto with GetParents=false and mapper that throws on CategoryDto mapping.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetCategories_WhenMapperThrowsExceptionOnCategoryDtoMapping_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            var controller = new CategoryController(
                mockMapper.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new CategoryFilterDto { Id = 1, GetParents = false };
            var categoryFilter = new CategoryFilter { Id = 1 };
            var categories = new List<Category>
            {
                new Category { Name = "Category1", ParentCategoryId = 1 }
            };
            var exceptionMessage = "Mapping to CategoryDto failed";

            mockMapper.Setup(m => m.Map<CategoryFilter>(filterDto)).Returns(categoryFilter);
            mockCategoryRepository.Setup(r => r.ListAsync(It.IsAny<CategorySpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);
            mockMapper.Setup(m => m.Map<List<CategoryDto>>(categories)).Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetCategories(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetCategories action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GetCategories correctly handles multiple categories with mixed ParentCategoryId values.
        /// Input: Categories with ParentCategoryId values of 0, positive, and null.
        /// Expected: Only categories with ParentCategoryId=0 have ParentCategoryName set to "1 PARENT CATEGORIES".
        /// </summary>
        [TestMethod]
        public async Task GetCategories_WithMixedParentCategoryIds_SetsParentCategoryNameOnlyForZero()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            var controller = new CategoryController(
                mockMapper.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new CategoryFilterDto { GetParents = false };
            var categoryFilter = new CategoryFilter();
            var categories = new List<Category>
            {
                new Category { Name = "RootCategory1", ParentCategoryId = 0, ParentCategoryName = "OldName" },
                new Category { Name = "RootCategory2", ParentCategoryId = 0, ParentCategoryName = null },
                new Category { Name = "ChildCategory", ParentCategoryId = 10, ParentCategoryName = "KeepThis" },
                new Category { Name = "NullParentCategory", ParentCategoryId = null, ParentCategoryName = "AlsoKeepThis" }
            };
            var mappedCategoryDtos = new List<CategoryDto>
            {
                new CategoryDto { Name = "RootCategory1" },
                new CategoryDto { Name = "RootCategory2" },
                new CategoryDto { Name = "ChildCategory" },
                new CategoryDto { Name = "NullParentCategory" }
            };

            mockMapper.Setup(m => m.Map<CategoryFilter>(filterDto)).Returns(categoryFilter);
            mockCategoryRepository.Setup(r => r.ListAsync(It.IsAny<CategorySpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);
            mockMapper.Setup(m => m.Map<List<CategoryDto>>(categories)).Returns(mappedCategoryDtos);

            // Act
            var result = await controller.GetCategories(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("1 PARENT CATEGORIES", categories[0].ParentCategoryName);
            Assert.AreEqual("1 PARENT CATEGORIES", categories[1].ParentCategoryName);
            Assert.AreEqual("KeepThis", categories[2].ParentCategoryName);
            Assert.AreEqual("AlsoKeepThis", categories[3].ParentCategoryName);
        }

        /// <summary>
        /// Tests that GetCategories correctly handles GetParents=true with empty result set.
        /// Input: CategoryFilterDto with GetParents=true and empty categories list.
        /// Expected: Returns OkObjectResult with empty ParentCategoryDto list.
        /// </summary>
        [TestMethod]
        public async Task GetCategories_WithGetParentsTrueAndEmptyResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            var controller = new CategoryController(
                mockMapper.Object,
                mockCategoryRepository.Object,
                mockLogger.Object);

            var filterDto = new CategoryFilterDto { Id = 999, GetParents = true };
            var categoryFilter = new CategoryFilter { Id = 999 };
            var emptyCategories = new List<Category>();
            var emptyMappedDtos = new List<ParentCategoryDto>();

            mockMapper.Setup(m => m.Map<CategoryFilter>(filterDto)).Returns(categoryFilter);
            mockCategoryRepository.Setup(r => r.ListAsync(It.IsAny<CategorySpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyCategories);
            mockMapper.Setup(m => m.Map<List<ParentCategoryDto>>(emptyCategories)).Returns(emptyMappedDtos);

            // Act
            var result = await controller.GetCategories(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var resultDtos = okResult.Value as List<ParentCategoryDto>;
            Assert.IsNotNull(resultDtos);
            Assert.AreEqual(0, resultDtos.Count);
        }

        /// <summary>
        /// Tests that the CategoryController constructor accepts null mapper parameter.
        /// Input: Null mapper with valid repository and logger.
        /// Expected: Constructor completes without throwing exceptions, documenting the lack of validation.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullMapper_CreatesInstanceWithoutValidation()
        {
            // Arrange
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            // Act
            var controller = new CategoryController(
                null!,
                mockCategoryRepository.Object,
                mockLogger.Object);

            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that the CategoryController constructor accepts null category repository parameter.
        /// Input: Null category repository with valid mapper and logger.
        /// Expected: Constructor completes without throwing exceptions, documenting the lack of validation.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullCategoryRepository_CreatesInstanceWithoutValidation()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<CategoryController>>();

            // Act
            var controller = new CategoryController(
                mockMapper.Object,
                null!,
                mockLogger.Object);

            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that the CategoryController constructor accepts null logger parameter.
        /// Input: Null logger with valid mapper and repository.
        /// Expected: Constructor completes without throwing exceptions, documenting the lack of validation.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullLogger_CreatesInstanceWithoutValidation()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();

            // Act
            var controller = new CategoryController(
                mockMapper.Object,
                mockCategoryRepository.Object,
                null!);

            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that the CategoryController constructor accepts all null parameters.
        /// Input: All null parameters.
        /// Expected: Constructor completes without throwing exceptions, documenting the lack of validation.
        /// </summary>
        [TestMethod]
        public void Constructor_WithAllNullParameters_CreatesInstanceWithoutValidation()
        {
            // Arrange & Act
            var controller = new CategoryController(
                null!,
                null!,
                null!);

            // Assert
            Assert.IsNotNull(controller);
        }
    }
}