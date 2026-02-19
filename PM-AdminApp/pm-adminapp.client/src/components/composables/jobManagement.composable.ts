import { useLocationFilters } from '@/components/composables/locationFilters'
import { useMultiSelectLists } from '@/components/composables/multiSelectList.composable'
import { Country } from '@/models/Countries/country.model'
import { Region } from '@/models/Countries/region.model'
import { JobFolder } from '@/models/Jobs/JobFolder.model'
import countryService from '@/services/Countries/CountryService'
import { ref } from 'vue'
export function useFolderManagement() {
  const folderDialog = ref(false)
  const jobFolder = ref(new JobFolder())
  const jobFolders = ref<JobFolder[] | null>(null)
  const submitted = ref(false)
  const folder_countrySelectList = ref<Country[] | null>([])
  const folder_regionSelectList = ref<Region[] | null>([])
  const folder_selectedCountries = ref<number[]>([])
  const folder_selectedRegions = ref<number[]>([])
  const folder_selectAllCountries = ref(false)
  const multiSelectLists = useMultiSelectLists()
  const locationFilters = useLocationFilters()

  const folderSelectedRegion = ref()
  const folderSelectedCountry = ref()
  const ms_selectedRegions = ref<number[] | null>(null) // MultiSelect binding

  ////////////////////////////////////////////////////
  // Location Handlers
  ////////////////////////////////////////////////////

  async function onFolderRegionChange(selectedRegion: number | null) {
    if (selectedRegion) {
      jobFolder.value.regionId = selectedRegion
      await countryService
        .initialise()
        .catch((error) => console.error('Error initializing Country Service:', error))
      await countryService.getCountries(selectedRegion, '').then((response) => {
        folder_countrySelectList.value = response
        console.log('Countries loaded for region', selectedRegion, folder_countrySelectList.value)
      })
    } else {
      folder_countrySelectList.value = []
    }

    //jobFolder.value.regionsList = jobFolder.value.regions?.map((r) => r.id).join(',') || ''
  }

  function setRegionSelectList(regions: Region[]) {
    folder_regionSelectList.value = [...regions]
  }

  function setSelectedRegions(regionIds: number[]) {
    folder_selectedRegions.value = regionIds
  }

  function setCountrySelectList(countries: Country[]) {
    folder_countrySelectList.value = [...countries]
  }

  function setSelectedCountries(countryIds: number[]) {
    folder_selectedCountries.value = countryIds
  }

  async function onFolderCountryChange(evt: any) {
    // manageSelectedCountries(evt.value)
    multiSelectLists.manageSelectedValues(
      evt.value,
      folder_countrySelectList.value ?? [],
      jobFolder.value.countries ?? [],
    )
    // jobFolder.value.countriesList = jobFolder.value.countries?.map((c) => c.id).join(',') || ''
  }

  function onSelectAllFolderCountriesChange(event: any) {
    folder_selectedCountries.value = event.checked
      ? (folder_countrySelectList.value?.map((item) => item.id) ?? [])
      : []
    folder_selectAllCountries.value = event.checked
    multiSelectLists.manageSelectedValues(
      folder_selectedCountries.value,
      folder_countrySelectList.value ?? [],
      jobFolder.value.countries ?? [],
    )
    // jobFolder.value.countriesList = jobFolder.value.countries?.map((c) => c.id).join(',') || ''
  }

  function clearCountrySelection() {
    folder_selectedCountries.value = []
    jobFolder.value.countries = []
    // jobFolder.value.countriesList = ''
  }

  return {
    folderDialog,
    jobFolder,
    jobFolders,
    submitted,
    folder_countrySelectList,
    folder_selectedCountries,
    folder_selectAllCountries,
    folderSelectedRegion,
    folder_regionSelectList,

    onFolderRegionChange,
    onFolderCountryChange,
    onSelectAllFolderCountriesChange,
    clearCountrySelection,
    setCountrySelectList,
    setSelectedCountries,
    setRegionSelectList,
    setSelectedRegions,
  }
}
