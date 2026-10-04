<script setup lang="ts">
;``
import { onMounted, ref, watch } from 'vue'
// import UserService from '@/services/UserService.js'
import { useLocationFilters } from '@/components/composables/locationFilters'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { PartFilter } from '@/models/Parts/partFilter.model'
import { SearchPartInfo } from '@/models/Parts/searchPartInfo.model'
import { default as countryService } from '@/services/Countries/CountryService'
import { partService } from '@/services/Parts/partService'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'
import CheckCircle from '@primeicons/vue/check-circle'
import TimesCircle from '@primeicons/vue/times-circle'
import { FilterMatchMode } from '@primevue/core/api/'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { useRouter } from 'vue-router'

const { regions, countries } = useLocationFilters()
const selectedRegion = ref()
const selectedCountry = ref()
const selectedCategory = ref()
const categories = ref<{ label: string; value: number }[]>([])
const selectedPart = ref<SearchPartInfo | null>(null)
const parts = ref<SearchPartInfo[]>([])
// const selectedParts = ref<SearchPartInfo[]>([])
const toast = useToast()
const loading = ref(true)
const layout = useSystemStore()
const brandStore = useBrandStore()
const brand = storeToRefs(brandStore).activeBrand
const searchText = ref('')
const router = useRouter()
const showPublished = ref(true)
const showUnPublished = ref(true)
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  categoryName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
  published: { value: null, matchMode: FilterMatchMode.EQUALS },
})

watch(brand, async (newBrand) => {
  if (newBrand) {
    let filter = new PartFilter()
    filter.brandId = newBrand.id
    await partService.searchParts(filter).then((response) => {
      parts.value = response
      console.log('Parts loaded for brand change', parts.value)
    })
    let rFilter = new regionFilter()
    rFilter.brandId = newBrand.id
    await countryService.getRegions(rFilter).then((response) => {
      regions.value = response
    })
  }
})

onMounted(async () => {
  loading.value = true
  layout.layoutState.disableBrandSelect = false
  await partService.initialise()
  await countryService.initialise()

  let brandid = brandStore.activeBrand?.id ?? 0
  // let brandid = layout.getActiveBrand?.id ?? 0
  let rFilter = new regionFilter()
  rFilter.brandId = brandid
  await useLocationFilters()
    .getRegions(rFilter)
    .then((response) => {
      regions.value = response
    })

  var filter = new PartFilter()
  filter.brandId = brandid
  await partService.searchParts(filter).then(async (response) => {
    parts.value = response
    await getCategoriesFromParts(parts.value)
    loading.value = false
  })
})

async function getCategoriesFromParts(partsList: SearchPartInfo[]) {
  // Implement the logic to set categories from parts
  for (var part of partsList) {
    let cat = { label: part.categoryName, value: part.categoryId }
    if (categories.value !== null && categories.value !== undefined) {
      if (!categories.value.some((c: { value: number }) => c.value === cat.value)) {
        categories.value.push(cat)
      }
    } else {
      categories.value = [cat]
    }
  }
  categories.value.sort((a, b) => a.label.localeCompare(b.label))
}

async function onRegionChange() {
  if (selectedRegion.value) {
    countries.value = await useLocationFilters().onRegionChange(selectedRegion.value)
    let filter = new PartFilter()
    filter.brandId = brandStore.activeBrand?.id ?? 0
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
    filter.brandId = brandStore.activeBrand?.id ?? 0
    filter.countryId = selectedCountry.value
    await partService.searchParts(filter).then((response) => {
      parts.value = response
      console.log('Parts loaded', parts.value)
    })
  } else {
    countries.value = []
  }
}

function onCategoryChange() {
  if (selectedCategory.value) {
    filters.value.categoryName.value = selectedCategory.value ?? null
  }
}

async function clearFilters() {
  selectedRegion.value = null
  selectedCountry.value = null
  selectedCategory.value = null
  countries.value = []
  let filter = new PartFilter()
  filter.brandId = brandStore.activeBrand?.id ?? 0
  await partService.searchParts(filter).then((response) => {
    parts.value = response
    console.log('Parts loaded', parts.value)
  })
  let rFilter = new regionFilter()
  rFilter.brandId = brandStore.activeBrand?.id ?? 0
  await countryService.getRegions(rFilter).then((response) => {
    regions.value = response
    console.log('Regions loaded', regions.value)
  })

  filters.value.categoryName.value = null
}

function editPart(part: SearchPartInfo) {
  console.log('Edit part', part)
  // layout.setActivePart(part)
  // Navigate to edit page
  router.push({ name: 'editPart', params: { id: part.id } })
}

function openNew() {
  router.push({ name: 'newPart' })
}

function copyPart(part: SearchPartInfo) {
  router.push({ name: 'copyPart', params: { id: part.id } })
}

function onShowPublishedChange() {
  if (showPublished.value) {
    if (showUnPublished.value) {
      filters.value.published.value = null
    } else {
      filters.value.published.value = true
    }
  } else {
    filters.value.published.value = false
    showUnPublished.value = true
  }
  console.log('Show Published changed', showPublished.value)
}

function onShowUnPublishedChange() {
  if (showUnPublished.value) {
    if (showPublished.value) {
      filters.value.published.value = null
    } else {
      filters.value.published.value = false
    }
  } else {
    filters.value.published.value = true
    showPublished.value = true
  }
  console.log('Show Unpublished changed', showUnPublished.value)
}
function deletePart(part: SearchPartInfo) {
  if (confirm(`Are you sure you want to delete the part "${part.name}"?`)) {
    partService
      .deletePart(part.id)
      .then((response) => {
        toast.add({
          severity: 'success',
          summary: 'Part Deleted',
          detail: 'Part deleted successfully.',
          life: 3000,
        })
        // // Refresh the part list after deletion
        // let filter = new PartFilter()
        // filter.brandId = brandStore.activeBrand?.id ?? 0
        // partService.searchParts(filter).then((response) => {
        //   parts.value = response
        // })

        //remove deleted part from the parts list
        parts.value = parts.value.filter((p) => p.id !== part.id)
      })
      .catch((error) => {
        toast.add({
          severity: 'error',
          summary: 'Error Deleting Part',
          detail: 'An error occurred while deleting the part.',
          life: 3000,
        })
      })
  }
}
</script>

<template>
  <div class="part-list-view">
    <h1>Part List View</h1>
    <!-- Part list content goes here -->
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

          <Select
            v-model="selectedCategory"
            :options="categories ?? []"
            @change="onCategoryChange"
            option-label="label"
            option-value="label"
            placeholder="Select a category"
            class="mr-2"
          />

          <ToggleButton
            v-model="showPublished"
            onLabel="Show Published"
            offLabel="Show Published"
            class="min-w-16 mr-2"
            @change="onShowPublishedChange"
          >
            <template #icon="{ value }">
              <CheckCircle class="text-green-500" v-if="value" />
              <TimesCircle class="text-red-500" v-else />
            </template>
          </ToggleButton>
          <ToggleButton
            v-model="showUnPublished"
            onLabel="Show Unpublished"
            offLabel="Show Unpublished"
            class="min-w-29 mr-2"
            @change="onShowUnPublishedChange"
          >
            <template #icon="{ value }">
              <CheckCircle class="text-green-500" v-if="value" />
              <TimesCircle class="text-red-500" v-else />
            </template>
          </ToggleButton>

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
          <IconField>
            <InputIcon>
              <i class="pi pi-search" />
            </InputIcon>
            <InputText v-model="filters['global'].value" placeholder="Search..." />
          </IconField>

          <Button
            label="New"
            icon="pi pi-plus"
            severity="primary"
            class="mr-2"
            @click="openNew"
            v-tooltip="'Create a part'"
          />
        </template>
      </Toolbar>
    </div>
    <div class="card">
      <DataTable
        ref="dt"
        dataKey="id"
        v-model:selection="selectedPart"
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
        :value="parts"
        :paginator="true"
        :rows="10"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rowsPerPageOptions="[5, 10, 25]"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} parts"
        scrollable
        scrollHeight="calc(70vh - 130px)"
      >
        <template #header>
          <div class="flex flex-wrap gap-2 items-center justify-between">
            <h4 class="m-0">Manage Parts</h4>
          </div>
        </template>
        <template #empty> No parts found. </template>
        <Column field="name" header="Name" sortable style="min-width: 12rem"></Column>
        <Column field="description" header="Description" sortable style="min-width: 12rem"></Column>
        <Column field="partNumber" header="Part Number" sortable style="min-width: 12rem"></Column>
        <Column field="published" header="Published" data-type="boolean" style="min-width: 5rem">
          <template #body="{ data }">
            <i
              class="pi"
              :class="{
                'pi-check-circle text-green-500 ': data.published,
                'pi-times-circle text-red-500': !data.published,
              }"
            ></i>
          </template>
        </Column>
        <Column
          field="categoryName"
          header="Category"
          filterField="categoryName"
          style="min-width: 10rem"
        >
          <!-- <template #filter="{ filterModel, filterCallback }">
            <InputText
              v-model="filterModel.value"
              type="text"
              @input="filterCallback()"
              placeholder="Search by category"
            />
          </template> -->
        </Column>
        <Column field="partTypeName" header="Part Type" sortable style="min-width: 10rem"></Column>
        <Column field="dateUpdated" header="Last Updated" sortable style="min-width: 12rem">
          <template #body="slotProps">
            {{ new Date(slotProps.data.dateUpdated).toLocaleDateString() }}
          </template>
        </Column>

        <Column field="facings" header="Facing" sortable style="min-width: 4rem"></Column>
        <Column field="stock" header="Stock" sortable style="min-width: 4rem"></Column>
        <Column
          header="Actions"
          :frozen="true"
          alignFrozen="right"
          :exportable="false"
          style="min-width: 12rem"
        >
          <template #body="slotProps">
            <Button
              v-tooltip="'Edit Part'"
              icon="pi pi-pencil"
              variant="outlined"
              rounded
              class="mr-2"
              @click="editPart(slotProps.data)"
            />
            <Button
              v-tooltip="'Copy Part'"
              icon="pi pi-copy"
              variant="outlined"
              rounded
              class="mr-2"
              @click="copyPart(slotProps.data)"
            />
            <Button
              v-tooltip="'Delete Part'"
              icon="pi pi-trash"
              severity="danger"
              variant="outlined"
              rounded
              class="mr-2"
              @click="deletePart(slotProps.data)"
            />
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>
