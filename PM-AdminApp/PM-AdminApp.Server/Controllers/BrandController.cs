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
    [Route("api/brands/[action]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly ILogger<BrandController> _logger;

        private readonly IMapper _mapper;
        private readonly IAsyncRepository<Brand> _brandRepository;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;


        public BrandController(IMapper mapper, IAsyncRepository<Brand> brandRepository, ILogger<BrandController> logger, IConfiguration configuration, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _configuration = configuration;
            _blobServiceClient = blobServiceClient;
            _brandRepository = brandRepository;
            _mapper = mapper;
        }

        [HttpGet(Name = "Brands")]
        public async Task<IActionResult> GetBrands([FromQuery] BrandFilterDto filterDto)
        {
            try
            {
                var spec = new BrandSpecification(_mapper.Map<BrandFilter>(filterDto));
                var brands = await _brandRepository.ListAsync(spec);

                return Ok(brands);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetBrands action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost]
        [Consumes("multipart/form-data")]
        //public IActionResult UpdateBrand([FromForm] string id, [FromForm] string name, [FromForm] string shelfLock, [FromForm] string disabled, [FromForm] IFormFile file = null)
        public async Task<IActionResult> UpdateBrand([FromForm] BrandUploadDto brand)
        {
            try
            {
                // Process the uploaded files and metadata
                if (brand.Id != null)
                {
                    //we have an existing brand - get the brand
                    var existingBrand = _brandRepository.GetByIdAsync(brand.Id.Value).Result;
                    if (existingBrand != null)
                    {
                        //update the brand details
                        existingBrand.Name = brand.Name ?? existingBrand.Name;
                        existingBrand.ShelfLock = brand.ShelfLock ?? existingBrand.ShelfLock;
                        existingBrand.Disabled = brand.Disabled ?? existingBrand.Disabled;
                        //handle file upload if a new file is provided
                        if (brand.file != null && brand.file.Length > 0)
                        {
                            // Here you would typically save the file to a storage location and update the BrandLogo property
                            // For demonstration, we'll just set a placeholder path
                            var fileType = brand.file.FileName.Split('.')[1];
                            var fileName = brand.Name.Replace(" ", "");
                            existingBrand.BrandLogo = fileName + "." + fileType;
                            var blobService = new AzureBlobService(_blobServiceClient);
                            var storeName = _configuration["AzureBlob:StoreName"];
                            var brandContainerName = _configuration["AzureBlob:BrandStoreContainer"];
                            var blobServiceClient = blobService.GetBlobServiceClient(storeName);

                            var containerClient = blobService.GetBlobContainerClient(blobServiceClient, brandContainerName);
                            await blobService.UploadFormFileAsync(containerClient, brand.file, existingBrand.BrandLogo);
                        }
                        _brandRepository.UpdateAsync(existingBrand).Wait();
                    }
                }

                return Accepted();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside UpdateBrand action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }



    }
}

