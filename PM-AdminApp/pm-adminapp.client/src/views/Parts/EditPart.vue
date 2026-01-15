<script setup lang="ts">
import { useCategoryFilters } from '@/components/composables/categoryFilters copy'
import { useLocationFilters } from '@/components/composables/locationFilters'
import { usePartTypes } from '@/components/composables/partTypes.composable'
import { useStandTypes } from '@/components/composables/standTypes.composable'
import { Category } from '@/models/Categories/category.model'
import { Country } from '@/models/Countries/country.model'
import { Region } from '@/models/Countries/region.model'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { PartFilter } from '@/models/Parts/partFilter.model'
import { PartType } from '@/models/Parts/partType.model'
import { Product } from '@/models/Products/product.model'
import { ProductFilter } from '@/models/Products/productFilter.model'
import { StandType } from '@/models/StandTypes/standType.model'
// import { default as countryService } from '@/services/Countries/CountryService'
import { useBrandStore } from '@/stores/brandStore'
import { usePartStore } from '@/stores/partStore'
import { useProductStore } from '@/stores/productStore'
import { useSystemStore } from '@/stores/systemStore'
import { Form } from '@primevue/forms'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'

const cassetteRenderUrl = import.meta.env.VITE_APP_CASSETTERENDER_URL
const cassettePhotoUrl = import.meta.env.VITE_APP_CASSETTEPHOTO_URL

const partTypeComposable = usePartTypes()
const standTypeComposable = useStandTypes()
const partStore = usePartStore()
const brandStore = useBrandStore()
const productStore = useProductStore()
const locationFilters = useLocationFilters()
const categoryFilters = useCategoryFilters()
const layout = useSystemStore()
const router = useRouter()

const brand = storeToRefs(layout).getActiveBrand
const toast = useToast()
const regions = ref<Region[] | null>(null)
const selectedRegion = ref()
const ms_selectedCountries = ref<number[] | null>(null) // MultiSelect binding
const selectAllCountries = ref(false)
const selectAllProducts = ref(false)
const selectAllStandTypes = ref(false)

const partTypes = ref<{ id: number; name: string }[] | null>(null)
const selectedPartType = ref<PartType | null>(null)
const countrySelectList = ref<Country[] | null>(null)
const parentCategories = ref<Category[] | null>(null)
const childCategories = ref<Category[] | null>(null)
const selectedParentCategory = ref<number | null>(null)
const selectedParentCategoryId = ref<number | null>(null)
const selectedChildCategory = ref<number | null>(null)
const ms_productList = ref<Product[] | null>(null)
const selectedProducts = ref<number[] | null>(null)
const standTypesList = ref<StandType[] | null>(null)
const selectedStandTypes = ref<number[] | null>(null)

const part = storeToRefs(partStore).part
const dateCreated = ref()
// const packShot = ref<string | null>(null)
const packShotFile = ref<File | null>(null)
// const renderImg = ref<string | null>(null)
const renderFile = ref<File | null>(null)
// const icon = ref<string | null>(null)
const iconFile = ref<File | null>(null)
// const initialValues = ref(new Part())

const initialValues = reactive({
  name: part.value?.name || '',
})
onMounted(async () => {
  var partFilter = new PartFilter()
  partFilter.Id = Number(router.currentRoute.value.params.id) || 0
  await partStore.initialize(partFilter)
  selectedParentCategoryId.value = part.value.parentCategoryId || null
  dateCreated.value = new Date(part.value.dateCreated) //added to bind date picker
  if (selectedParentCategoryId.value) {
    await categoryFilters.getChildCategories(selectedParentCategoryId.value).then((response) => {
      childCategories.value = response
      console.log('Child Categories loaded', childCategories.value)
    })
  }
  selectedChildCategory.value = part.value.categoryId || null
  let brandid = brandStore.activeBrand?.id ?? 0
  let rFilter = new regionFilter()
  rFilter.brandId = brandid
  await locationFilters.getRegions(rFilter).then((response) => {
    regions.value = response
  })

  await categoryFilters.getParentCategories().then((response) => {
    parentCategories.value = response
    console.log('Parent Categories loaded', parentCategories.value)
  })

  await partTypeComposable.getPartTypes().then((response) => {
    partTypes.value = response
    console.log('Part Types loaded', partTypes.value)
  })

  await standTypeComposable.getPartStandTypes().then((response) => {
    standTypesList.value = response
    selectedStandTypes.value = part.value.standTypes.map((st) => st.id)
    console.log('Stand Types loaded', standTypesList.value)
  })

  let productFilter = new ProductFilter()
  productFilter.brandId = brandid
  productFilter.categoryId = part.value.categoryId || 0
  await productStore.getProductsByCategory(productFilter).then((response) => {
    ms_productList.value = response ?? null
    console.log('Products loaded', ms_productList.value)
  })

  selectedProducts.value = mapPubishedProducts()
})

function mapPubishedProducts() {
  let publishedProducts = part.value.products.filter((p) => p.published === true)
  return publishedProducts.map((p) => p.id)
}

/////////////////////////////////////////////////////
// Category Handlers
/////////////////////////////////////////////////////
async function onParentCategoryChange() {
  if (selectedParentCategoryId.value) {
    categoryFilters.getChildCategories(selectedParentCategoryId.value).then((response) => {
      childCategories.value = response
      console.log('Child Categories loaded', childCategories.value)
    })
  } else {
    childCategories.value = []
  }
}

async function onChildCategoryChange() {
  if (selectedChildCategory.value) {
    var productFilter = new ProductFilter()
    productFilter.brandId = layout.getActiveBrand?.id ?? 0
    productFilter.categoryId = selectedChildCategory.value
    await productStore.getProductsByCategory(productFilter).then((response) => {
      ms_productList.value = response ?? null
      console.log('Products loaded', ms_productList.value)
    })
  } else {
    ms_productList.value = []
  }
}

////////////////////////////////////////////////////
// Location Handlers
////////////////////////////////////////////////////

async function onRegionChange() {
  if (selectedRegion.value) {
    countrySelectList.value = await locationFilters.onRegionChange(selectedRegion.value)
    ms_selectedCountries.value = []
    if (countrySelectList.value) {
      for (const cntry of countrySelectList.value) {
        let foundCountry = part.value.countries.find((c) => c.id === cntry.id)
        if (foundCountry) {
          ms_selectedCountries.value.push(cntry.id)
        }
      }
    }
  } else {
    countrySelectList.value = []
  }
}

function manageSelectedCountries(selectValues: number[]) {
  if (selectValues.length > 0 && selectValues.length >= (part.value?.countries.length ?? 0)) {
    // You can implement additional logic here if needed
    for (const cntryId of selectValues) {
      let foundCountry = part.value.countries.find((c) => c.id === cntryId)
      if (!foundCountry) {
        let cntry = countrySelectList.value?.find((c) => c.id === cntryId)
        if (cntry) part.value.countries?.push(cntry)
      }
    }
  } else {
    if (selectValues.length <= (part.value?.countries.length ?? 0)) {
      let countriesToRemove = new Array<number>()
      for (const country of part.value.countries ?? []) {
        let index = selectValues.indexOf(country.id) //if it's been removed
        if (index == -1) {
          countriesToRemove.push(country.id)
        }
      }
      for (const cntryId of countriesToRemove) {
        let removeIndex = part.value.countries.findIndex((c) => c.id === cntryId)
        if (removeIndex !== -1) {
          part.value.countries.splice(removeIndex, 1)
        }
      }
    }
  }
}
async function onCountryChange(evt: any) {
  manageSelectedCountries(evt.value)
}

function onSelectAllCountriesChange(event: any) {
  ms_selectedCountries.value = event.checked
    ? (countrySelectList.value?.map((item) => item.id) ?? [])
    : []
  selectAllCountries.value = event.checked
  manageSelectedCountries(ms_selectedCountries.value)
}

function clearCountrySelection() {
  ms_selectedCountries.value = []
  part.value.countries = []
}

////////////////////////////////////////////////////
// File Upload Handlers
////////////////////////////////////////////////////
function onPackShotSelect(event: any) {
  packShotFile.value = event.files[0]
  const reader = new FileReader()
  reader.onload = async (e) => {
    // packShot.value = e.target?.result as string
    part.value.packShotImageSrc = e.target?.result as string
  }

  reader.readAsDataURL(packShotFile.value!)
}
function onRenderSelect(event: any) {
  renderFile.value = event.files[0]
  const reader = new FileReader()
  reader.onload = async (e) => {
    // renderImg.value = e.target?.result as string
    part.value.render2dImage = e.target?.result as string
  }

  reader.readAsDataURL(renderFile.value!)
}
function onIconSelect(event: any) {
  iconFile.value = event.files[0]
  const reader = new FileReader()
  reader.onload = async (e) => {
    // icon.value = e.target?.result as string
    part.value.svgLineGraphic = e.target?.result as string
  }

  reader.readAsDataURL(iconFile.value!)
}

/////////////////////////////////////////////////////
// Product Handlers
/////////////////////////////////////////////////////

function manageSelectedProducts(selectValues: number[]) {
  if (selectValues.length > 0 && selectValues.length >= (part.value?.products.length ?? 0)) {
    for (const productId of selectValues) {
      let foundProduct = part.value.products.find((p) => p.id === productId)
      if (!foundProduct) {
        let product = ms_productList.value?.find((p) => p.id === productId)
        if (product) part.value.products?.push(product)
      }
    }
  } else {
    if (selectValues.length <= (part.value?.products.length ?? 0)) {
      let productsToRemove = new Array<number>()
      for (const product of part.value.products ?? []) {
        // let index = part.value.products.findIndex((p) => p.id === productId)
        if (product.published === true) {
          let index = selectValues.indexOf(product.id) //if it's been removed
          if (index == -1) {
            productsToRemove.push(product.id)
          }
        }
      }
      for (const prodId of productsToRemove) {
        let removeIndex = part.value.products.findIndex((p) => p.id === prodId)
        if (removeIndex !== -1) {
          part.value.products.splice(removeIndex, 1)
        }
      }
    }
  }
}

async function onProductChange(evt: any) {
  manageSelectedProducts(evt.value)
}

function onSelectAllProductsChange(event: any) {
  selectedProducts.value = event.checked ? (ms_productList.value?.map((p) => p.id) ?? []) : []
  selectAllProducts.value = event.checked
  manageSelectedProducts(selectedProducts.value)
}

function clearProductSelection() {
  part.value.products = []
  selectedProducts.value = []
}

/////////////////////////////////////////////////////
// Stand Type Handlers
/////////////////////////////////////////////////////

function manageSelectedStandTypes(selectValues: number[]) {
  if (selectValues.length > 0 && selectValues.length >= (part.value?.standTypes.length ?? 0)) {
    for (const standTypeId of selectValues) {
      let foundStandType = part.value.standTypes.find((st) => st.id === standTypeId)
      if (!foundStandType) {
        let standType = standTypesList.value?.find((st) => st.id === standTypeId)
        if (standType) part.value.standTypes?.push(standType)
      }
    }
  } else {
    if (selectValues.length <= (part.value?.standTypes.length ?? 0)) {
      let standTypesToRemove = new Array<number>()
      for (const standType of part.value.standTypes ?? []) {
        let index = selectValues.indexOf(standType.id) //if it's been removed
        if (index == -1) {
          standTypesToRemove.push(standType.id)
        }
      }
      for (const stId of standTypesToRemove) {
        let removeIndex = part.value.standTypes.findIndex((st) => st.id === stId)
        if (removeIndex !== -1) {
          part.value.standTypes.splice(removeIndex, 1)
        }
      }
    }
  }
}

function onStandTypeChange(evt: any) {
  manageSelectedStandTypes(evt.value)
}

function onSelectAllStandTypesChange(event: any) {
  selectedStandTypes.value = event.checked ? (standTypesList.value?.map((st) => st.id) ?? []) : []
  selectAllStandTypes.value = event.checked
  manageSelectedStandTypes(selectedStandTypes.value)
}

function clearStandTypeSelection() {
  part.value.standTypes = []
  selectedStandTypes.value = []
}

/////////////////////////////////////////////////////
// Date Handlers
////////////////////////////////////////////////////

function updateDateCreated(evnt: any) {
  console.log('Date selected', evnt)
}
/////////////////////////////////////////////////////
// Form Handlers
/////////////////////////////////////////////////////
const resolver = ({ values }: any) => {
  const errors = {} as any

  if (!values.name) {
    ;``
    errors.name = [{ message: 'Username is required.' }]
  }

  return {
    values, // (Optional) Used to pass current form values to submit event.
    errors,
  }
}

const onFormSubmit = ({ valid }: any) => {
  if (valid) {
    toast.add({
      severity: 'success',
      summary: 'Form is submitted.',
      life: 3000,
    })
  }
}
</script>

<style>
.cassette-icon svg {
  max-width: 100%;
  max-height: 200px;
}
</style>
<template>
  <div class="edit-part-view">
    <div class="mb-5">
      <Button
        label="Back to Parts"
        icon="pi pi-arrow-left"
        class="p-button-text"
        @click="router.back()"
      />
    </div>
    <div class="w-full sticky bg-white top-16 block p-10 z-10">
      <h2>Edit Part</h2>
    </div>
    <div class="card grid grid-cols-1 gap-4 justify-center">
      <Form
        v-slot="$form"
        :initialValues
        :resolver
        @submit="onFormSubmit"
        class="grid grid-cols-2 gap- w-full p-20 pt-0"
      >
        <div class="bg-gray-50 col-span-2 p-10 mb-5">
          <fieldset legend="Part Details" class="col-span-2">
            <legend class="text-lg font-bold mb-2">Visibility</legend>
            <div class="grid grid-cols-2 gap-10">
              <div class="flex flex-col gap-1">
                <label for="published">Published:</label>
                <ToggleButton
                  v-model="part.published"
                  onLabel="Yes"
                  offLabel="No"
                  onIcon="pi pi-check"
                  offIcon="pi pi-times"
                  class="w-24 mt-2"
                ></ToggleButton>
              </div>
              <div class="flex flex-col gap-1">
                <label for="discontinued">Discontinued:</label>
                <ToggleButton
                  v-model="part.discontinued"
                  onLabel="Yes"
                  offLabel="No"
                  onIcon="pi pi-check"
                  offIcon="pi pi-times"
                  class="w-24 mt-2"
                ></ToggleButton>
              </div>
            </div>
          </fieldset>
        </div>
        <div class="bg-gray-50 col-span-2 p-10 mb-5">
          <fieldset legend="Part Details" class="col-span-2 mb-12">
            <legend class="text-lg font-bold mb-2">Details</legend>
            <div class="grid grid-cols-2 gap-10">
              <div class="flex flex-col gap-1">
                <label for="name">Part Name:</label>
                <InputText
                  v-model="part.name"
                  name="name"
                  type="text"
                  placeholder="Part Name"
                  fluid
                />
                <Message
                  v-if="$form.name?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.name.error?.message }}</Message
                >
              </div>
              <div class="flex flex-col gap-1">
                <label for="customerReference">Customer Reference:</label>
                <InputText
                  v-model="part.customerRefNo"
                  name="customerReference"
                  type="text"
                  placeholder="Customer Reference"
                  fluid
                />
              </div>
              <div class="flex flex-col gap-1">
                <label for="partNumber">Part Number:</label>
                <InputText
                  v-model="part.partNumber"
                  name="partNumber"
                  type="text"
                  placeholder="Part Number"
                  fluid
                />
              </div>
              <div class="flex flex-col gap-1">
                <label for="altPartNumber">Alt Part Number:</label>
                <InputText
                  v-model="part.altPartNumber"
                  name="altPartNumber"
                  type="text"
                  placeholder="Alt Part Number"
                  fluid
                />
              </div>

              <div class="flex flex-col gap-1">
                <label for="description">Description:</label>
                <InputText
                  v-model="part.description"
                  name="description"
                  type="text"
                  placeholder="Description"
                  fluid
                />
              </div>
            </div>
          </fieldset>
        </div>
        <div class="bg-gray-50 col-span-2 p-10 mb-5">
          <fieldset legend="Part Images" class="col-span-2">
            <legend class="text-lg font-bold mb-2">Images</legend>
            <div class="grid grid-cols-3 gap-10">
              <div class="flex flex-col gap-1">
                <!-- <div class="card flex flex-wrap gap-6 items-center justify-between"> -->
                <label for="svgLineGraphic">Cassette Icon:</label>
                <div v-html="part.svgLineGraphic" class="cassette-icon max-w-40"></div>
                <FileUpload
                  name="svgLineGraphic"
                  ref="fileupload"
                  mode="basic"
                  @select="onIconSelect"
                  customUpload
                  accept="image/*"
                  :maxFileSize="1000000"
                />
              </div>
              <div class="flex flex-col gap-1">
                <!-- <div class="card flex flex-wrap gap-6 items-center justify-between"> -->
                <label for="packShotImgSrc">Cassette Image:</label>
                <img
                  v-if="part.packShotImageSrc != null"
                  :src="cassettePhotoUrl + part.packShotImageSrc"
                  alt="Pack Shot"
                  class="block m-auto pb-4 max-h-60"
                />
                <FileUpload
                  name="packShotImgSrc"
                  ref="fileupload"
                  mode="basic"
                  @select="onPackShotSelect"
                  customUpload
                  accept="image/*"
                  :maxFileSize="1000000"
                />
              </div>
              <!-- </div> -->
              <div class="flex flex-col gap-1">
                <label for="render2dImage">Planogram 2d Render:</label>
                <!-- <InputText name="render2dImage" type="text" placeholder="Planogram 2d Render" fluid /> -->
                <img
                  v-if="part.render2dImage != null"
                  :src="cassetteRenderUrl + part.render2dImage"
                  alt="Pack Shot"
                  class="block m-auto pb-4 max-h-60"
                />
                <FileUpload
                  name="render2dImage"
                  ref="fileupload"
                  mode="basic"
                  @select="onRenderSelect"
                  customUpload
                  accept="image/*"
                  :maxFileSize="1000000"
                />
              </div>
            </div>
          </fieldset>
        </div>
        <div class="bg-gray-50 col-span-2 p-10 mb-5">
          <fieldset legend="Location" class="col-span-2">
            <legend class="text-lg font-bold mb-2">Location</legend>
            <div class="grid grid-cols-3 gap-10">
              <div class="flex flex-col gap-2">
                <label for="region">Region:</label>
                <Select
                  v-model="selectedRegion"
                  :options="regions ?? []"
                  id="region"
                  class="w-full"
                  option-label="name"
                  option-value="id"
                  @change="onRegionChange"
                >
                  <template #option="option">
                    <div class="flex align-items-center">
                      <span>{{ option.option.name }}</span>
                    </div>
                  </template>
                </Select>
              </div>
              <div class="flex flex-col gap-2">
                <label for="country">Country:</label>
                <MultiSelect
                  v-model="ms_selectedCountries"
                  :options="countrySelectList ?? []"
                  id="country"
                  class="w-full"
                  option-label="name"
                  option-value="id"
                  @change="onCountryChange"
                  :selectAll="selectAllCountries"
                  @selectall-change="onSelectAllCountriesChange($event)"
                >
                  <template #option="option">
                    <div class="">
                      <span>{{ option.option.name }}</span>
                    </div>
                  </template>
                </MultiSelect>
              </div>
              <div class="flex gap-2 flex-wrap max-h-40 overflow-auto">
                <Button
                  label="Clear Selection"
                  class="w-text-left"
                  @click="clearCountrySelection"
                />
                <template v-for="country in part.countries">
                  <Chip class="flex-wrap" :label="country.name"></Chip>
                </template>
              </div>
            </div>
          </fieldset>
        </div>
        <div class="bg-gray-50 col-span-2 p-10 mb-5">
          <fieldset legend="Categorisation" class="col-span-2">
            <legend class="text-lg font-bold mb-2">Categorisation</legend>
            <div class="grid grid-cols-2 gap-10">
              <div class="flex flex-col gap-2">
                <label for="parentCategory">Parent Category:</label>
                <Select
                  v-model="part.parentCategoryId"
                  :options="parentCategories ?? []"
                  @change="onParentCategoryChange"
                  id="parentCategory"
                  class="w-full"
                  option-label="name"
                  option-value="id"
                >
                  <template #option="option">
                    <div class="flex align-items-center">
                      <span>{{ option.option.name }}</span>
                    </div>
                  </template>
                </Select>
              </div>
              <div class="flex flex-col gap-2">
                <label for="childCategory">Child Category:</label>
                <Select
                  v-model="part.categoryId"
                  :options="childCategories ?? []"
                  id="childCategory"
                  class="w-full"
                  option-label="name"
                  option-value="id"
                  @change="onChildCategoryChange"
                >
                  <template #option="option">
                    <div class="flex align-items-center">
                      <span>{{ option.option.name }}</span>
                    </div>
                  </template>
                </Select>
              </div>
              <div class="flex flex-col gap-2">
                <label for="Part Type">Part Types:</label>
                <Select
                  v-model="selectedPartType"
                  :options="partTypes ?? []"
                  id="region"
                  class="w-full"
                  option-label="name"
                  option-value="id"
                >
                  <template #option="option">
                    <div class="flex align-items-center">
                      <span>{{ option.option.name }}</span>
                    </div>
                  </template>
                </Select>
              </div>
            </div>
          </fieldset>
        </div>
        <div class="bg-gray-50 col-span-2 p-10 mb-5">
          <fieldset legend="Products" class="col-span-2">
            <legend class="text-lg font-bold mb-2">Products</legend>
            <div class="grid grid-cols-2 gap-10">
              <div class="flex flex-col gap-2">
                <label for="products">Products:</label>
                <MultiSelect
                  v-model="selectedProducts"
                  :options="ms_productList ?? []"
                  id="products"
                  class="w-full"
                  option-label="name"
                  option-value="id"
                  @change="onProductChange($event)"
                  :selectAll="selectAllProducts"
                  @selectall-change="onSelectAllProductsChange($event)"
                >
                  <template #option="option">
                    <div class="flex align-items-center">
                      <span>{{ option.option.name }}</span>
                    </div>
                  </template>
                </MultiSelect>
              </div>
              <div class="flex gap-2 flex-wrap max-h-40 overflow-y-auto pt-1">
                <Button
                  v-if="part.products?.length > 0"
                  label="Clear Selection"
                  class="w-text-left h-10"
                  @click="clearProductSelection"
                />
                <template v-for="product in part.products">
                  <template v-if="!product.published">
                    <OverlayBadge severity="danger" v-tooltip="'this product is not published'">
                      <Chip :label="product.name"> </Chip>
                    </OverlayBadge>
                  </template>
                  <template v-else>
                    <Chip :label="product.name"></Chip>
                  </template>
                </template>
              </div>
            </div>
          </fieldset>
        </div>
        <div class="bg-gray-50 col-span-2 p-10 mb-5">
          <fieldset legend="Stands" class="col-span-2">
            <legend class="text-lg font-bold mb-2">Stands</legend>
            <div class="grid grid-cols-2 gap-10">
              <div class="flex flex-col gap-2">
                <label for="standType">Stand Type:</label>
                <MultiSelect
                  v-model="selectedStandTypes"
                  :options="standTypesList ?? []"
                  id="standType"
                  class="w-full"
                  option-label="name"
                  option-value="id"
                  @change="onStandTypeChange($event)"
                  :selectAll="selectAllStandTypes"
                  @selectall-change="onSelectAllStandTypesChange($event)"
                >
                  <template #option="option">
                    <div class="flex align-items-center">
                      <span>{{ option.option.name }}</span>
                    </div>
                  </template>
                </MultiSelect>
              </div>
              <div class="flex gap-2 flex-wrap max-h-40 overflow-y-auto pt-1">
                <label class="w-full font-bold mb-2">Selected Stand Types:</label>
                <template v-for="standType in part.standTypes">
                  <Chip :label="standType.name"></Chip>
                </template>
              </div>
            </div>
          </fieldset>
        </div>
        <div class="bg-gray-50 col-span-2 p-10 mb-5">
          <fieldset legend="Dimensions" class="col-span-2">
            <legend class="text-lg font-bold mb-2">Dimensions</legend>
            <div class="grid grid-cols-2 gap-10">
              <div class="flex flex-col gap-1">
                <label for="facings">Facings:</label>
                <InputNumber name="facings" placeholder="Facings" fluid v-model="part.facings" />
              </div>
              <div class="flex flex-col gap-1">
                <label for="Stock">Stock:</label>
                <InputNumber
                  name="stock"
                  type="text"
                  placeholder="Stock"
                  fluid
                  v-model="part.stock"
                />
              </div>
              <div class="flex flex-col gap-1">
                <label for="shoppingHeight">Shopping Height:</label>
                <InputNumber
                  name="shoppingHeight"
                  placeholder="Shopping Height"
                  fluid
                  v-model="part.shoppingHeight"
                />
              </div>
              <div class="flex flex-col gap-1">
                <label for="Height">Height:</label>
                <InputNumber name="height" placeholder="Height" fluid v-model="part.height" />
              </div>
              <div class="flex flex-col gap-1">
                <label for="width">Width:</label>
                <InputNumber name="width" placeholder="Width" fluid v-model="part.width" />
              </div>
              <div class="flex flex-col gap-1">
                <label for="depth">Depth:</label>
                <InputNumber name="depth" placeholder="Depth" fluid v-model="part.depth" />
              </div>
              <div class="flex flex-col gap-1">
                <label for="unitCost">Unit Cost:</label>
                <InputNumber
                  name="unitCost"
                  placeholder="Unit Cost"
                  fluid
                  v-model="part.unitCost"
                />
              </div>
              <div class="flex flex-col gap-1">
                <label for="dateCreated">Created Date:</label>
                <DatePicker
                  name="createdDate"
                  placeholder="Created Date"
                  fluid
                  v-model="dateCreated"
                  @date-select="updateDateCreated($event)"
                />
              </div>
            </div>
          </fieldset>
        </div>

        <Button type="submit" severity="secondary" label="Submit" />
      </Form>
    </div>
  </div>
</template>
