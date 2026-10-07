<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
// import UserService from '@/services/UserService.js'
import { useLocationFilters } from '@/components/composables/locationFilters'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { searchStandInfo } from '@/models/Stands/searchStandInfo.model'
import { StandFilter } from '@/models/Stands/standFilter.model'
import { default as countryService } from '@/services/Countries/CountryService'
import { default as standService } from '@/services/Stands/StandService'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'
import { FilterMatchMode } from '@primevue/core/api/'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { useRouter } from 'vue-router'

const router = useRouter()
const { regions, countries } = useLocationFilters()
const selectedRegion = ref()
const selectedCountry = ref()
const selectedStandType = ref()
const selectedParentStandType = ref()
const standTypes = ref<{ label: string; value: string }[]>([])
const parentStandTypes = ref<{ label: string; value: string }[]>([])
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
    let filter = new StandFilter()
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

  var filter = new StandFilter()
  filter.brandId = brandid
  await standService.searchStands(filter).then(async (response) => {
    stands.value = response
    // await getParentStandTypesFromStands(stands.value)
    await getStandTypesFromStands(stands.value)
    console.log('Stands loaded', stands.value)
  })
  loading.value = false
})

async function onRegionChange() {
  if (selectedRegion.value) {
    countries.value = await useLocationFilters().onRegionChange(selectedRegion.value)
    let filter = new StandFilter()
    filter.brandId = brandStore.activeBrand?.id ?? 0
    filter.regionId = selectedRegion.value
    loading.value = true
    await standService.searchStands(filter).then((response) => {
      stands.value = response
      console.log('Stands loaded', stands.value)
    })
    loading.value = false
  } else {
    countries.value = []
  }
}

async function onCountryChange() {
  if (selectedCountry.value) {
    let filter = new StandFilter()
    filter.brandId = brandStore.activeBrand?.id ?? 0
    filter.countryId = selectedCountry.value
    loading.value = true
    await standService.searchStands(filter).then((response) => {
      stands.value = response
      console.log('Stands loaded', stands.value)
    })
    loading.value = false
  } else {
    countries.value = []
  }
}

async function clearFilters() {
  selectedRegion.value = null
  selectedCountry.value = null
  selectedStandType.value = null
  countries.value = []
  let filter = new StandFilter()
  filter.brandId = brandStore.activeBrand?.id ?? 0
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

  filters.value.standTypeName.value = null
}

function onStandTypeChange() {
  if (selectedStandType.value) {
    filters.value.standTypeName.value = selectedStandType.value ?? null
  }
}

function onParentStandTypeChange() {
  if (selectedParentStandType.value) {
    filters.value.parentStandTypeName.value = selectedParentStandType.value ?? null
  }
}

async function getStandTypesFromStands(standsList: searchStandInfo[]) {
  // Implement the logic to set categories from parts
  for (var stn of standsList) {
    let cat = { label: stn.standTypeName, value: stn.standTypeName }
    if (cat.label != null) {
      if (standTypes.value !== null && standTypes.value !== undefined) {
        if (!standTypes.value.some((c: { value: string }) => c.value === cat.value)) {
          standTypes.value.push(cat)
        }
      } else {
        standTypes.value = [cat]
      }
    }
  }
  standTypes.value.sort((a, b) => a.label.localeCompare(b.label))
}

async function getParentStandTypesFromStands(standsList: searchStandInfo[]) {
  // Implement the logic to set categories from parts
  for (var stn of standsList) {
    let cat = { label: stn.parentStandTypeName, value: stn.parentStandTypeName }
    if (cat.label != null) {
      if (parentStandTypes.value !== null && parentStandTypes.value !== undefined) {
        if (!parentStandTypes.value.some((c: { value: string }) => c.value === cat.value)) {
          parentStandTypes.value.push(cat)
        }
      } else {
        parentStandTypes.value = [cat]
      }
    }
  }
  parentStandTypes.value.sort((a, b) => a.label.localeCompare(b.label))
}

function editStand(stand: searchStandInfo) {
  console.log('Edit stand', stand)
  // layout.setActiveStand(stand) --- IGNORE ---
  // Navigate to edit page
  router.push({ name: 'editStand', params: { id: stand.id } })
}

function deleteStand(stand: searchStandInfo) {
  console.log('Delete stand', stand)
  // Call the service to delete the stand
  standService.deleteStand(stand.id).then(() => {
    // Remove the deleted stand from the list
    stands.value = stands.value.filter((s) => s.id !== stand.id)
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: 'Stand deleted successfully',
      group: 'center',
    })
  })
}
function openNew() {
  router.push({ name: 'newStand' })
}

function copyStand(stand: searchStandInfo) {
  router.push({ name: 'copyStand', params: { id: stand.id } })
}
</script>

<template>
  <div>
    <h1>Stand List View</h1>
    <div class="w-full sticky top-16 block z-10 bg-slate-50">
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

          <!-- <Select
          v-model="selectedParentStandType"
          :options="parentStandTypes ?? []"
          @change="onParentStandTypeChange"
          option-label="label"
          option-value="label"
          placeholder="Select a parent stand type"
          class="mr-2"
        /> -->

          <Select
            v-model="selectedStandType"
            :options="standTypes ?? []"
            @change="onStandTypeChange"
            option-label="label"
            option-value="label"
            placeholder="Select a stand type"
            class="mr-2"
          />

          <Button
            type="button"
            icon="pi pi-filter-slash"
            label="Clear"
            variant="outlined"
            @click="clearFilters()"
            v-tooltip="'Clear filters'"
          />
        </template>

        <template #end>
          <Button
            label="New"
            icon="pi pi-plus"
            severity="primary"
            class="mr-2"
            @click="openNew"
            v-tooltip.left="'Create a stand'"
          />
        </template>
      </Toolbar>
    </div>
    <div class="card">
      <DataTable
        ref="dt"
        v-model:selection="selectedStands"
        v-model:filters="filters"
        :loading="loading"
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
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} stands"
        scrollable
        scrollHeight="calc(70vh - 130px)"
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
        <Column field="name" header="Name" sortable style="min-width: 12rem"></Column>
        <Column
          field="parentStandTypeName"
          header="Parent StandType"
          filterField="parentStandTypeName"
          style="min-width: 16rem"
        >
          <!-- <template #filter="{ filterModel, filterCallback }">
            <InputText
              v-model="filterModel.value"
              type="text"
              @input="filterCallback()"
              placeholder="Search by parent stand type"
            />
          </template> -->
        </Column>
        <Column
          field="standTypeName"
          header="StandType"
          filterField="standTypeName"
          style="min-width: 16rem"
        >
          <!-- <template #filter="{ filterModel, filterCallback }">
            <InputText
              v-model="filterModel.value"
              type="text"
              @input="filterCallback()"
              placeholder="Search by stand type"
            />
          </template> -->
        </Column>
        <Column
          field="standAssemblyNumber"
          header="Assembly Number"
          sortable
          style="min-width: 12rem"
        ></Column>
        <Column field="height" header="Height" sortable style="min-width: 4rem"></Column>
        <Column field="width" header="Width" sortable style="min-width: 4rem"></Column>

        <Column field="dateCreated" header="Date Created" sortable style="min-width: 4rem">
          <template #body="slotProps">
            {{ new Date(slotProps.data.dateCreated).toLocaleDateString() }}
          </template></Column
        >
        <Column field="dateUpdated" header="Date Updated" sortable style="min-width: 4rem">
          <template #body="slotProps">
            {{ new Date(slotProps.data.dateUpdated).toLocaleDateString() }}
          </template>
        </Column>

        <Column
          header="Actions"
          :exportable="false"
          style="min-width: 4rem"
          :frozen="true"
          alignFrozen="right"
        >
          <template #body="slotProps">
            <div class="flex gap-2 justify-center">
              <Button
                v-tooltip="'Edit Stand'"
                icon="pi pi-pencil"
                variant="outlined"
                rounded
                class="mr-2"
                @click="editStand(slotProps.data)"
              />
              <Button
                v-tooltip="'Delete Stand'"
                icon="pi pi-trash"
                severity="danger"
                variant="outlined"
                rounded
                class="mr-2"
                @click="deleteStand(slotProps.data)"
              />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
  <Toast position="center" group="center" />
</template>
