using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PM_AdminApp.Server.Controllers;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.ClusterAggregate;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.StandAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;

namespace PM_AdminApp.Server.Controllers.UnitTests
{
    [TestClass]
    public class ClustersControllerTests
    {
        /// <summary>
        /// Tests that the ClustersController constructor successfully initializes with all valid dependencies.
        /// Input: All required dependencies (IMapper, IAsyncRepository instances, ILogger, IClusterRepository).
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
            var mockLogger = new Mock<ILogger<ClustersController>>();
            var mockClusterRepository = new Mock<IClusterRepository>();

            // Act
            var controller = new ClustersController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockClusterRepository.Object);

            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that SearchClusters returns OkObjectResult with clusters when repository returns results.
        /// Input: Valid ClusterFilterDto.
        /// Expected: Returns OkObjectResult containing the list of clusters from repository.
        /// </summary>
        [TestMethod]
        public async Task SearchClusters_WithValidFilter_ReturnsOkWithResults()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ClustersController>>();
            var mockClusterRepository = new Mock<IClusterRepository>();

            var controller = new ClustersController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockClusterRepository.Object);

            var filterDto = new ClusterFilterDto { BrandId = 1, RegionId = 2, CountryId = 3 };
            var expectedClusters = new List<SearchClusterInfo>
            {
                new SearchClusterInfo(),
                new SearchClusterInfo()
            };

            mockClusterRepository.Setup(r => r.SearchClusters(filterDto))
                .ReturnsAsync(expectedClusters);

            // Act
            var result = await controller.SearchClusters(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var clusters = okResult.Value as IReadOnlyList<SearchClusterInfo>;
            Assert.IsNotNull(clusters);
            Assert.AreEqual(2, clusters.Count);
            mockClusterRepository.Verify(r => r.SearchClusters(filterDto), Times.Once);
        }

        /// <summary>
        /// Tests that SearchClusters returns OkObjectResult with empty list when repository returns no clusters.
        /// Input: Valid ClusterFilterDto.
        /// Expected: Returns OkObjectResult containing an empty list.
        /// </summary>
        [TestMethod]
        public async Task SearchClusters_WithNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ClustersController>>();
            var mockClusterRepository = new Mock<IClusterRepository>();

            var controller = new ClustersController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockClusterRepository.Object);

            var filterDto = new ClusterFilterDto { BrandId = 999, RegionId = null, CountryId = null };
            var emptyClusters = new List<SearchClusterInfo>();

            mockClusterRepository.Setup(r => r.SearchClusters(filterDto))
                .ReturnsAsync(emptyClusters);

            // Act
            var result = await controller.SearchClusters(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var clusters = okResult.Value as IReadOnlyList<SearchClusterInfo>;
            Assert.IsNotNull(clusters);
            Assert.AreEqual(0, clusters.Count);
        }

        /// <summary>
        /// Tests that SearchClusters returns 500 status code when repository throws an exception.
        /// Input: ClusterFilterDto that causes repository to throw.
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task SearchClusters_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ClustersController>>();
            var mockClusterRepository = new Mock<IClusterRepository>();

            var controller = new ClustersController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockClusterRepository.Object);

            var filterDto = new ClusterFilterDto { BrandId = 1 };
            var exceptionMessage = "Database connection failed";

            mockClusterRepository.Setup(r => r.SearchClusters(filterDto))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.SearchClusters(filterDto);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside SearchClusters action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SearchClusters handles various edge case filter values correctly.
        /// Input: ClusterFilterDto with various combinations of valid edge case values.
        /// Expected: Returns OkObjectResult with results from repository for all filter variations.
        /// </summary>
        [TestMethod]
        [DataRow(0, null, null, DisplayName = "BrandId=0, null RegionId and CountryId")]
        [DataRow(int.MaxValue, int.MaxValue, int.MaxValue, DisplayName = "All values at int.MaxValue")]
        [DataRow(1, 0, 0, DisplayName = "BrandId=1, zero RegionId and CountryId")]
        [DataRow(int.MinValue, int.MinValue, int.MinValue, DisplayName = "All values at int.MinValue")]
        public async Task SearchClusters_WithEdgeCaseFilterValues_ReturnsOkWithResults(int brandId, int? regionId, int? countryId)
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockAsyncStandRepository = new Mock<IAsyncRepository<Stand>>();
            var mockCountryRepository = new Mock<IAsyncRepository<Country>>();
            var mockCategoryRepository = new Mock<IAsyncRepository<Category>>();
            var mockLogger = new Mock<ILogger<ClustersController>>();
            var mockClusterRepository = new Mock<IClusterRepository>();

            var controller = new ClustersController(
                mockMapper.Object,
                mockAsyncStandRepository.Object,
                mockCountryRepository.Object,
                mockCategoryRepository.Object,
                mockLogger.Object,
                mockClusterRepository.Object);

            var filterDto = new ClusterFilterDto { BrandId = brandId, RegionId = regionId, CountryId = countryId };
            var expectedClusters = new List<SearchClusterInfo> { new SearchClusterInfo() };

            mockClusterRepository.Setup(r => r.SearchClusters(It.IsAny<ClusterFilterDto>()))
                .ReturnsAsync(expectedClusters);

            // Act
            var result = await controller.SearchClusters(filterDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            mockClusterRepository.Verify(r => r.SearchClusters(It.Is<ClusterFilterDto>(f =>
                f.BrandId == brandId && f.RegionId == regionId && f.CountryId == countryId)), Times.Once);
        }
    }
}