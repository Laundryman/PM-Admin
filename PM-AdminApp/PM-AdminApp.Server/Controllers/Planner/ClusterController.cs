using AutoMapper;
//using IronPdf.Engines.Chrome;
//using IronPdf.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PM_AdminApp.Server.Exceptions;
using PM_AdminApp.Server.Extensions;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities;
using PMApplication.Entities.ClusterAggregate;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Enums;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Interfaces.ServiceInterfaces;
using PMApplication.Services;
using PMApplication.Specifications.Filters;
using System.Net;
using System.Net.Http.Headers;
using System.Web;

namespace PM_AdminApp.Server.Controllers.Planner
{
    [Authorize]
    [ApiController]
    [Route("api/clusters/[action]")]
    public class ClusterController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ClusterController> _logger;
        private readonly IBrandService _brandService;
        private readonly IPartService _partService;
        private readonly IPlanogramService _planogramService;
        private readonly IClusterService _clusterService;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _config;
        private readonly ICategoryService _categoryService;
        private readonly IClusterRepository _clusterRepository;
        private readonly IStandService _standService;

        public ClusterController(IPartService partService,
                IStandService standService,
                IBrandService brandService,
                IPlanogramService planogramService,
                IMapper mapper, ILogger<ClusterController> logger, IAuditService auditService, IConfiguration config, ICategoryService categoryService, IClusterService clusterService, IClusterRepository clusterRepository)
        {
            _partService = partService;
            _standService = standService;
            _brandService = brandService;
            _planogramService = planogramService;
            _mapper = mapper;
            _logger = logger;
            _auditService = auditService;
            _config = config;
            _categoryService = categoryService;
            _clusterService = clusterService;
            _clusterRepository = clusterRepository;
            //this._versionService = versionService;
        }

        [HttpPost]
        public async Task<IActionResult> SearchClusters(ClusterFilterDto filterDto)
        {
            try
            {
                //var spec = new ProductSpecification(_mapper.Map<ProductFilter>(filterDto));
                var clusters = await _clusterRepository.SearchClusters(filterDto);

                return Ok(clusters);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside SearchClusters action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuCategories(long id)
        {
            var menu = new PlanmMenuDto();
            try
            {

                var clusterFilter = new ClusterFilter()
                {
                    Id = id
                };
                var cluster = await _clusterService.GetCluster(clusterFilter);
                var standTypeId = cluster.Stand.StandTypeId; //need to get from planogram when denormalised
                var brandId = cluster.BrandId;
                //var countryId = cluster.CountryId ?? 0;

                var partFilter = new PartFilter
                {
                    BrandId = brandId,
                    ClusterId = id,
                    StandTypeId = standTypeId
                };
                var catList = await _partService.GetPlanmClusterMenu(partFilter);
                var menuCats = new List<Category>();
                var currentCatId = 0;
                foreach (var cat in catList)
                {
                    if (cat.ParentCategoryId != currentCatId)
                    {
                        currentCatId = cat.ParentCategoryId;
                        if (!menuCats.Any(c => c.Id == cat.ParentCategoryId))
                        {
                            var pcat = await _categoryService.GetCategory(cat.ParentCategoryId);
                            menuCats.Add(pcat);
                        }
                    }
                }

                //Get Parent Categoriesry);
                var menuCategories = new List<CategoryMenuDto>();
                //loop through each category to build menu
                foreach (var cat in menuCats)
                {
                    var menucat = _mapper.Map<CategoryMenuDto>(cat);
                    menuCategories.Add(menucat);
                }
                menu.Categories = menuCategories;


                //We're not using the country and region here: but we need to think about how we might regarding users.
                return Ok(menu);
            }
            catch (Exception ex)
            {
                //log an error
                _logger.LogError("Could not get menu");
                return BadRequest("Could not get Menu");
            }
            finally
            {

            }
        }

        //[Route("api/v2/planx/get-menu/{planogramId}")]
        [HttpGet]
        public async Task<IActionResult> GetMenu(long id)
        {
            var menu = new PlanmMenuDto();
            var filter = new ClusterFilter
            {
                Id = id,
                //StandTypeId = menuParams.StandTypeId ?? 0,
                //BrandId = menuParams.BrandId ?? 0
            };

            var cluster = await _clusterService.GetCluster(filter);
            var standTypeId = cluster.Stand.StandTypeId;
            var brandId = cluster.BrandId;
            var clusterId = cluster.Id;

            try
            {

                var partFilter = new PartFilter
                {
                    BrandId = brandId,
                    ClusterId = clusterId,
                    StandTypeId = standTypeId
                };
                var menuParts = await _partService.GetPlanmClusterMenu(partFilter);

                return Ok(menuParts);
            }
            catch (Exception ex)
            {
                //log an error
                _logger.LogError("Error getting menu for cluster - " + id + " error message:  " +
                                 ex.Message);

                return StatusCode(500, "Internal server error getting menu");
            }
            finally
            {

            }

        }


        //[Route("api/v2/planx/get-planogram/{PlanogramId}")]
        [HttpGet]
        public async Task<IActionResult> GetCluster(long id)
        {
            try
            {
                var cluster = await _clusterService.GetCluster(id);


                //var planogramView = (PlanogramDTO)planogram;
                var clusterView = _mapper.Map<PlanmClusterDto>(cluster);
                return Ok(clusterView);
            }
            catch (Exception ex)
            {

                //log an error
                _logger.LogError("Error getting cluster for clusterId " + id + "---- error message - " + ex.Message + " --- " + ex.StackTrace);

                return StatusCode(500, "Internal server error getting cluster");
            }
        }

        //[Route("api/v2/planx/get-stand/{standId}")]
        [HttpPost]
        public async Task<IActionResult> GetStand(GetPlanogramParams planoParams)
        {
            //var stand = new PlanXStandViewModel();
            try
            {
                var StandFilter = new StandFilter
                {
                    Id = (int)planoParams.StandId,
                    IncludeColumnUprights = true
                };
                var stand = await _standService.GetStand(StandFilter);
                var brand = await _brandService.GetBrand(stand.BrandId);
                var standView = _mapper.Map<PlanmStandDto>(stand);

                if (brand.ShelfLock)
                {
                    standView.ShelfLock = true;
                }

                var filter = new StandTypeFilter
                {
                    Id = stand.StandTypeId,
                };
                var standType = await _standService.GetStandType(filter);
                standView.StandTypeName = standType.Name;
                standView.ParentStandTypeName = standType.ParentStandType.Name;
                return Ok(standView);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting stand for standId " + planoParams.StandId + "---- error message - " + ex.Message + " --- " + ex.StackTrace);
                return StatusCode(500, "Internal server error getting stand");
            }

        }

        //[Route("api/v2/planx/get-planogram-shelves/{planogramId}")]
        [HttpGet]
        public async Task<IActionResult> GetShelves(long id )
        {
            try
            {
                //var plano = await _planogramService.GetPlanogram(planogramId);
                var filter = new ClusterFilter()
                {
                    Id = id
                };
                var shelves = await _clusterService.GetClusterShelves(filter);
                var planoShelves = _mapper.Map<List<PlanmPartInfo>>(shelves);
                return Ok(planoShelves);
            }
            catch (Exception ex)
            {
                //log an error

                _logger.LogError("Error getting cluster shelves for clusterId " + id + "---- error message - " + ex.Message + " --- " + ex.StackTrace);
                return StatusCode(500, "Internal server error getting cluster shelves");
            }
            finally
            {

            }
        }

        //[Route("api/v2/planx/get-planogram-parts/{planogramId}")]
        [HttpGet]
        public async Task<IActionResult> GetParts(long id)
        {
            try
            {
                //var plano = await _planogramService.GetPlanogram(planogramId);
                var filter = new ClusterPartFilter()
                {
                    ClusterId = id
                };
                var parts = await _clusterService.GetClusterParts(filter);
                var planoParts = _mapper.Map<List<PlanmPartInfo>>(parts);
                //var planoShelves = plano.PlanogramShelves.Where(s => s.ScratchPadId == null || s.ScratchPadId == 0);

                //var shelfCatId = 0;

                //var response = _mapper.Map<List<PlanmPartInfo>>(planoParts.ToList());
                return Ok(planoParts);
            }
            catch (Exception ex)
            {
                //log an error

                _logger.LogError("Error getting cluster parts for clusterId " + id + "---- error message - " + ex.Message + " --- " + ex.StackTrace);
                return StatusCode(500, "Internal server error getting cluster parts");
            }
            finally
            {

            }
        }

        //[Route("api/v2/planx/get-new-parts/{planogramId}")]
        [HttpGet]
        public async Task<IActionResult> GetNewPlanogramParts(int clusterId)
        {
            //var menu = new PlanmMenuDto();
            try
            {
                //var plano = _planogramService.GetPlanogram(planogramId);
                var filter = new PlanogramPartFilter
                {
                    PlanogramId = clusterId,
                    NewParts = true
                };
                var planoParts = await _planogramService.GetPlanogramParts(filter);
                var partInfos = _mapper.Map<List<PlanmPartInfo>>(planoParts);
                //var results = plano.PlanogramParts.Where(p => p.ScratchPadId == null && p.DateUpdated == null).OrderBy(p => p.Position_x).ThenBy(p => p.Position_y).Select(p => (PlanxPartInfo)p);

                //var planoParts = results.ToList();

                //We're not using the country and region here: but we need to think about how we might regarding users.
                return Ok(partInfos);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting new parts for clusterId " + clusterId + "---- error message - " + ex.Message + " --- " + ex.StackTrace);
                return StatusCode(500, "Error getting parts");

            }
            finally
            {

            }
        }

        //[Route("api/v2/planx/get-part-products/{partId}/{planogramId}")]
        [HttpPost]
        public async Task<IActionResult> GetPartProducts(GetMenuParams menuParams)
        {
            try
            {
                var partFilter = new PartFilter
                {
                    Id = menuParams.PartId
                };
                var part = await _partService.GetPart(partFilter);

                var products = part.Products;

                var planxPartProducts = new PartProductsDto();
                planxPartProducts.PartId = (long)menuParams.PartId;
                var pvmList = _mapper.Map<List<ProductDto>>(products);


                planxPartProducts.Products = pvmList;
                return Ok(planxPartProducts);

            }
            catch (Exception ex)
            {

                //log an error
                _logger.LogError("Error getting part products for partId " + menuParams.PartId + " and clusterId " + menuParams.ClusterId + "---- error message - " + ex.Message + " --- " + ex.StackTrace);
                return BadRequest("Error getting part products");

            }
            finally
            {

            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveLayoutDetails(SaveLayoutDto layoutData)
        {

            try
            {
                var userProfile = await this.MappedUser();
                //get the clusterID
                var clusterId = layoutData.Id;
                var layout = await _clusterService.GetCluster((long)clusterId);

                if (layout == null)
                {
                    return NotFound("Cluster not found");
                }

                layout.Name = layoutData.Name;
                layout.Published = layoutData.Published;
                layout.DateUpdated = DateTime.Now;
                await _clusterService.SaveCluster(layout);

                var audit = new AuditLog
                {
                    UserId = userProfile.Id,
                    Date = DateTime.Now,
                    BrandId = layout.BrandId,
                    Roles = userProfile.RoleIds,
                    UserName = userProfile.DisplayName,
                    Action = (int)LogActionEnum.EditlLayout,
                    Message = userProfile.DisplayName + " updated layout details for layout with Id " + layout.Id,
                    PlanoId = layout.Id
                };

                var auditEvent = await _auditService.AuditEvent(audit);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error saving layout details for clusterId " + layoutData.Id +
                                 "---- error message - " + ex.Message + " --- " + ex.StackTrace);
                return StatusCode(500, "Internal server error saving layout details");
            }
        }

        //[Route("api/v2/planx/save-planogram")]
            [HttpPost]
        public async Task<IActionResult> SaveCluster(PlanmPlanogramInfo clusterData)
        {

            try
            {
                //get the clusterID
                var clusterId = clusterData.ClusterId;
                var cluster = await _clusterService.GetCluster((long)clusterId);
                var clusterParts = cluster.ClusterParts.ToList();

                var brandId = clusterData.BrandId;
                var userProfile = await this.MappedUser();

                var brand = _brandService.GetBrand((int)brandId);

                //////////////////////////////////////////////////////////////////
                //Finish user checks
                //////////////////////////////////////////////////////////////////

                cluster.DateUpdated = DateTime.Now;
                cluster.UserId = userProfile.Id;
                cluster.Name = clusterData.PlanogramName;
                cluster.CurrentVersion += 1;
                cluster.BrandId = (int)brandId;
                if (cluster.StatusId != (int)StatusEnums.PlanogramStatusEnum.Approved &&
                    cluster.StatusId != (int)StatusEnums.PlanogramStatusEnum.Archived &&
                    cluster.StatusId != (int)StatusEnums.PlanogramStatusEnum.Validated &&
                    cluster.StatusId != (int)StatusEnums.PlanogramStatusEnum.Deleted &&
                    cluster.StatusId != (int)StatusEnums.PlanogramStatusEnum.Submitted)
                {
                    cluster.StatusId = (int)StatusEnums.PlanogramStatusEnum.Edit;
                    //PlanogramStatus status =
                    //    await _planogramService.GetPlanogramStatus((int)StatusEnums.PlanogramStatusEnum.Edit);
                    //planogram.Status = status; //redundant?
                }
                //await _clusterService.SaveCluster(cluster);
                ////Audit the action
                //var audit = new AuditLog
                //{
                //    UserId = userProfile.Id,
                //    Date = DateTime.Now,
                //    BrandId = cluster.BrandId,
                //    Roles = userProfile.RoleIds,
                //    UserName = userProfile.DisplayName,
                //    Action = (int)LogActionEnum.EditCluster,
                //    Message = userProfile.DisplayName + " edited cluster with Id " + clusterId.ToString(),
                //    ClusterId = (long)clusterId
                //};

                //await _auditService.AuditEvent(audit);


                //Handle Deletions now
                if (clusterData.DeletedInfo.partInfos != null)
                {
                    await DeleteParts(clusterData.DeletedInfo.partInfos.ToList());
                }

                if (clusterData.DeletedInfo.shelfInfos != null)
                {
                    await DeleteShelves(clusterData.DeletedInfo.shelfInfos.ToList());
                }

                //Handle the scratchpad now
                //await UpdateScratchPad((long)clusterData.ClusterId, clusterData.ScratchPadInfo);


                //Handle the planogram now
                var shelves = clusterData.PlanogramInfo.shelfInfos;
                if (shelves != null)
                {
                    foreach (var shelf in shelves)
                    {
                        var planogramShelf = await SaveShelf(shelf);
                        if (planogramShelf != null)
                        {
                            if (shelf.Parts != null)
                            {
                                foreach (var part in shelf.Parts)
                                {
                                    if (part.PlanogramShelfId == 0)
                                    {
                                        part.PlanogramShelfId = planogramShelf.Id;
                                    }
                                }
                            }

                            await SaveCassettes(planogramShelf.ClusterId, shelf.Parts.ToList());
                        }
                    }
                }

                var audit = new AuditLog
                {
                    UserId = userProfile.Id,
                    Date = DateTime.Now,
                    BrandId = cluster.BrandId,
                    Roles = userProfile.RoleIds,
                    UserName = userProfile.DisplayName,
                    Action = (int)LogActionEnum.EditlLayout,
                    Message = userProfile.DisplayName + " updated layout details for layout with Id " + cluster.Id,
                    PlanoId = cluster.Id
                };

                var auditEvent = await _auditService.AuditEvent(audit);
                return Ok();
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

                message.ReasonPhrase = "Error saving cassettes";
                //log an error
                _logger.LogError("Error saving cluster " + clusterData.ClusterId + "---- error message - " + ex.Message + " --- " + ex.StackTrace);
                return StatusCode(500, "Error saving cluster");
            }

            finally
            {

            }
        }



        [Route("api/v2/planx/save-planogram-jpeg-image")]
        [HttpPost]
        public async Task<IActionResult> SavePlanogramJPEG(PlanmImageDto planoJpeg)
        {
            try
            {
                Planogram planogram = await _planogramService.GetPlanogram((int)planoJpeg.PlanogramId);
                PlanogramPreview? preview = await _planogramService.GetPlanogramPreview((int)planoJpeg.PlanogramId);

                if (preview != null)
                {
                    preview.PreviewSrc = planoJpeg.Image;
                    await _planogramService.SavePlanogramPreview(preview);
                }
                else
                {
                    preview = new PlanogramPreview();
                    preview.PlanogramId = planoJpeg.PlanogramId;
                    preview.PreviewSrc = planoJpeg.Image;
                    await _planogramService.CreatePlanogramPreview(preview);
                }
                    //_planogramService.SavePlanogram(planogram);
                var userProfile = await this.MappedUser();
                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                var audit = new AuditLog
                {
                    UserId = userProfile.Id,
                    Date = DateTime.Now,
                    BrandId = planogram.BrandId,
                    Roles = userProfile.RoleIds,
                    UserName = userProfile.DisplayName,
                    Action = (int)LogActionEnum.EditPlano,
                    Message = userProfile.DisplayName + " edited planogram with Id " + planogram.Id,
                    PlanoId = planogram.Id
                };

                var auditEvent = await _auditService.AuditEvent(audit);

                return Ok();

            }
            catch (Exception ex)
            {
                //log an error
                _logger.LogError("Error saving planogram jpeg image " + planoJpeg.PlanogramId +
                                 "---- error message - " +
                                 ex.Message + " --- " + ex.StackTrace);
                return StatusCode(500, "Internal server error saving jpeg");

            }
        }




        [Route("api/v2/planx/save-planogram-svg-image")]
        [HttpPut]
        public async Task<IActionResult> SavePlanogramSVG(PlanmImageDto planoSvg)
        {
            if (planoSvg != null)
            {
                Planogram planogram = await _planogramService.GetPlanogram((int)planoSvg.PlanogramId);



                bool isDevServer = _config["AppSettings:isDevServer"] == "True" ? true : false;

                try
                {
                    return Ok();
                }
                catch (Exception ex)
                {
                    //log an error
                    _logger.LogError("Error saving planogram svg image " + planoSvg.PlanogramId +
                                     "---- error message - " +
                                     ex.Message + " --- " + ex.StackTrace);
                    return StatusCode(500, "Internal server error saving snapshot");

                }
            }
            else
            {

                _logger.LogError("No Planogram Id Supplied");
                return StatusCode(500, "Internal server error saving snapshot");

            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCluster(long Id)
        {
            try
            {
                var cluster = await _clusterRepository.GetByIdAsync(Id);
                await _clusterRepository.DeleteAsync(cluster);
                return Ok();
            }
            catch (Exception ex)
            {
                var errorMessage = $"Cannot delete cluster with id {Id}";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
        }

        private async Task SaveCassettes(long clusterId, List<PlanmPartInfo> cassettes, ScratchPad? scratchPad = null)
        {
            //////////////////////////////////////////////////////////////////////////////////////////////
            //HANDLE CASSETTES
            //now handle the cassettes available products
            try
            {
                foreach (var cassette in cassettes)
                {
                    switch (cassette.PartTypeId)
                    {
                        case (int)PartTypeEnum.Cassette:
                            await ClusterCassetteUpdate(cassette, clusterId);
                            break;
                        case (int)PartTypeEnum.Glorifier:
                            await ClusterCassetteUpdate(cassette, clusterId);
                            break;
                        case (int)PartTypeEnum.RedFrame:
                            await ClusterCassetteUpdate(cassette, clusterId);
                            break;
                        case (int)PartTypeEnum.Blanking:
                            await ClusterCassetteUpdate(cassette, clusterId);
                            break;
                        case (int)PartTypeEnum.Accessory:
                            await ClusterCassetteUpdate(cassette, clusterId);
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                //we caught an exception
                throw;
            }

        }


        private async Task ClusterCassetteUpdate(PlanmPartInfo clusterPart, long clusterId) //, clusterId, planogramToSave, newShelf)
        {
            try
            {


                Part? part = null;

                long clusterPartId = clusterPart.PlanogramPartId ?? 0;
                long partId = clusterPart.PartId ?? 0;
                var cluster = _clusterService.GetCluster(clusterId);

                // part status
                int clusterPartStatusId = clusterPart.StatusId ?? 0;


                if (partId != 0)
                {
                    part = await _partService.GetPart(partId);
                }
                else
                {
                    part = await _partService.GetPart(clusterPart.PartNumber);
                }

                if (part != null)
                {

                    ClusterPart newPart = new ClusterPart();
                    if (clusterPartId != 0)
                    {
                        newPart = await _clusterService.GetClusterPart(clusterPartId);
                        //_planogramService.SavePlanogramPart();
                    }

                    if (newPart == null)
                    {
                        newPart = new ClusterPart();
                        clusterPartId = 0;
                    }
                    else
                    {
                        newPart.ScratchPadId = null; //need this to fix issue with restoring from scratchpad
                    }
                    newPart.ClusterId = clusterId;
                    newPart.ClusterShelfId = clusterPart.PlanogramShelfId == 0 ? null : clusterPart.PlanogramShelfId;
                    //this bit basically sets whether the part is in the scratch pad or not.
                    newPart.ScratchPadId = clusterPart.ScratchPadId;
                    newPart.PositionX = clusterPart.Position.x;
                    newPart.PositionY = clusterPart.Position.y;
                    newPart.Notes = clusterPart.Notes;

                    newPart.Part = part;

                    newPart.PartStatusId = clusterPartStatusId;

                    if (clusterPartId != 0)
                    {
                        await _clusterService.SaveClusterPart(newPart);
                    }
                    else
                    {
                        newPart.DateCreated = DateTime.Now;
                        newPart.DateUpdated = DateTime.Now;
                        //_clusterService.CreateClusterPart(newPart);
                    } //ERROR Part_CatPartId not exist


                }//end if part is null
            }
            catch (Exception ex)
            {
                string message = String.Format("An error occurred updating the cassette id {0} with partId {1} on cluster {2}", clusterPart.PartId, clusterPart.PartId, clusterId);
                Exception newException = new Exception(message, ex);
                string exceptionString = newException.ToString(); // full stack trace
                                                                  //TODO: write exceptionString to log.
                throw newException;
            }

        }






        #region PlanogramFunctions



        /// <summary>
        /// Checks a planogram isn't locked
        /// </summary>
        /// <param name="planogramId">The id of the planogram to check.</param>
        /// <returns>true or false.</returns>
        private async Task<bool> IsLocked(int planogramId, CurrentUser user)
        {
            var filter = new PlanogramLockFilter
            {
                PlanogramId = planogramId,
                User = user
            };
            return await _planogramService.IsLocked(filter);
        }




        private async Task<ClusterShelf> SaveShelf(PlanmShelfInfo shelf, ScratchPad? scratchPad = null)
        {

            try
            {
                var newShelf = new ClusterShelf();
                long? shelfId = shelf.Id; //the planogramShelfId
                //var cluster = await _clusterService.GetCluster(shelf.PlanogramId);

                if (shelfId.Value != 0)
                {
                    newShelf = await _clusterService.GetClusterShelf((int)shelfId);
                }
                //else
                //{
                //    {
                //        if (ShelfHasDuplicate(shelf, cluster))
                //        {
                //            throw new DuplicatePartException();
                //        }
                //    }
                //}


                if (newShelf == null)
                {
                    newShelf = new ClusterShelf();
                    shelfId = 0;
                }

                newShelf.ClusterId = (long)shelf.PlanogramId;
                newShelf.ShelfTypeId = shelf.ShelfTypeId;
                newShelf.Height = (short)shelf.Height;
                newShelf.Width = (short)shelf.Width;
                newShelf.PositionX = shelf.Position.x;
                newShelf.PositionY = shelf.Position.y;

                newShelf.PartId = (long)shelf.PartId;
                newShelf.PartStatusId = shelf.StatusId ?? 0;
                newShelf.Label = shelf.Label;

                if (shelfId != null)
                {
                    if (shelfId != 0)
                    {
                        await _clusterService.UpdateClusterShelf(newShelf);
                    }
                    else //shelfId == 0
                    {
                        await _clusterService.CreateClusterShelf(newShelf);
                    }

                }
                else //no shelfId attribute
                {
                    await _clusterService.CreateClusterShelf(newShelf);
                }

                return newShelf;
            }
            catch (DuplicatePartException ex)
            {
                string message = string.Format("Duplicate found with shelf id {0} with partId {1}, and with label {2}", shelf.PlanxShelfId, shelf.PartId, shelf.Label);
                DuplicatePartException newException = new DuplicatePartException(message, ex);
                _logger.LogError(message);

                return null;

            }
            catch (Exception ex)
            {
                string message = string.Format("An error occurred updating the shelf id {0} with partId {1}", shelf.PlanxShelfId, shelf.PartId);
                Exception newException = new Exception(message, ex);
                string exceptionString = newException.ToString(); // full stack trace
                _logger.LogError(message);
                throw newException;
            }
        }
        private async Task DeleteParts(List<PlanmPartInfo> parts)
        {
            foreach (var delItem in parts)
            {
                if (delItem != null)
                {
                    var ppart = await _clusterService.GetClusterPart((int)delItem.ClusterPartId);
                    if (ppart != null) //part hasn't already been deleted
                    {
                        await _clusterService.DeleteClusterPart((int)delItem.ClusterPartId);
                    }
                }
            }
        }

        private async Task DeleteShelves(List<PlanmShelfInfo> shelves)
        {
            foreach (var delItem in shelves)
            {
                if (delItem.Id != 0)
                {
                    await _clusterService.DeleteClusterShelf(delItem.Id);
                }
            }
        }

        //private async Task UpdateScratchPad(int planogramId, PlanmShelfInfoList scratchPad)
        //{
        //    //Handle the planogram now
        //    var shelves = scratchPad.shelfInfos;
        //    var parts = scratchPad.partInfos;
        //    int? sPadId = 0;
        //    if (parts.Any() || shelves.Any())
        //    {
        //        if (parts.Any())
        //        {
        //            sPadId = parts.FirstOrDefault().ScratchPadId;
        //        }
        //        else
        //        {
        //            if (shelves.Any())
        //            {
        //                sPadId = shelves.FirstOrDefault().ScratchPadId;
        //            }
        //        }

        //        int scratchPadId = sPadId == null ? 0 : (int)sPadId;
        //        var spad = await _planogramService.GetScratchPad(scratchPadId);

        //        if (shelves != null)
        //        {
        //            foreach (var shelf in shelves)
        //            {
        //                var planogramShelf = await SaveShelf(shelf, spad);
        //                if (planogramShelf != null)
        //                {
        //                    if (shelf.Parts != null)
        //                    {
        //                        foreach (var part in shelf.Parts)
        //                        {
        //                            if (part.PlanogramShelfId == 0)
        //                            {
        //                                part.PlanogramShelfId = planogramShelf.Id;
        //                            }
        //                        }

        //                        //await SaveCassettes(planogramShelf.PlanogramId, shelf.Parts.ToList());
        //                    }
        //                }
        //            }
        //        }

        //        //save parts
        //        if (parts != null)
        //        {
        //           await SaveCassettes(planogramId, parts.ToList());
        //        }
        //    }
        //}

        private bool ShelfHasDuplicate(PlanmShelfInfo shelf, Planogram planogram)
        {
            PlanogramShelf? foundShelf = new PlanogramShelf();
            //check if this has been duplicated
            foundShelf = planogram.PlanogramShelves.FirstOrDefault(sf =>
                sf.PositionX == shelf.Position.x && sf.PositionY == shelf.Position.y && sf.Part.Id == shelf.PartId);
            if (foundShelf != null)
            {
                _logger.LogError("Duplicate Found - planogramId = " + planogram.Id + " --- partId = " + shelf.PartId);

            }
            return foundShelf != null;
        }

        //private bool CassetteHasDuplicate(PlanmPartInfo part, Planogram planogram)
        //{
        //    PlanogramPart? foundPart = new PlanogramPart();
        //    //check if this has been duplicated
        //    foundPart = planogram.PlanogramParts.FirstOrDefault(pp =>
        //        pp.PositionX == part.Position.x && pp.PositionY == part.Position.y && pp.Part.Id == part.PartId);
        //    if (foundPart != null)
        //    {
        //        _logger.LogError("Duplicate Found - planogramId = " + planogram.Id + " --- partId = " + part.PartId);

        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        #endregion PlanogramFunctions


    }


}
