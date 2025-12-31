<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
// import UserService from '@/services/UserService.js'
import { useLayoutStore } from '@/layout/composables/layout'
import { clusterFilter } from '@/models/Clusters/clusterFilter.model'
import { searchClusterInfo } from '@/models/Clusters/searchClusterInfo.model'
import { Country } from '@/models/Countries/country.model'
import { Region } from '@/models/Countries/region.model'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { default as clusterService } from '@/services/Clusters/ClusterService'
import { default as countryService } from '@/services/Countries/CountryService'
import { FilterMatchMode } from '@primevue/core/api/'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { useRouter } from 'vue-router'

const regions = ref<Region[]>([])
const selectedRegion = ref()
const selectedCountry = ref()
const countries = ref<Country[]>([])
const clusters = ref<searchClusterInfo[]>([])
const selectedClusters = ref<searchClusterInfo[]>([])
const toast = useToast()
const loading = ref(true)
const layout = useLayoutStore()
const brand = storeToRefs(layout).getActiveBrand
const searchText = ref('')
const router = useRouter()
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  standTypeName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
})

watch(brand, async (newBrand) => {
  if (newBrand) {
    let filter = new clusterFilter()
    filter.brandId = newBrand.id
    await clusterService.searchClusters(filter).then((response) => {
      clusters.value = response
      console.log('Clusters loaded for brand change', clusters.value)
    })

    let rFilter = new regionFilter()
    rFilter.brandId = newBrand.id
    await countryService.getRegions(rFilter).then((response) => {
      regions.value = response
      console.log('Regions loaded', regions.value)
    })
  }
})

onMounted(async () => {
  loading.value = true
  await clusterService.initialise()
  await countryService.initialise()

  let brandid = layout.getActiveBrand?.id ?? 0
  let rFilter = new regionFilter()
  rFilter.brandId = brandid
  await countryService.getRegions(rFilter).then((response) => {
    regions.value = response
    console.log('Regions loaded', regions.value)
  })

  var filter = new clusterFilter()
  filter.brandId = brandid
  await clusterService.searchClusters(filter).then((response) => {
    clusters.value = response
    console.log('Clusters loaded', clusters.value)
  })

  //   FilterService.register(part_FILTER.value, (value: any, filter: any) => {
  //     if (filter === undefined || filter === null || filter.trim() === '') {
  //       return true
  //     }

  //     if (value === undefined || value === null) {
  //       return false
  //     }

  //     return value.toString() === filter.toString()
  //   })
})

async function onRegionChange() {
  if (selectedRegion.value) {
    countryService.getCountries(selectedRegion.value, '').then((response) => {
      countries.value = response
      console.log('Countries loaded for region', selectedRegion.value, countries.value)
    })
    let filter = new clusterFilter()
    filter.brandId = layout.getActiveBrand?.id ?? 0
    filter.regionId = selectedRegion.value
    await clusterService.searchClusters(filter).then((response) => {
      clusters.value = response
      console.log('Clusters loaded', clusters.value)
    })
  } else {
    countries.value = []
  }
}

async function onCountryChange() {
  if (selectedCountry.value) {
    let filter = new clusterFilter()
    filter.brandId = layout.getActiveBrand?.id ?? 0
    filter.countryId = selectedCountry.value
    await clusterService.searchClusters(filter).then((response) => {
      clusters.value = response
      console.log('Clusters loaded', clusters.value)
    })
  } else {
    countries.value = []
  }
}

async function clearFilters() {
  selectedRegion.value = null
  selectedCountry.value = null
  countries.value = []
  let filter = new clusterFilter()
  filter.brandId = layout.getActiveBrand?.id ?? 0
  await clusterService.searchClusters(filter).then((response) => {
    clusters.value = response
    console.log('Clusters loaded', clusters.value)
  })
  let rFilter = new regionFilter()
  rFilter.brandId = layout.getActiveBrand?.id ?? 0
  await countryService.getRegions(rFilter).then((response) => {
    regions.value = response
    console.log('Regions loaded', regions.value)
  })
}

function editCluster(cluster: searchClusterInfo) {
  console.log('Edit cluster', cluster)
  layout.setActiveCluster(cluster)
  // Navigate to edit page
  router.push({ name: 'editCluster', params: { id: cluster.id } })
}
</script>

<template>
  <div>
    <h1>Product List View</h1>
    <!-- Product list content goes here -->
    <Toolbar class="mb-6">
      <template #start>
        <Select
          v-model="selectedRegion"
          :options="regions"
          @change="onRegionChange"
          option-label="name"
          option-value="id"
          placeholder="Select a region"
          class="mr-2"
        />

        <Select
          v-model="selectedCountry"
          :options="countries"
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
        v-model:selection="selectedClusters"
        v-model:filters="filters"
        :globalFilterFields="[
          //'categoryName',
          'name',
          'description',
          'partNumber',
          'partTypeName',
          'facings',
          'stock',
        ]"
        filterDisplay="row"
        :value="clusters"
        dataKey="id"
        :paginator="true"
        :rows="10"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rowsPerPageOptions="[5, 10, 25]"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} parts"
      >
        <template #header>
          <div class="flex flex-wrap gap-2 items-center justify-between">
            <h4 class="m-0">Manage Clusters</h4>
            <IconField>
              <InputIcon>
                <i class="pi pi-search" />
              </InputIcon>
              <InputText v-model="filters['global'].value" placeholder="Search..." />
            </IconField>
          </div>
        </template>
        <Column selectionMode="multiple" style="width: 3rem" :exportable="false"></Column>
        <Column field="name" header="Name" sortable style="min-width: 12rem"></Column>
        <Column field="standName" header="Stand Name" sortable style="min-width: 12rem"></Column>
        <Column
          field="standTypeName"
          header="StandType"
          filterField="standTypeName"
          style="min-width: 16rem"
        >
          <template #filter="{ filterModel, filterCallback }">
            <InputText
              v-model="filterModel.value"
              type="text"
              @input="filterCallback()"
              placeholder="Search by stand type"
            />
          </template>
        </Column>
        <Column
          field="standAssemblyNumber"
          header="Assembly Number"
          sortable
          style="min-width: 12rem"
        ></Column>

        <Column field="height" header="Height" sortable style="min-width: 16rem"></Column>
        <Column field="width" header="Width" sortable style="min-width: 12rem"></Column>
        <Column :exportable="false" style="min-width: 12rem">
          <template #body="slotProps">
            <Button
              icon="pi pi-pencil"
              variant="outlined"
              rounded
              class="mr-2"
              @click="editCluster(slotProps.data)"
            />
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>
