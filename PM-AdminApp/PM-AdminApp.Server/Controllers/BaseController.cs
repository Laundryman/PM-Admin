using PMApplication.Specifications.Filters;
using PMApplication.Specifications;
using Microsoft.AspNetCore.Mvc;
using PMApplication.Interfaces;
using AutoMapper;
using PMApplication.Entities.CountriesAggregate;

namespace LMXApi.Controllers
{
    public class BaseController : ControllerBase
    {
        internal async Task<bool> IsAllCountries(string countryList, IAsyncRepository<Country> countryRepository, IMapper _mapper )
        {
            var countries = new List<Country>();
            var countFilter = new CountryFilter();
            countFilter.IsPagingEnabled = false;
            var countSpec = new CountrySpecification(_mapper.Map<CountryFilter>(countFilter));
            int totalItems = await countryRepository.CountAsync(countSpec);

            var requiredCountryList = countryList.Split(',');
            var requiredListItemCount = requiredCountryList.Length;

            return totalItems == requiredListItemCount;
        }
    }
}
