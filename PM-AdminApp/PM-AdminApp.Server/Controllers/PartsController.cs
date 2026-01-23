using ApplicationCore.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.PartAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.ProductAggregate;
using PMApplication.Entities.StandAggregate;

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


        public PartController(IMapper mapper, IAsyncRepository<PartType> partTypeRepository, IAsyncRepositoryLong<Part> partAsyncRepository, IAsyncRepository<Category> categoryRepository, ILogger<PartController> logger, IPartRepository partRepository)
        {
            _logger = logger;
            _partRepository = partRepository;
            _partAsyncRepository = partAsyncRepository;
            _partTypeRepository = partTypeRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
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
        public async Task<IActionResult> CreatePart([FromBody] Part part)
        {
            try
            {
                if (part == null)
                {
                    _logger.LogError("Part object sent from client is null.");
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid part object sent from client.");
                    return BadRequest("Invalid model object");
                }

                var newPart = await _partAsyncRepository.AddAsync(part);

                return CreatedAtRoute("PartById", new { id = part.Id }, part);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside CreatePart action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SavePart([FromBody] PartDto partEdit)
        {
            try
            {
                if (partEdit == null)
                {
                    _logger.LogError("Part object sent from client is null.");
                    return BadRequest("Part object is null");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid part object sent from client.");
                    return BadRequest("Invalid model object");
                }

                var id = partEdit.Id ?? 0;
                var partFilter = new PartFilter() { Id = id };
                var spec = new GetPartSpecification(partFilter);
                var dbPart = await _partAsyncRepository.FirstAsync(spec);


                if (dbPart == null)
                {
                    _logger.LogError($"Part with id: {id}, hasn't been found in db.");
                    return NotFound();
                }

                //map updates to part
                _mapper.Map(partEdit, dbPart);



                //update childItems here
                UpdatePartCountryCollection(dbPart, partEdit);
                UpdatePartProductsCollection(dbPart, partEdit);
                UpdatePartTypesCollection(dbPart, partEdit);



                await _partAsyncRepository.UpdateAsync(dbPart);

                return Ok(dbPart);
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

        private void UpdatePartCountryCollection(Part origPart, PartDto updatePart)
        {
            //add new countries
            foreach (var country in updatePart.Countries)
            {
                var origCountry = origPart.Countries.FirstOrDefault(c => c.Id == country.Id);
                if (origCountry == null)
                {
                    origPart.Countries.Add(_mapper.Map<Country>(country));
                }
            }
            //remove deleted countries
            for (int i = origPart.Countries.Count - 1; i >= 0; i--)
            {
                var origCountry = origPart.Countries[i];
                var updatedCountry = updatePart.Countries.FirstOrDefault(c => c.Id == origCountry.Id);
                if (updatedCountry == null)
                {
                    origPart.Countries.Remove(origCountry);
                }
            }
        }

        private void UpdatePartProductsCollection(Part origPart, PartDto updatePart)
        {
            foreach (var product in updatePart.Products)
            {
                var origProduct = origPart.Products.FirstOrDefault(p => p.Id == product.Id);
                if (origProduct == null)
                {
                    origPart.Products.Add(_mapper.Map<Product>(product));
                }
            }
            for (int i = origPart.Products.Count - 1; i >= 0; i--)
            {
                var origProduct = origPart.Products[i];
                var updatedProduct = updatePart.Products.FirstOrDefault(p => p.Id == origProduct.Id);
                if (updatedProduct == null)
                {
                    origPart.Products.Remove(origProduct);
                }
            }
        }

        private void UpdatePartTypesCollection(Part origPart, PartDto updatePart)
        {
            foreach (var standType in updatePart.StandTypes)
            {
                var origStandType = origPart.StandTypes.FirstOrDefault(s => s.Id == standType.Id);
                if (origStandType == null)
                {
                    origPart.StandTypes.Add(_mapper.Map<StandType>(standType));
                }
            }
            for (int i = origPart.StandTypes.Count - 1; i >= 0; i--)
            {
                var origStandType = origPart.StandTypes[i];
                var updatedStandType = updatePart.StandTypes.FirstOrDefault(s => s.Id == origStandType.Id);
                if (updatedStandType == null)
                {
                    origPart.StandTypes.Remove(origStandType);
                }
            }
        }

        ///Consider this perhaps -> https://medium.com/@hamidmusayev/synchronizing-entity-framework-core-child-collections-a-clean-and-reusable-approach-2ebd8e853f4d
    }
}
