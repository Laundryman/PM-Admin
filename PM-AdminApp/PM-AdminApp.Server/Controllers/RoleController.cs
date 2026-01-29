using AutoMapper;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Interfaces;
using PMApplication.Services;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using PMInfrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PMApplication.Services.AzureBlobService;

namespace PM_AdminApp.Server.Controllers
{
    [Authorize]
    [Route("api/roles/[action]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly ILogger<RoleController> _logger;

        private readonly IMapper _mapper;
        private readonly IAsyncRepository<Role> _roleRepository;
        private readonly IConfiguration _configuration;


        public RoleController(IMapper mapper, IAsyncRepository<Role> roleRepository, ILogger<RoleController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        [HttpGet(Name = "Roles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _roleRepository.ListAllAsync();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetRoles action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }





    }
}

