<script setup lang="ts">
import { useLocationFilters } from '@/components/composables/locationFilters'
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
import { onMounted, ref } from 'vue'
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
const allStandsForStandType = ref<Stand[] | null>(null)
const availableCountries = ref<number[] | null>(null)
const availableRegions = ref<number[] | null>(null)

const resolver = ref(
  zodResolver(
    z.object({
      country: z.array(z.number(), { message: 'Country is required.' }),
      clusterName: z.string().min(1, { message: 'Cluster name is required.' }),
      // layoutPartNumber: z.string().min(1, { message: 'Layout part number is required.' }),
      region: z.array(z.number(), { message: 'Region is required.' }),
      standType: z.number({ message: 'Stand type is required.' }),
      stand: z.number({ message: 'Stand is required.' }),
    }),
  ),
)
onMounted(async () => {
  await countryService.initialise()

  standTypes.value = await getStandTypes()
})
// async function onRegionChange() {
//   if (selectedRegion.value) {
//     countries.value = await useLocationFilters().onRegionChange(selectedRegion.value)
//   } else {
//     countries.value = []
//   }
// }

// async function onCountryChange() {
//   if (selectedCountryId.value) {
//     selectedCountry.value =
//       countries.value?.find((c) => c.id === selectedCountryId.value)?.name ?? ''
//     standTypes.value = await getStandTypes()
//     console.log('Stand Types:', standTypes.value)
//   }
// }
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

async function getAvailableCountries() {
  // Implement the logic to get available countries here
  availableCountries.value = []
  availableRegions.value = []
  for (let stand of allStandsForStandType.value) {
    // Implement the logic to get available countries for each stand type here
    let countryArray = stand.countriesList.split(',').map((id) => parseInt(id))
    availableCountries.value = availableCountries.value.concat(countryArray ?? [])
    let regionArray = stand.regionsList.split(',').map((id) => parseInt(id))
    availableRegions.value = availableRegions.value.concat(regionArray ?? [])
  }

  //remove duplicates
  availableCountries.value = [...new Set(availableCountries.value)]
  availableRegions.value = [...new Set(availableRegions.value)]

  //get regions
  let brandid = brandStore.activeBrand?.id ?? 0
  let rFilter = new regionFilter()
  rFilter.brandId = brandid
  rFilter.idList = availableRegions.value.join(',')

  regions.value = []
  countries.value = []
  await useLocationFilters()
    .getRegions(rFilter)
    .then((response) => {
      regions.value = response
    })
}

async function onStandTypeChange() {
  //clear previous selections related to stands
  selectedStandId.value = null
  selectedStand.value = null
  selectedLayoutId.value = null
  selectedLayout.value = null
  ms_selectedCountries.value = []
  ms_selectedRegions.value = []

  // Implement the logic to get stands here
  if (selectedStandTypeId.value) {
    selectedStandType.value =
      standTypes.value?.find((st) => st.id === selectedStandTypeId.value) ?? null
    await standService.initialise()
    let filter = new StandFilter()
    filter.brandId = brandStore.activeBrand?.id

    filter.standTypeId = selectedStandTypeId.value as number
    await clusterService.initialise()
    allStandsForStandType.value = await clusterService.getStands(filter)
    // Do something with the retrieved stands

    await getAvailableCountries()
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
  filter.brandId = brandStore.activeBrand?.id as number
  // filter.regionId = selectedRegion.value as number
  // filter.countryId = selectedCountryId.value
  filter.standTypeId = selectedStandTypeId.value as number
  filter.standId = selectedStandId.value as number
  filter.name = clusterName.value
  filter.countryIds = ms_selectedCountries.value?.map((id) => id.toString()).join(',') ?? ''
  filter.regionIds = ms_selectedRegions.value?.map((id) => id.toString()).join(',') ?? ''

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
      availableCountries.value,
    )
    //remove any countries from the selected list that are no longer in the available list
    if (ms_selectedCountries.value) {
      ms_selectedCountries.value = ms_selectedCountries.value.filter((id) =>
        countrySelectList.value.some((country) => country.id === id),
      )
    }
  }

  stands.value = []
  selectedStand.value = null
  selectedLayoutId.value = null
  selectedLayout.value = null
  ms_selectedCountries.value = []

  // emit('update:selectedRegions', selectedRegionIds)
}

async function onCountryChange(evt: any) {
  if (selectedStandTypeId.value) {
    selectedStandType.value =
      standTypes.value?.find((st) => st.id === selectedStandTypeId.value) ?? null
    await standService.initialise()
    let filter = new StandFilter()
    filter.brandId = brandStore.activeBrand?.id
    // filter.countryId = selectedCountryId.value
    if (ms_selectedCountries.value?.length) {
      filter.countryIds = ms_selectedCountries.value.map((id) => id.toString()).join(',') // Convert to array of numbers
    }
    if (ms_selectedRegions.value?.length) {
      filter.regionIds = ms_selectedRegions.value.map((id) => id.toString()).join(',')
    }

    filter.standTypeId = selectedStandTypeId.value as number
    await clusterService.initialise()
    stands.value = await clusterService.getStands(filter)
    // Do something with the retrieved stands
  }
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
</script>

<template>
  <Fluid>
    <div class="planogram-create container">
      <h1>Create Cluster</h1>
      <Form v-slot="$form" :resolver="resolver" @submit="createLayout">
        <div class="flex flex-col md:flex-row gap-8">
          <div class="md:w-1/2">
            <div class="card flex flex-col gap-4">
              <div class="form-group">
                <label for="standType">Stand Type:</label>
                <Select
                  name="standType"
                  id="standType"
                  v-model="selectedStandTypeId"
                  :options="standTypes ?? []"
                  @change="onStandTypeChange"
                  option-label="name"
                  option-value="id"
                  placeholder="Select a stand type"
                  required
                />
              </div>

              <div class="flex flex-col gap-2">
                <label for="regions">Regions:</label>
                <MultiSelect
                  name="regions"
                  v-model="ms_selectedRegions"
                  :options="regions ?? []"
                  id="regions"
                  class="w-full"
                  option-label="name"
                  option-value="id"
                  placeholder="select a region"
                  @change="onRegionChange"
                >
                  <template #option="option">
                    <div class="flex align-items-center">
                      <span>{{ option.option.name }}</span>
                    </div>
                  </template>
                </MultiSelect>

                <Message
                  v-if="$form.regions?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.regions.error?.message ?? '&nbsp;' }}</Message
                >
              </div>
              <div class="flex flex-col gap-2">
                <label for="countries">Countries:</label>
                <MultiSelect
                  name="countries"
                  v-model="ms_selectedCountries"
                  :options="countrySelectList ?? []"
                  id="countries"
                  class="w-full"
                  option-label="name"
                  option-value="id"
                  @change="onCountryChange"
                  placeholder="select a country"
                  :selectAll="selectAllCountries"
                  @selectall-change="onSelectAllCountriesChange($event)"
                >
                  <template #option="option">
                    <div class="">
                      <span>{{ option.option.name }}</span>
                    </div>
                  </template>
                </MultiSelect>
                <Message
                  v-if="$form.countries?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.countries.error?.message ?? '&nbsp;' }}</Message
                >
              </div>

              <div class="form-group">
                <label for="stand">Stand:</label>
                <Select
                  name="stand"
                  id="stand"
                  v-model="selectedStandId"
                  :options="stands ?? []"
                  @change="onStandChange"
                  option-label="name"
                  option-value="id"
                  placeholder="Select a stand"
                  required
                />
              </div>

              <div class="form-group">
                <label for="planogramName">Cluster Name:</label>
                <InputText
                  name="clusterName"
                  id="clusterName"
                  type="text"
                  v-model="clusterName"
                  placeholder="Enter cluster name"
                />
                <Message
                  v-if="$form.clusterName?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.clusterName.error?.message ?? '&nbsp;' }}</Message
                >
              </div>
              <div class="form-group">
                <label for="layoutPartNumber">Part Number:</label>
                <InputText
                  name="layoutPartNumber"
                  id="layoutPartNumber"
                  type="text"
                  v-model="layoutPartNumber"
                  placeholder="Enter part number"
                />
                <Message
                  v-if="$form.layoutPartNumber?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.layoutPartNumber.error?.message ?? '&nbsp;' }}</Message
                >
              </div>

              <div class="flex gap-2 justify-between">
                <Button type="submit" class="w-60" :fluid="false">Create Cluster</Button>
                <!-- <Button
                  type="button"
                  class="w-50"
                  icon="pi pi-filter-slash"
                  label="Clear"
                  variant="outlined"
                  @click="clearFilters()"
                  :fluid="false"
                /> -->
              </div>
            </div>
          </div>
          <div class="md:w-1/2">
            <div class="card flex flex-col gap-4">
              <div class="font-semibold text-xl">Cluster Details</div>
              <div class="grid grid-cols-12 gap-2">
                <label
                  for="name3"
                  class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0 text-lg"
                  >Name</label
                >
                <div class="col-span-12 md:col-span-10">
                  <p class="font-bold text-lg">{{ clusterName ?? '' }}</p>
                </div>
              </div>
              <div class="grid grid-cols-12 gap-2">
                <label
                  for="email3"
                  class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0 text-lg"
                  >Countries</label
                >
                <div class="col-span-12 md:col-span-10">
                  <p v-for="country in countrySelectList" :key="country.id" class="mr-2 text-lg">
                    <span
                      v-if="
                        ms_selectedCountries != null && ms_selectedCountries.includes(country.id)
                      "
                      >{{ country.name ?? '' }}</span
                    >
                  </p>
                </div>
              </div>
              <div class="grid grid-cols-12 gap-2">
                <label
                  for="email3"
                  class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0 text-lg"
                  >Stand Type</label
                >
                <div class="col-span-12 md:col-span-10">
                  <p class="text-lg">{{ selectedStandType?.name ?? '' }}</p>
                </div>
              </div>
              <div class="grid grid-cols-12 gap-2">
                <label
                  for="email3"
                  class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0 text-lg"
                  >Stand</label
                >
                <div class="col-span-12 md:col-span-10">
                  <p class="text-lg">{{ selectedStand?.name ?? '' }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </Form>
    </div>
  </Fluid>
</template>
