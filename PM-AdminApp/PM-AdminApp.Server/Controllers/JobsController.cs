using AutoMapper;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.JobsAggregate;
using PMApplication.Entities.ProductAggregate;
using PMApplication.Interfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using System.Text.Json;
using System.Text.Json.Serialization;
using PMApplication.Interfaces.RepositoryInterfaces;

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
        private readonly IAsyncRepository<Region> _regionRepository;
        private readonly IAsyncRepository<Brand> _brandRepository;


        public JobsController(ILogger<JobsController> logger, IMapper mapper, IAsyncRepository<Job> jobRepository, IAsyncRepository<JobFolder> jobFolderRepository, IConfiguration configuration, IAsyncRepository<Country> countryRepository, IAsyncRepository<Region> regionRepository, IAsyncRepository<Brand> brandRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _jobRepository = jobRepository;
            _jobFolderRepository = jobFolderRepository;
            _configuration = configuration;
            _countryRepository = countryRepository;
            _regionRepository = regionRepository;
            _brandRepository = brandRepository;
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
                jobFolderFilter.IncludeChildren = true; 
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

                    foreach (var country in folderEdit.Countries.ToList())
                    {       
                        var keepCountry = jobFolderDto.Countries.FirstOrDefault(c => c.Id == country.Id);
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
                //var brand = await _brandRepository.GetByIdAsync(jobFolderDto.BrandId);
                //jobFolder.Brand = brand;

                //var region = await _regionRepository.GetByIdAsync((int)jobFolderDto.RegionId);
                //if (region != null)
                //{
                //    jobFolder.Region = region;
                //}

                //jobFolder.Brand = null;
                //jobFolder.Region = null;

                await UpdateCountryCollection(jobFolder, jobFolderDto);
                var createdFolder = await _jobFolderRepository.AddAsync(jobFolder);
                var responseFolder = _mapper.Map<JobFolderDto>(createdFolder);
                return Ok(responseFolder);
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

        [HttpPost]
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
        
        //[ApiExplorerSettings(IgnoreApi = true)]
        //private async Task UpdateRegionsCollection(JobFolder origJob, JobFolderDto updateJob)
        //{
        //    var options = new JsonSerializerOptions();
        //    options.PropertyNameCaseInsensitive = true;
        //    options.Converters.Add(new JsonStringEnumConverter());
        //    var regionDtos = JsonSerializer.Deserialize<List<RegionDto>>(updateJob.Region, options);
        //    //var regionDtos = updateProduct.Regions;
        //    if (regionDtos == null)
        //    {
        //        regionDtos = new List<RegionDto>();
        //    }
        //    foreach (var region in regionDtos)
        //    {
        //        var origRegion = origJob.Regions.FirstOrDefault(r => r.Id == region.Id);
        //        if (origRegion == null)
        //        {

        //            var dbRegion = await _regionRepository.GetByIdAsync(region.Id);
        //            origJob.Regions.Add(dbRegion);
        //        }
        //    }

        //    var regionsToDelete = new List<Region>();
        //    for (int i = origJob.Regions.Count - 1; i >= 0; i--)
        //    {
        //        var origRegion = origJob.Regions[i];
        //        var updatedRegion = regionDtos.FirstOrDefault(r => r.Id == origRegion.Id);
        //        if (updatedRegion == null)
        //        {
        //            var dbRegion = origJob.Regions.FirstOrDefault(r => r.Id == origRegion.Id);
        //            regionsToDelete.Add(dbRegion);
        //        }
        //    }

        //    foreach (var region in regionsToDelete)
        //    {
        //        origJob.Regions.Remove(region);
        //    }

        //    //update Part.RegionList string
        //    origJob.RegionsList = string.Join(",", origJob.Regions.Select(r => r.Id));
        //}
        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task UpdateCountryCollection(JobFolder origJobFolder, JobFolderDto updateJob)
        {
            //add new countries
            //var options = new JsonSerializerOptions();
            //options.PropertyNameCaseInsensitive = true;
            //options.Converters.Add(new JsonStringEnumConverter());
            //var productountries = JsonSerializer.Deserialize<List<CountryDto>>(updateJob.Countries, options);
            var jobCountries = updateJob.Countries;
            if (jobCountries == null)
            {
                jobCountries = new List<CountryDto>();
            }
            foreach (var country in jobCountries)
            {
                var origCountry = origJobFolder.Countries.FirstOrDefault(c => c.Id == country.Id);
                if (origCountry == null)
                {
                    var dbCountry = await _countryRepository.GetByIdAsync(country.Id);
                    if (dbCountry != null)
                    {
                        origJobFolder.Countries.Add(dbCountry);
                    }
                }
            }
            //remove deleted countries
            var countriesToDelete = new List<Country>();
            // iterate over a snapshot so we can examine/remove safely
            foreach (var origCountry in origJobFolder.Countries.ToList())
            {
                var updatedCountry = jobCountries.FirstOrDefault(c => c.Id == origCountry.Id);
                if (updatedCountry == null)
                {
                    countriesToDelete.Add(origCountry);
                }
            }

            foreach (var country in countriesToDelete)
            {
                origJobFolder.Countries.Remove(country);
            }

            //update Part.CountryList string
            //origJobFolder.CountriesList = string.Join(",", origJobFolder.Countries.Select(c => c.Id));
        }

    }
}

