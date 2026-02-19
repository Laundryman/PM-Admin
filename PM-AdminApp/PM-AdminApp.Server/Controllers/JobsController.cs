using AutoMapper;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.JobsAggregate;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using PMApplication.Dtos;

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
        private readonly IAsyncRepository<Country> _countryRepository;
        private readonly IConfiguration _configuration;


        public JobsController(ILogger<JobsController> logger, IMapper mapper, IAsyncRepository<Job> jobRepository, IAsyncRepository<JobFolder> jobFolderRepository, IConfiguration configuration, IAsyncRepository<Country> countryRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _jobRepository = jobRepository;
            _jobFolderRepository = jobFolderRepository;
            _configuration = configuration;
            _countryRepository = countryRepository;
        }

        [HttpPost(Name = "JobFolders")]
        public async Task<IActionResult> SearchJobFolders(JobFolderFilter filterDto)
        {
            try
            {
                var spec = new JobFolderSpecification(filterDto);
                var jobFolders = await _jobFolderRepository.ListAsync(spec);

                var jfResponse = _mapper.Map<List<JobFolderDto>>(jobFolders);
                    return Ok(jfResponse);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetBrands action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Updates an existing JobFolder entity with the provided data.
        /// </summary>
        /// <param name="jobFolderDto">The JobFolder data transfer object containing the updated information.</param>
        /// <returns>
        /// Returns an <see cref="OkObjectResult"/> with the updated JobFolder if successful,
        /// <see cref="BadRequestObjectResult"/> if the input is invalid or null,
        /// or <see cref="NotFoundResult"/> if the JobFolder with the specified Id is not found.
        /// </returns>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="number">
        /// <item><description>Validates that the jobFolderDto parameter is not null</description></item>
        /// <item><description>Validates the ModelState</description></item>
        /// <item><description>Retrieves the existing JobFolder from the repository using the Id</description></item>
        /// <item><description>Maps the DTO properties to the existing entity using AutoMapper</description></item>
        /// <item><description>Persists the changes to the database</description></item>
        /// </list>
        /// </remarks>
        [HttpPost(Name = "SaveJobFolder")]
        public async Task<IActionResult> SaveJobFolder(JobFolderDto jobFolderDto)
        {
            try
            {
                if (jobFolderDto == null)
                {
                    _logger.LogError("JobFolder object sent from client is null.");
                    return BadRequest("JobFolder object is null");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid JobFolder object sent from client.");
                    return BadRequest("Invalid JobFolder object");
                }

                var id = jobFolderDto.Id;
                var jobFolderFilter = new JobFolderFilter() { Id = id };
                var spec = new JobFolderSpecification(jobFolderFilter);
                var folderEdit = await _jobFolderRepository.FirstAsync(spec);

                if (folderEdit == null)
                {
                    _logger.LogError($"JobFolder with id: {id}, hasn't been found in db.");
                    return NotFound();
                }

                //map updates to part
                if (jobFolderDto.BrandId != 0)
                {
                    folderEdit.BrandId = jobFolderDto.BrandId;
                }
                if (jobFolderDto.RegionId != null)
                {
                    folderEdit.RegionId = jobFolderDto.RegionId;
                }

                if (jobFolderDto.Countries != null)
                {
                    var countriesToAdd = new List<Country>();
                    var countriesToRemove = new List<Country>();

                    foreach (var country in folderEdit.Countries)
                    {
                        var keepCountry = jobFolderDto.Countries.Where(c => c.Id == country.Id).FirstOrDefault();
                        if (keepCountry == null)
                        {
                            folderEdit.Countries.Remove(country);
                        }
                    }

                    foreach (var cntry in jobFolderDto.Countries)
                    {
                        var country = folderEdit.Countries.Where(c => c.Id == cntry.Id).FirstOrDefault();
                        if (country == null)
                        {
                            var newCountry = await _countryRepository.GetByIdAsync(cntry.Id);
                            if (newCountry != null)
                            {
                                folderEdit.Countries.Add(newCountry);
                            }
                        }
                    }
                }

                    if (jobFolderDto.Name != null)
                        folderEdit.Name = jobFolderDto.Name;
                    if (jobFolderDto.Description != null)
                        folderEdit.Description = jobFolderDto.Description;



                await _jobFolderRepository.UpdateAsync(folderEdit);

                return Ok(folderEdit);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside SaveJobFolder action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpPost(Name = "CreateJobFolder")]
        public async Task<IActionResult> CreateJobFolder(JobFolderDto jobFolderDto)
        {
            try
            {
                var jobFolder = new JobFolder();
                _mapper.Map(jobFolderDto, jobFolder);
                jobFolder.DateCreated = DateTime.Now;
                var createdFolder = await _jobFolderRepository.AddAsync(jobFolder);
                return Ok(createdFolder);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetJobFolders action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost(Name = "SaveJob")]
        public async Task<IActionResult> SaveJob(JobDto jobDto)
        {
            try
            {
                if (jobDto == null)
                {
                    _logger.LogError("Job object sent from client is null.");
                    return BadRequest("Job object is null");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid Job object sent from client.");
                    return BadRequest("Invalid Job object");
                }

                var id = jobDto.Id;

                var jobEdit = await _jobRepository.GetByIdAsync(id);

                if (jobEdit == null)
                {
                    _logger.LogError($"JobFolder with id: {id}, hasn't been found in db.");
                    return NotFound();
                }

                //map updates to part
                if (jobDto.BrandId != 0)
                {
                    jobEdit.BrandId = jobDto.BrandId;
                }
                if (jobDto.JobCode != null)
                {
                    jobEdit.JobCode = jobDto.JobCode;
                }

                if (jobDto.CustomerCode != null)
                {
                    jobEdit.CustomerCode = jobDto.CustomerCode;
                }

                if (jobDto.DateFrom != null)
                {
                    jobEdit.DateFrom = DateTime.Parse(jobDto.DateFrom);
                }

                if (jobDto.DateTo != null)
                {
                    jobEdit.DateTo = DateTime.Parse(jobDto.DateTo);
                }

                if (jobDto.Name != null)
                    jobEdit.Name = jobDto.Name;
                if (jobDto.Description != null)
                    jobEdit.Description = jobDto.Description;


                await _jobRepository.UpdateAsync(jobEdit);

                return Ok(jobEdit);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside SaveJobFolder action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }


        public async Task<IActionResult> CreateJob(JobDto jobDto)
        {
            try
            {
                var job = new Job();
                _mapper.Map(jobDto, job);
                job.UploadedOn = DateTime.Now;
                var createdJob = await _jobRepository.AddAsync(job);
                return Ok(createdJob);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside CreateJob action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}

