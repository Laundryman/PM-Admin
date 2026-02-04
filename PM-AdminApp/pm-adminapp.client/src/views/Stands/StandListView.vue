<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
// import UserService from '@/services/UserService.js'
import { useLocationFilters } from '@/components/composables/locationFilters'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { searchStandInfo } from '@/models/Stands/searchStandInfo.model'
import { standFilter } from '@/models/Stands/standFilter.model'
import { default as countryService } from '@/services/Countries/CountryService'
import { default as standService } from '@/services/Stands/StandService'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'
import { FilterMatchMode } from '@primevue/core/api/'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'

const { regions, countries } = useLocationFilters()
const selectedRegion = ref()
const selectedCountry = ref()
const stands = ref<searchStandInfo[]>([])
const selectedStands = ref<searchStandInfo[]>([])
const toast = useToast()
const loading = ref(true)
const layout = useSystemStore()
const brandStore = useBrandStore()
const brand = storeToRefs(brandStore).activeBrand
const searchText = ref('')
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  standTypeName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
  parentStandTypeName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
})

watch(brand, async (newBrand) => {
  if (newBrand) {
    let filter = new standFilter()
    filter.brandId = newBrand.id
    await standService.searchStands(filter).then((response) => {
      stands.value = response
      console.log('Stands loaded for brand change', stands.value)
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
  await standService.initialise()
  await countryService.initialise()

  let brandid = brandStore.activeBrand?.id ?? 0

  let rFilter = new regionFilter()
  rFilter.brandId = brandid

  await useLocationFilters()
    .getRegions(rFilter)
    .then((response) => {
      regions.value = response
    })

  var filter = new standFilter()
  filter.brandId = brandid
  await standService.searchStands(filter).then((response) => {
    stands.value = response
    console.log('Stands loaded', stands.value)
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
    countries.value = await useLocationFilters().onRegionChange(selectedRegion.value)
    let filter = new standFilter()
    filter.brandId = layout.getActiveBrand?.id ?? 0
    filter.regionId = selectedRegion.value
    await standService.searchStands(filter).then((response) => {
      stands.value = response
      console.log('Stands loaded', stands.value)
    })
  } else {
    countries.value = []
  }
}

async function onCountryChange() {
  if (selectedCountry.value) {
    let filter = new standFilter()
    filter.brandId = layout.getActiveBrand?.id ?? 0
    filter.countryId = selectedCountry.value
    await standService.searchStands(filter).then((response) => {
      stands.value = response
      console.log('Stands loaded', stands.value)
    })
  } else {
    countries.value = []
  }
}

async function clearFilters() {
  selectedRegion.value = null
  selectedCountry.value = null
  countries.value = []
  let filter = new standFilter()
  filter.brandId = layout.getActiveBrand?.id ?? 0
  await standService.searchStands(filter).then((response) => {
    stands.value = response
    console.log('Stands loaded', stands.value)
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
    <h1>Product List View</h1>
    <!-- Product list content goes here -->
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
        v-model:selection="selectedStands"
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
        :value="stands"
        dataKey="id"
        :paginator="true"
        :rows="10"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rowsPerPageOptions="[5, 10, 25]"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} parts"
      >
        <template #header>
          <div class="flex flex-wrap gap-2 items-center justify-between">
            <h4 class="m-0">Manage Stands</h4>
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
        <Column
          field="parentStandTypeName"
          header="Parent StandType"
          filterField="parentStandTypeName"
          style="min-width: 16rem"
        >
          <template #filter="{ filterModel, filterCallback }">
            <InputText
              v-model="filterModel.value"
              type="text"
              @input="filterCallback()"
              placeholder="Search by parent stand type"
            />
          </template>
        </Column>
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
          field="standAssembleyNumber"
          header="Assembly Number"
          sortable
          style="min-width: 12rem"
        ></Column>

        <Column field="height" header="Height" sortable style="min-width: 16rem"></Column>
        <Column field="width" header="Width" sortable style="min-width: 12rem"></Column>
      </DataTable>
    </div>
  </div>
</template>
