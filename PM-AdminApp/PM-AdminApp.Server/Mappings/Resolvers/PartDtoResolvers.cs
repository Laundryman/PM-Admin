using AutoMapper;
using AutoMapper.Execution;
using Microsoft.Graph.Models;
using PMApplication.Dtos;
using PMApplication.Dtos.PlanModels;
using PMApplication.Dtos.StandTypes;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Entities.StandAggregate;

namespace PM_AdminApp.Server.Mappings.Resolvers
{
    public class ProductDtoPartProductResolver : IValueResolver<PartDto, Part, List<PartProduct>>
    {
        public List<PartProduct> Resolve(PartDto source, Part destination, List<PartProduct> destMember, ResolutionContext context)
        {
            var products = new List<PartProduct>();
            foreach (var prod in source.Products)
            {
                products.Add(new PartProduct
                {
                    PartId = source.Id ?? 0,
                    ProductId = prod.Id,
                    
                });
            }
            return products;
        }
    }

    public class StandTypeDtoStandTypePartResolver : IValueResolver<PartDto, Part, List<StandTypePart>>
    {
        public List<StandTypePart> Resolve(PartDto source, Part destination, List<StandTypePart> destMember, ResolutionContext context)
        {
            var products = new List<StandTypePart>();
            foreach (var stype in source.StandTypes)
            {
                products.Add(new StandTypePart
                {
                    PartId = source.Id ?? 0,
                    StandTypeId = stype.Id,

                });
            }
            return products;
        }
    }

    public class CountryDtoCountryPartResolver : IValueResolver<PartDto, Part, List<CountryPart>>
    {
        public List<CountryPart> Resolve(PartDto source, Part destination, List<CountryPart> destMember, ResolutionContext context)
        {
            var countries = new List<CountryPart>();
            foreach (var country in source.Countries)
            {
                countries.Add(new CountryPart
                {
                    PartId = source.Id ?? 0,
                    CountryId = country.Id,

                });
            }
            return countries;
        }
    }

}
