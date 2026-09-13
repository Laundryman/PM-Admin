using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using System.Net;

namespace PM_AdminApp.Server.Controllers
{
    [Authorize]
    [Route("api/reporting/[action]")]
    [ApiController]
    public class ReportingController : ControllerBase
    {
            private readonly ILogger<ReportingController> _logger;

            private readonly IMapper _mapper;
            private readonly IAsyncRepositoryLong<AuditLog> _auditLogRepository;
            private readonly IConfiguration _configuration;

            public ReportingController(ILogger<ReportingController> logger, IMapper mapper, IAsyncRepositoryLong<AuditLog> auditLogRepository, IConfiguration configuration)
            {
                _logger = logger;
                _mapper = mapper;
                _auditLogRepository = auditLogRepository;
                _configuration = configuration;
            }

            [HttpPost]
            public async Task<IActionResult> GetUserActionsReport([FromBody] ReportingFilterDto filterDto)
            {
                try
                {
                    var specification = new UsageReportSpecification(filterDto);
                var result = await _auditLogRepository.ListAsync(specification);
                //var result = await _auditLogRepository.ListAllAsync();
                return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while generating the user actions report.");
                    return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while generating the report.");
                }
            }
    }
}
