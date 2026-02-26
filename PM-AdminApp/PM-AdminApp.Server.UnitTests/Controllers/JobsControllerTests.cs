using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Ardalis.Specification;
using AutoMapper;
using Azure.Storage;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PM_AdminApp.Server;
using PM_AdminApp.Server.Controllers;
using PMApplication;
using PMApplication.Dtos;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.JobsAggregate;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;

namespace PM_AdminApp.Server.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the JobsController class.
    /// </summary>
    [TestClass]
    public class JobsControllerTests
    {
        /// <summary>
        /// Tests that the JobsController constructor successfully initializes with all valid dependencies.
        /// Input: All required dependencies (ILogger, IMapper, repositories, IConfiguration).
        /// Expected: Constructor completes without throwing exceptions and instance is created successfully.
        /// </summary>
        [TestMethod]
        public void Constructor_WithAllValidDependencies_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            // Act
            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that SearchJobFolders returns OkObjectResult with empty list when repository returns no job folders.
        /// Input: Valid JobFolderFilter.
        /// Expected: Returns OkObjectResult containing an empty list.
        /// </summary>
        [TestMethod]
        public async Task SearchJobFolders_WithNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var filterDto = new JobFolderFilter { Id = 999 };
            var emptyJobFolders = new List<JobFolder>();
            var emptyMappedDtos = new List<JobFolderDto>();

            mockJobFolderRepository.Setup(r => r.ListAsync(It.IsAny<ISpecification<JobFolder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyJobFolders);
            mockMapper.Setup(m => m.Map<List<JobFolderDto>>(emptyJobFolders)).Returns(emptyMappedDtos);

            // Act
            var result = await controller.SearchJobFolders(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var response = okResult.Value as List<JobFolderDto>;
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count);
        }

        /// <summary>
        /// Tests that SearchJobFolders returns 500 status code when repository throws an exception.
        /// Input: JobFolderFilter that causes repository to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task SearchJobFolders_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var filterDto = new JobFolderFilter { BrandId = 1 };
            var exceptionMessage = "Database connection failed";

            mockJobFolderRepository.Setup(r => r.ListAsync(It.IsAny<ISpecification<JobFolder>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.SearchJobFolders(filterDto);

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
        /// Tests that SearchJobFolders handles extreme integer values in filter properties correctly.
        /// Input: JobFolderFilter with extreme integer values.
        /// Expected: Returns OkObjectResult with mapped results or handles appropriately.
        /// </summary>
        [TestMethod]
        [DataRow(int.MaxValue, null, null, null)]
        [DataRow(null, int.MinValue, null, null)]
        [DataRow(null, null, int.MaxValue, null)]
        [DataRow(null, null, null, int.MinValue)]
        [DataRow(-1, -1, -1, -1)]
        public async Task SearchJobFolders_WithExtremeIntegerValues_ReturnsOkResult(
            int? brandId, int? id, int? countryId, int? regionId)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var filterDto = new JobFolderFilter
            {
                BrandId = brandId,
                Id = id,
                CountryId = countryId,
                RegionId = regionId
            };
            var emptyJobFolders = new List<JobFolder>();
            var emptyMappedDtos = new List<JobFolderDto>();

            mockJobFolderRepository.Setup(r => r.ListAsync(It.IsAny<ISpecification<JobFolder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyJobFolders);
            mockMapper.Setup(m => m.Map<List<JobFolderDto>>(emptyJobFolders)).Returns(emptyMappedDtos);

            // Act
            var result = await controller.SearchJobFolders(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that SearchJobFolders handles null exception in catch block correctly.
        /// Input: Repository throws exception with null message.
        /// Expected: Returns ObjectResult with status code 500, handles null exception message gracefully.
        /// </summary>
        [TestMethod]
        public async Task SearchJobFolders_WhenExceptionHasNullMessage_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var filterDto = new JobFolderFilter { BrandId = 1 };

            mockJobFolderRepository.Setup(r => r.ListAsync(It.IsAny<ISpecification<JobFolder>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await controller.SearchJobFolders(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
        }

        /// <summary>
        /// Tests that SearchJobFolders correctly invokes repository with created specification.
        /// Input: Valid JobFolderFilter.
        /// Expected: Repository ListAsync is called exactly once with a specification, returns OkResult.
        /// </summary>
        [TestMethod]
        public async Task SearchJobFolders_InvokesRepositoryWithSpecification_ReturnsOkResult()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var filterDto = new JobFolderFilter { BrandId = 1 };
            var jobFolders = new List<JobFolder>();
            var mappedDtos = new List<JobFolderDto>();

            mockJobFolderRepository.Setup(r => r.ListAsync(It.IsAny<ISpecification<JobFolder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(jobFolders);
            mockMapper.Setup(m => m.Map<List<JobFolderDto>>(jobFolders)).Returns(mappedDtos);

            // Act
            var result = await controller.SearchJobFolders(filterDto);

            // Assert
            mockJobFolderRepository.Verify(r => r.ListAsync(It.IsAny<ISpecification<JobFolder>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMapper.Verify(m => m.Map<List<JobFolderDto>>(It.IsAny<IReadOnlyList<JobFolder>>()), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that CreateJobFolder returns 500 status code when mapper throws an exception.
        /// Input: JobFolderDto that causes mapper to throw an exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task CreateJobFolder_WhenMapperThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobFolderDto = new JobFolderDto { Id = 1 };
            var exceptionMessage = "Mapping failed";

            mockMapper.Setup(m => m.Map(jobFolderDto, It.IsAny<JobFolder>()))
                .Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.CreateJobFolder(jobFolderDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetJobFolders action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that CreateJobFolder returns 500 status code when repository AddAsync throws an exception.
        /// Input: Valid JobFolderDto but repository throws an exception during AddAsync.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task CreateJobFolder_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobFolderDto = new JobFolderDto
            {
                Id = 0,
                Name = "Test Folder",
                BrandId = 1
            };

            var exceptionMessage = "Database connection failed";

            mockMapper.Setup(m => m.Map(jobFolderDto, It.IsAny<JobFolder>()));

            mockJobFolderRepository.Setup(r => r.AddAsync(It.IsAny<JobFolder>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.CreateJobFolder(jobFolderDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetJobFolders action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that CreateJobFolder returns 500 status code when null JobFolderDto causes exception.
        /// Input: Null JobFolderDto.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task CreateJobFolder_WithNullJobFolderDto_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            JobFolderDto? jobFolderDto = null;
            var exceptionMessage = "Value cannot be null";

            mockMapper.Setup(m => m.Map(jobFolderDto, It.IsAny<JobFolder>()))
                .Throws(new ArgumentNullException("jobFolderDto", exceptionMessage));

            // Act
            var result = await controller.CreateJobFolder(jobFolderDto!);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetJobFolders action")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that CreateJobFolder sets DateCreated property before saving.
        /// Input: Valid JobFolderDto.
        /// Expected: DateCreated property is set on the JobFolder entity before calling AddAsync.
        /// </summary>
        [TestMethod]
        public async Task CreateJobFolder_WithValidInput_SetsDateCreatedBeforeSaving()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobFolderDto = new JobFolderDto
            {
                Id = 0,
                Name = "Test Folder",
                BrandId = 1
            };

            JobFolder? capturedJobFolder = null;
            var beforeCall = DateTime.Now;

            mockMapper.Setup(m => m.Map(jobFolderDto, It.IsAny<JobFolder>()))
                .Callback<JobFolderDto, JobFolder>((src, dest) =>
                {
                    dest.Name = src.Name!;
                    dest.BrandId = src.BrandId;
                });

            mockJobFolderRepository.Setup(r => r.AddAsync(It.IsAny<JobFolder>(), It.IsAny<CancellationToken>()))
                .Callback<JobFolder, CancellationToken>((jf, ct) => capturedJobFolder = jf)
                .ReturnsAsync((JobFolder jf, CancellationToken ct) => jf);

            // Act
            var result = await controller.CreateJobFolder(jobFolderDto);
            var afterCall = DateTime.Now;

            // Assert
            Assert.IsNotNull(capturedJobFolder);
            Assert.IsNotNull(capturedJobFolder.DateCreated);
            Assert.IsTrue(capturedJobFolder.DateCreated >= beforeCall);
            Assert.IsTrue(capturedJobFolder.DateCreated <= afterCall);
        }

        /// <summary>
        /// Tests that CreateJobFolder returns 500 status code with multiple types of exceptions.
        /// Input: JobFolderDto with various exception scenarios.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message for all exception types.
        /// </summary>
        [TestMethod]
        [DataRow("InvalidOperationException", DisplayName = "InvalidOperationException")]
        [DataRow("ArgumentException", DisplayName = "ArgumentException")]
        [DataRow("NullReferenceException", DisplayName = "NullReferenceException")]
        public async Task CreateJobFolder_WithVariousExceptions_ReturnsInternalServerError(string exceptionType)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobFolderDto = new JobFolderDto { Id = 1 };

            Exception exception = exceptionType switch
            {
                "InvalidOperationException" => new InvalidOperationException("Invalid operation"),
                "ArgumentException" => new ArgumentException("Invalid argument"),
                "NullReferenceException" => new NullReferenceException("Null reference"),
                _ => new Exception("Generic exception")
            };

            mockMapper.Setup(m => m.Map(jobFolderDto, It.IsAny<JobFolder>()))
                .Throws(exception);

            // Act
            var result = await controller.CreateJobFolder(jobFolderDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error", objectResult.Value);
        }

        /// <summary>
        /// Tests that SaveJob returns BadRequest when jobDto is null.
        /// Input: null jobDto.
        /// Expected: Returns BadRequestObjectResult with message "Job object is null", logs error.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithNullJobDto_ReturnsBadRequest()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            // Act
            var result = await controller.SaveJob(null!);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual("Job object is null", badRequestResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Job object sent from client is null.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SaveJob returns BadRequest when ModelState is invalid.
        /// Input: Valid jobDto but invalid ModelState.
        /// Expected: Returns BadRequestObjectResult with message "Invalid Job object", logs error.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            controller.ModelState.AddModelError("Name", "Required");

            var jobDto = new JobDto { Id = 1 };

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual("Invalid Job object", badRequestResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid Job object sent from client.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SaveJob returns NotFound when job does not exist in repository.
        /// Input: Valid jobDto with Id that does not exist.
        /// Expected: Returns NotFoundResult, logs error with job id.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithNonExistentJobId_ReturnsNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 999 };

            mockJobRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Job?)null);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("JobFolder with id: 999, hasn't been found in db.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SaveJob successfully updates job when BrandId is non-zero.
        /// Input: Valid jobDto with non-zero BrandId.
        /// Expected: Returns OkObjectResult with updated job, BrandId is updated.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithNonZeroBrandId_UpdatesBrandId()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, BrandId = 5 };
            var existingJob = new Job { BrandId = 2 };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual(5, updatedJob.BrandId);
            mockJobRepository.Verify(r => r.UpdateAsync(existingJob, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that SaveJob does not update BrandId when it is zero.
        /// Input: Valid jobDto with BrandId = 0.
        /// Expected: Returns OkObjectResult with updated job, BrandId remains unchanged.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithZeroBrandId_DoesNotUpdateBrandId()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, BrandId = 0 };
            var existingJob = new Job { BrandId = 2 };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual(2, updatedJob.BrandId);
        }

        /// <summary>
        /// Tests that SaveJob successfully updates job when JobCode is provided.
        /// Input: Valid jobDto with JobCode.
        /// Expected: Returns OkObjectResult with updated job, JobCode is updated.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithJobCode_UpdatesJobCode()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, JobCode = "NEW123" };
            var existingJob = new Job { JobCode = "OLD123" };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual("NEW123", updatedJob.JobCode);
        }

        /// <summary>
        /// Tests that SaveJob does not update JobCode when it is null.
        /// Input: Valid jobDto with null JobCode.
        /// Expected: Returns OkObjectResult with updated job, JobCode remains unchanged.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithNullJobCode_DoesNotUpdateJobCode()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, JobCode = null };
            var existingJob = new Job { JobCode = "OLD123" };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual("OLD123", updatedJob.JobCode);
        }

        /// <summary>
        /// Tests that SaveJob successfully updates CustomerCode when provided.
        /// Input: Valid jobDto with CustomerCode.
        /// Expected: Returns OkObjectResult with updated job, CustomerCode is updated.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithCustomerCode_UpdatesCustomerCode()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, CustomerCode = "CUST123" };
            var existingJob = new Job { CustomerCode = "CUST000" };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual("CUST123", updatedJob.CustomerCode);
        }

        /// <summary>
        /// Tests that SaveJob does not update CustomerCode when it is null.
        /// Input: Valid jobDto with null CustomerCode.
        /// Expected: Returns OkObjectResult with updated job, CustomerCode remains unchanged.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithNullCustomerCode_DoesNotUpdateCustomerCode()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, CustomerCode = null };
            var existingJob = new Job { CustomerCode = "CUST000" };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual("CUST000", updatedJob.CustomerCode);
        }

        /// <summary>
        /// Tests that SaveJob successfully updates DateFrom when valid date string is provided.
        /// Input: Valid jobDto with DateFrom as valid date string.
        /// Expected: Returns OkObjectResult with updated job, DateFrom is parsed and updated.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithValidDateFrom_UpdatesDateFrom()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, DateFrom = "2024-01-15" };
            var existingJob = new Job { DateFrom = new DateTime(2023, 1, 1) };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual(new DateTime(2024, 1, 15), updatedJob.DateFrom);
        }

        /// <summary>
        /// Tests that SaveJob does not update DateFrom when it is null.
        /// Input: Valid jobDto with null DateFrom.
        /// Expected: Returns OkObjectResult with updated job, DateFrom remains unchanged.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithNullDateFrom_DoesNotUpdateDateFrom()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var originalDate = new DateTime(2023, 1, 1);
            var jobDto = new JobDto { Id = 1, DateFrom = null };
            var existingJob = new Job { DateFrom = originalDate };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual(originalDate, updatedJob.DateFrom);
        }

        /// <summary>
        /// Tests that SaveJob successfully updates DateTo when valid date string is provided.
        /// Input: Valid jobDto with DateTo as valid date string.
        /// Expected: Returns OkObjectResult with updated job, DateTo is parsed and updated.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithValidDateTo_UpdatesDateTo()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, DateTo = "2024-12-31" };
            var existingJob = new Job { DateTo = new DateTime(2023, 12, 31) };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual(new DateTime(2024, 12, 31), updatedJob.DateTo);
        }

        /// <summary>
        /// Tests that SaveJob does not update DateTo when it is null.
        /// Input: Valid jobDto with null DateTo.
        /// Expected: Returns OkObjectResult with updated job, DateTo remains unchanged.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithNullDateTo_DoesNotUpdateDateTo()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var originalDate = new DateTime(2023, 12, 31);
            var jobDto = new JobDto { Id = 1, DateTo = null };
            var existingJob = new Job { DateTo = originalDate };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual(originalDate, updatedJob.DateTo);
        }

        /// <summary>
        /// Tests that SaveJob successfully updates Name when provided.
        /// Input: Valid jobDto with Name.
        /// Expected: Returns OkObjectResult with updated job, Name is updated.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithName_UpdatesName()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, Name = "New Job Name" };
            var existingJob = new Job { Name = "Old Job Name" };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual("New Job Name", updatedJob.Name);
        }

        /// <summary>
        /// Tests that SaveJob does not update Name when it is null.
        /// Input: Valid jobDto with null Name.
        /// Expected: Returns OkObjectResult with updated job, Name remains unchanged.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithNullName_DoesNotUpdateName()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, Name = null };
            var existingJob = new Job { Name = "Old Job Name" };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual("Old Job Name", updatedJob.Name);
        }

        /// <summary>
        /// Tests that SaveJob successfully updates Description when provided.
        /// Input: Valid jobDto with Description.
        /// Expected: Returns OkObjectResult with updated job, Description is updated.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithDescription_UpdatesDescription()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, Description = "New Description" };
            var existingJob = new Job { Description = "Old Description" };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual("New Description", updatedJob.Description);
        }

        /// <summary>
        /// Tests that SaveJob does not update Description when it is null.
        /// Input: Valid jobDto with null Description.
        /// Expected: Returns OkObjectResult with updated job, Description remains unchanged.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithNullDescription_DoesNotUpdateDescription()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, Description = null };
            var existingJob = new Job { Description = "Old Description" };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual("Old Description", updatedJob.Description);
        }

        /// <summary>
        /// Tests that SaveJob successfully updates all properties when provided.
        /// Input: Valid jobDto with all updatable properties set.
        /// Expected: Returns OkObjectResult with updated job, all properties are updated.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithAllProperties_UpdatesAllProperties()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto
            {
                Id = 1,
                BrandId = 5,
                JobCode = "JOB123",
                CustomerCode = "CUST123",
                DateFrom = "2024-01-01",
                DateTo = "2024-12-31",
                Name = "Test Job",
                Description = "Test Description"
            };

            var existingJob = new Job
            {
                BrandId = 1,
                JobCode = "OLD",
                CustomerCode = "OLD",
                DateFrom = null,
                DateTo = null,
                Name = "Old",
                Description = "Old"
            };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual(5, updatedJob.BrandId);
            Assert.AreEqual("JOB123", updatedJob.JobCode);
            Assert.AreEqual("CUST123", updatedJob.CustomerCode);
            Assert.AreEqual(new DateTime(2024, 1, 1), updatedJob.DateFrom);
            Assert.AreEqual(new DateTime(2024, 12, 31), updatedJob.DateTo);
            Assert.AreEqual("Test Job", updatedJob.Name);
            Assert.AreEqual("Test Description", updatedJob.Description);
        }

        /// <summary>
        /// Tests that SaveJob returns 500 status code when repository GetByIdAsync throws exception.
        /// Input: Valid jobDto but GetByIdAsync throws exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WhenGetByIdAsyncThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1 };
            var exceptionMessage = "Database connection failed";

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.SaveJob(jobDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside SaveJobFolder action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SaveJob returns 500 status code when repository UpdateAsync throws exception.
        /// Input: Valid jobDto but UpdateAsync throws exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WhenUpdateAsyncThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, BrandId = 5 };
            var existingJob = new Job { BrandId = 2 };
            var exceptionMessage = "Update failed";

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.SaveJob(jobDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside SaveJobFolder action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SaveJob returns 500 status code when DateTime.Parse throws FormatException for DateFrom.
        /// Input: Valid jobDto with invalid DateFrom format.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithInvalidDateFromFormat_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, DateFrom = "invalid-date" };
            var existingJob = new Job();

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);

            // Act
            var result = await controller.SaveJob(jobDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside SaveJobFolder action")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SaveJob returns 500 status code when DateTime.Parse throws FormatException for DateTo.
        /// Input: Valid jobDto with invalid DateTo format.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithInvalidDateToFormat_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, DateTo = "not-a-date" };
            var existingJob = new Job();

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);

            // Act
            var result = await controller.SaveJob(jobDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside SaveJobFolder action")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SaveJob handles empty string values correctly for string properties.
        /// Input: Valid jobDto with empty strings for JobCode, CustomerCode, Name, Description.
        /// Expected: Returns OkObjectResult with updated job, empty strings are set.
        /// </summary>
        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public async Task SaveJob_WithEmptyOrWhitespaceStrings_UpdatesProperties(string value)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto
            {
                Id = 1,
                JobCode = value,
                CustomerCode = value,
                Name = value,
                Description = value
            };

            var existingJob = new Job
            {
                JobCode = "OLD",
                CustomerCode = "OLD",
                Name = "OLD",
                Description = "OLD"
            };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual(value, updatedJob.JobCode);
            Assert.AreEqual(value, updatedJob.CustomerCode);
            Assert.AreEqual(value, updatedJob.Name);
            Assert.AreEqual(value, updatedJob.Description);
        }

        /// <summary>
        /// Tests that SaveJob handles boundary values for Id parameter.
        /// Input: Valid jobDto with boundary Id values (int.MinValue, int.MaxValue, 0, 1).
        /// Expected: Returns appropriate response based on whether job exists.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue)]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(int.MaxValue)]
        public async Task SaveJob_WithBoundaryIdValues_HandlesCorrectly(int id)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = id };

            mockJobRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Job?)null);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        /// <summary>
        /// Tests that SaveJob handles negative BrandId correctly (should update if != 0).
        /// Input: Valid jobDto with negative BrandId.
        /// Expected: Returns OkObjectResult with updated job, BrandId is updated.
        /// </summary>
        [TestMethod]
        public async Task SaveJob_WithNegativeBrandId_UpdatesBrandId()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { Id = 1, BrandId = -1 };
            var existingJob = new Job { BrandId = 2 };

            mockJobRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingJob);
            mockJobRepository.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SaveJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var updatedJob = okResult.Value as Job;
            Assert.IsNotNull(updatedJob);
            Assert.AreEqual(-1, updatedJob.BrandId);
        }

        /// <summary>
        /// Tests that CreateJob sets the UploadedOn property to current datetime before saving.
        /// Input: Valid JobDto.
        /// Expected: Job entity passed to repository has UploadedOn property set to a recent datetime.
        /// </summary>
        [TestMethod]
        public async Task CreateJob_WithValidJobDto_SetsUploadedOnProperty()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto
            {
                JobCode = "JOB002",
                BrandId = 1,
                CustomerCode = "CUST002",
                JobFolderId = 1
            };

            Job? capturedJob = null;
            var beforeCall = DateTime.Now;

            mockJobRepository.Setup(r => r.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Callback<Job, CancellationToken>((job, ct) => capturedJob = job)
                .ReturnsAsync((Job job, CancellationToken ct) => job);

            // Act
            await controller.CreateJob(jobDto);
            var afterCall = DateTime.Now;

            // Assert
            Assert.IsNotNull(capturedJob);
            Assert.IsTrue(capturedJob.UploadedOn >= beforeCall && capturedJob.UploadedOn <= afterCall,
                $"Expected UploadedOn to be between {beforeCall} and {afterCall}, but was {capturedJob.UploadedOn}");
        }

        /// <summary>
        /// Tests that CreateJob returns 500 Internal Server Error when mapper throws an exception.
        /// Input: JobDto that causes mapper to throw exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task CreateJob_WhenMapperThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto { JobCode = "TEST" };
            var exceptionMessage = "Mapping error occurred";

            mockMapper.Setup(m => m.Map(It.IsAny<JobDto>(), It.IsAny<Job>()))
                .Throws(new Exception(exceptionMessage));

            // Act
            var result = await controller.CreateJob(jobDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside CreateJob action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that CreateJob returns 500 Internal Server Error when repository throws an exception.
        /// Input: Valid JobDto, but repository.AddAsync throws exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task CreateJob_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto
            {
                JobCode = "JOB003",
                BrandId = 1,
                CustomerCode = "CUST003",
                JobFolderId = 1
            };

            var exceptionMessage = "Database connection failed";

            mockJobRepository.Setup(r => r.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.CreateJob(jobDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside CreateJob action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that CreateJob handles null JobDto parameter by catching the resulting NullReferenceException.
        /// Input: Null JobDto parameter.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task CreateJob_WithNullJobDto_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            mockMapper.Setup(m => m.Map(It.IsAny<JobDto>(), It.IsAny<Job>()))
                .Throws(new NullReferenceException("Object reference not set to an instance of an object."));

            // Act
            var result = await controller.CreateJob(null!);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside CreateJob action")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that CreateJob returns OkObjectResult when JobDto has boundary values for numeric properties.
        /// Input: JobDto with int.MaxValue and int.MinValue for Id and BrandId.
        /// Expected: Returns OkObjectResult with created job, handles boundary values correctly.
        /// </summary>
        [TestMethod]
        [DataRow(int.MaxValue, int.MaxValue)]
        [DataRow(int.MinValue, int.MinValue)]
        [DataRow(0, 0)]
        public async Task CreateJob_WithBoundaryNumericValues_ReturnsOkWithCreatedJob(int brandId, int uploadedBy)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobDto = new JobDto
            {
                JobCode = "JOB005",
                BrandId = brandId,
                CustomerCode = "CUST005",
                UploadedBy = uploadedBy,
                JobFolderId = 1
            };

            var createdJob = new Job
            {
                JobCode = "JOB005",
                BrandId = brandId,
                CustomerCode = "CUST005",
                UploadedBy = uploadedBy,
                JobFolderId = 1
            };

            mockJobRepository.Setup(r => r.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdJob);

            // Act
            var result = await controller.CreateJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
        }

        /// <summary>
        /// Tests that CreateJob handles special characters and very long strings in JobDto properties.
        /// Input: JobDto with special characters and long strings in Name, JobCode, and CustomerCode.
        /// Expected: Returns OkObjectResult with created job, handles special characters correctly.
        /// </summary>
        [TestMethod]
        public async Task CreateJob_WithSpecialCharactersAndLongStrings_ReturnsOkWithCreatedJob()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var longString = new string('A', 1000);
            var specialChars = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~";

            var jobDto = new JobDto
            {
                JobCode = specialChars,
                BrandId = 1,
                CustomerCode = longString,
                Name = $"{specialChars}{longString}",
                Description = "\t\n\r",
                JobFolderId = 1
            };

            var createdJob = new Job
            {
                JobCode = specialChars,
                CustomerCode = longString,
                Name = $"{specialChars}{longString}",
                JobFolderId = 1
            };

            mockJobRepository.Setup(r => r.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdJob);

            // Act
            var result = await controller.CreateJob(jobDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that SaveJobFolder returns BadRequest when jobFolderDto parameter is null.
        /// Input: Null jobFolderDto.
        /// Expected: Returns BadRequestObjectResult with message "JobFolder object is null" and logs error.
        /// </summary>
        [TestMethod]
        public async Task SaveJobFolder_WithNullJobFolderDto_ReturnsBadRequest()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            // Act
            var result = await controller.SaveJobFolder(null!);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual("JobFolder object is null", badRequestResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("JobFolder object sent from client is null.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SaveJobFolder returns BadRequest when ModelState is invalid.
        /// Input: Valid jobFolderDto but invalid ModelState.
        /// Expected: Returns BadRequestObjectResult with message "Invalid JobFolder object" and logs error.
        /// </summary>
        [TestMethod]
        public async Task SaveJobFolder_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            controller.ModelState.AddModelError("Name", "Required");

            var jobFolderDto = new JobFolderDto { Id = 1, Name = "Test" };

            // Act
            var result = await controller.SaveJobFolder(jobFolderDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual("Invalid JobFolder object", badRequestResult.Value);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid JobFolder object sent from client.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SaveJobFolder returns NotFound when JobFolder with specified Id does not exist.
        /// Input: Valid jobFolderDto with Id that does not exist in repository.
        /// Expected: Returns NotFoundResult and logs error.
        /// </summary>
        [TestMethod]
        public async Task SaveJobFolder_WithNonExistentJobFolderId_ReturnsNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobFolderDto = new JobFolderDto { Id = 999, Name = "Test" };

            mockJobFolderRepository.Setup(r => r.FirstAsync(It.IsAny<JobFolderSpecification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((JobFolder?)null);

            // Act
            var result = await controller.SaveJobFolder(jobFolderDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("JobFolder with id: 999, hasn't been found in db.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SaveJobFolder returns 500 status code when FirstAsync throws an exception.
        /// Input: Valid jobFolderDto that causes repository to throw exception.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task SaveJobFolder_WhenFirstAsyncThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<JobsController>>();
            var mockMapper = new Mock<IMapper>();
            var mockJobRepository = new Mock<IAsyncRepository<Job>>();
            var mockJobFolderRepository = new Mock<IAsyncRepository<JobFolder>>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();

            var controller = new JobsController(
                mockLogger.Object,
                mockMapper.Object,
                mockJobRepository.Object,
                mockJobFolderRepository.Object,
                mockConfiguration.Object,
                mockCountryRepository.Object);

            var jobFolderDto = new JobFolderDto { Id = 1, Name = "Test" };
            var exceptionMessage = "Database connection failed";

            mockJobFolderRepository.Setup(r => r.FirstAsync(It.IsAny<JobFolderSpecification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.SaveJobFolder(jobFolderDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside SaveJobFolder action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

    }
}