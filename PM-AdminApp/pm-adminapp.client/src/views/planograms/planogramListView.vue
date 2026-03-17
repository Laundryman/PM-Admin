<script setup lang="ts">
import { useLocationFilters } from '@/components/composables/locationFilters'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { PlanogramFilter } from '@/models/Planograms/planogramFilter.model'
import type { searchPlanogramInfo } from '@/models/Planograms/searchPlanogramnfo.model'
import { default as countryService } from '@/services/Countries/CountryService'
import { default as planogramService } from '@/services/Planograms/PlanogramService'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'
import { FilterMatchMode } from '@primevue/core/api/'
import { storeToRefs } from 'pinia'
import { onMounted, ref, watch } from 'vue'

const layout = useSystemStore()
const brandStore = useBrandStore()
const loading = ref(false)
const planogramFilter = ref<PlanogramFilter>(new PlanogramFilter())
const { regions, countries } = useLocationFilters()
const planograms = ref<searchPlanogramInfo[]>([])
const selectedPlanograms = ref<searchPlanogramInfo[]>([])
const selectedRegion = ref()
const selectedCountry = ref()

const brand = storeToRefs(brandStore).activeBrand

const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  name: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
  standTypeName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
  statusName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },

  locked: { value: null, matchMode: FilterMatchMode.EQUALS },
})
const statuses = ref([
  'Edit',
  'Submitted',
  'Ordered',
  'Deleted',
  'Approved',
  'Validated',
  'Archived',
])
watch(brand, async (newBrand) => {
  if (newBrand) {
    let filter = new PlanogramFilter()
    filter.brandId = newBrand.id
    await planogramService.searchPlanograms(filter).then((response) => {
      planograms.value = response
      console.log('Planograms loaded for brand change', planograms.value)
    })
    let rFilter = new regionFilter()
    rFilter.brandId = newBrand.id
    useLocationFilters()
      .getRegions(rFilter)
      .then((response) => {
        regions.value = response
      })
  }
})

onMounted(async () => {
  loading.value = true
  layout.layoutState.disableBrandSelect = false
  await planogramService.initialise()
  await countryService.initialise()

  let brandid = brandStore.activeBrand?.id ?? 0
  let rFilter = new regionFilter()
  rFilter.brandId = brandid
  await useLocationFilters()
    .getRegions(rFilter)
    .then((response) => {
      regions.value = response
    })

  var filter = new PlanogramFilter()
  filter.brandId = brandid
  await planogramService
    .searchPlanograms(filter)
    .then((response) => {
      planograms.value = response
      console.log('Planograms loaded', planograms.value)
    })
    .catch((error) => {
      console.error('Error loading planograms', error)
    })
  loading.value = false
})

async function onRegionChange() {
  if (selectedRegion.value) {
    countries.value = await useLocationFilters().onRegionChange(selectedRegion.value)
    let filter = new PlanogramFilter()
    filter.brandId = brandStore.activeBrand?.id ?? 0

    filter.regionId = selectedRegion.value
    await planogramService.searchPlanograms(filter).then((response) => {
      planograms.value = response
      console.log('Planograms loaded', planograms.value)
    })
  } else {
    countries.value = []
  }
}

async function onCountryChange() {
  if (selectedCountry.value) {
    let filter = new PlanogramFilter()
    filter.brandId = brandStore.activeBrand?.id ?? 0
    filter.countryId = selectedCountry.value
    await planogramService.searchPlanograms(filter).then((response) => {
      planograms.value = response
      console.log('Planograms loaded', planograms.value)
    })
  } else {
    countries.value = []
  }
}

// function getStatusText(statusId: number): string {
//   switch (statusId) {
//     case 1:
//       return 'In Progress'
//     case 2:
//       return 'Submitted'
//     case 3:
//       return 'Ordered'
//     case 4:
//       return 'Deleted'
//     case 5:
//       return 'Approved'
//     case 6:
//       return 'Validated'
//     case 7:
//       return 'Archived'
//     default:
//       return 'Unknown'
//   }
// }

function getStatusSeverity(status: string): string {
  switch (status.toLowerCase()) {
    case 'edit':
      return 'info'
    case 'submitted':
      return 'warn'
    case 'ordered':
      return 'info'
    case 'deleted':
      return 'danger'
    case 'approved':
      return 'success'
    case 'validated':
      return 'secondary'
    case 'archived':
      return 'success'
    default:
      return 'danger'
  }
}

async function clearFilters() {
  selectedRegion.value = null
  selectedCountry.value = null
  countries.value = []
  let filter = new PlanogramFilter()
  filter.brandId = brandStore.activeBrand?.id ?? 0
  await planogramService.searchPlanograms(filter).then((response) => {
    planograms.value = response
    console.log('Planograms loaded', planograms.value)
  })
  let rFilter = new regionFilter()
  rFilter.brandId = brandStore.activeBrand?.id ?? 0
  await countryService.getRegions(rFilter).then((response) => {
    regions.value = response
    console.log('Regions loaded', regions.value)
  })
}

function unlock(planogram: searchPlanogramInfo) {
  planogramService
    .unlockPlanogram(planogram.id)
    .then(() => {
      console.log('Planogram unlocked', planogram)
      // Refresh the planogram list after unlocking
      planograms.value = planograms.value.map((p) =>
        p.id === planogram.id ? { ...p, locked: false } : p,
      )
      // clearFilters()
    })
    .catch((error) => {
      console.error('Error unlocking planogram', error)
    })
  console.log('Unlock planogram', planogram)
  // layout.setActivePart(part)
  // Navigate to edit page
}
</script>

<template>
  <div class="planogram-list-view">
    <h1>Planogram List View</h1>
    <!-- Add your planogram list view content here -->
    <Toolbar class="mb-6">
      <template #start>
        <Select
          v-model="selectedRegion"
          :options="regions ?? []"
          @change="onRegionChange"
          option-label="name"
          option-value="id"
          placeholder="Select a region"
          class="mr-2"
        />

        <Select
          v-model="selectedCountry"
          :options="countries ?? []"
          @change="onCountryChange"
          option-label="name"
          option-value="id"
          placeholder="Select a country"
          class="mr-2"
        />

        <Button
          type="button"
          icon="pi pi-filter-slash"
          label="Clear"
          variant="outlined"
          @click="clearFilters()"
        />
      </template>

      <template #end> </template>
    </Toolbar>
    <div class="card">
      <DataTable
        ref="dt"
        v-model:filters="filters"
        :value="planograms"
        :globalFilterFields="[
          'name',
          'standTypeName',
          'statusName',
          'locked',
          'countryName',
          'regionName',
          'lubName',
        ]"
        showGridlines
        filterDisplay="menu"
        :paginator="true"
        :rows="10"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rowsPerPageOptions="[5, 10, 25]"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} planograms"
      >
        <template #header>
          <div class="flex flex-wrap gap-2 items-center justify-between">
            <h4 class="m-0">Manage Planograms</h4>
            <IconField>
              <InputIcon>
                <i class="pi pi-search" />
              </InputIcon>
              <InputText v-model="filters['global'].value" placeholder="Search..." />
            </IconField>
          </div>
        </template>

        <Column field="name" header="Name" sortable style="min-width: 12rem">
          <template #body="{ data }">
            <span class="font-bold">{{ data.name }}</span>
          </template>
          <template #filter="{ filterModel, filterCallback }">
            <InputText
              v-model="filterModel.value"
              type="text"
              @input="filterCallback()"
              placeholder="Search by planogram name"
            />
          </template>
        </Column>
        <Column field="standTypeName" header="Stand Type" sortable style="min-width: 6rem">
          <template #filter="{ filterModel, filterCallback }">
            <InputText
              v-model="filterModel.value"
              type="text"
              @input="filterCallback()"
              placeholder="Search by stand type"
            />
          </template>
        </Column>
        <Column field="statusName" header="Status" :showFilterMenu="false" style="min-width: 6rem">
          <template #body="{ data }">
            <Tag :value="data.statusName" :severity="getStatusSeverity(data.statusName)" />
          </template>
          <template #filter="{ filterModel, filterCallback }">
            <Select
              v-model="filterModel.value"
              @change="filterCallback()"
              :options="statuses"
              placeholder="Select One"
              style="min-width: 12rem"
              :showClear="true"
            >
              <template #option="slotProps">
                <Tag :value="slotProps.option" :severity="getStatusSeverity(slotProps.option)" />
              </template>
            </Select>
          </template>
        </Column>
        <Column field="dateCreated" header="Created At" sortable style="min-width: 6rem">
          <template #body="slotProps">
            {{ new Date(slotProps.data.dateCreated).toLocaleDateString() }}
          </template></Column
        >
        <Column field="dateUpdated" header="Last Updated" sortable style="min-width: 6rem">
          <template #body="slotProps">
            {{ new Date(slotProps.data.dateUpdated).toLocaleDateString() }}
          </template>
        </Column>
        <Column field="locked" header="Locked" datatype="boolean" sortable style="min-width: 6rem">
          <template #body="{ data }">
            <i
              class="pi"
              :class="{
                'pi-check-circle text-green-500': !data.locked,
                'pi-times-circle text-red-400': data.locked,
              }"
            ></i>
          </template>
          <template #filter="{ filterModel }">
            <Checkbox
              v-model="filterModel.value"
              :indeterminate="filterModel.value === null"
              binary
            />
          </template>
        </Column>
        <Column field="countryName" header="Country" sortable style="min-width: 6rem"></Column>
        <Column field="regionName" header="Region" sortable style="min-width: 6rem"></Column>
        <Column field="lubName" header="Last Updated By" sortable style="min-width: 6rem">
          <template #filter="{ filterModel, filterCallback }">
            <InputText
              v-model="filterModel.value"
              type="text"
              @input="filterCallback()"
              placeholder="Search by last updated by"
            />
          </template>
        </Column>
        <Column :exportable="false" style="min-width: 12rem">
          <template #body="slotProps">
            <Button
              v-if="slotProps.data.locked"
              v-tooltip="'Unlock Planogram'"
              icon="pi pi-lock-open"
              variant="outlined"
              rounded
              class="mr-2"
              @click="unlock(slotProps.data)"
            />
            <!-- <Button
              v-tooltip="'Copy Planogram'"
              icon="pi pi-copy"
              variant="outlined"
              rounded
              class="mr-2"
              @click="copyPart(slotProps.data)"
            /> -->
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>
