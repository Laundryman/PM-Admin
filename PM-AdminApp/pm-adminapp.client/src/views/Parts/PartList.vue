<script setup lang="ts">
;``
import { onMounted, ref, watch } from 'vue'
// import UserService from '@/services/UserService.js'
import { useLayoutStore } from '@/layout/composables/layout'
import { Country } from '@/models/Countries/country.model'
import { Region } from '@/models/Countries/region.model'
import { PartFilter } from '@/models/Parts/partFilter.model'
import { SearchPartInfo } from '@/models/Parts/searchPartInfo.model'
import { default as countryService } from '@/services/Countries/CountryService'
import { default as partService } from '@/services/Parts/partService'
import { FilterMatchMode } from '@primevue/core/api/'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'

const regions = ref<Region[]>([])
const selectedRegion = ref()
const selectedCountry = ref()
const countries = ref<Country[]>([])
const parts = ref<SearchPartInfo[]>([])
const selectedParts = ref<SearchPartInfo[]>([])
const toast = useToast()
const loading = ref(true)
const layout = useLayoutStore()
const brand = storeToRefs(layout).getActiveBrand
const searchText = ref('')
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  categoryName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
})

watch(brand, async (newBrand) => {
  if (newBrand) {
    let filter = new PartFilter()
    filter.brandId = newBrand.id
    await partService.searchParts(filter).then((response) => {
      parts.value = response
      console.log('Parts loaded for brand change', parts.value)
    })

    await countryService.getRegions(newBrand.id, '').then((response) => {
      regions.value = response
      console.log('Regions loaded', regions.value)
    })
  }
})

onMounted(async () => {
  loading.value = true
  await partService.initialise()
  await countryService.initialise()

  let brandid = layout.getActiveBrand?.id ?? 0

  await countryService.getRegions(brandid, '').then((response) => {
    regions.value = response
    console.log('Regions loaded', regions.value)
  })

  var filter = new PartFilter()
  filter.brandId = brandid
  await partService.searchParts(filter).then((response) => {
    parts.value = response
    console.log('Parts loaded', parts.value)
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
    let filter = new PartFilter()
    filter.brandId = layout.getActiveBrand?.id ?? 0
    filter.regionId = selectedRegion.value
    await partService.searchParts(filter).then((response) => {
      parts.value = response
      console.log('Parts loaded', parts.value)
    })
  } else {
    countries.value = []
  }
}

async function onCountryChange() {
  if (selectedCountry.value) {
    let filter = new PartFilter()
    filter.brandId = layout.getActiveBrand?.id ?? 0
    filter.countryId = selectedCountry.value
    await partService.searchParts(filter).then((response) => {
      parts.value = response
      console.log('Parts loaded', parts.value)
    })
  } else {
    countries.value = []
  }
}

async function clearFilters() {
  selectedRegion.value = null
  selectedCountry.value = null
  countries.value = []
  let filter = new PartFilter()
  filter.brandId = layout.getActiveBrand?.id ?? 0
  await partService.searchParts(filter).then((response) => {
    parts.value = response
    console.log('Parts loaded', parts.value)
  })
  await countryService.getRegions(filter.brandId, '').then((response) => {
    regions.value = response
    console.log('Regions loaded', regions.value)
  })
}
</script>

<template>
  <div>
    <h1>Part List View</h1>
    <!-- Part list content goes here -->
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
        v-model:selection="selectedParts"
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
        :value="parts"
        dataKey="id"
        :paginator="true"
        :rows="10"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rowsPerPageOptions="[5, 10, 25]"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} parts"
      >
        <template #header>
          <div class="flex flex-wrap gap-2 items-center justify-between">
            <h4 class="m-0">Manage Parts</h4>
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
        <Column field="description" header="Description" sortable style="min-width: 16rem"></Column>
        <Column field="partNumber" header="Part Number" sortable style="min-width: 12rem"></Column>
        <Column
          field="categoryName"
          header="Category"
          filterField="categoryName"
          style="min-width: 16rem"
        >
          <template #filter="{ filterModel, filterCallback }">
            <InputText
              v-model="filterModel.value"
              type="text"
              @input="filterCallback()"
              placeholder="Search by country"
            />
          </template>
        </Column>
        <Column field="partTypeName" header="Part Type" sortable style="min-width: 12rem"></Column>
        <Column field="facings" header="Facing" sortable style="min-width: 16rem"></Column>
        <Column field="stock" header="Stock" sortable style="min-width: 12rem"></Column>
      </DataTable>
    </div>
  </div>
</template>
