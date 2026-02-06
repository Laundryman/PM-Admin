<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
// import UserService from '@/services/UserService.js'
import { useLocationFilters } from '@/components/composables/locationFilters'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { ProductFilter } from '@/models/Products/productFilter.model'
import { searchProductInfo } from '@/models/Products/searchProductInfo.model'
import { default as countryService } from '@/services/Countries/CountryService'
import { default as productService } from '@/services/Products/ProductService'
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
const products = ref<searchProductInfo[]>([])
const selectedProducts = ref<searchProductInfo[]>([])
const toast = useToast()
const loading = ref(true)
const layout = useSystemStore()
const brandStore = useBrandStore()
const brand = storeToRefs(brandStore).activeBrand
const searchText = ref('')
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  categoryName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
  parentCategoryName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
})

watch(brand, async (newBrand) => {
  if (newBrand) {
    let filter = new ProductFilter()
    filter.brandId = newBrand.id
    await productService.searchProducts(filter).then((response) => {
      products.value = response
      console.log('Products loaded for brand change', products.value)
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
  await productService.initialise()
  await countryService.initialise()

  let brandid = brandStore.activeBrand?.id ?? 0
  let rFilter = new regionFilter()
  rFilter.brandId = brandid
  await useLocationFilters()
    .getRegions(rFilter)
    .then((response) => {
      regions.value = response
    })

  var filter = new ProductFilter()
  filter.brandId = brandid
  await productService.searchProducts(filter).then((response) => {
    products.value = response
    console.log('Products loaded', products.value)
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
    let filter = new ProductFilter()
    filter.brandId = layout.getActiveBrand?.id ?? 0
    filter.regionId = selectedRegion.value
    await productService.searchProducts(filter).then((response) => {
      products.value = response
      console.log('Products loaded', products.value)
    })
  } else {
    countries.value = []
  }
}

async function onCountryChange() {
  if (selectedCountry.value) {
    let filter = new ProductFilter()
    filter.brandId = layout.getActiveBrand?.id ?? 0
    filter.countryId = selectedCountry.value
    await productService.searchProducts(filter).then((response) => {
      products.value = response
      console.log('Products loaded', products.value)
    })
  } else {
    countries.value = []
  }
}

async function clearFilters() {
  selectedRegion.value = null
  selectedCountry.value = null
  countries.value = []
  let filter = new ProductFilter()
  filter.brandId = layout.getActiveBrand?.id ?? 0
  await productService.searchProducts(filter).then((response) => {
    products.value = response
    console.log('Products loaded', products.value)
  })
  let rFilter = new regionFilter()
  rFilter.brandId = layout.getActiveBrand?.id ?? 0
  await countryService.getRegions(rFilter).then((response) => {
    regions.value = response
    console.log('Regions loaded', regions.value)
  })
}

function editProduct(product: searchProductInfo) {
  console.log('Edit product', product)
  // layout.setActiveProduct(product) --- IGNORE ---
  // Navigate to edit page
  router.push({ name: 'editProduct', params: { id: product.id } })
}

function openNew() {
  router.push({ name: 'newProduct' })
}

function copyProduct(product: searchProductInfo) {
  router.push({ name: 'copyProduct', params: { id: product.id } })
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
        v-model:selection="selectedProducts"
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
        :value="products"
        dataKey="id"
        :paginator="true"
        :rows="10"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rowsPerPageOptions="[5, 10, 25]"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} parts"
      >
        <template #header>
          <div class="flex flex-wrap gap-2 items-center justify-between">
            <h4 class="m-0">Manage Products</h4>
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
          field="parentCategoryName"
          header="Parent Category"
          filterField="parentCategoryName"
          style="min-width: 16rem"
        >
          <template #filter="{ filterModel, filterCallback }">
            <InputText
              v-model="filterModel.value"
              type="text"
              @input="filterCallback()"
              placeholder="Search by parent category"
            />
          </template>
        </Column>
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
              placeholder="Search by category"
            />
          </template>
        </Column>
        <Column :exportable="false" style="min-width: 12rem">
          <template #body="slotProps">
            <Button
              v-tooltip="'Edit Part'"
              icon="pi pi-pencil"
              variant="outlined"
              rounded
              class="mr-2"
              @click="editProduct(slotProps.data)"
            />
            <Button
              v-tooltip="'Copy Part'"
              icon="pi pi-copy"
              variant="outlined"
              rounded
              class="mr-2"
              @click="copyProduct(slotProps.data)"
            />
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>
