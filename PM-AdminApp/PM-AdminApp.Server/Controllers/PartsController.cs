using ApplicationCore.Entities;
using AutoMapper;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using PMApplication.Dtos;
using PMApplication.Dtos.StandTypes;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.ProductAggregate;
using PMApplication.Entities.StandAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Services;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMXApi.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PartController : ControllerBase
    {
        private readonly ILogger<PartController> _logger;
        private readonly IMapper _mapper;
        private readonly IAsyncRepositoryLong<Part> _partAsyncRepository;
        private readonly IPartRepository _partRepository;
        private readonly IAsyncRepository<PartType> _partTypeRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;
        private readonly IAsyncRepository<Country> _countryRepository;
        private readonly IAsyncRepositoryLong<Product> _productRepository;
        private readonly IAsyncRepository<StandType> _standTypeRepository;
        private readonly IAsyncRepository<Region> _regionRepository;


        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly IAsyncRepository<Brand> _brandRepository;

        public PartController(IMapper mapper,
                              IAsyncRepository<PartType> partTypeRepository,
                              IAsyncRepositoryLong<Part> partAsyncRepository,
                              IAsyncRepository<Category> categoryRepository,
                              ILogger<PartController> logger,
                              IPartRepository partRepository,
                              IAsyncRepository<Country> countryRepository,
                              IAsyncRepositoryLong<Product> productRepository,
                              IAsyncRepository<StandType> standTypeRepository,
                              IAsyncRepository<Region> regionRepository,
                              BlobServiceClient blobServiceClient,
                              IConfiguration configuration, IAsyncRepository<Brand> brandRepository)
        {
            _logger = logger;
            _partRepository = partRepository;
            _partAsyncRepository = partAsyncRepository;
            _partTypeRepository = partTypeRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
            _countryRepository = countryRepository;
            _productRepository = productRepository;
            _standTypeRepository = standTypeRepository;
            _regionRepository = regionRepository;
            _brandRepository = brandRepository;
        }


        [HttpGet]
        public async Task<IActionResult> GetFilteredParts([FromQuery] PartFilterDto filterDto)
        {
            try
            {
                var ptSpec = new PartTypeSpecification();
                var partTypes = await _partTypeRepository.ListAsync(ptSpec);

                var categoryFilter = new CategoryFilter();
                //categoryFilter.ParentCategory = 0;
                var catSpec = new CategorySpecification(categoryFilter);
                var categories = await _categoryRepository.ListAsync(catSpec);

                var spec = new PartSpecification(_mapper.Map<PartFilter>(filterDto));
                var parts = await _partAsyncRepository.ListAsync(spec);
                //var countFilter = filterDto;
                //countFilter.IsPagingEnabled = false;
                //var countSpec = new PartSpecification(_mapper.Map<PartFilter>(countFilter));
                //int totalItems = await _partRepository.CountAsync(countSpec);

                _logger.LogInformation($"Returned all parts from database.");
                //var ownersResult = _mapper.Map<IEnumerable<OwnerDto>>(owners);
                //return Ok(ownersResult);

                var response = new PagedPartsListDto();
                response.Data = _mapper.Map<List<PartListDto>>(parts);
                ////update partTypenames here
                //response.Page = new Page();
                //response.Page.PageNumber = filterDto.Page;
                //response.Page.TotalItems = totalItems;
                //response.Page.TotalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)filterDto.PageSize);
                //response.Page.Size = filterDto.PageSize;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetAllParts action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SearchParts(PartFilterDto filterDto)
        {
            try
            {
                var parts = await _partRepository.SearchParts(filterDto);
                _logger.LogInformation($"Returned all parts from database.");

                return Ok(parts);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside SearchParts action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPart([FromQuery] int id)
        {
            try
            {
                var partFilter = new PartFilter() { Id = id };
                var spec = new GetPartSpecification(partFilter);
                var part = await _partAsyncRepository.FirstAsync(spec);

                if (part == null)
                {
                    _logger.LogWarning($"Part with id: {id}, hasn't been found in db.");
                    return NotFound();
                }
                else
                {
                    _logger.LogInformation($"Returned part with id: {id}");
                    var response = _mapper.Map<PartDto>(part);
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetPartById action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        //[HttpGet("{id}/account")]
        //public async Task<IActionResult> GetPartWithDetails(int id)
        //{
        //    try
        //    {
        //        var part = await _partRepository.

        //        if (part == null)
        //        {
        //            _logger.LogError($"Part with id: {id}, hasn't been found in db.");
        //            return NotFound();
        //        }
        //        else
        //        {
        //            _logger.LogInfo($"Returned part with details for id: {id}");
        //            //var ownerResult = _mapper.Map<OwnerDto>(owner);
        //            //return Ok(ownerResult);
        //            return Ok(part);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Something went wrong inside GetPartWithDetails action: {ex.Message}");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}


        [HttpPost]
        public async Task<IActionResult> SavePart([FromForm] PartUploadDto partUpload)
        {
            try
            {
                if (partUpload == null)
                {
                    _logger.LogError("Part object sent from client is null.");
                    return BadRequest("Part object is null");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid part object sent from client.");
                    return BadRequest("Invalid model object");
                }

                //var partData = JsonConvert.DeserializeObject<PartDto>(partUpload.part);

                var id = partUpload.Id ?? 0;
                var partFilter = new PartFilter() { Id = id };
                var spec = new GetPartSpecification(partFilter);
                var partEdit = await _partAsyncRepository.FirstAsync(spec);


                if (partEdit == null)
                {
                    _logger.LogError($"Part with id: {id}, hasn't been found in db.");
                    return NotFound();
                }

                //map updates to part
                _mapper.Map(partUpload, partEdit);

                await _partAsyncRepository.UpdateAsync(partEdit);


                //update childItems here
                await UpdatePartCountryCollection(partEdit, partUpload);
                await UpdatePartProductsCollection(partEdit, partUpload);
                await UpdateStandTypesCollection(partEdit, partUpload);
                await UpdateRegionsCollection(partEdit, partUpload);
                await UpdatePartImages(partEdit, partUpload);

                await _partAsyncRepository.UpdateAsync(partEdit);

                return Ok(partEdit);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside UpdatePart action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreatePart([FromForm] PartUploadDto partFormData)
        {
            try
            {
                if (partFormData == null)
                {
                    _logger.LogError("Part object sent from client is null.");
                    return BadRequest("Part object is null");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid part object sent from client.");
                    return BadRequest("Invalid model object");
                }
                //var partData = JsonConvert.DeserializeObject<PartDto>(partFormData.part);
                var createPart = new Part();
              
                //map updates to part
                _mapper.Map(partFormData, createPart);


                var newPart = await _partAsyncRepository.AddAsync(createPart);



                //update childItems here
                //UpdatePartCategories(newPart, partFormData);
                //UpdatePartType(newPart, partFormData);
                //UpdatePartBrand(newPart, partFormData);
                await UpdatePartCountryCollection(newPart, partFormData);
                await UpdatePartProductsCollection(newPart, partFormData);
                await UpdateStandTypesCollection(newPart, partFormData);
                await UpdateRegionsCollection(newPart, partFormData);
                await UpdatePartImages(newPart, partFormData);

                await _partAsyncRepository.UpdateAsync(newPart);

                return Ok(newPart);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside UpdatePart action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePart(int id)
        {
            try
            {

                var part = await _partAsyncRepository.GetByIdAsync(id);
                if (part == null)
                {
                    _logger.LogError($"Part with id: {id}, hasn't been found in db.");
                    return NotFound();
                }

                //if (_partRepository.Account.AccountsByPart(id).Any())
                //{
                //    _logger.LogError($"Cannot delete part with id: {id}. It has related accounts. Delete those accounts first");
                //    return BadRequest("Cannot delete part. It has related accounts. Delete those accounts first");
                //}

                await _partAsyncRepository.DeleteAsync(part);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside DeletePart action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        //private async Task UpdatePartCategories(Part origPart, PartUploadDto updatePart)
        //{
        //    if (updatePart.CategoryId.HasValue)
        //    {
        //        var category = await _categoryRepository.GetByIdAsync(updatePart.CategoryId.Value);
        //        if (category != null)
        //        {
        //            origPart.CategoryId = category.Id;
        //            origPart.CategoryName = category.Name;
        //            origPart.ParentCategoryId = category.ParentCategoryId;
        //            origPart.ParentCategoryName = category.ParentCategory?.Name;
        //        }
        //    }
        //}

        //private async Task UpdatePartType(Part origPart, PartUploadDto updatePart)
        //{
        //    if (updatePart.PartTypeId.HasValue)
        //    {
        //        var partType = await _partTypeRepository.GetByIdAsync(updatePart.PartTypeId.Value);
        //        if (partType != null)
        //        {
        //            origPart.PartTypeId = partType.Id;
        //            origPart.PartTypeName = partType.Name;
        //        }
        //    }
        //}

        //private async Task UpdatePartBrand(Part origPart, PartUploadDto updatePart)
        //{
        //    if (updatePart.BrandId.HasValue)
        //    {
        //        var brand = await _brandRepository.GetByIdAsync(updatePart.BrandId.Value);
        //        origPart.Brand = brand;
        //    }
        //}
        private async Task UpdatePartCountryCollection(Part origPart, PartUploadDto updatePart)
        {
            //add new countries
            var partCountries = JsonConvert.DeserializeObject<List<CountryDto>>(updatePart.Countries ?? "[]");
            foreach (var country in partCountries)
            {
                var origCountry = origPart.Countries.FirstOrDefault(c => c.Id == country.Id);
                if (origCountry == null)
                {
                    var dbCountry = await _countryRepository.GetByIdAsync(country.Id);
                    if (dbCountry != null)
                    {
                        origPart.Countries.Add(dbCountry);
                    }
                }
            }
            //remove deleted countries
            for (int i = origPart.Countries.Count - 1; i >= 0; i--)
            {
                var origCountry = origPart.Countries[i];
                var updatedCountry = partCountries.FirstOrDefault(c => c.Id == origCountry.Id);
                if (updatedCountry == null)
                {
                    var dbCountry = await _countryRepository.GetByIdAsync(origCountry.Id);
                    origPart.Countries.Remove(dbCountry);
                }
            }

            //update Part.CountryList string
            origPart.CountriesList = string.Join(",", origPart.Countries.Select(c => c.Id));
        }

        private async Task UpdatePartProductsCollection(Part origPart, PartUploadDto updatePart)
        {
            var partProducts = JsonConvert.DeserializeObject<List<ProductDto>>(updatePart.Products ?? "[]");
            foreach (var product in partProducts)
            {
                var origProduct = origPart.Products.FirstOrDefault(p => p.Id == product.Id);
                if (origProduct == null)
                {
                    var dbProduct = await _productRepository.GetByIdAsync(product.Id);
                    origPart.Products.Add(_mapper.Map<Product>(product));
                }
            }
            for (int i = origPart.Products.Count - 1; i >= 0; i--)
            {
                var origProduct = origPart.Products[i];
                var updatedProduct = partProducts.FirstOrDefault(p => p.Id == origProduct.Id);
                if (updatedProduct == null)
                {
                    var dbProduct = await _productRepository.GetByIdAsync(origProduct.Id);
                    origPart.Products.Remove(dbProduct);
                }
            }
        }

        private async Task UpdateStandTypesCollection(Part origPart, PartUploadDto updatePart)
        {
            var standTypes = JsonConvert.DeserializeObject<List<StandTypeDto>>(updatePart.StandTypes ?? "[]");
            foreach (var standType in standTypes)
            {
                var origStandType = origPart.StandTypes.FirstOrDefault(s => s.Id == standType.Id);
                if (origStandType == null)
                {
                    var dbStandType =  await _standTypeRepository.GetByIdAsync(standType.Id);
                    origPart.StandTypes.Add(dbStandType);
                }
            }
            for (int i = origPart.StandTypes.Count - 1; i >= 0; i--)
            {
                var origStandType = origPart.StandTypes[i];
                var updatedStandType = standTypes.FirstOrDefault(s => s.Id == origStandType.Id);
                if (updatedStandType == null)
                {
                    var dbStandType = await _standTypeRepository.GetByIdAsync(origStandType.Id);
                    origPart.StandTypes.Remove(dbStandType);
                }
            }
        }


        private async Task UpdateRegionsCollection(Part origPart, PartUploadDto updatePart)
        {
            var regionDtos = JsonConvert.DeserializeObject<List<RegionDto>>(updatePart.Regions ?? "[]");
            foreach (var region in regionDtos)
            {
                var origRegion = origPart.Regions.FirstOrDefault(r => r.Id == region.Id);
                if (origRegion == null)
                {
                    var dbRegion =  await _regionRepository.GetByIdAsync(region.Id);
                    origPart.Regions.Add(dbRegion);
                }
            }
            for (int i = origPart.Regions.Count - 1; i >= 0; i--)
            {
                var origRegion = origPart.Regions[i];
                var updatedRegion = regionDtos.FirstOrDefault(r => r.Id == origRegion.Id);
                if (updatedRegion == null)
                {
                    var dbRegion = await _regionRepository.GetByIdAsync(origRegion.Id);
                    origPart.Regions.Remove(dbRegion);
                }
            }

            //update Part.RegionList string
            origPart.RegionsList = string.Join(",", origPart.Regions.Select(r => r.Id));
        }


        private async Task UpdatePartImages(Part origPart, PartUploadDto updatePart)
        {
            //handle file upload if a new file is provided
            var blobService = new AzureBlobService(_blobServiceClient);
            var storeName = _configuration["AzureBlob:StoreName"];
            var blobServiceClient = blobService.GetBlobServiceClient(storeName);
            
            var fileName = origPart.PartNumber.Replace(" ", "") + "_" + origPart.Id.ToString();


            if (updatePart.packShotFile != null && updatePart.packShotFile.Length > 0)
            {
                var fileType = updatePart.packShotFile.FileName.Split('.')[1];
                origPart.PackShotImageSrc = fileName + "." + fileType;
                var containerName = _configuration["AzureBlob:CassettePhotoContainer"];
                var containerClient = blobService.GetBlobContainerClient(blobServiceClient, containerName);
                await blobService.UploadFormFileAsync(containerClient, updatePart.packShotFile, origPart.PackShotImageSrc);
            }

            if (updatePart.renderFile != null && updatePart.renderFile.Length > 0)
            {
                // Here you would typically save the file to a storage location and update the BrandLogo property
                // For demonstration, we'll just set a placeholder path
                var fileType = updatePart.renderFile.FileName.Split('.')[1];
                origPart.Render2dImage = fileName + "." + fileType;
                var containerName = _configuration["AzureBlob:CassetteRenderContainer"];

                var containerClient = blobService.GetBlobContainerClient(blobServiceClient, containerName);
                
                await blobService.UploadFormFileAsync(containerClient, updatePart.renderFile, origPart.Render2dImage);
            }

            if (updatePart.iconFile != null && updatePart.iconFile.Length > 0)
            {
                // Here you would typically save the file to a storage location and update the BrandLogo property
                // For demonstration, we'll just set a placeholder path
                var fileType = updatePart.iconFile.FileName.Split('.')[1];
                origPart.SvgLineGraphic = fileName + "." + fileType;
                var containerName = _configuration["AzureBlob:CassetteTemplateContainer"];

                var containerClient = blobService.GetBlobContainerClient(blobServiceClient, containerName);
                await blobService.UploadFormFileAsync(containerClient, updatePart.iconFile, origPart.SvgLineGraphic);
            }
        }
        ///Consider this perhaps -> https://medium.com/@hamidmusayev/synchronizing-entity-framework-core-child-collections-a-clean-and-reusable-approach-2ebd8e853f4d
    }
}
