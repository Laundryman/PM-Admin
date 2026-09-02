using AutoMapper;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Interfaces;
using System.Net;
using PMApplication.Specifications;

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
            public IActionResult GetUserActionsReport([FromBody] ReportingFilterDto filterDto)
            {
                var specification = new UsageReportSpecification(filterDto);
                var result = _auditLogRepository.ListAsync(specification).Result;
                return Ok(result);
            }
    }
}
