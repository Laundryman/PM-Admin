<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
// import UserService from '@/services/UserService.js'
import { useLocationFilters } from '@/components/composables/locationFilters'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { ProductFilter } from '@/models/Products/productFilter.model'
import { searchProductInfo } from '@/models/Products/searchProductInfo.model'
import { default as categoryService } from '@/services/Categories/CategoryService'
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
const categories = ref()
const selectedRegion = ref()
const selectedCountry = ref()
const selectedCategory = ref()
const products = ref<searchProductInfo[]>([])
const selectedProducts = ref<searchProductInfo[]>([])
const toast = useToast()
const loading = ref(true)
const layout = useSystemStore()
const brandStore = useBrandStore()
const brand = storeToRefs(brandStore).activeBrand
const showPublished = ref(true)
const showUnPublished = ref(true)
const searchText = ref('')
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  categoryName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
  parentCategoryName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
  published: { value: null, matchMode: FilterMatchMode.EQUALS },
})

watch(brand, async (newBrand) => {
  if (newBrand) {
    let filter = new ProductFilter()
    filter.brandId = newBrand.id
    await productService.searchProducts(filter).then((response) => {
      products.value = response
      console.log('Products loaded for brand change', products.value)
    })

    await getCategoriesFromProducts(products.value)
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
  await categoryService.initialise()
  let brandid = brandStore.activeBrand?.id ?? 0
  let rFilter = new regionFilter()
  rFilter.brandId = brandid
  await useLocationFilters()
    .getRegions(rFilter)
    .then((response) => {
      regions.value = response
    })

  // await categoryService.getAllCategories().then((data) => {
  //   // create categoryOptions array for select dropdown
  //   categories.value = data.data
  //   loading.value = false
  // })

  var filter = new ProductFilter()
  filter.brandId = brandid
  await productService.searchProducts(filter).then((response) => {
    products.value = response
    loading.value = false
  })

  await getCategoriesFromProducts(products.value)
})

async function onRegionChange() {
  if (selectedRegion.value) {
    countries.value = await useLocationFilters().onRegionChange(selectedRegion.value)
    let filter = new ProductFilter()
    filter.brandId = brandStore.activeBrand?.id ?? 0
    filter.regionId = selectedRegion.value
    await productService.searchProducts(filter).then((response) => {
      products.value = response
      console.log('Products loaded', products.value)
    })
    filters.value.categoryName.value = selectedCategory.value ?? null
  } else {
    countries.value = []
  }
}

async function onCountryChange() {
  if (selectedCountry.value) {
    let filter = new ProductFilter()
    filter.brandId = brandStore.activeBrand?.id ?? 0
    filter.countryId = selectedCountry.value
    await productService.searchProducts(filter).then((response) => {
      products.value = response
      console.log('Products loaded', products.value)
    })
    filters.value.categoryName.value = selectedCategory.value ?? null
  } else {
    countries.value = []
  }
}

async function getCategoriesFromProducts(products: searchProductInfo[]) {
  // Implement the logic to set categories from parts
  for (var product of products) {
    let cat = { label: product.categoryName, value: product.categoryId }
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

function onCategoryChange() {
  if (selectedCategory.value) {
    filters.value.categoryName.value = selectedCategory.value ?? null
  }
}
// async function filterPublished() {
//   let filter = new ProductFilter()
//   filter.brandId = layout.getActiveBrand?.id ?? 0
//   if (showPublishedOnly.value) {
//     filter.isPublished = true
//   }
//   await productService.searchProducts(filter).then((response) => {
//     products.value = response
//     console.log('Products loaded', products.value)
//   })
// }

async function clearFilters() {
  selectedRegion.value = null
  selectedCountry.value = null
  countries.value = []
  let filter = new ProductFilter()
  filter.brandId = brandStore.activeBrand?.id ?? 0
  await productService.searchProducts(filter).then((response) => {
    products.value = response
    console.log('Products loaded', products.value)
  })
  let rFilter = new regionFilter()
  rFilter.brandId = brandStore.activeBrand?.id ?? 0
  await countryService.getRegions(rFilter).then((response) => {
    regions.value = response
    console.log('Regions loaded', regions.value)
  })

  filters.value.categoryName.value = null
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

function onShowPublishedChange() {
  if (showPublished.value) {
    if (showUnPublished.value) {
      filters.value.published.value = null
    } else {
      filters.value.published.value = true
    }
  } else {
    filters.value.published.value = false
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
  }
  console.log('Show Unpublished changed', showUnPublished.value)
}
</script>

<template>
  <div>
    <h1>Product List View</h1>
    <!-- Product list content goes here -->
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
          <Button
            label="New"
            icon="pi pi-plus"
            severity="primary"
            class="mr-2"
            @click="openNew"
            v-tooltip="'Create a product'"
          />
        </template>
      </Toolbar>
    </div>
    <div class="card">
      <DataTable
        ref="dt"
        v-model:selection="selectedProducts"
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
        filterDisplay="menu"
        :value="products"
        dataKey="id"
        :paginator="true"
        :rows="10"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rowsPerPageOptions="[5, 10, 25]"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} products"
        scrollable
        scrollHeight="calc(70vh - 130px)"
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
        <Column field="name" header="Name" sortable style="min-width: 12rem"></Column>
        <Column
          field="parentCategoryName"
          header="Parent Category"
          filterField="parentCategoryName"
          style="min-width: 16rem"
        >
        </Column>
        <Column
          field="categoryName"
          header="Category"
          filterField="categoryName"
          style="min-width: 16rem"
        >
        </Column>
        <Column field="published" header="Published" data-type="boolean" style="min-width: 20rem">
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
        <Column header="Actions" :exportable="false" style="min-width: 12rem">
          <template #body="slotProps">
            <Button
              v-tooltip="'Edit Product'"
              icon="pi pi-pencil"
              variant="outlined"
              rounded
              class="mr-2"
              @click="editProduct(slotProps.data)"
            />
            <Button
              v-tooltip="'Copy Product'"
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
