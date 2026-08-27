using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using PM_AdminApp.Server.Extensions;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.ProductAggregate;
using PMApplication.Entities.StandAggregate;
using PMApplication.Interfaces;
using PMApplication.Interfaces.RepositoryInterfaces;
using PMApplication.Specifications;
using PMApplication.Specifications.Filters;
using PMInfrastructure.Repositories;
using System.Text.Json;
using PMApplication.Dtos.PlanModels;
using Page = PMApplication.Dtos.Page;

namespace PM_AdminApp.Server.Controllers
{
    [Authorize]
    [Route("api/stands/[action]")]
    [ApiController]
    public class StandsController : BaseController
    {
        private readonly ILogger<StandsController> _logger;
        private readonly IMapper _mapper;
        private readonly IAsyncRepository<Stand> _asyncStandRepository;
        private readonly IStandRepository _standRepository;
        private readonly IStandTypeRepository _standTypeRepository;
        private readonly IStandColumnRepository _standColumnRepository;
        private readonly IStandColumnUprightRepository _standColumnUprightRepository;
        private readonly IAsyncRepository<Region> _regionRepository;
        private readonly IAsyncRepository<Country> _countryRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;



        public StandsController(IMapper mapper, IAsyncRepository<Stand> asyncStandRepository,
            IAsyncRepository<Country> countryRepository, IAsyncRepository<Category> categoryRepository,
            ILogger<StandsController> logger, IStandRepository standRepository,
            IAsyncRepository<Region> regionRepository, IStandTypeRepository standTypeRepository, IStandColumnRepository standColumnRepository, IStandColumnUprightRepository standColumnUprightRepository)
        {
            _logger = logger;
            _standRepository = standRepository;
            _asyncStandRepository = asyncStandRepository;
            _countryRepository = countryRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _regionRepository = regionRepository;
            _standTypeRepository = standTypeRepository;
            _standColumnRepository = standColumnRepository;
            _standColumnUprightRepository = standColumnUprightRepository;
        }


        [HttpPost]
        public async Task<IActionResult> SearchStands(StandFilterDto filterDto)
        {
            try
            {
                //var spec = new ProductSpecification(_mapper.Map<ProductFilter>(filterDto));
                var stands = await _standRepository.SearchStands(filterDto);

                return Ok(stands);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside SearchStands action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetStands(StandFilterDto filterDto)
        {
            try
            {
                //var spec = new ProductSpecification(_mapper.Map<ProductFilter>(filterDto));
                var stands = await _standRepository.SearchStands(filterDto);

                return Ok(stands);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside SearchStands action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        //[HttpGet(Name = "ProductSelectList")]
        //public async Task<IActionResult> GetStandSelectList([FromQuery] ProductFilterDto filterDto)
        //{
        //    try
        //    {
        //        //if (filterDto.CountriesList != null)
        //        //{
        //        //    var allCountries = await IsAllCountries(filterDto.CountriesList, _countryRepository, _mapper);
        //        //    if (allCountries)
        //        //    {
        //        //        filterDto.CountriesList = null;
        //        //    }
        //        //}

        //        var spec = new StandSpecification(_mapper.Map<StandFilter>(filterDto));
        //        var stands = await _asyncStandRepository.ListAsync(spec);

        //        var StandSelectList = CreateSelectList(stands);
        //        return Ok(StandSelectList);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogWarning($"Something went wrong inside GetStand action: {ex.Message}");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        [HttpGet()]
        public async Task<IActionResult> GetStand([FromQuery] int id)
        {
            try
            {
                var spec = new StandByIdSpecification(id);
                var stand = await _asyncStandRepository.FirstOrDefaultAsync(spec);

                if (stand == null)
                {
                    _logger.LogWarning($"Stand with id: {id}, hasn't been found in db.");
                    return NotFound();
                }
                else
                {
                    _logger.LogInformation($"Returned stand with id: {id}");
                    var response = _mapper.Map<StandDto>(stand);

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetProductById action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveStand(StandUpdateDto updateStand)
        {
            try
            {
                if (updateStand == null)
                {
                    _logger.LogError("Stand object sent from client is null.");
                    return BadRequest("Stand object is null");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid stand object sent from client.");
                    return BadRequest("Invalid model object");
                }
                //var stand = _mapper.Map<Stand>(updateStand);

                var id = updateStand.Id;
                var standFilter = new StandFilter() { Id = id };
                var spec = new EditStandSpecification(standFilter);
                var standEdit = await _standRepository.FirstAsync(spec);
                if (standEdit == null)
                {
                    _logger.LogError($"Stand with id: {id}, hasn't been found in db.");
                    return NotFound();
                }

                _mapper.Map(updateStand, standEdit);
                standEdit.DateUpdated = DateTime.Now;

                await _standRepository.UpdateAsync(standEdit);

                //Now manage relationships.
                await UpdateStandCountryCollection(standEdit, updateStand);
                await UpdateRegionsCollection(standEdit, updateStand);
                await UpdateColumnCollection(standEdit, updateStand);
                await UpdateRowCollection(standEdit, updateStand);
                await _standRepository.UpdateAsync(standEdit);
                var response = _mapper.Map<StandDto>(standEdit);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside UpdateStand action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateStand(StandUpdateDto createDto)
        {
            try
            {
                //var userProfile = await this.MappedUser();

                var stand = _mapper.Map<Stand>(createDto);
                stand.DateCreated = DateTime.Now;
                stand.DateUpdated = DateTime.Now;
                stand.DateAvailable = DateTime.Now;

                var standType = await _standTypeRepository.GetByIdAsync(createDto.StandTypeId);
                var parentStandType = await _standTypeRepository.GetByIdAsync((int)standType.ParentStandTypeId);
                stand.StandTypeName = standType.Name;
                stand.ParentStandTypeId = standType.ParentStandTypeId;
                stand.ParentStandTypeName = parentStandType.Name;
                var createdStand = await _standRepository.AddAsync(stand);

                await UpdateRegionsCollection(createdStand, createDto);
                await UpdateStandCountryCollection(createdStand, createDto);
                await UpdateColumnCollection(createdStand, createDto);
                await UpdateRowCollection(createdStand, createDto);
                //var createdStand = await _standRepository.AddAsync(stand);
                await _standRepository.UpdateAsync(createdStand);
                var response = _mapper.Map<StandDto>(createdStand);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside CreateStand action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteStand(int Id)
        {
            try
            {
                var stand = await _standRepository.GetByIdAsync(Id);
                await _standRepository.DeleteAsync(stand);
                return Ok();
            }
            catch (Exception ex)
            {
                var errorMessage = $"Cannot delete stand with id {Id}";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task UpdateRegionsCollection(Stand origStand, StandUpdateDto updateStand)
        {
            //var regionDtos = JsonConvert.DeserializeObject<List<RegionDto>>(updateStand.Regions);
            foreach (var region in updateStand.Regions)
            {
                var origRegion = origStand.Regions.FirstOrDefault(r => r.Id == region.Id);
                if (origRegion == null)
                {

                    var dbRegion = await _regionRepository.GetByIdAsync(region.Id);
                    origStand.Regions.Add(dbRegion);
                }
            }

            var regionsToDelete = new List<Region>();
            for (int i = origStand.Regions.Count - 1; i >= 0; i--)
            {
                var origRegion = origStand.Regions[i];
                var updatedRegion = updateStand.Regions.FirstOrDefault(r => r.Id == origRegion.Id);
                if (updatedRegion == null)
                {
                    var dbRegion = origStand.Regions.FirstOrDefault(r => r.Id == origRegion.Id);
                    regionsToDelete.Add(dbRegion);
                }
            }

            foreach (var region in regionsToDelete)
            {
                origStand.Regions.Remove(region);
            }

            //update Part.RegionList string
            origStand.RegionsList = string.Join(",", origStand.Regions.Select(r => r.Id));
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task UpdateStandCountryCollection(Stand origStand, StandUpdateDto updateStand)
        {
            //add new countries
            //var standCountries = JsonConvert.DeserializeObject<List<CountryDto>>(updateStand.Countries);
            foreach (var country in updateStand.Countries)
            {
                var origCountry = origStand.Countries.FirstOrDefault(c => c.Id == country.Id);
                if (origCountry == null)
                {
                    var dbCountry = await _countryRepository.GetByIdAsync(country.Id);
                    if (dbCountry != null)
                    {
                        origStand.Countries.Add(dbCountry);
                    }
                }
            }

            //remove deleted countries
            var countriesToDelete = new List<Country>();
            for (int i = 0; i < origStand.Countries.Count; i++)
            {
                var origCountry = origStand.Countries[i];
                var updatedCountry = updateStand.Countries.FirstOrDefault(c => c.Id == origCountry.Id);
                if (updatedCountry == null)
                {
                    countriesToDelete.Add(origCountry);
                }
            }

            foreach (var country in countriesToDelete)
            {
                origStand.Countries.Remove(country);
            }

            //update Part.CountryList string
            origStand.CountriesList = string.Join(",", origStand.Countries.Select(c => c.Id));
        }

        private async Task UpdateColumnCollection(Stand origStand, StandUpdateDto updateStand)
        {
            var existingCols = origStand.ColumnList;
            var newCols = updateStand.ColumnList;

            foreach (var col in updateStand.ColumnList)
            {
                var origCol = origStand.ColumnList.FirstOrDefault(c => c.Position == col.Position);
                if (origCol == null)
                {
                    StandColumn newCol = new StandColumn()
                    {
                        Position = col.Position,
                        StandId = col.StandId,
                        Width = col.Width,
                    };
                    List<StandColumnUpright> newUprightList = new List<StandColumnUpright>();
                    foreach (var upright in col.ColumnUprightList)
                    {
                        StandColumnUpright newUpright = new StandColumnUpright()
                        {
                            Height = upright.Height,
                            Position = upright.Position,
                            StandId = col.StandId

                        };
                        newUprightList.Add(newUpright);

                    }

                    newCol.StandColumnUprights = newUprightList;
                    origStand.ColumnList.Add(newCol);
                }
                else
                {
                    origCol.Width = col.Width;
                    await updateStandColumnUprights(origCol, col);
                }
            }

            var columnsToDelete = new List<StandColumn>();
            //remove deleted columns
            for (int i = origStand.ColumnList.Count - 1; i >= 0; i--)
            {
                var origCol = origStand.ColumnList[i];
                var updatedColumn = updateStand.ColumnList.FirstOrDefault(c => c.Id == origCol.Id);
                if (updatedColumn == null)
                {
                    var dbCol = origStand.ColumnList.FirstOrDefault(c => c.Id == origCol.Id);
                    columnsToDelete.Add(dbCol);
                }
            }

            foreach (var col in columnsToDelete)
            {
                origStand.ColumnList.Remove(col);
            }

        }

        private async Task updateStandColumnUprights(StandColumn origCol, PlanmStandColumnDto editCol)
        {
            List<StandColumnUpright> newUprightList = new List<StandColumnUpright>();
            foreach (var upright in editCol.ColumnUprightList)
            {
                var currUpright = origCol.StandColumnUprights.First(c => c.Id == upright.Id);
                if (currUpright != null)
                {
                    currUpright.Position = upright.Position;
                    currUpright.Height = upright.Height;
                }
                else
                {
                    StandColumnUpright newUpright = new StandColumnUpright()
                    {
                        Height = upright.Height,
                        Position = upright.Position,
                        //StandId = origCol.StandId

                    };
                    origCol.StandColumnUprights.Add(newUpright);
                }

            }

            //remove uprights
            var urToRemove = new List<StandColumnUpright>();
            foreach (var ur in origCol.StandColumnUprights)
            {
                var existUR = editCol.ColumnUprightList.First(c => c.Id == ur.Id);
                if (existUR == null)
                {
                    urToRemove.Add(ur);
                }
            }

            foreach (var ur in urToRemove)
            {
                origCol.StandColumnUprights.Remove(ur);
            }

            //reset the upright list
            origCol.StandColumnUprights = newUprightList;
        }

        private async Task UpdateRowCollection(Stand origStand, StandUpdateDto updateStand)
        {
            var existingRows = origStand.RowList;
            var newRows = updateStand.RowList;
            if (newRows == null)
            {
                return ;
            }
            foreach (var row in updateStand.RowList)
            {
                var origRow = origStand.RowList.FirstOrDefault(r => r.Id == row.Id);
                if (origRow == null)
                {
                    StandRow newRow = new StandRow()
                    {
                        Position = row.Position,
                        StandId = row.StandId,
                        Height = row.Height,
                    };
                    origStand.RowList.Add(newRow);
                }
                else
                {
                    origRow.Height = row.Height;
                }
            }

            var rowsToDelete = new List<StandRow>();
            //remove deleted rows
            for (int i = origStand.RowList.Count - 1; i >= 0; i--)
            {
                var origRow = origStand.RowList[i];
                var updatedRow = updateStand.RowList.FirstOrDefault(r => r.Id == origRow.Id);
                if (updatedRow == null)
                {
                    var dbRow = origStand.RowList.FirstOrDefault(r => r.Id == origRow.Id);
                    rowsToDelete.Add(dbRow);
                }
            }

            foreach (var row in rowsToDelete)
            {
                origStand.RowList.Remove(row);
            }

        }
    }
}
