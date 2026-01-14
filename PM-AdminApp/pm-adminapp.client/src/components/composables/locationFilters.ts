import { Country } from '@/models/Countries/country.model'
import { Region } from '@/models/Countries/region.model'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { default as countryService } from '@/services/Countries/CountryService'
import { ref } from 'vue'

export function useLocationFilters() {
  const regions = ref<Region[] | null>([])
  const countries = ref<Country[] | null>([])

  async function getRegions(rFilter: regionFilter) {
    await countryService
      .initialise()
      .catch((error) => console.error('Error initializing Country Service:', error))
    await countryService.getRegions(rFilter).then((response) => {
      regions.value = response
      console.log('Regions loaded', regions.value)
    })
    return regions.value
  }

  async function onRegionChange(selectedRegion: number | null) {
    if (selectedRegion) {
      await countryService
        .initialise()
        .catch((error) => console.error('Error initializing Country Service:', error))
      await countryService.getCountries(selectedRegion, '').then((response) => {
        countries.value = response
        console.log('Countries loaded for region', selectedRegion, countries.value)
      })
    } else {
      countries.value = []
    }
    return countries.value
  }

  return { regions, countries, getRegions, onRegionChange }
}
