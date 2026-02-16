using AutoMapper;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.JobsAggregate;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;

namespace PM_AdminApp.Server.Controllers
{
    [Authorize]
    [Route("api/jobs/[action]")]
    [ApiController]
    public class JobsController : ControllerBase
    {

        private readonly ILogger<JobsController> _logger;
        private readonly IMapper _mapper;
        private readonly IAsyncRepository<Job> _jobRepository;
        private readonly IAsyncRepository<JobFolder> _jobFolderRepository;
        private readonly IConfiguration _configuration;


        public JobsController(ILogger<JobsController> logger, IMapper mapper, IAsyncRepository<Job> jobRepository, IAsyncRepository<JobFolder> jobFolderRepository, IConfiguration configuration)
        {
            _logger = logger;
            _mapper = mapper;
            _jobRepository = jobRepository;
            _jobFolderRepository = jobFolderRepository;
            _configuration = configuration;
        }

        [HttpPost(Name = "JobFolders")]
        public async Task<IActionResult> SearchJobFolders(JobFolderFilter filterDto)
        {
            try
            {
                var spec = new JobFolderSpecification(filterDto);
                var jobFolders = await _jobFolderRepository.ListAsync(spec);

                var jfResponse = _mapper.Map<JobFolder>(jobFolders);
                return Ok(jobFolders);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetBrands action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }




    }
}
