import { useLocationFilters } from '@/components/composables/locationFilters'
import { useMultiSelectLists } from '@/components/composables/multiSelectList.composable'
import { Country } from '@/models/Countries/country.model'
import { Region } from '@/models/Countries/region.model'
import { Shade } from '@/models/Products/shade.model'
import { ref } from 'vue'
export function useShadeManagement() {
  const shadeDialog = ref(false)
  const shade = ref(new Shade())
  const shades = ref<Shade[] | null>(null)
  const submitted = ref(false)
  const shade_countrySelectList = ref<Country[] | null>([])
  const shade_regionSelectList = ref<Region[] | null>([])
  const shade_selectedCountries = ref<number[]>([])
  const shade_selectedRegions = ref<number[]>([])
  const shade_selectAllCountries = ref(false)
  const multiSelectLists = useMultiSelectLists()
  const locationFilters = useLocationFilters()
  ////////////////////////////////////////////////////
  // Location Handlers
  ////////////////////////////////////////////////////

  // async function onRegionChange(evt: any) {
  //   shade_countrySelectList.value = await locationFilters.getCountriesForRegions(
  //     shade_selectedRegions.value,
  //   )
  //   //remove any countries no longer in the list
  //   let newSelectList = shade_selectedCountries.value?.filter((countryId) =>
  //     shade_countrySelectList.value?.some((c) => c.id === countryId),
  //   )
  //   if (newSelectList) {
  //     shade_selectedCountries.value = newSelectList
  //   } else {
  //     shade_selectedCountries.value = []
  //   }
  //   multiSelectLists.manageSelectedValues(
  //     evt.value,
  //     shade_regionSelectList.value ?? [],
  //     shade.value.regions ?? [],
  //   )
  //   multiSelectLists.manageSelectedValues(
  //     shade_selectedCountries.value ?? [],
  //     shade_countrySelectList.value ?? [],
  //     shade.value.countries ?? [],
  //   )

  //   shade.value.regionsList = shade.value.regions?.map((r) => r.id).join(',') || ''
  // }

  function setCountrySelectList(countries: Country[]) {
    shade_countrySelectList.value = [...countries]
  }

  function setSelectedCountries(countryIds: number[]) {
    shade_selectedCountries.value = countryIds
  }

  async function onCountryChange(evt: any) {
    // manageSelectedCountries(evt.value)
    multiSelectLists.manageSelectedValues(
      evt.value,
      shade_countrySelectList.value ?? [],
      shade.value.countries ?? [],
    )
    shade.value.countriesList = shade.value.countries?.map((c) => c.id).join(',') || ''
  }

  function onSelectAllCountriesChange(event: any) {
    shade_selectedCountries.value = event.checked
      ? (shade_countrySelectList.value?.map((item) => item.id) ?? [])
      : []
    shade_selectAllCountries.value = event.checked
    multiSelectLists.manageSelectedValues(
      shade_selectedCountries.value,
      shade_countrySelectList.value ?? [],
      shade.value.countries ?? [],
    )
    shade.value.countriesList = shade.value.countries?.map((c) => c.id).join(',') || ''
  }

  function clearCountrySelection() {
    shade_selectedCountries.value = []
    shade.value.countries = []
    shade.value.countriesList = ''
  }

  return {
    shadeDialog,
    shade,
    shades,
    submitted,
    shade_countrySelectList,
    shade_selectedCountries,
    shade_selectAllCountries,
    // onRegionChange,
    onCountryChange,
    onSelectAllCountriesChange,
    clearCountrySelection,
    setCountrySelectList,
    setSelectedCountries,
  }
}
