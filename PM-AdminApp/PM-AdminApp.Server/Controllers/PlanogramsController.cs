using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PM_AdminApp.Server.Extensions;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Interfaces.ServiceInterfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using PMInfrastructure.Repositories;
using System.Net;
using static PMApplication.Enums.StatusEnums;

namespace PM_AdminApp.Server.Controllers
{
    [Authorize]
    [Route("api/planograms/[action]")]
    [ApiController]
    public class PlanogramsController : BaseController
    {
        private readonly ILogger<ProductsController> _logger;

        private readonly IMapper _mapper;
        private readonly IAsyncRepositoryLong<Planogram> _asyncPlanogramRepository;
        private readonly IPlanogramRepository _planogramRepository;
        private readonly IAsyncRepositoryLong<PlanogramLock> _planogramLockRepository;
        private readonly IAsyncRepository<Country> _countryRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;
        private readonly IAsyncRepository<Region> _regionRepository;

        private readonly IBrandService _brandService;
        private readonly IPartService _partService;
        private readonly IProductService _productService;
        private readonly IPlanogramService _planogramService;
        private readonly IAsyncRepositoryLong<Planogram> _planogramAsyncRepository;

        private readonly ICountryService _countryService;
        private readonly IRegionService _regionService;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public PlanogramsController(ILogger<ProductsController> logger, IMapper mapper,
            IAsyncRepositoryLong<Planogram> asyncPlanogramRepository, IAsyncRepository<Country> countryRepository,
            IAsyncRepository<Category> categoryRepository, IAsyncRepository<Region> regionRepository, IAsyncRepositoryLong<PlanogramLock> planogramLockRepository, IPlanogramRepository planogramRepository, IBrandService brandService, IPartService partService, IProductService productService, IPlanogramService planogramService, IAsyncRepositoryLong<Planogram> planogramAsyncRepository, ICountryService countryService, IRegionService regionService, IAuditService auditService, IConfiguration config, IWebHostEnvironment env)
        {
            _logger = logger;
            _mapper = mapper;
            _asyncPlanogramRepository = asyncPlanogramRepository;
            _countryRepository = countryRepository;
            _categoryRepository = categoryRepository;
            _regionRepository = regionRepository;
            _planogramLockRepository = planogramLockRepository;
            _planogramRepository = planogramRepository;
            _brandService = brandService;
            _partService = partService;
            _productService = productService;
            _planogramService = planogramService;
            _planogramAsyncRepository = planogramAsyncRepository;
            _countryService = countryService;
            _regionService = regionService;
            _auditService = auditService;
            _config = config;
            _env = env;
        }



        //[HttpPost]
        //public async Task<IActionResult> SearchPlanograms(PlanogramFilterDto filterDto)
        //{
        //    try
        //    {
        //        //var spec = new ProductSpecification(_mapper.Map<ProductFilter>(filterDto));
        //        var planograms = await _planogramRepository.SearchPlanograms(filterDto);

        //        return Ok(planograms);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogWarning($"Something went wrong inside SearchProducts action: {ex.Message}");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        //[HttpGet]
        //public async Task<IActionResult> Unlock([FromQuery] long id)
        //{
        //    try
        //    {
        //        var filter = new PlanogramLockFilter { PlanogramId = id };
        //        var spec = new PlanogramLockSpecification(filter);
        //        var planogramLocks = await _planogramLockRepository.ListAsync(spec);
        //        foreach (var lockItem in planogramLocks)
        //        {
        //            await _planogramLockRepository.DeleteAsync(lockItem);
        //        }

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogWarning($"Something went wrong inside Unlock action: {ex.Message}");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> GetPlanogramPreview(int planogramId)
        {
            try
            {
                var preview = await _planogramService.GetPlanogramPreview(planogramId);
                if (preview != null)
                {
                    return Ok(preview.PreviewSrc);
                }
                else
                {
                    return Ok(string.Empty);

                }


            }
            catch (Exception Ex)
            {
                //log an error

                _logger.LogError("Error getting image - " + Ex.Message + " -- stack trace is:  " + Ex.StackTrace);
                return BadRequest("Error getting image");

            }
        }

        //[Route("api/v2/planogram/rename/{planogramId}/{planoName}")]
        [HttpGet]
        public async Task<int> Rename(int planogramId, string name)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;
            var planogram = await _planogramService.GetPlanogram(planogramId);

            planogram.Name = name;
            await _planogramService.SavePlanogram(planogram);

            //Audit the action
            var audit = new AuditLog
            {
                UserId = userId,
                Date = DateTime.Now,
                BrandId = planogram.BrandId,
                Roles = userProfile?.RoleIds,
                UserName = userProfile?.DisplayName,
                Action = (int)LogActionEnum.RenamePlano,
                Message = userProfile?.DisplayName + " renamed planogram with Id " + planogramId.ToString() + " to " + name,
                PlanoId = planogramId
            };
            await _auditService.AuditEvent(audit);

            return planogramId;

        }

        //[Route("api/v2/planogram/submit/{planogramId}")]
        [HttpGet]
        public async Task<int> SubmitPlanogram(int planogramId)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;
            var planogram = await _planogramService.GetPlanogram(planogramId);
            planogram.StatusId = (int)PlanogramStatusEnum.Submitted;
            await _planogramService.SavePlanogram(planogram);

            //Audit the action
            var audit = new AuditLog
            {
                UserId = userId,
                Date = DateTime.Now,
                BrandId = planogram.BrandId,
                Roles = userProfile?.RoleIds,
                UserName = userProfile?.DisplayName,
                Action = (int)LogActionEnum.SubmitPlano,
                Message = userProfile?.DisplayName + " submitted planogram with Id " + planogramId.ToString(),
                PlanoId = planogramId
            };
            await _auditService.AuditEvent(audit);

            return planogramId;

        }

        //[Route("api/v2/planogram/delete/{planogramId}")]
        [HttpGet]
        public async Task<int> DeletePlanogram(int planogramId)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;
            var planogram = await _planogramService.GetPlanogram(planogramId);
            planogram.StatusId = (int)PlanogramStatusEnum.Deleted;
            await _planogramService.SavePlanogram(planogram);

            //Audit the action
            var audit = new AuditLog
            {
                UserId = userId,
                Date = DateTime.Now,
                BrandId = planogram.BrandId,
                Roles = userProfile?.RoleIds,
                UserName = userProfile?.DisplayName,
                Action = (int)LogActionEnum.EditPlano,
                Message = userProfile?.DisplayName + " deleted planogram with Id " + planogramId.ToString(),
                PlanoId = planogramId
            };
            await _auditService.AuditEvent(audit);

            return planogramId;

        }

        [HttpGet]
        public async Task<int> ArchivePlanogram(int planogramId, int jobId)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;
            var planogram = await _planogramService.GetPlanogram(planogramId);
            planogram.Archived = true;
            planogram.ArchivedBy = userProfile?.DisplayName;
            planogram.ArchivedDate = DateTime.Now;
            planogram.JobId = jobId;

            //planogram.StatusId = (int)PlanogramStatusEnum.Edit;
            await _planogramService.SavePlanogram(planogram);

            //Audit the action
            var audit = new AuditLog
            {
                UserId = userId,
                Date = DateTime.Now,
                BrandId = planogram.BrandId,
                Roles = userProfile?.RoleIds,
                UserName = userProfile?.DisplayName,
                Action = (int)LogActionEnum.ArchivePlano,
                Message = userProfile?.DisplayName + " archived planogram with Id " + planogramId.ToString(),
                PlanoId = planogramId
            };
            await _auditService.AuditEvent(audit);

            return planogramId;

        }

        [HttpGet]
        public async Task<int> ApprovePlanogram(int planogramId)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;
            var planogram = await _planogramService.GetPlanogram(planogramId);
            planogram.StatusId = (int)PlanogramStatusEnum.Approved;
            await _planogramService.SavePlanogram(planogram);

            //Audit the action
            var audit = new AuditLog
            {
                UserId = userId,
                Date = DateTime.Now,
                BrandId = planogram.BrandId,
                Roles = userProfile?.RoleIds,
                UserName = userProfile?.DisplayName,
                Action = (int)LogActionEnum.ApprovePlano,
                Message = userProfile?.DisplayName + " approved planogram with Id " + planogramId.ToString(),
                PlanoId = planogramId
            };
            await _auditService.AuditEvent(audit);

            return planogramId;

        }

        [HttpGet]
        public async Task<int> RestorePlanogram(int planogramId, int statusId)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;
            var planogram = await _planogramService.GetPlanogram(planogramId);
            planogram.JobId = null;
            planogram.StatusId = statusId;
            planogram.Archived = false;
            planogram.ArchivedBy = null;
            planogram.ArchivedByName = null;
            planogram.ArchivedDate = null;
            planogram.LastUpdatedBy = userProfile.Id;
            planogram.LubName = userProfile.DisplayName;
            planogram.DateUpdated = DateTime.Now;

            await _planogramService.SavePlanogram(planogram);

            //Audit the action
            var audit = new AuditLog
            {
                UserId = userId,
                Date = DateTime.Now,
                BrandId = planogram.BrandId,
                Roles = userProfile?.RoleIds,
                UserName = userProfile?.DisplayName,
                Action = (int)LogActionEnum.RestorePlano,
                Message = userProfile?.DisplayName + " approved planogram with Id " + planogramId.ToString(),
                PlanoId = planogramId
            };
            await _auditService.AuditEvent(audit);

            return planogramId;

        }

        //[Route("api/v2/planogram/validate/{planogramId}")]
        [HttpGet]
        public async Task<int> ValidatePlanogram(int planogramId)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;
            var planogram = await _planogramService.GetPlanogram(planogramId);
            planogram.StatusId = (int)PlanogramStatusEnum.Validated;
            await _planogramService.SavePlanogram(planogram);

            //Audit the action
            var audit = new AuditLog
            {
                UserId = userId,
                Date = DateTime.Now,
                BrandId = planogram.BrandId,
                Roles = userProfile?.RoleIds,
                UserName = userProfile?.DisplayName,
                Action = (int)LogActionEnum.ValidatePlano,
                Message = userProfile?.DisplayName + " validated planogram with Id " + planogramId.ToString(),
                PlanoId = planogramId
            };
            await _auditService.AuditEvent(audit);

            return planogramId;

        }


        [HttpGet]
        public async Task<int> RejectPlanogram(int planogramId)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;
            var planogram = await _planogramService.GetPlanogram(planogramId);
            planogram.StatusId = (int)PlanogramStatusEnum.Edit;
            await _planogramService.SavePlanogram(planogram);

            //Audit the action
            var audit = new AuditLog
            {
                UserId = userId,
                Date = DateTime.Now,
                BrandId = planogram.BrandId,
                Roles = userProfile?.RoleIds,
                UserName = userProfile?.DisplayName,
                Action = (int)LogActionEnum.RejectPlano,
                Message = userProfile?.DisplayName + " rejected planogram with Id " + planogramId.ToString(),
                PlanoId = planogramId
            };
            await _auditService.AuditEvent(audit);

            return planogramId;

        }

        //[Route("api/v2/planogram/getCommentCount/{planogramId}/{brandId}")]
        [HttpGet]
        public async Task<IActionResult> GetCommentCount(int planogramId, int brandId)
        {
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();

            var countryId = userProfile.CountryId;
            try
            {
                //We're not using the country and region here: but we need to think about how we might regarding users.
                var noteFilter = new NoteFilter
                {
                    UserId = userProfile.Id,
                    BrandId = brandId,
                    CountryId = countryId,
                    PlanogramId = planogramId
                };
                var planogramNotes = await _planogramService.GetPlanogramNotes(noteFilter);
                var commentCount = planogramNotes.Count();
                return Ok(commentCount);
            }
            catch (Exception ex)
            {
                var message = "";
                if (ex.InnerException != null)
                {
                    message = new string(ex.Message +
                                                        ex.InnerException.ToString());
                }
                else
                {
                    message = new string(ex.Message
                                         + ex.StackTrace);
                }
                //message.ReasonPhrase = "Error retrieving comment count";
                //log an error
                _logger.LogError("Error retrieving comment count -- " + message);

                return BadRequest("Error retrieving comment count");
            }
            finally
            {

            }


        }


        //[Route("api/v2/planogram/get/jsonskulist/{planogramId}")]
        [HttpGet]

        public async Task<IActionResult> GetSkuList(int planogramId)
        {

            try
            {
                var filter = new PlanogramFilter
                {
                    Id = planogramId,
                    LoadRelatedEntities = true

                };

                var spec = new PlanogramSpecification(filter);

                var planogram = await _planogramAsyncRepository.FirstOrDefaultAsync(spec);


                var hasColumns = planogram.Stand.ColumnList.Count != 0;
                var skuList = await _planogramService.GetSkuList(planogram.Id, planogram.UserId, hasColumns);
                var currentSkuPart = 0;
                foreach (var sku in skuList)
                {
                    var skuPart = sku.PlanogramPartsId;
                    if (skuPart == currentSkuPart)
                    {
                        //set values to null to prevent over counting in spreadsheet
                        sku.Facings = null;
                        sku.Stock = null;
                        sku.TotalSKU = null;
                        sku.UnitCost = null;
                    }
                    //set current sku part if skupart has changed
                    if (currentSkuPart != skuPart)
                    {
                        currentSkuPart = skuPart ?? 0;
                    }
                }
                var exportSku = _mapper.Map<List<ExportSkuDto>>(skuList);
                //var exportSkuJson = JsonSerializer.Serialize(exportSku);

                return Ok(exportSku);

            }
            catch (Exception Ex)
            {
                var message = "";

                if (Ex.InnerException != null)
                {
                    message = new string(Ex.Message +
                                                    Ex.InnerException.Data.ToString());
                }
                else
                {
                    message = new string(Ex.Message
                                    + Ex.StackTrace);
                }
                _logger.LogError(message);

                return Ok("Error creating Sku list " + Ex.Message);
            }

        }
        [HttpPost]
        public async Task<IActionResult> SearchPlanograms([FromBody] PlanogramFilterDto filterDto)
        {
            try
            {
                var userProfile = await this.MappedUser();
                var countriesList = new List<int>();
                var regionsList = new List<int>();
                if (!string.IsNullOrEmpty(filterDto.RegionsList))
                    regionsList = filterDto.RegionsList.Split(",").Select(int.Parse).ToList();
                if (!string.IsNullOrEmpty(filterDto.CountriesList))
                    countriesList = filterDto.CountriesList.Split(",").Select(int.Parse).ToList();

                // Only use the user's regions and countries if the filterDto does not provide them

                if (regionsList == null || regionsList.Count == 0)
                {
                    regionsList = userProfile.RegionList.Split(",").Select(int.Parse).ToList();
                }

                if (countriesList == null || countriesList.Count == 0)
                {
                    if (userProfile.CountryList != null)
                    {
                        countriesList = userProfile.CountryList.Split(",").Select(int.Parse).ToList();
                        var userCountryList = userProfile.CountryList.Split(",");
                        if (userCountryList != null && userCountryList.Length > 0)
                        {
                            countriesList = userCountryList.Select(int.Parse).ToList();
                        }
                    }

                    else
                    {
                        countriesList = new List<int>();
                    }
                }
                var regions = await _regionService.GetRegions(new RegionFilter { idList = filterDto.RegionsList, BrandId = filterDto.BrandId ?? 0 });
                var filterCountryList = new List<int>();
                var filterRegionList = new List<int>();
                foreach (var region in regions)
                {
                    if (region.CountryList != null)
                    {
                        var regionCountryList = region.CountryList.Split(",").Select(int.Parse).ToList();
                        for (int i = 0; i < regionCountryList.Count; i++)
                        {
                            if (countriesList.Contains(regionCountryList[i]))
                            {
                                filterCountryList.Add(regionCountryList[i]);
                            }
                        }

                        if (regionsList.Contains(region.Id))
                        {
                            filterRegionList.Add(region.Id);
                        }
                    }
                }

                filterDto.CountriesList = string.Join(",", filterCountryList);
                filterDto.RegionsList = string.Join(",", filterRegionList);
                var planograms = await _planogramRepository.SearchPlanograms(filterDto);

                return Ok(planograms);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside SearchProducts action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }



        /// <summary>
        /// Update to clone planogram will take account of IsUpdate to copy notes and update colours.
        /// </summary>
        /// <param name="planogramId"></param>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <param name="IsUpdate"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ClonePlanogram([FromBody] ClonePlanogramDto clonePlanogramDto)
        {
            try
            {
                //Planogram originalPlanogram = await _planogramRepository.GetByIdAsync(clonePlanogramDto.Id);

                //Planogram newPlanogram = new Planogram();


                var userProfile = await this.MappedUser();
                var userRegions = userProfile.RegionList.Split(",").Select(int.Parse).ToList();
                var userCountries = userProfile.CountryList.Split(",").Select(int.Parse).ToList();
                var regions = await _regionService.GetRegions(new RegionFilter { idList = userProfile.RegionList, BrandId = clonePlanogramDto.BrandId });
                var filterCountryList = new List<int>();

                var clonedPlanogramId = await _planogramService.ClonePlanogram(clonePlanogramDto.Id, clonePlanogramDto.Name, userProfile, clonePlanogramDto.IsUpdate ?? false);


                return Ok(clonedPlanogramId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside ClonePlanogram action: {ex.Message}");
                return StatusCode(500, "Internal server error");

            }
        }

        [HttpGet]
        public async Task<IActionResult> LockPlanogram(long planogramId = 0)
        {
            //var planoRepoService = dpRes.GetService<IPlanogramRepository>();
            Planogram planogram = await _planogramService.GetPlanogram(planogramId);
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;



            try
            {
                var filter = new PlanogramLockFilter
                {
                    PlanogramId = planogramId,
                    User = userProfile
                };
                var isLocked = await _planogramService.IsLocked(filter);
                if (!isLocked)
                {
                    //lock the planogram Now
                    return Ok("success");
                }
                else
                {
                    //it's already locked by someone else.
                    return Conflict("fail");
                }
            }
            catch (Exception Ex)
            {
                return BadRequest(Ex.Message);
            }

        }

        [HttpGet]
        public async Task<IActionResult> UnLock(long planogramId = 0)
        {
            //var planoRepoService = dpRes.GetService<IPlanogramRepository>();
            Planogram planogram = await _planogramService.GetPlanogram(planogramId);
            // we can retrieve the userId from the request
            var userProfile = await this.MappedUser();
            string? userId = userProfile?.Id;



            try
            {
                var filter = new PlanogramLockFilter
                {
                    PlanogramId = planogramId,
                    //User = userProfile
                };
                await _planogramService.UnLockPlanogram(filter);
                return Ok("success");
            }
            catch (Exception Ex)
            {
                _logger.LogError(Ex.Message);
                return BadRequest(Ex.Message);
            }

        }
        //[Route("api/v2/planx/get-plano-lock/{planogramId}/{userId}/{userName}")]
        [HttpGet]
        public async Task<IActionResult> GetPlanoLock(int planogramId)
        {
            try
            {
                var userProfile = await this.MappedUser();
                var filter = new PlanogramLockFilter
                {
                    PlanogramId = planogramId,
                    User = userProfile
                };
                var isLocked = await _planogramService.IsLocked(filter);
                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                if (!isLocked)
                {
                    message = new HttpResponseMessage(HttpStatusCode.OK);
                    await _planogramService.LockPlanogram(filter);
                    return Ok("unlocked");
                }
                return Ok("locked");
            }
            catch (Exception ex)
            {
                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.BadRequest);

                if (ex.InnerException != null)
                {
                    message.Content = new StringContent(ex.Message +
                                                        ex.InnerException.ToString());  
                }
                else
                {
                    message.Content = new StringContent(ex.Message
                                                        + ex.StackTrace);
                }
                message.ReasonPhrase = "Error getting lock";
                //log an error
                _logger.LogError("Error getting lock - " + ex.Message + " -- stack trace is:  " + ex.StackTrace);

                return BadRequest("Error getting lock info");
            }
            finally
            {

            }
        }


    }


}
