<script setup lang="ts">
import { useLocationFilters } from '@/components/composables/locationFilters'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { LayoutFilter } from '@/models/Layout/LayoutFilter.model'
import { LayoutInfo } from '@/models/Layout/searchLayoutInfo.model'
import { CreatePlanogramFilter } from '@/models/Planograms/createPlanogramFilter.model'
import { Stand } from '@/models/Stands/stand.model'
import { StandFilter } from '@/models/Stands/standFilter.model'
import { StandType } from '@/models/StandTypes/standType.model'
import { standTypeFilter } from '@/models/StandTypes/standTypeFilter.model'
import { default as countryService } from '@/services/Countries/CountryService'
import planogramService from '@/services/Planograms/PlanogramService'
import standService from '@/services/Stands/StandService'
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
const planogramName = ref('')
const selectedRegion = ref()
const selectedCountryId = ref()
const selectedCountry = ref()
const selectedStandTypeId = ref()
const selectedStandId = ref()
const selectedStand = ref<Stand | null>(null)
const selectedStandType = ref<StandType | null>(null)
const selectedLayoutId = ref()
const selectedLayout = ref<LayoutInfo | null>(null)
const layouts = ref<LayoutInfo[] | null>([])
const { regions, countries } = useLocationFilters()
const standTypes = ref<StandType[] | null>([])
const stands = ref<Stand[] | null>([]) // Replace 'any' with the appropriate type for stands
const resolver = ref(
  zodResolver(
    z.object({
      country: z.number({ message: 'Country is required.' }),
      planogramName: z.string().min(1, { message: 'Planogram name is required.' }),
      region: z.number({ message: 'Region is required.' }),
      standType: z.number({ message: 'Stand type is required.' }),
      stand: z.number({ message: 'Stand is required.' }),
    }),
  ),
)
onMounted(async () => {
  await countryService.initialise()

  let brandid = brandStore.activeBrand?.id ?? 0
  let rFilter = new regionFilter()
  rFilter.brandId = brandid
  await useLocationFilters()
    .getRegions(rFilter)
    .then((response) => {
      regions.value = response
    })
})
async function onRegionChange() {
  if (selectedRegion.value) {
    countries.value = await useLocationFilters().onRegionChange(selectedRegion.value)
  } else {
    countries.value = []
  }
}

async function onCountryChange() {
  if (selectedCountryId.value) {
    selectedCountry.value =
      countries.value?.find((c) => c.id === selectedCountryId.value)?.name ?? ''
    standTypes.value = await getStandTypes()
    console.log('Stand Types:', standTypes.value)
  }
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
  filter.countryId = selectedCountryId.value

  await standService.initialise()
  return await standService.getAllStandTypes(filter)
}

async function getStands() {
  // Implement the logic to get stands here
  if (selectedStandTypeId.value) {
    selectedStandType.value =
      standTypes.value?.find((st) => st.id === selectedStandTypeId.value) ?? null
    await standService.initialise()
    let filter = new StandFilter()
    filter.brandId = brandStore.activeBrand?.id
    filter.countryId = selectedCountryId.value
    filter.standTypeId = selectedStandTypeId.value as number
    stands.value = await standService.getStands(filter)
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
  await planogramService.initialise()
  await planogramService.getLayouts(filter).then((response) => {
    layouts.value = response
  })
}

function onLayoutChange() {
  if (selectedLayoutId.value) {
    selectedLayout.value = layouts.value?.find((l) => l.id === selectedLayoutId.value) ?? null
  }
}
async function createPlanogram({ valid }: any) {
  if (!valid) {
    return
  }

  // Implement the logic to create a planogram here
  let filter = new CreatePlanogramFilter()
  filter.name = planogramName.value
  filter.countryId = selectedCountryId.value
  filter.standTypeId = selectedStandTypeId.value as number
  filter.standId = selectedStandId.value as number
  filter.clusterId = selectedLayoutId.value as number
  filter.regionId = selectedRegion.value as number
  filter.brandId = brandStore.activeBrand?.id as number

  await planogramService.initialise()
  await planogramService.createPlanogram(filter).then((response) => {
    if (response) {
      router.push({ name: 'planogramDetails', params: { id: response.id } })
    }
  })
}
</script>

<template>
  <Fluid>
    <div class="planogram-create container">
      <h1>Create Planogram</h1>
      <Form v-slot="$form" :resolver="resolver" @submit="createPlanogram">
        <div class="flex flex-col md:flex-row gap-8">
          <div class="md:w-1/2">
            <div class="card flex flex-col gap-4">
              <div class="form-group">
                <label for="planogramName">Planogram Name:</label>
                <InputText
                  name="planogramName"
                  id="planogramName"
                  type="text"
                  v-model="planogramName"
                />
                <Message
                  v-if="$form.planogramName?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.planogramName.error?.message ?? '&nbsp;' }}</Message
                >
              </div>
              <div class="flex flex-col gap-2">
                <Select
                  name="region"
                  v-model="selectedRegion"
                  :options="regions ?? []"
                  @change="onRegionChange"
                  option-label="name"
                  option-value="id"
                  placeholder="Select a region"
                  class="mr-2"
                  fluid
                />
                <Message
                  v-if="$form.region?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.region.error?.message ?? '&nbsp;' }}</Message
                >
              </div>
              <div class="flex flex-col gap-2">
                <Select
                  name="country"
                  v-model="selectedCountryId"
                  :options="countries ?? []"
                  @change="onCountryChange"
                  option-label="name"
                  option-value="id"
                  placeholder="Select a country"
                  class="mr-2"
                  fluid
                />
                <Message
                  v-if="$form.country?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.country.error?.message ?? '&nbsp;' }}</Message
                >
              </div>
              <div class="form-group">
                <label for="standType">Stand Type:</label>
                <Select
                  name="standType"
                  id="standType"
                  v-model="selectedStandTypeId"
                  :options="standTypes ?? []"
                  @change="getStands"
                  option-label="name"
                  option-value="id"
                  placeholder="Select a stand type"
                  required
                />
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
                <label for="layout">Layout:</label>
                <Select
                  name="layout"
                  id="layout"
                  v-model="selectedLayoutId"
                  :options="layouts ?? []"
                  @change="onLayoutChange"
                  option-label="name"
                  option-value="id"
                  placeholder="Select a layout"
                  required
                />
              </div>

              <div class="flex gap-2 justify-between">
                <Button type="submit" severity="secondary" class="w-60" :fluid="false"
                  >Create Planogram</Button
                >
                <Button
                  type="button"
                  class="w-50"
                  icon="pi pi-filter-slash"
                  label="Clear"
                  variant="outlined"
                  @click="clearFilters()"
                  :fluid="false"
                />
              </div>
            </div>
          </div>
          <div class="md:w-1/2">
            <div class="card flex flex-col gap-4">
              <div class="font-semibold text-xl">New Planogram</div>
              <div class="grid grid-cols-12 gap-2">
                <label
                  for="name3"
                  class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0 text-lg"
                  >Name</label
                >
                <div class="col-span-12 md:col-span-10">
                  <p class="font-bold text-lg">{{ planogramName ?? '' }}</p>
                </div>
              </div>
              <div class="grid grid-cols-12 gap-2">
                <label
                  for="email3"
                  class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0 text-lg"
                  >Country</label
                >
                <div class="col-span-12 md:col-span-10">
                  <p class="text-lg">{{ selectedCountry ?? '' }}</p>
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
              <div class="grid grid-cols-12 gap-2">
                <label
                  for="email3"
                  class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0 text-lg"
                  >Layout</label
                >
                <div class="col-span-12 md:col-span-10">
                  <p class="text-lg">{{ selectedLayout?.name ?? '' }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </Form>
    </div>
  </Fluid>
</template>
