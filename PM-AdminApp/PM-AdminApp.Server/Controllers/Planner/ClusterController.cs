using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities;
using PMApplication.Interfaces;
using PMApplication.Interfaces.ServiceInterfaces;
using PMApplication.Dtos;
using PMApplication.Dtos.PlanModels;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Enums;
using PMApplication.Services;
using PMApplication.Specifications.Filters;
using System.Web;
using dplo_system.Exceptions;
//using IronPdf.Engines.Chrome;
//using IronPdf.Rendering;
using Microsoft.AspNetCore.Authorization;
using PM_AdminApp.Server.Extensions;
using PMApplication.Entities.CountriesAggregate;

namespace PM_AdminApp.Server.Controllers.Planner
{
    [Authorize]
    [ApiController]
    [Route("api/planner/cluster/[action]")]
    public class ClusterController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ClusterController> _logger;
        private readonly IBrandService _brandService;
        private readonly IPartService _partService;
        private readonly IProductService _productService;
        private readonly IPlanogramService _planogramService;
        private readonly IClusterService _clusterService;
        private readonly ICountryService _countryService;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly ICategoryService _categoryService;
        //private readonly IAIdentityService _identityService;
        //private readonly IProductService _productService;
        private readonly IStandService _standService;

        public ClusterController(IPartService partService,
                //ICategoryService categoryService,
                IStandService standService,
                IBrandService brandService,
                //ICountryService countryService,
                //IProductService productService,
                IPlanogramService planogramService,
                //IAIdentityService identityService, 
                IMapper mapper, ILogger<ClusterController> logger, ICountryService countryService, IAuditService auditService, IConfiguration config, IProductService productService, IWebHostEnvironment env, ICategoryService categoryService, IClusterService clusterService)
            //IPlanogramVersionService versionService)
        {
            _partService = partService;
            _standService = standService;
            //this._categoryService = categoryService;
            //this._productService = productService;
            //this._countryService = countryService;
            _brandService = brandService;
            _planogramService = planogramService;
            //this._identityService = identityService;
            _mapper = mapper;
            _logger = logger;
            _countryService = countryService;
            _auditService = auditService;
            _config = config;
            _productService = productService;
            _env = env;
            _categoryService = categoryService;
            _clusterService = clusterService;
            //this._versionService = versionService;
        }


        //[Route("/get-menu-categories/{planogramId}")]
        [HttpPost]
        public async Task<IActionResult> GetMenuCategories(GetMenuParams menuParams)
        {
            var menu = new PlanmMenuDto();
            try
            {

                var clusterFilter = new ClusterFilter()
                {
                    Id = (long)menuParams.ClusterId
                };
                var cluster = await _clusterService.GetCluster(clusterFilter);
                var standTypeId = cluster.Stand.StandTypeId; //need to get from planogram when denormalised
                var brandId = cluster.BrandId;
                //var countryId = cluster.CountryId ?? 0;

                var partFilter = new PartFilter
                {
                    BrandId = brandId,
                    ClusterId = (long)menuParams.ClusterId,
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
                        if (!menuCats.Any(c => c.ParentCategoryId == cat.CategoryId))
                        {
                            var pcat = await _categoryService.GetCategory(cat.CategoryId);
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
        [HttpPost]
        public async Task<IActionResult> GetMenu(GetMenuParams menuParams)
        {
            var menu = new PlanmMenuDto();
            var filter = new ClusterFilter
            {
                Id = menuParams.ClusterId ?? 0,
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
                _logger.LogError("Error getting menu for cluster - " + menuParams.ClusterId + " error message:  " +
                                 ex.Message);

                return StatusCode(500, "Internal server error getting menu");
            }
            finally
            {

            }

        }


        //[Route("api/v2/planx/get-planogram/{PlanogramId}")]
        [HttpPost]
        public async Task<IActionResult> GetCluster(GetMenuParams menuParams)
        {
            try
            {
                var cluster = await _clusterService.GetCluster((long)menuParams.ClusterId);


                //var planogramView = (PlanogramDTO)planogram;
                var clusterView = _mapper.Map<PlanmClusterDto>(cluster);
                return Ok(clusterView);
            }
            catch (Exception ex)
            {

                //log an error
                _logger.LogError("Error getting cluster for clusterId " + menuParams.ClusterId + "---- error message - " + ex.Message + " --- " + ex.StackTrace);

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
                    includeColumnUprights = true
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
        [HttpPost]
        public async Task<IActionResult> GetShelves(GetMenuParams menuParams)
        {
            try
            {
                //var plano = await _planogramService.GetPlanogram(planogramId);
                var filter = new ClusterFilter()
                {
                    Id = (long)menuParams.ClusterId
                };
                var shelves = await _clusterService.GetClusterShelves(filter);
                var planoShelves = _mapper.Map<List<PlanmPartInfo>>(shelves);
                return Ok(planoShelves);
            }
            catch (Exception ex)
            {
                //log an error

                _logger.LogError("Error getting cluster shelves for clusterId " + menuParams.ClusterId + "---- error message - " + ex.Message + " --- " + ex.StackTrace);
                return StatusCode(500, "Internal server error getting cluster shelves");
            }
            finally
            {

            }
        }

        //[Route("api/v2/planx/get-planogram-parts/{planogramId}")]
        [HttpPost]
        public async Task<IActionResult> GetParts(GetMenuParams menuParams)
        {
            try
            {
                //var plano = await _planogramService.GetPlanogram(planogramId);
                var filter = new ClusterFilter()
                {
                    Id = (long)menuParams.ClusterId
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

                _logger.LogError("Error getting cluster parts for clusterId " + menuParams.ClusterId + "---- error message - " + ex.Message + " --- " + ex.StackTrace);
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
                    await DeletePlanogramParts(clusterData.DeletedInfo.partInfos.ToList());
                }

                if (clusterData.DeletedInfo.shelfInfos != null)
                {
                    await DeletePlanogramShelves(clusterData.DeletedInfo.shelfInfos.ToList());
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

                            //await SaveCassettes(planogramShelf.PlanogramId, shelf.Parts.ToList());
                        }
                    }
                }

                //Now handle any parts not associated with shelves
                //await SaveCassettes(clusterData.ClusterId, clusterData.CassetteInfo.ToList());

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
                PlanogramPreview preview = await _planogramService.GetPlanogramPreview((int)planoJpeg.PlanogramId);

                if (preview != null)
                {
                    preview.PreviewSrc = planoJpeg.Image;
                    _planogramService.SavePlanogramPreview(preview);
                }
                else
                {
                    preview = new PlanogramPreview();
                    preview.PlanogramId = planoJpeg.PlanogramId;
                    preview.PreviewSrc = planoJpeg.Image;
                    _planogramService.CreatePlanogramPreview(preview);
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


        //[Route("api/v2/planx/get-planogram-pdf")]
        //[HttpPost]
        //public async Task<IActionResult> PlanogramToPdf(PlanmImageDto planoSvg)
        //{
        //    //var content = Request.Content.ReadAsStringAsync();

        //    if (planoSvg != null)
        //    {



        //        Planogram planogram = await _planogramService.GetPlanogram((int)planoSvg.PlanogramId);
        //        // we can retrieve the userId from the request
        //        var skuCount = 0;
        //        var shelfCount = 0;
        //        foreach (var part in planogram.PlanogramParts)
        //        {
        //            if (part.Part.PartTypeId == 4 || part.Part.PartTypeId == 10)
        //            {
        //                shelfCount += 1;
        //            }
        //            else
        //            {
        //                skuCount += part.Part.Facings * part.Part.Stock;
        //            }
        //        }

        //        foreach (var shelf in planogram.PlanogramShelves)
        //        {
        //            shelfCount += 1;
        //        }

        //        var pageHtmlTop =
        //            "<html><head><link rel=\"stylesheet\" href=\"https://use.typekit.net/oov2wcw.css\"><style>html { font-family: century-gothic, sans-serif; font-weight: 400; font-style: normal; } </style></head><body >";

        //        var pageHtmlBottom = "</body></html>";


        //        var headerHtml = "<div class=\"header-section\" style=\"width:100%;height:80px;font-size: 16px; \">" +
        //        "<div class=\"row title-row\" style=\"width:100%\">" +
        //          //"<div class=\"user-name\" style=\"width:40%;float:left;\">" + planoSvg.UserName + "</div>" +
        //          "<div class=\"plano-name\" style=\"text-align:center;\"><div>" + planogram.Name + "</div><div>" + planogram.Stand.StandType.Name + " | SKU " + skuCount + " | SHELVES " + shelfCount + "</div> </div>" +
        //        "</div>" +
        //        "<div class=\"row name-row\" style=\"width:100%;display:flex;grid-auto-column:50%;\">" +
        //          "<div class=\"view-name\" style=\"width:50%;text-align:left;\"><strong>Planogram</strong> View</div>" +
        //          "<div class=\"diam-logo\" style=\"width:50%; text-align:right;\"><img src = \"" + _config["AppSettings:BaseImageDomain"] + "/Content/images/DIAM_pdf_logo.png\" style=\"height:40px;\" /></div>" +
        //        "</div>" +
        //      "</div>";


        //        try
        //        {

        //            // Render any HTML fragment or document to HTML
        //            //var Renderer = new IronPdf.HtmlToPdf();
        //            ChromePdfRenderer Renderer = new ChromePdfRenderer();
        //            Renderer.RenderingOptions.Timeout = 200;
        //            Renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter() { MaxHeight = 45, Spacing = 25, HtmlFragment = headerHtml, FontSize = 16, LoadStylesAndCSSFromMainHtmlDocument = true };
        //            //Renderer.RenderingOptions.PaperSize = PdfPrintOptions.PdfPaperSize.A4;
        //            Renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

        //            Renderer.RenderingOptions.MarginTop = 15;
        //            Renderer.RenderingOptions.MarginBottom = 5;
        //            Renderer.RenderingOptions.MarginLeft = 5;
        //            Renderer.RenderingOptions.MarginRight = 5;
        //            //Renderer.RenderingOptions.FirstPageNumber = 1;
        //            var imageHtml = "<div style=\"width:90%; margin:auto\"><image src=\"" +
        //                            HttpUtility.UrlDecode(planoSvg.Image) + "\" style=\"max-height:100%;max-width: 100%;\"></div>";

        //            Renderer.RenderingOptions.FitToPaperMode = FitToPaperModes.FixedPixelWidth;
        //            //Renderer.RenderingOptions.PaperFit.UseFitToPageRendering();
        //            //Renderer.RenderingOptions.PaperFit.UseChromeDefaultRendering();

        //            if (planogram.Stand.Width > planogram.Stand.Height)
        //            {
        //                //Renderer.RenderingOptions.FitToPaperMode = FitToPaperModes.FixedPixelWidth;
        //                Renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
        //                //Renderer.RenderingOptions.PaperFit.UseFitToPageRendering(550);
        //                imageHtml = "<div style=\"width:95%;margin:auto\"><image src=\"" +
        //                            HttpUtility.UrlDecode(planoSvg.Image) + "\" style=\"max-height:100%;max-width: 100%;\"></div>";

        //            }




        //            var htmlPage = pageHtmlTop + imageHtml + pageHtmlBottom;
        //            var PDF = Renderer.RenderHtmlAsPdf(htmlPage);
        //            if (PDF.PageCount > 2)
        //            {
        //                PDF.Pages.Remove(PDF.Pages.First());
        //            }
        //            string PdfFileLocation = "~/planogram/pdf/";

        //            var OutputPath = Path.Combine(_env.WebRootPath + "\n" + _env.ContentRootPath, planogram.Name + ".pdf");


        //            var stream = PDF.Stream.ToArray();

        //            var response = File(stream, "application/pdf");


        //            return response;
        //            // This neat trick opens our PDF file so we can see the result in our default PDF viewer
        //            //System.Diagnostics.Process.Start(OutputPath);
        //        }
        //        catch (Exception Ex)
        //        {

        //            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.BadRequest);

        //            _logger.LogError("Error generating PDF");
        //            if (Ex.InnerException != null)
        //            {
        //                message.Content = new StringContent(Ex.Message +
        //                                              Ex.InnerException.ToString());

        //                _logger.LogError(Ex.Message + Ex.InnerException.ToString());
        //            }
        //            else
        //            {
        //                message.Content = new StringContent(Ex.Message
        //                              + Ex.StackTrace);

        //                _logger.LogError(Ex.Message + Ex.StackTrace);
        //            }
        //            //message.ReasonPhrase = "Error creating pdf";
        //            //log an error

        //            return StatusCode(500, "Error generating pdf");

        //        }
        //    }
        //    else
        //    {
        //        return StatusCode(500, "Error generating pdf");
        //    }
        //}

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

        //private async Task SaveCassettes(long planogramId, List<PlanmPartInfo> cassettes, ScratchPad scratchPad = null)
        //{
        //    //////////////////////////////////////////////////////////////////////////////////////////////
        //    //HANDLE CASSETTES
        //    //now handle the cassettes available products

        //    //bool debugSave = ConfigurationManager.AppSettings["debugSave"] == "True" ? true : false;
        //    bool debugSave = _config["AppSettings:DebugSave"] == "True" ? true : false;
        //    bool throwError = false;
        //    var cassetteCounter = 0;
        //    foreach (var cassette in cassettes)
        //    {

        //        if (cassettes.Count > 3)
        //        {

        //            if (cassette.PlanogramPartId != 0 && debugSave && cassetteCounter > 3)
        //                throwError = true;
        //        }

        //        cassetteCounter++;
        //        switch (cassette.PartTypeId)
        //        {
        //            case (int)PartTypeEnum.Cassette:
        //                await PlanogramCassetteUpdate(cassette, planogramId, throwError);
        //                break;
        //            case (int)PartTypeEnum.Glorifier:
        //                await PlanogramCassetteUpdate(cassette, planogramId, throwError);
        //                break;
        //            case (int)PartTypeEnum.RedFrame:
        //                await PlanogramCassetteUpdate(cassette, planogramId, throwError);
        //                break;
        //            case (int)PartTypeEnum.Blanking:
        //                await PlanogramCassetteUpdate(cassette, planogramId, throwError);
        //                break;
        //            case (int)PartTypeEnum.FasciaPlate:
        //                await PlanogramCassetteUpdate(cassette, planogramId, throwError);
        //                break;
        //            case (int)PartTypeEnum.Accessory:
        //                await PlanogramCassetteUpdate(cassette, planogramId, throwError);
        //                break;
        //        }

        //    }
        //}

        //private async Task PlanogramCassetteUpdate(PlanmPartInfo planoPart, long planogramId, bool throwError) //, planogramId, planogramToSave, newShelf)
        //{

        //    try
        //    {
        //        //Part part;

        //        int planogramPartId = (int)planoPart.PlanogramPartId;
        //        int partId = (int)planoPart.PartId;
        //        var planogram = await _planogramService.GetPlanogram(planogramId);
        //        var part = await _partService.GetPart(partId);

        //        if (planogram.ScratchPadId == null)
        //        {
        //            //we need to create a new scratchpad
        //            ScratchPad sPad = new ScratchPad();
        //            sPad.DateCreated = DateTime.Now;
        //            sPad.DateUpdated = DateTime.Now;
        //            await _planogramService.CreateScratchPad(sPad);
        //            planogram.ScratchPad = sPad;
        //            await _planogramService.SavePlanogram(planogram);
        //        }

        //        //var scratchPadId = planogram.ScratchPadId;
        //        // part status
        //        int planogramPartStatusId = planoPart.StatusId ?? 0;


        //        if (partId != 0)
        //        {
        //            var anotherpart = await _partService.GetPart(partId);
        //        }
        //        else
        //        {
        //            if (!CassetteHasDuplicate(planoPart, planogram))
        //            {
        //                part = await _partService.GetPart(planoPart.PartNumber);
        //            }
        //            else
        //            {
        //                throw new DuplicatePartException();
        //            }
        //        }

        //        if (part.Id != 0)
        //        {

        //            PlanogramPart newPart;
        //            if (planogramPartId != 0)
        //            {
        //                newPart = await _planogramService.GetPlanogramPart(planogramPartId);
        //                //newPart = planogramPart;
        //                //newPart.ScratchPadId = planogram.ScratchPadId;
        //                //await _planogramService.SavePlanogramPart(newPart);
        //            }
        //            else
        //            {
        //                newPart = new PlanogramPart();
        //            }

        //            newPart.ScratchPadId = null; //need this to fix issue with restoring from scratchpad

        //            newPart.PlanogramId = planogramId;
        //            newPart.PlanogramShelfId = planoPart.PlanogramShelfId == 0 ? null : planoPart.PlanogramShelfId;
        //            //this bit basically sets whether the part is in the scratch pad or not.
        //            newPart.ScratchPadId = planoPart.ScratchPadId == 0 ? null : planoPart.ScratchPadId;
        //            newPart.PositionX = planoPart.Position.x;
        //            newPart.PositionY = planoPart.Position.y;
        //            newPart.Notes = planoPart.Notes;
        //            newPart.Label = planoPart.Label;
        //            newPart.PartId = (long)planoPart.PartId;
        //            //newPart.Part = part;

        //            newPart.PartStatusId = planogramPartStatusId;



        //            if (planogramPartId != 0)
        //            {
        //                if (throwError)
        //                    throw new Exception("Debug Save Error Thrown");

        //                newPart.DateUpdated = DateTime.Now;
        //                await _planogramService.SavePlanogramPart(newPart);
        //            }
        //            else
        //            {
        //                if (throwError)
        //                    throw new Exception("Debug Save Error Thrown");

        //                newPart.DateCreated = DateTime.Now;
        //                await _planogramService.CreatePlanogramPart(newPart);
        //            } //ERROR Part_CatPartId not exist



        //            //now handle the products (selected products and facings and stock)

        //            ////////////////////////////////////////////////////////////////////////////////
        //            //  CASSETTE ITEM LOOP
        //            ///////////////////////////////////////////////////////////////////////////////

        //            var selectedFacings = planoPart.facingProducts;
        //            if (selectedFacings != null)
        //            {
        //                //Assume that the nodes are in the correct order for the facing position, counting 1 to n from the left.
        //                int FacingPosition = 1;
        //                foreach (var facingItem in selectedFacings)
        //                {
        //                    if (newPart.PlanogramPartFacings != null && newPart.PlanogramPartFacings.Count > 0)
        //                    {
        //                        var currentFacing = newPart.PlanogramPartFacings.FirstOrDefault(pf => pf.Position == FacingPosition);
        //                        if (currentFacing != null)
        //                        {
        //                            if (facingItem.ProductId != 0)
        //                            {
        //                                FacingPosition = await InsertFacing(facingItem, planogramId, part.Stock, newPart,
        //                                    FacingPosition);
        //                            }
        //                            else
        //                            {
        //                                //we need to remove this productFacing info
        //                                //we need to delete the item in this position
        //                                await _planogramService.DeletePlanogramPartFacing(currentFacing.Id);
        //                                FacingPosition++;
        //                            }

        //                        }
        //                        else
        //                        {
        //                            if (facingItem.ProductId != 0)
        //                            {
        //                                FacingPosition = await InsertFacing(facingItem, planogramId, part.Stock, newPart,
        //                                    FacingPosition);
        //                            }
        //                            else
        //                            {
        //                                FacingPosition++;
        //                            }
        //                            //must be a new facing
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (facingItem.ProductId != 0)
        //                        {
        //                            FacingPosition = await InsertFacing(facingItem, planogramId, part.Stock, newPart,
        //                                FacingPosition);
        //                        }
        //                        else
        //                        {
        //                            FacingPosition++;
        //                        }
        //                    }
        //                } //end of cassetteProduct loop               

        //            } //end of if selectedProducts is null


        //        } //end if part is null

        //    }
        //    catch (DuplicatePartException ex)
        //    {
        //        string message = string.Format("Duplicate found with cassette id {0} with partId {1}, partName {2}", planoPart.PartId, planoPart.PartId, planoPart.Name);
        //        Exception newException = new Exception(message, ex);
        //        string exceptionString = newException.ToString(); // full stack trace
        //        _logger.LogError(message);

        //        return;

        //    }
        //    catch (Exception ex)
        //    {
        //        string message = string.Format("An error occurred updating the cassette id {0} with partId {1}", planoPart.PartId, planoPart.PartId);
        //        Exception newException = new Exception(message, ex);
        //        string exceptionString = newException.ToString(); // full stack trace
        //        _logger.LogError(message);
        //        throw newException;
        //    }

        //}

    //    private async Task<int> InsertFacing(PlanmPartFacing cassetteProductFacing, long planogramId, int stockCount,
    //PlanogramPart newPart, int facingPosition)
    //    {

    //        //if (cassetteProductFacing.ProductId == 0)
    //        //{
    //        //    facingPosition++;
    //        //    return facingPosition;
    //        //}
    //        var partFacingId = cassetteProductFacing.Id;
    //        //////////////////////////////////////////////////////////////////////////////
    //        // facing status ////
    //        //////////////////////////////////////////////////////////////////////////////
    //        int planogramFacingStatusId = cassetteProductFacing.FacingStatus ?? 0;

    //        //////////////////////////////////////////////////////////////////////////////
    //        // END of facing status ////
    //        /////////////////////////////////////////////////////////////////////////////

    //        //we make the assumption that there is a one to one relationship between facing - product - shade
    //        PlanogramPartFacing currentFacing = new PlanogramPartFacing();
    //        if (partFacingId == 0)
    //        {
    //            currentFacing.PlanogramId = planogramId;
    //            currentFacing.PlanogramPartId = newPart.Id;
    //            //currentFacing.PlanogramPart = newPart;
    //            //currentFacing.Id = 0;
    //        }
    //        else
    //        {
    //            currentFacing = await _planogramService.GetPlanogramPartFacing(partFacingId);
    //        }

    //        if (currentFacing != null)
    //        {
    //            var currentProduct = await _productService.GetProduct(cassetteProductFacing.ProductId);
    //            currentFacing.Position = facingPosition;
    //            currentFacing.ProductId = cassetteProductFacing.ProductId;
    //            currentFacing.ProductName = currentProduct.Name;
    //            currentFacing.FacingStatusId = planogramFacingStatusId;
    //            if (cassetteProductFacing.ShadeId != null)
    //            {
    //                var shade = await _productService.GetShade(cassetteProductFacing.ShadeId ?? 0);
    //                currentFacing.ShadeId = shade.Id;
    //                currentFacing.ShadeName = shade.ShadeNumber;
    //            }


    //            currentFacing.StockCount = stockCount;

    //            //var filter = new PlanogramPartFilter();
    //            //filter.PartId = newPart.Id;

    //            //var partFactices = await _planogramService.GetPlanogramParts(filter);

    //        }

    //        //REMOVE ANY EXISTING ITEM IN THE POSITION - FACINGS
    //        if (newPart.PlanogramPartFacings != null)
    //        {
    //            if (newPart.PlanogramPartFacings.Any(pf => pf.Position == facingPosition))
    //            {
    //                //we need to delete the item in this position
    //                PlanogramPartFacing pfToDelete = newPart.PlanogramPartFacings.FirstOrDefault(pf => pf.Position == facingPosition);
    //                await _planogramService.DeletePlanogramPartFacing(pfToDelete.Id);
    //            }
    //            await _planogramService.CreatePlanogramPartFacing(currentFacing);
    //        }
    //        else
    //        {
    //            if (partFacingId == 0)
    //            {
    //                await _planogramService.CreatePlanogramPartFacing(currentFacing);
    //            }
    //            else
    //            {
    //                await _planogramService.SavePlanogramPartFacing(currentFacing);
    //            }
    //        }

    //        facingPosition++;
    //        return facingPosition;
    //    }

        private async Task<PlanogramShelf> SaveShelf(PlanmShelfInfo shelf, ScratchPad scratchPad = null)
        {

            try
            {
                var newShelf = new PlanogramShelf();
                long? shelfId = shelf.Id; //the planogramShelfId
                var planogram = await _planogramService.GetPlanogram(shelf.PlanogramId);

                if (shelfId.Value != 0)
                {
                    newShelf = await _planogramService.GetPlanogramShelf((int)shelfId);
                }
                else
                {
                    {
                        if (ShelfHasDuplicate(shelf, planogram))
                        {
                            throw new DuplicatePartException();
                        }
                    }
                }


                if (newShelf == null)
                {
                    newShelf = new PlanogramShelf();
                    shelfId = 0;
                }



                newShelf.PlanogramId = shelf.PlanogramId;
                if (scratchPad != null)
                {
                    //then we are dealing with the scratchpad
                    newShelf.ScratchPadId = scratchPad.Id;
                }
                else
                {
                    newShelf.ScratchPadId = null;
                }

                newShelf.ShelfTypeId = shelf.ShelfTypeId;
                newShelf.Height = (short)shelf.Height;
                newShelf.Width = (short)shelf.Width;
                newShelf.PositionX = shelf.Position.x;
                newShelf.PositionY = shelf.Position.y;

                newShelf.PartId = shelf.PartId;
                newShelf.PartStatusId = shelf.StatusId ?? 0;
                var label = shelf.Label;
                if (label != null)
                    newShelf.Label = label;

                if (shelfId != null)
                {
                    if (shelfId != 0)
                    {
                        await _planogramService.UpdatePlanogramShelf(newShelf);
                    }
                    else //shelfId == 0
                    {
                        await _planogramService.CreatePlanogramShelf(newShelf);
                    }

                }
                else //no shelfId attribute
                {
                    await _planogramService.CreatePlanogramShelf(newShelf);
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
        private async Task DeletePlanogramParts(List<PlanmPartInfo> parts)
        {
            foreach (var delItem in parts)
            {
                if (delItem != null)
                {
                    var ppart = await _planogramService.GetPlanogramPart((int)delItem.PlanogramPartId);
                    if (ppart != null) //part hasn't already been deleted
                    {
                        List<PlanogramPartFacing> idsToDelete = new List<PlanogramPartFacing>();
                        foreach (PlanogramPartFacing facing in ppart.PlanogramPartFacings)
                        {
                            idsToDelete.Add(facing);
                        }
                        foreach (var facing in idsToDelete)
                        {
                            await _planogramService.DeletePlanogramPartFacing(facing.Id);
                        }
                        await _planogramService.DeletePlanogramPart((int)delItem.PlanogramPartId);
                    }
                }
            }
        }

        private async Task DeletePlanogramShelves(List<PlanmShelfInfo> shelves)
        {
            foreach (var delItem in shelves)
            {
                if (delItem.Id != 0)
                {
                    await _planogramService.DeletePlanogramShelf(delItem.Id);
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
