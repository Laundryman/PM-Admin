using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PM_AdminApp.Server.Controllers;
using PMApplication.Entities;
using PMApplication.Interfaces;

namespace PM_AdminApp.Server.Controllers.UnitTests
{
    [TestClass]
    public class RoleControllerTests
    {
        /// <summary>
        /// Tests that the RoleController constructor successfully initializes with all valid dependencies.
        /// Input: All required dependencies (IMapper, IAsyncRepository, ILogger, IConfiguration).
        /// Expected: Constructor completes without throwing exceptions and instance is created successfully.
        /// </summary>
        [TestMethod]
        public void Constructor_WithAllValidDependencies_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockRoleRepository = new Mock<IAsyncRepository<Role>>();
            var mockLogger = new Mock<ILogger<RoleController>>();
            var mockConfiguration = new Mock<IConfiguration>();

            // Act
            var controller = new RoleController(
                mockMapper.Object,
                mockRoleRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object);

            // Assert
            Assert.IsNotNull(controller);
        }

        /// <summary>
        /// Tests that GetRoles returns OkObjectResult with roles when repository returns data.
        /// Input: None (method has no parameters).
        /// Expected: Returns OkObjectResult containing the list of roles from repository.
        /// </summary>
        [TestMethod]
        public async Task GetRoles_WhenRepositoryReturnsRoles_ReturnsOkWithRoles()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockRoleRepository = new Mock<IAsyncRepository<Role>>();
            var mockLogger = new Mock<ILogger<RoleController>>();
            var mockConfiguration = new Mock<IConfiguration>();

            var controller = new RoleController(
                mockMapper.Object,
                mockRoleRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object);

            var expectedRoles = new List<Role>
            {
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" }
            };

            mockRoleRepository.Setup(r => r.ListAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedRoles);

            // Act
            var result = await controller.GetRoles();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var roles = okResult.Value as IReadOnlyList<Role>;
            Assert.IsNotNull(roles);
            Assert.AreEqual(2, roles.Count);
        }

        /// <summary>
        /// Tests that GetRoles returns OkObjectResult with empty list when repository returns no roles.
        /// Input: None (method has no parameters).
        /// Expected: Returns OkObjectResult containing an empty list.
        /// </summary>
        [TestMethod]
        public async Task GetRoles_WithNoResults_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockRoleRepository = new Mock<IAsyncRepository<Role>>();
            var mockLogger = new Mock<ILogger<RoleController>>();
            var mockConfiguration = new Mock<IConfiguration>();

            var controller = new RoleController(
                mockMapper.Object,
                mockRoleRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object);

            var emptyRoles = new List<Role>();

            mockRoleRepository.Setup(r => r.ListAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyRoles);

            // Act
            var result = await controller.GetRoles();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var roles = okResult.Value as IReadOnlyList<Role>;
            Assert.IsNotNull(roles);
            Assert.AreEqual(0, roles.Count);
        }

        /// <summary>
        /// Tests that GetRoles returns 500 status code when repository throws an exception.
        /// Input: None (method has no parameters).
        /// Expected: Returns ObjectResult with status code 500 and "Internal server error" message, logs warning.
        /// </summary>
        [TestMethod]
        public async Task GetRoles_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var mockRoleRepository = new Mock<IAsyncRepository<Role>>();
            var mockLogger = new Mock<ILogger<RoleController>>();
            var mockConfiguration = new Mock<IConfiguration>();

            var controller = new RoleController(
                mockMapper.Object,
                mockRoleRepository.Object,
                mockLogger.Object,
                mockConfiguration.Object);

            var exceptionMessage = "Database connection failed";

            mockRoleRepository.Setup(r => r.ListAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetRoles();

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Something went wrong inside GetRoles action") && v.ToString()!.Contains(exceptionMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}