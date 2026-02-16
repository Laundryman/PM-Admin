<script setup lang="ts">
import { useLocationFilters } from '@/components/composables/locationFilters'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { JobFolderFilter } from '@/models/Jobs/JobFolderFilter.model'

import { default as countryService } from '@/services/Countries/CountryService'
import { default as jobsService } from '@/services/Jobs/JobsService'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'
import { FilterMatchMode } from '@primevue/core/api/'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'

import { onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'

const jobFolderFilter = ref(new JobFolderFilter())
const jobFolders = ref()
const selectedJobFolders = ref()
const selectedRegion = ref()
const selectedCountry = ref()

const router = useRouter()
const { regions, countries } = useLocationFilters()
const toast = useToast()
const loading = ref(true)
const layout = useSystemStore()
const brandStore = useBrandStore()
const brand = storeToRefs(brandStore).activeBrand

const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  name: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
  description: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
})

watch(brand, async (newBrand) => {
  if (newBrand) {
    let filter = new JobFolderFilter()
    filter.brandId = newBrand.id
    await jobsService.searchJobFolders(filter).then((response) => {
      jobFolders.value = response
      console.log('Job Folders loaded for brand change', jobFolders.value)
    })
    let rFilter = new regionFilter()
    rFilter.brandId = newBrand.id
    await useLocationFilters()
      .getRegions(rFilter)
      .then((response) => {
        regions.value = response
      })
  }
})

onMounted(async () => {
  loading.value = true
  layout.layoutState.disableBrandSelect = false
  await jobsService.initialise()
  await countryService.initialise()

  let brandid = brandStore.activeBrand?.id ?? 0

  let rFilter = new regionFilter()
  rFilter.brandId = brandid

  await useLocationFilters()
    .getRegions(rFilter)
    .then((response) => {
      regions.value = response
    })

  var filter = new JobFolderFilter()
  filter.brandId = brandid
  await jobsService.searchJobFolders(filter).then((response) => {
    jobFolders.value = response
    console.log('Job Folders loaded', jobFolders.value)
  })
})

async function onRegionChange() {
  if (selectedRegion.value) {
    countries.value = await useLocationFilters().onRegionChange(selectedRegion.value)
    let filter = new JobFolderFilter()
    filter.brandId = layout.getActiveBrand?.id ?? 0
    filter.regionId = selectedRegion.value
    await jobsService.searchJobFolders(filter).then((response) => {
      jobFolders.value = response
      console.log('Job Folders loaded', jobFolders.value)
    })
  } else {
    countries.value = []
  }
}

async function onCountryChange() {
  if (selectedCountry.value) {
    let filter = new JobFolderFilter()
    filter.brandId = layout.getActiveBrand?.id ?? 0
    filter.countryId = selectedCountry.value
    await jobsService.searchJobFolders(filter).then((response) => {
      jobFolders.value = response
      console.log('Job Folders loaded', jobFolders.value)
    })
  } else {
    countries.value = []
  }
}

async function clearFilters() {
  selectedRegion.value = null
  selectedCountry.value = null
  countries.value = []
  let filter = new JobFolderFilter()
  filter.brandId = layout.getActiveBrand?.id ?? 0
  await jobsService.searchJobFolders(filter).then((response) => {
    jobFolders.value = response
    console.log('Job Folders loaded', jobFolders.value)
  })
  let rFilter = new regionFilter()
  rFilter.brandId = filter.brandId
  await countryService.getRegions(rFilter).then((response) => {
    regions.value = response
    console.log('Regions loaded', regions.value)
  })
}
</script>

<template>
  <div>
    <h1>JobFolders View</h1>
    <!-- JobFolders list content goes here -->
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

        <!-- <Button label="Clear" icon="pi pi-filter" @click="clearFilters" /> -->
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
        v-model:selection="selectedJobFolders"
        v-model:filters="filters"
        :globalFilterFields="[
          //'categoryName',
          'name',
          'description',
          'regionName',
        ]"
        filterDisplay="row"
        :value="jobFolders"
        dataKey="id"
        :paginator="true"
        :rows="10"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rowsPerPageOptions="[5, 10, 25]"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} job folders"
      >
      </DataTable>
    </div>
  </div>
</template>
