using AutoMapper;
using AutoMapper.Execution;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using PMApplication.Dtos;
using PMApplication.Dtos.PlanModels;
using PMApplication.Dtos.StandTypes;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Entities.ProductAggregate;
using PMApplication.Entities.StandAggregate;

namespace PM_AdminApp.Server.Mappings.Resolvers
{
    public class ProductDtoProductResolver : IValueResolver<PartUploadDto, Part, List<Product>>
    {
        private readonly IMapper _mapper;

        public ProductDtoProductResolver(IMapper mapper)
        {
            _mapper = mapper;
        }
        public List<Product> Resolve(PartUploadDto source, Part destination, List<Product> destMember, ResolutionContext context)
        {
            var partProducts = JsonConvert.DeserializeObject<List<ProductDto>>(source.Products ?? "[]");
            var products = _mapper.Map<List<Product>>(partProducts);

            return products;
        }
    }

    public class StandTypePartResolver : IValueResolver<PartUploadDto, Part, List<StandType>>
    {
        private readonly IMapper _mapper;

        public StandTypePartResolver(IMapper mapper)
        {
            _mapper = mapper;
        }
        public List<StandType> Resolve(PartUploadDto source, Part destination, List<StandType> destMember, ResolutionContext context)
        {
            var partStandTypes = JsonConvert.DeserializeObject<List<StandTypeDto>>(source.StandTypes ?? "[]");
            foreach (var standType in partStandTypes)
            {
                // Ensure Description is not null
                standType.Brand = null;
            }
            var standTypes = _mapper.Map<List<StandType>>(partStandTypes);
            return standTypes;
        }
    }

    public class RegionPartResolver : IValueResolver<PartUploadDto, Part, List<Region>>
    {
        private readonly IMapper _mapper;

        public RegionPartResolver(IMapper mapper)
        {
            _mapper = mapper;
        }
        public List<Region> Resolve(PartUploadDto source, Part destination, List<Region> destMember, ResolutionContext context)
        {
            var regionParts = JsonConvert.DeserializeObject<List<RegionDto>>(source.Regions ?? "[]");
            var regions = _mapper.Map<List<Region>>(regionParts);
            return regions;
        }
    }

    public class CountryPartResolver : IValueResolver<PartUploadDto, Part, List<Country>>
    {
        private readonly IMapper _mapper;

        public CountryPartResolver(IMapper mapper)
        {
            _mapper = mapper;
        }
        public List<Country> Resolve(PartUploadDto source, Part destination, List<Country> destMember, ResolutionContext context)
        {
            var countryParts = JsonConvert.DeserializeObject<List<CountryDto>>(source.Countries ?? "[]");
            var countries = _mapper.Map<List<Country>>(countryParts);
            return countries;
        }
    }

}
