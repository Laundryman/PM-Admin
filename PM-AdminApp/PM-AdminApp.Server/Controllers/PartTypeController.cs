using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PMApplication.Entities;
using PMApplication.Entities.PartAggregate;
using PMApplication.Interfaces;
using PMApplication.Specifications;

namespace LMXApi.Controllers
{
    [Route("api/parttypes")]
    [ApiController]
    public class PartTypesController : ControllerBase
    {
        //private ILoggerManager _logger;
        //private IRepositoryWrapper _repository;
        //private ILoggerManager _logger;
        private readonly IAppLogger<PartTypesController> _logger;
        private readonly IMapper _mapper;
        //private readonly IReadRepository<Part> _partReadRepository;
        private readonly IAsyncRepository<PartType> _partTypeRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;


        public PartTypesController(IMapper mapper, IAsyncRepository<PartType> partTypeRepository, IAsyncRepository<Category> categoryRepository, IAppLogger<PartTypesController> logger)
        {
            _logger = logger;
            _partTypeRepository = partTypeRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpGet(Name = "PartTypes")]
        public async Task<IActionResult> GetPartTypes()
        {
            try
            {
                var spec = new PartTypeSpecification();
                var partTypes = await _partTypeRepository.ListAsync(spec);

                var partTypesSelectList = createSelectList(partTypes);
                return Ok(partTypesSelectList);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Something went wrong inside GetPartTypes action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        private List<PartType> createSelectList(IReadOnlyList<PartType> list)
        {
            var selectList = new List<PartType>();
            var partTypeSelect = new PartType("Select Type", "Select Type");
            //partTypeSelect.Id = 0;

            selectList.Add(partTypeSelect);

            for (int i = 0; i < list.Count; i++)
            {
                selectList.Add(list[i]);
            }

            return selectList;
        }

    }
}
