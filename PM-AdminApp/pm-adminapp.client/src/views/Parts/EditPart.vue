<script setup lang="ts">
import { useCategoryFilters } from '@/components/composables/categoryFilters copy'
import { useLocationFilters } from '@/components/composables/locationFilters'
import { usePartTypes } from '@/components/composables/partTypes.composable'
import { useStandTypes } from '@/components/composables/standTypes.composable'
import { Category } from '@/models/Categories/category.model'
import { Country } from '@/models/Countries/country.model'
import { Region } from '@/models/Countries/region.model'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { Part } from '@/models/Parts/part.model'
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
import type { FormInstance } from '@primevue/forms'
import { Form } from '@primevue/forms'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { onMounted, ref, useTemplateRef } from 'vue'
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
const regionSelectList = ref<Region[] | null>(null)
const selectedRegion = ref()
const ms_selectedRegions = ref<number[] | null>(null) // MultiSelect binding
const ms_selectedCountries = ref<number[] | null>(null) // MultiSelect binding
const selectAllCountries = ref(false)
const selectAllProducts = ref(false)
const selectAllStandTypes = ref(false)

const partTypes = ref<{ id: number; name: string }[] | null>(null)
const selectedPartType = ref<number | null>(null)
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

const dateCreated = ref()
// const packShot = ref<string | null>(null)
const packShotFile = ref<File | null>(null)
// const renderImg = ref<string | null>(null)
const renderFile = ref<File | null>(null)
// const icon = ref<string | null>(null)
const iconFile = ref<File | null>(null)

const partform = useTemplateRef<FormInstance>('part-form')
const { part } = storeToRefs(partStore)
const partModel = ref<Part>(new Part())

const initialValues = ref(new Part())

//const initialValues = ref({})
onMounted(async () => {
  var partFilter = new PartFilter()
  partFilter.Id = Number(router.currentRoute.value.params.id) || 0
  await partStore.initialize(partFilter)
  partModel.value = { ...part.value } as Part //clone(part.value)
  selectedParentCategoryId.value = partModel.value.parentCategoryId || null
  ms_selectedRegions.value = partModel.value.regions.map((c) => c.id)
  countrySelectList.value = await locationFilters.getCountriesForRegions(ms_selectedRegions.value)
  ms_selectedCountries.value = partModel.value.countries.map((c) => c.id)
  dateCreated.value = new Date(partModel.value.dateCreated) //added to bind date picker

  if (selectedParentCategoryId.value) {
    await categoryFilters.getChildCategories(selectedParentCategoryId.value).then((response) => {
      childCategories.value = response
      console.log('Child Categories loaded', childCategories.value)
    })
  }
  selectedChildCategory.value = partModel.value.categoryId || null
  let brandid = brandStore.activeBrand?.id ?? 0
  let rFilter = new regionFilter()
  rFilter.brandId = brandid
  await locationFilters.getRegions(rFilter).then((response) => {
    regionSelectList.value = response
  })

  await categoryFilters.getParentCategories().then((response) => {
    parentCategories.value = response
    console.log('Parent Categories loaded', parentCategories.value)
  })

  await partTypeComposable.getPartTypes().then((response) => {
    partTypes.value = response
    console.log('Part Types loaded', partTypes.value)
    partModel.value.PartType = partTypes.value?.find(
      (pt) => pt.id === partModel.value.partTypeId,
    ) as PartType
    selectedPartType.value = partModel.value.PartType.id
  })

  await standTypeComposable.getPartStandTypes().then((response) => {
    standTypesList.value = response
    selectedStandTypes.value = partModel.value.standTypes.map((st: StandType) => st.id)
    console.log('Stand Types loaded', standTypesList.value)
    partModel.value.standTypes = standTypesList.value?.filter((st) =>
      selectedStandTypes.value?.includes(st.id),
    ) as StandType[]
  })

  let productFilter = new ProductFilter()
  productFilter.brandId = brandid
  productFilter.categoryId = partModel.value.categoryId || 0
  await productStore.getProductsByCategory(productFilter).then((response) => {
    ms_productList.value = response ?? null
    console.log('Products loaded', ms_productList.value)
  })

  selectedProducts.value = mapPubishedProducts()
  initialisePartForm()
})

function initialisePartForm() {
  partform.value?.setValues({ ...partModel.value })
  partform.value?.setFieldValue('countries', [])

  //default the selection to ALL COUNTRIES LP - so user sees something
  // selectedRegion.value =
  //   regionSelectList.value?.find((region) => region.name === 'ALL COUNTRIES LP')?.id || null
  // onRegionChange()
}

function mapPubishedProducts() {
  let publishedProducts = partModel.value.products.filter((p: Product) => p.published === true)
  return publishedProducts.map((p: Product) => p.id)
}

/////////////////////////////////////////////////////
// Category Handlers
/////////////////////////////////////////////////////
async function onParentCategoryChange() {
  if (selectedParentCategoryId.value) {
    categoryFilters.getChildCategories(selectedParentCategoryId.value).then((response) => {
      childCategories.value = response
      selectedChildCategory.value = null
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
// Generic Multi-Select Handler
///////////////////////////////////////////////////

function manageSelectedValues(
  selectedValues: number[],
  availableValues: any[],
  targetArray: any[],
) {
  if (selectedValues.length > 0) {
    for (const valueId of selectedValues) {
      let foundValue = targetArray.find((st) => st.id === valueId)
      if (!foundValue) {
        //handling unpublished items in the target array
        let item = availableValues?.find((st) => st.id === valueId)
        if (item) targetArray?.push(item)
      }
    }
  }
  let selectedValuesToRemove = new Array<number>()
  for (const item of targetArray ?? []) {
    let index = selectedValues.indexOf(item.id) //if it's been removed
    if (index == -1) {
      // let foundItem = availableValues.find((st) => st.id === item.id)
      // if (!foundItem)
      var published = item.published ?? true
      if (published === true) {
        selectedValuesToRemove.push(item.id)
      }
    }
  }
  for (const stId of selectedValuesToRemove) {
    let removeIndex = targetArray.findIndex((st) => st.id === stId)
    if (removeIndex !== -1) {
      targetArray.splice(removeIndex, 1)
    }
  }
}

////////////////////////////////////////////////////
// Location Handlers
////////////////////////////////////////////////////

async function onRegionChange(evt: any) {
  countrySelectList.value = await locationFilters.getCountriesForRegions(ms_selectedRegions.value)
  //remove any countries no longer in the list
  let newSelectList = ms_selectedCountries.value?.filter((countryId) =>
    countrySelectList.value?.some((c) => c.id === countryId),
  )
  if (newSelectList) {
    ms_selectedCountries.value = newSelectList
  } else {
    ms_selectedCountries.value = []
  }
  manageSelectedValues(evt.value, regionSelectList.value ?? [], partModel.value.regions ?? [])
  manageSelectedValues(
    ms_selectedCountries.value ?? [],
    countrySelectList.value ?? [],
    partModel.value.countries ?? [],
  )
}

async function onCountryChange(evt: any) {
  // manageSelectedCountries(evt.value)
  manageSelectedValues(evt.value, countrySelectList.value ?? [], partModel.value.countries ?? [])
}

function onSelectAllCountriesChange(event: any) {
  ms_selectedCountries.value = event.checked
    ? (countrySelectList.value?.map((item) => item.id) ?? [])
    : []
  selectAllCountries.value = event.checked
  //manageSelectedCountries(ms_selectedCountries.value)
  manageSelectedValues(
    ms_selectedCountries.value,
    countrySelectList.value ?? [],
    partModel.value.countries ?? [],
  )
}

function clearCountrySelection() {
  ms_selectedCountries.value = []
  partModel.value.countries = []
}

////////////////////////////////////////////////////
// File Upload Handlers
////////////////////////////////////////////////////
function onPackShotSelect(event: any) {
  packShotFile.value = event.files[0]
  const reader = new FileReader()
  reader.onload = async (e) => {
    // packShot.value = e.target?.result as string
    partModel.value.packShotImageSrc = e.target?.result as string
  }

  reader.readAsDataURL(packShotFile.value!)
}
function onRenderSelect(event: any) {
  renderFile.value = event.files[0]
  const reader = new FileReader()
  reader.onload = async (e) => {
    // renderImg.value = e.target?.result as string
    partModel.value.render2dImage = e.target?.result as string
  }

  reader.readAsDataURL(renderFile.value!)
}
function onIconSelect(event: any) {
  iconFile.value = event.files[0]
  const reader = new FileReader()
  reader.onload = async (e) => {
    // icon.value = e.target?.result as string
    partModel.value.svgLineGraphic = e.target?.result as string
  }

  reader.readAsDataURL(iconFile.value!)
}

/////////////////////////////////////////////////////
// Product Handlers
/////////////////////////////////////////////////////

async function onProductChange(evt: any) {
  // manageSelectedProducts(evt.value)
  manageSelectedValues(evt.value, ms_productList.value ?? [], partModel.value.products ?? [])
}

function onSelectAllProductsChange(event: any) {
  selectedProducts.value = event.checked ? (ms_productList.value?.map((p) => p.id) ?? []) : []
  selectAllProducts.value = event.checked
  // manageSelectedProducts(selectedProducts.value)
  manageSelectedValues(
    selectedProducts.value,
    ms_productList.value ?? [],
    partModel.value.products ?? [],
  )
}

function clearProductSelection() {
  partModel.value.products = []
  selectedProducts.value = []
}

/////////////////////////////////////////////////////
// Part Type Handlers
////////////////////////////////////////////////////

function onPartTypeChange(evt: any) {
  let selectedId = evt.value as number
  let selectedPartTypeItem = partTypes.value?.find((pt) => pt.id === selectedId) as PartType
  partModel.value.partTypeId = selectedPartTypeItem.id
  partModel.value.PartType = selectedPartTypeItem
}

/////////////////////////////////////////////////////
// Stand Type Handlers
/////////////////////////////////////////////////////

function onStandTypeChange(evt: any) {
  // manageSelectedStandTypes(evt.value)
  manageSelectedValues(evt.value, standTypesList.value ?? [], partModel.value.standTypes ?? [])
}

function onSelectAllStandTypesChange(event: any) {
  selectedStandTypes.value = event.checked ? (standTypesList.value?.map((st) => st.id) ?? []) : []
  selectAllStandTypes.value = event.checked
  manageSelectedValues(
    selectedStandTypes.value,
    standTypesList.value ?? [],
    partModel.value.standTypes ?? [],
  )
}

function clearStandTypeSelection() {
  partModel.value.standTypes = []
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
    errors.name = [{ message: 'Name is required.' }]
  }
  if (!values.partNumber) {
    errors.partNumber = [{ message: 'Part Number is required.' }]
  }
  if (!values.description) {
    errors.description = [{ message: 'Description is required.' }]
  }
  if (!values.categoryId) {
    errors.categoryId = [{ message: 'Category is required.' }]
  }
  if (!values.parentCategoryId) {
    errors.parentCategoryId = [{ message: 'Parent Category is required.' }]
  }
  if (!values.facings && values.facings !== 0) {
    errors.facings = [{ message: 'Facings is required.' }]
  }
  if (!values.height && values.height !== 0) {
    errors.height = [{ message: 'Height is required.' }]
  }
  if (!values.width && values.width !== 0) {
    errors.width = [{ message: 'Width is required.' }]
  }
  if (!values.stock && values.stock !== 0) {
    errors.stock = [{ message: 'Stock is required.' }]
  }
  if (!values.depth && values.depth !== 0) {
    errors.depth = [{ message: 'Depth is required.' }]
  }
  if (!values.unitCost && values.unitCost !== 0) {
    errors.unitCost = [{ message: 'Unit Cost is required.' }]
  }
  return {
    values, // (Optional) Used to pass current form values to submit event.
    errors,
  }
}

async function onFormSubmit({ valid }: any) {
  if (valid) {
    await partStore.savePart(partModel.value)
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
      <div class="card flex flex-col gap-2">
        <span class="font-bold text-xl">{{ partModel.name }}</span>
        <span class="text-gray-600">Part No: {{ partModel.partNumber }}</span>
      </div>
    </div>
    <div class="card grid grid-cols-1 gap-4 justify-center">
      <Form
        ref="part-form"
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
                  name="published"
                  v-model="partModel.published"
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
                  name="discontinued"
                  v-model="partModel.discontinued"
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
                  v-model="partModel.name"
                  name="name"
                  :value="partModel.name"
                  type="text"
                  length="255"
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
                  v-model="partModel.customerRefNo"
                  name="customerReference"
                  type="text"
                  placeholder="Customer Reference"
                  fluid
                  length="255"
                />
                <Message
                  v-if="$form.customerReference?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.customerReference.error?.message }}</Message
                >
              </div>
              <div class="flex flex-col gap-1">
                <label for="partNumber">Part Number:</label>
                <InputText
                  v-model="partModel.partNumber"
                  name="partNumber"
                  type="text"
                  placeholder="Part Number"
                  fluid
                  length="255"
                />
              </div>
              <div class="flex flex-col gap-1">
                <label for="altPartNumber">Alt Part Number:</label>
                <InputText
                  v-model="partModel.altPartNumber"
                  name="altPartNumber"
                  type="text"
                  placeholder="Alt Part Number"
                  fluid
                  length="255"
                />
              </div>

              <div class="flex flex-col gap-1">
                <label for="description">Description:</label>
                <InputText
                  v-model="partModel.description"
                  name="description"
                  type="text"
                  length="255"
                  placeholder="Description"
                  fluid
                />
                <Message
                  v-if="$form.description?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.description.error?.message }}</Message
                >
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
                <div v-html="partModel.svgLineGraphic" class="cassette-icon max-w-40"></div>
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
                  v-if="partModel.packShotImageSrc != null"
                  :src="cassettePhotoUrl + partModel.packShotImageSrc"
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
                  v-if="partModel.render2dImage != null"
                  :src="cassetteRenderUrl + partModel.render2dImage"
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
                <MultiSelect
                  name="region"
                  v-model="ms_selectedRegions"
                  :options="regionSelectList ?? []"
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
                </MultiSelect>
              </div>
              <div class="flex flex-col gap-2">
                <label for="country">Country:</label>
                <MultiSelect
                  name="countries"
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
                <Message
                  v-if="$form.countries?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.countries.error?.message }}</Message
                >
              </div>
              <div class="flex gap-2 flex-wrap max-h-40 overflow-auto">
                <Button
                  label="Clear Selection"
                  class="w-text-left"
                  @click="clearCountrySelection"
                />
                <template v-for="country in partModel.countries">
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
                  name="ParentCategory"
                  v-model="selectedParentCategoryId"
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
                <Message
                  v-if="$form.parentCategoryId?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.parentCategoryId.error?.message }}</Message
                >
              </div>
              <div class="flex flex-col gap-2">
                <label for="childCategory">Child Category:</label>
                <Select
                  v-model="selectedChildCategory"
                  :options="childCategories ?? []"
                  name="childCategory"
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
                <Message
                  v-if="$form.childCategoryId?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.childCategoryId.error?.message }}</Message
                >
              </div>
              <div class="flex flex-col gap-2">
                <label for="Part Type">Part Type:</label>
                <Select
                  v-model="selectedPartType"
                  :options="partTypes ?? []"
                  id="partType"
                  name="partType"
                  class="w-full"
                  option-label="name"
                  option-value="id"
                  @change="onPartTypeChange"
                >
                  <template #option="option">
                    <div class="flex align-items-center">
                      <span>{{ option.option.name }}</span>
                    </div>
                  </template>
                </Select>
                <Message
                  v-if="$form.partTypeId?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.partTypeId.error?.message }}</Message
                >
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
                  name="products"
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
                  v-if="partModel.products?.length > 0"
                  label="Clear Selection"
                  class="w-text-left h-10"
                  @click="clearProductSelection"
                />
                <template v-for="product in partModel.products">
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
                  name="standType"
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
                <template v-for="standType in partModel.standTypes">
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
                <InputNumber
                  name="facings"
                  placeholder="Facings"
                  fluid
                  v-model="partModel.facings"
                />
                <Message
                  v-if="$form.facings?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.facings.error?.message }}</Message
                >
              </div>
              <div class="flex flex-col gap-1">
                <label for="Stock">Stock:</label>
                <InputNumber
                  name="stock"
                  type="text"
                  placeholder="Stock"
                  fluid
                  v-model="partModel.stock"
                />
                <Message
                  v-if="$form.stock?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.stock.error?.message }}</Message
                >
              </div>
              <div class="flex flex-col gap-1">
                <label for="shoppingHeight">Shopping Height:</label>
                <InputNumber
                  name="shoppingHeight"
                  placeholder="Shopping Height"
                  fluid
                  v-model="partModel.shoppingHeight"
                />
              </div>
              <div class="flex flex-col gap-1">
                <label for="Height">Height:</label>
                <InputNumber name="height" placeholder="Height" fluid v-model="partModel.height" />
                <Message
                  v-if="$form.height?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.height.error?.message }}</Message
                >
              </div>
              <div class="flex flex-col gap-1">
                <label for="width">Width:</label>
                <InputNumber name="width" placeholder="Width" fluid v-model="partModel.width" />
                <Message
                  v-if="$form.width?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.width.error?.message }}</Message
                >
              </div>
              <div class="flex flex-col gap-1">
                <label for="depth">Depth:</label>
                <InputNumber name="depth" placeholder="Depth" fluid v-model="partModel.depth" />
                <Message
                  v-if="$form.depth?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.depth.error?.message }}</Message
                >
              </div>
              <div class="flex flex-col gap-1">
                <label for="unitCost">Unit Cost:</label>
                <InputNumber
                  name="unitCost"
                  placeholder="Unit Cost"
                  fluid
                  v-model="partModel.unitCost"
                />
              </div>
              <div class="flex flex-col gap-1">
                <label for="dateCreated">Created Date:</label>
                <DatePicker
                  name="createdDate"
                  placeholder="Created Date"
                  fluid
                  v-model="dateCreated"
                  disabled
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
