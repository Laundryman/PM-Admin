import { useLocationFilters } from '@/components/composables/locationFilters'
import type { searchClusterInfo } from '@/models/Clusters/searchClusterInfo.model'
import { Country } from '@/models/Countries/country.model'
import { Region } from '@/models/Countries/region.model'
import { regionFilter } from '@/models/Countries/regionFilter.model'

import { CreateLayoutFilter } from '@/models/Layout/createLayoutFilter.model'
import { LayoutFilter } from '@/models/Layout/LayoutFilter.model'
import { LayoutInfo } from '@/models/Layout/searchLayoutInfo.model'
import { Stand } from '@/models/Stands/stand.model'
import { StandFilter } from '@/models/Stands/standFilter.model'
import { StandType } from '@/models/StandTypes/standType.model'
import { standTypeFilter } from '@/models/StandTypes/standTypeFilter.model'
import clusterService from '@/services/Clusters/ClusterService'
import { default as countryService } from '@/services/Countries/CountryService'

import standService from '@/services/Stands/StandService'
import standTypeService from '@/services/StandTypes/StandTypeService'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import { storeToRefs } from 'pinia'
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { z } from 'zod'

const router = useRouter()
const systemStore = useSystemStore()
const brandStore = useBrandStore()
const { brands } = storeToRefs(brandStore)
const clusterName = ref('')
const layoutPartNumber = ref('')
const selectedRegion = ref()
const selectedCountryId = ref()
const selectedCountry = ref()
const selectedStandTypeId = ref()
const selectedStandId = ref()
const selectedStand = ref<Stand | null>(null)
const selectedStandType = ref<StandType | null>(null)
const selectedLayoutId = ref()
const selectedLayout = ref<LayoutInfo | null>(null)

const ms_selectedRegions = ref<number[] | null>(null) // MultiSelect binding
const ms_selectedCountries = ref<number[] | null>(null) // MultiSelect binding
const selectedRegionIds = ref<number[] | null>(null)
const selectedCountryIds = ref<number[] | null>(null)
const countrySelectList = ref<Country[] | null>(null)
const selectAllCountries = ref(false)

const allSelectedRegions = ref<Region[] | null>(null)
const allSelectedCountries = ref<Country[] | null>(null)

const layouts = ref<LayoutInfo[] | null>([])
const { regions, countries } = useLocationFilters()
const locationFilters = useLocationFilters()
const standTypes = ref<StandType[] | null>([])
const stands = ref<Stand[] | null>([]) // Replace 'any' with the appropriate type for stands
const resolver = ref(
  zodResolver(
    z.object({
      clusterName: z.string().min(1, { message: 'Cluster name is required.' }),
    }),
  ),
)

export function useManageCluster() {
  async function initialise(selectedCluster: searchClusterInfo) {
    await countryService.initialise()

    ms_selectedRegions.value = selectedCluster.regionsList.split(',').map((id) => parseInt(id, 10)) // Convert to array of numbers
    selectedRegionIds.value = selectedCluster.regionsList.split(',').map((id) => parseInt(id, 10)) // Convert to array of numbers
    selectedCountryId.value = selectedCluster.countriesList
    ms_selectedCountries.value = selectedCluster.countriesList
      .split(',')
      .map((id) => parseInt(id, 10)) // Convert to array of numbers
    selectedStandId.value = selectedCluster.standId
    selectedStandTypeId.value = selectedCluster.standTypeId
    let brandid = brandStore.activeBrand?.id ?? 0
    let rFilter = new regionFilter()
    rFilter.brandId = brandid
    await useLocationFilters()
      .getRegions(rFilter)
      .then((response) => {
        regions.value = response
      })

    await useLocationFilters()
      .getCountriesForRegions(selectedRegionIds.value ?? [])
      .then((response) => {
        countrySelectList.value = response
      })

    standTypes.value = await getStandTypes()
    await getStands()
  }

  function clearFilters() {
    selectedRegion.value = null
    selectedCountryId.value = null
    selectedCountry.value = null
    selectedStandTypeId.value = null
    selectedStandId.value = null
    selectedStand.value = null
    selectedStandType.value = null
    standTypes.value = []
    stands.value = []
  }

  async function getStandTypes() {
    // Implement the logic to get stand types here
    let filter = new standTypeFilter()
    filter.brandId = brandStore.activeBrand?.id

    await standTypeService.initialise()
    return await standTypeService.getAllStandTypes(filter)
  }

  async function getStands() {
    // Implement the logic to get stands here
    if (selectedStandTypeId.value) {
      selectedStandType.value =
        standTypes.value?.find((st) => st.id === selectedStandTypeId.value) ?? null
      await standService.initialise()
      let filter = new StandFilter()
      filter.brandId = brandStore.activeBrand?.id
      // filter.countryId = selectedCountryId.value
      filter.countryIds = ms_selectedCountries.value.map((id) => id.toString()).join(',') // Convert to array of numbers
      filter.regionIds = ms_selectedRegions.value.map((id) => id.toString()).join(',')

      filter.standTypeId = selectedStandTypeId.value as number
      await clusterService.initialise()
      stands.value = await clusterService.getStands(filter)
      selectedStand.value = stands.value.find((s) => (s.id = selectedStandId.value)) ?? null
      // Do something with the retrieved stands
    }
  }

  async function onStandChange() {
    if (selectedStandId.value) {
      selectedStand.value = stands.value?.find((s) => s.id === selectedStandId.value) ?? null
    }
    let filter = new LayoutFilter()
    filter.brandId = brandStore.activeBrand?.id
    filter.countryId = selectedCountryId.value
    filter.standTypeId = selectedStandTypeId.value as number
    filter.standId = selectedStandId.value as number
    await clusterService.initialise()
    // await clusterService.getLayouts(filter).then((response) => {
    //     layouts.value = response;
    // });
  }

  function onLayoutChange() {
    if (selectedLayoutId.value) {
      selectedLayout.value = layouts.value?.find((l) => l.id === selectedLayoutId.value) ?? null
    }
  }
  async function createLayout({ valid }: any) {
    if (!valid) {
      return
    }

    // Implement the logic to create a cluster here
    let filter = new CreateLayoutFilter()
    filter.name = clusterName.value
    filter.countryId = selectedCountryId.value
    filter.standTypeId = selectedStandTypeId.value as number
    filter.standId = selectedStandId.value as number
    filter.regionId = selectedRegion.value as number
    filter.countryIds = ms_selectedCountries.value?.map((id) => id.toString()).join(',') ?? ''
    filter.regionIds = ms_selectedRegions.value?.map((id) => id.toString()).join(',') ?? ''
    filter.brandId = brandStore.activeBrand?.id as number

    await clusterService.initialise()
    await clusterService.createLayout(filter).then((newClusterId) => {
      if (newClusterId) {
        router.push({ name: 'editCluster', params: { id: newClusterId } })
      }
    })
  }

  ////////////////////////////////////////////////////
  // Location Handlers
  ////////////////////////////////////////////////////

  async function onRegionChange(evt: any) {
    selectedRegionIds.value = evt.value as number[]
    if (selectedRegionIds.value.length > 0) {
      countrySelectList.value = await locationFilters.getCountriesForRegions(
        selectedRegionIds.value ?? [],
      )
      //remove any countries from the selected list that are no longer in the available list
      if (ms_selectedCountries.value) {
        ms_selectedCountries.value = ms_selectedCountries.value.filter((id) =>
          countrySelectList.value.some((country) => country.id === id),
        )
      }
    }

    // emit('update:selectedRegions', selectedRegionIds)
  }

  async function onCountryChange(evt: any) {
    //let emitData = { countries: ms_selectedCountries.value, regions: ms_selectedRegions.value }
    // emit('update:selectedCountries', emitData)

    standTypes.value = await getStandTypes()
    // console.log('Stand Types:', standTypes.value)
  }

  function onSelectAllCountriesChange(event: any) {
    ms_selectedCountries.value = event.checked
      ? (countrySelectList.value?.map((item) => item.id) ?? [])
      : []
    selectAllCountries.value = event.checked
    // manageSelectedValues(
    //   ms_selectedCountries.value,
    //   countrySelectList.value ?? [],
    //   userModel.value.countries ?? [],
    // )
    // userModel.value.countriesList = userModel.value.countries?.map((c) => c.id).join(',') || ''
  }

  function clearCountrySelection() {
    ms_selectedCountries.value = []
    // userModel.value.countries = []
    // userModel.value.countriesList = ''
  }

  return {
    resolver,
    clusterName,
    layoutPartNumber,
    selectedRegion,
    selectedCountryId,
    selectedCountry,
    selectedStandTypeId,
    selectedStandId,
    selectedStand,
    selectedStandType,
    selectedLayoutId,
    selectedLayout,
    ms_selectedRegions,
    ms_selectedCountries,
    selectedRegionIds,
    selectedCountryIds,
    countrySelectList,
    selectAllCountries,
    allSelectedRegions,
    allSelectedCountries,
    layouts,
    regions,
    countries,
    standTypes,
    stands,
    initialise,
    clearFilters,
    getStandTypes,
    getStands,
    onStandChange,
    onLayoutChange,
    createLayout,
    onRegionChange,
    onCountryChange,
    onSelectAllCountriesChange,
    clearCountrySelection,
  }
}
