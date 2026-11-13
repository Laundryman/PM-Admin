<script setup lang="ts">
import { useCategoryFilters } from '@/components/composables/categoryFilters copy'
import { useLocationFilters } from '@/components/composables/locationFilters'
import { usePartForm } from '@/components/composables/partForm.composable'
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
import { zodResolver } from '@primevue/forms/resolvers/zod'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { onMounted, ref, useTemplateRef } from 'vue'
import { useRouter } from 'vue-router'
import { z } from 'zod'

const cassetteRenderUrl = import.meta.env.VITE_APP_CASSETTERENDER_URL
const cassettePhotoUrl = import.meta.env.VITE_APP_CASSETTEPHOTO_URL
const cassetteIconUrl = import.meta.env.VITE_APP_CASSETTEICON_URL
const cassetteIconSrc = ref()
const cassetteRenderSrc = ref()
const cassettePhotoSrc = ref()

const partTypeComposable = usePartTypes()
const standTypeComposable = useStandTypes()
const partStore = usePartStore()
const brandStore = useBrandStore()
const productStore = useProductStore()
const locationFilters = useLocationFilters()
const categoryFilters = useCategoryFilters()
const partForm = usePartForm()
const layout = useSystemStore()
const router = useRouter()
const newPart = ref(false)
const copyPart = ref(false)
const editPart = ref(false)
//const brand = storeToRefs(layout).getActiveBrand
const toast = useToast()
const regionSelectList = ref<Region[] | null>(null)
const selectedRegion = ref()
const ms_selectedRegions = ref<number[]>([]) // MultiSelect binding
const ms_selectedCountries = ref<number[]>([]) // MultiSelect binding
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
const selectedProducts = ref<number[]>([])
const standTypesList = ref<StandType[] | null>(null)
const selectedStandTypes = ref<number[]>([])

const dateCreated = ref()
// const packShot = ref<string | null>(null)
const packShotFile = ref<File | null>(null)
// const renderImg = ref<string | null>(null)
const renderFile = ref<File | null>(null)
// const icon = ref<string | null>(null)
const iconFile = ref<File | null>(null)

const partFormTemplateRef = useTemplateRef<FormInstance>('part-form')
const { part } = storeToRefs(partStore)
const partModel = ref<Part>(
  new Part({
    name: '',
    description: '',
    partNumber: '',
    altPartNumber: '',
    customerRefNo: '',
    published: false,
    discontinued: false,
    status: 0,
    facings: 0,
    height: 0,
    width: 0,
    depth: 0,
    stock: 0,
    regionsList: '',
    countriesList: '',
    categoryId: 0,
    parentCategoryId: 0,
    categoryName: '',
    parentCategoryName: '',
    partTypeId: 0,
    partTypeName: '',
    shoppingHeight: 0,
    dateCreated: new Date(),
    dateUpdated: new Date(),
    shoppable: false,
    packShotImageSrc: '',
    render2dImage: '',
    svgLineGraphic: '',
    unitCost: 0,
    launchPrice: 0,
    launchDate: new Date(),
    presentation: '',
    productList: '',
    cassetteBio: '',
    manufacturingProcess: '',
    standTypeList: '',
    testingType: '',
    internationalPart: false,
    dmiReco: false,
    hidePrices: false,
    regions: [],
    countries: [],
    products: [],
    partType: {} as PartType,
    standTypes: [],
  }),
)

const loading = ref(true)

const initialValues = ref({
  name: partModel.value.name,
  description: partModel.value.description,
  // brandId: partModel.value.brandId,
  categoryId: partModel.value.categoryId,
  parentCategoryId: partModel.value.parentCategoryId,
  partTypeId: partModel.value.partTypeId,
  standTypeList: partModel.value.standTypeList,
  facings: partModel.value.facings,
  height: partModel.value.height,
  width: partModel.value.width,
  stock: partModel.value.stock,
  depth: partModel.value.depth,
  unitCost: partModel.value.unitCost,
  products: partModel.value.products,
  standTypes: partModel.value.standTypes,
})

onMounted(async () => {
  layout.toggleLoading()
  if (!brandStore.activeBrand) {
    toast.add({
      severity: 'warn',
      summary: 'Please select a brand to continue',
      life: 3000,
      group: 'tr',
    })
  }
  layout.layoutState.disableBrandSelect = true
  var partFilter = new PartFilter()
  partFilter.Id = Number(router.currentRoute.value.params.id) || 0
  await partStore.initialize(partFilter)
  if (router.currentRoute.value.name !== 'newPart') {
    if (part.value == null) {
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Part not found.',
        life: 5000,
      })
      router.push({ name: 'parts' })
      layout.toggleLoading()
      return
    }

    if (part.value.brandId != brandStore.activeBrand?.id) {
      const wrongBrand = brandStore.brands.find((b) => b.id === part.value.brandId)?.name ?? ''
      toast.add({
        severity: 'error',
        summary: 'Brand Mismatch',
        detail:
          'The part does not belong to the active brand. Please switch to ' +
          wrongBrand +
          ' to edit this part.',
        life: 5000,
      })
      router.push({ name: 'parts' }) // Stop further execution if brand mismatch occurs
      layout.toggleLoading()
      return
    }
  }

  partModel.value = { ...part.value } as Part //clone(part.value)

  if (router.currentRoute.value.name === 'copyPart') {
    copyPart.value = true
    partModel.value.id = 0 //reset for copy
    partModel.value.name = partModel.value.name + ' - Copy'
    partModel.value.partNumber = partModel.value.partNumber + '-COPY'
    partModel.value.packShotImageSrc = ''
    partModel.value.render2dImage = ''
    partModel.value.svgLineGraphic = ''
    cassetteRenderSrc.value = ''
    cassettePhotoSrc.value = ''
    cassetteIconSrc.value = ''
  }

  if (router.currentRoute.value.name === 'newPart') {
    newPart.value = true
    partModel.value.brandId = brandStore.activeBrand?.id ?? 0
    partModel.value.regions = new Array<Region>()
    partModel.value.countries = new Array<Country>()
  }
  if (router.currentRoute.value.name === 'editPart') {
    editPart.value = true
  }

  if (
    router.currentRoute.value.name === 'editPart' ||
    router.currentRoute.value.name === 'copyPart'
  ) {
    newPart.value = false
    initialisePartForm()
  }

  if (partModel.value.packShotImageSrc != null && partModel.value.packShotImageSrc.length > 0) {
    cassettePhotoSrc.value = cassettePhotoUrl + partModel.value.packShotImageSrc
  }
  if (partModel.value.render2dImage != null && partModel.value.render2dImage.length > 0) {
    cassetteRenderSrc.value = cassetteRenderUrl + partModel.value.render2dImage
  }
  if (partModel.value.svgLineGraphic != null && partModel.value.svgLineGraphic.length > 0) {
    cassetteIconSrc.value = partModel.value.svgLineGraphic
  }
  selectedParentCategoryId.value = partModel.value.parentCategoryId || null
  if (
    router.currentRoute.value.name === 'editPart' ||
    router.currentRoute.value.name === 'copyPart'
  ) {
    ms_selectedRegions.value = partModel.value.regions.map((c) => c.id)
    countrySelectList.value = await locationFilters.getCountriesForRegions(ms_selectedRegions.value)
    ms_selectedCountries.value = partModel.value.countries.map((c) => c.id)
    dateCreated.value = new Date(partModel.value.dateCreated) //added to bind date picker
  }

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
    // console.log('Part Types loaded', partTypes.value)
    partModel.value.partType = partTypes.value?.find(
      (pt) => pt.id === partModel.value.partTypeId,
    ) as PartType
    if (
      router.currentRoute.value.name === 'editPart' ||
      router.currentRoute.value.name === 'copyPart'
    ) {
      selectedPartType.value = partModel.value.partType.id ?? null
    }
  })

  await standTypeComposable.getStandTypes().then((response) => {
    standTypesList.value = response
    if (
      router.currentRoute.value.name === 'editPart' ||
      router.currentRoute.value.name === 'copyPart'
    ) {
      selectedStandTypes.value = partModel.value.standTypes.map((st: StandType) => st.id)
      // console.log('Stand Types loaded', standTypesList.value)
      partModel.value.standTypes = standTypesList.value?.filter((st) =>
        selectedStandTypes.value?.includes(st.id),
      ) as StandType[]
    }
  })

  let productFilter = new ProductFilter()
  productFilter.brandId = brandid
  productFilter.categoryId = partModel.value.categoryId || 0
  productFilter.isPublished = true
  await productStore.getProductsByCategory(productFilter).then((response) => {
    ms_productList.value = response ?? null
    // console.log('Products loaded', ms_productList.value)
  })

  if (
    router.currentRoute.value.name === 'editPart' ||
    router.currentRoute.value.name === 'copyPart'
  )
    selectedProducts.value = mapPubishedProducts()
  layout.toggleLoading()
})

function initialisePartForm() {
  partFormTemplateRef.value?.setValues({ ...partModel.value })
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
    partModel.value.parentCategoryId = selectedParentCategoryId.value
    partModel.value.parentCategoryName =
      parentCategories.value?.find((pc) => pc.id === selectedParentCategoryId.value)?.name || ''

    await categoryFilters.getChildCategories(selectedParentCategoryId.value).then((response) => {
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
    productFilter.brandId = brandStore.activeBrand?.id ?? 0
    productFilter.categoryId = selectedChildCategory.value
    partModel.value.categoryId = selectedChildCategory.value
    partModel.value.categoryName =
      childCategories.value?.find((cc) => cc.id === selectedChildCategory.value)?.name || ''
    await getProducts()
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
  if (selectedValuesToRemove.length > 0) {
    for (const stId of selectedValuesToRemove) {
      let removeIndex = targetArray.findIndex((st) => st.id === stId)
      if (removeIndex !== -1) {
        targetArray.splice(removeIndex, 1)
      }
    }
  }
}

////////////////////////////////////////////////////
// Location Handlers
////////////////////////////////////////////////////
async function onSelectAllRegionsChange() {
  ms_selectedRegions.value = regionSelectList.value?.map((item) => item.id) ?? []
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

  manageSelectedValues(
    ms_selectedRegions.value,
    regionSelectList.value ?? [],
    partModel.value.regions ?? [],
  )
  manageSelectedValues(
    ms_selectedCountries.value ?? [],
    countrySelectList.value ?? [],
    partModel.value.countries ?? [],
  )

  partModel.value.regions = ms_selectedRegions.value
    .map((regionId) => regionSelectList.value.find((r) => r.id === regionId))
    .filter((r) => r !== undefined)
  partModel.value.regionsList = partModel.value.regions?.map((r) => r.id).join(',') || ''
}

function clearRegionSelection() {
  ms_selectedRegions.value = []
  partModel.value.regions = []
  partModel.value.regionsList = ''
  clearCountrySelection()
}

async function onRegionChange(evt: any) {
  countrySelectList.value = await locationFilters.getCountriesForRegions(ms_selectedRegions.value)
  //remove any countries no longer in the list
  let newSelectList = ms_selectedCountries.value?.filter((countryId) =>
    countrySelectList.value?.some((c) => c.id === countryId),
  )
  manageSelectedValues(evt.value, regionSelectList.value ?? [], partModel.value.regions ?? [])

  console.log('Selected Regions after change', ms_selectedRegions.value)
  console.log('Part Model Regions after change', partModel.value)

  partModel.value.regionsList = partModel.value.regions?.map((r) => r.id).join(',') || ''
  console.log('Part Model Regions List after change', partModel.value.regionsList)

  if (newSelectList) {
    ms_selectedCountries.value = newSelectList
  } else {
    ms_selectedCountries.value = []
  }
  manageSelectedValues(
    ms_selectedCountries.value ?? [],
    countrySelectList.value ?? [],
    partModel.value.countries ?? [],
  )
}

async function onCountryChange(evt: any) {
  // manageSelectedCountries(evt.value)
  manageSelectedValues(evt.value, countrySelectList.value ?? [], partModel.value.countries ?? [])
  if (partModel.value.products != null && partModel.value.products.length > 0) {
    await getProducts()
  }

  partModel.value.countriesList = partModel.value.countries?.map((c) => c.id).join(',') || ''
}

function onSelectAllCountriesChange(event: any) {
  ms_selectedCountries.value = event.checked
    ? (countrySelectList.value?.map((item) => item.id) ?? [])
    : []
  selectAllCountries.value = event.checked
  //reset the product selection
  clearProductSelection()
  clearStandTypeSelection()
  //manageSelectedCountries(ms_selectedCountries.value)
  manageSelectedValues(
    ms_selectedCountries.value,
    countrySelectList.value ?? [],
    partModel.value.countries ?? [],
  )
  partModel.value.countriesList = partModel.value.countries?.map((c) => c.id).join(',') || ''
}

function clearCountrySelection() {
  ms_selectedCountries.value = []
  countrySelectList.value = []
  partModel.value.countries = []
  partModel.value.countriesList = ''
}

const isRegionSelected = (region) => ms_selectedRegions.value.includes(region.id)
const isCountrySelected = (country) => ms_selectedCountries.value.includes(country.id)

////////////////////////////////////////////////////
// Image Handlers
////////////////////////////////////////////////////
function onPackShotSelect(event: any) {
  packShotFile.value = event.files[0]
  const reader = new FileReader()
  reader.onload = async (e) => {
    // packShot.value = e.target?.result as string
    // partModel.value.packShotImageSrc = e.target?.result as string
    partModel.value.packShotImageSrc = ''
    cassettePhotoSrc.value = e.target?.result
  }

  reader.readAsDataURL(packShotFile.value!)
}
function onRenderSelect(event: any) {
  renderFile.value = event.files[0]
  const reader = new FileReader()
  reader.onload = async (e) => {
    // partModel.value.render2dImage = e.target?.result as string
    partModel.value.render2dImage = ''
    cassetteRenderSrc.value = e.target?.result
  }

  reader.readAsDataURL(renderFile.value!)
}
function onIconSelect(event: any) {
  iconFile.value = event.files[0]
  if (iconFile.value?.type === 'image/svg+xml') {
    const reader = new FileReader()
    reader.onload = async (e) => {
      let svgString = e.target?.result as string
      var svg = atob(svgString.replace(/data:image\/svg\+xml;base64,/, ''))
      // console.log(svg)

      partModel.value.svgLineGraphic = svg
      cassetteIconSrc.value = svgString
    }
    reader.readAsDataURL(iconFile.value!)
  }
}

/////////////////////////////////////////////////////
// Product Handlers
/////////////////////////////////////////////////////

async function getProducts() {
  let productFilter = new ProductFilter()
  productFilter.brandId = brandStore.activeBrand?.id ?? 0
  if (ms_selectedCountries == null || ms_selectedCountries.value?.length == 0) {
    toast.add({
      severity: 'warn',
      summary: 'Please select at least one country to load products',
      group: 'bc',
      life: 3000,
    })
    return
  }
  productFilter.countriesList = ms_selectedCountries.value?.join(',') ?? ''
  if (selectedChildCategory.value) {
    productFilter.categoryId = selectedChildCategory.value
  } else {
    //must select category to get products
    toast.add({
      severity: 'warn',
      summary: 'Please select a category to load products',
      group: 'bc',
      life: 3000,
    })
    return
  }
  await productStore.getProductsByCategory(productFilter).then((response) => {
    ms_productList.value = response ?? null
    console.log('Products loaded', ms_productList.value)
  })
}
function onShowProducts() {
  if (partModel.value.countries == null || partModel.value.countries.length == 0) {
    toast.add({
      severity: 'error',
      summary: 'Please select at least one country to load products',
      life: 3000,
      group: 'bc',
    })
    return
  }
  if (selectedChildCategory.value == null) {
    //must select category to get products
    toast.add({
      severity: 'error',
      summary: 'Please select a category to load products',
      life: 3000,
      group: 'bc',
    })
    return
  }
}
async function onProductChange(evt: any) {
  // manageSelectedProducts(evt.value)
  manageSelectedValues(evt.value, ms_productList.value ?? [], partModel.value.products ?? [])
  partModel.value.productList = partModel.value.products?.map((p) => p.id).join(',') || ''
}

function onSelectAllProductsChange(event: any) {
  selectedProducts.value = ms_productList.value?.map((item) => item.id) ?? []

  // selectedProducts.value = event.checked ? (ms_productList.value?.map((p) => p.id) ?? []) : []
  // selectAllProducts.value = event.checked
  // manageSelectedProducts(selectedProducts.value)
  manageSelectedValues(
    selectedProducts.value,
    ms_productList.value ?? [],
    partModel.value.products ?? [],
  )
  partModel.value.productList = partModel.value.products?.map((p) => p.id).join(',') || ''
}

function clearProductSelection() {
  partModel.value.products = []
  partModel.value.productList = ''
  selectedProducts.value = []
  partFormTemplateRef.value.fields['products'].states.value = []
}

const isProductSelected = (product) => selectedProducts.value.includes(product.id)

/////////////////////////////////////////////////////
// Part Type Handlers
////////////////////////////////////////////////////

function onPartTypeChange(evt: any) {
  let selectedId = evt.value as number
  let selectedPartTypeItem = partTypes.value?.find((pt) => pt.id === selectedId) as PartType
  partModel.value.partTypeId = selectedPartTypeItem.id
  partModel.value.partTypeName = selectedPartTypeItem.name
  partModel.value.partType = selectedPartTypeItem
}

/////////////////////////////////////////////////////
// Stand Type Handlers
/////////////////////////////////////////////////////

function onStandTypeChange(evt: any) {
  // manageSelectedStandTypes(evt.value)
  manageSelectedValues(evt.value, standTypesList.value ?? [], partModel.value.standTypes ?? [])
  partModel.value.standTypeList = partModel.value.standTypes?.map((st) => st.id).join(',') || ''
}

function onSelectAllStandTypesChange(event: any) {
  selectedStandTypes.value = standTypesList.value?.map((st) => st.id) ?? []
  manageSelectedValues(
    selectedStandTypes.value,
    standTypesList.value ?? [],
    partModel.value.standTypes ?? [],
  )
  partModel.value.standTypeList = partModel.value.standTypes?.map((st) => st.id).join(',') || ''
}

function clearStandTypeSelection() {
  partModel.value.standTypes = []
  partModel.value.standTypeList = ''
  selectedStandTypes.value = []
  partFormTemplateRef.value.fields['standTypes'].states.value = []
}

const isStandTypeSelected = (standType) => selectedStandTypes.value.includes(standType.id)

/////////////////////////////////////////////////////
// Date Handlers
////////////////////////////////////////////////////

function updateDateCreated(evnt: any) {
  console.log('Date selected', evnt)
}
/////////////////////////////////////////////////////
// Form Handlers
/////////////////////////////////////////////////////

const baseSchema = z.object({
  name: z.string().regex(/^[\p{L}0-9 !@#$£&()\-_`.+,"]+$/u, {
    message:
      'Name is required and can only contain alphanumeric characters plus some special characters.',
  }),
  description: z.string().regex(/^[\p{L}0-9 !@#$£&()\-_`.+,"]+$/u, {
    message:
      'Description is required and can only contain alphanumeric characters plus some special characters..',
  }),

  partNumber: z.string().regex(/^[\p{L}0-9 !@#$£&()\-_`.+,"]+$/u, {
    message:
      'Part Number is required and can only contain alphanumeric characters plus some special characters..',
  }),
  altPartNumber: z.optional(
    z
      .string()
      .regex(/^[\p{L}0-9 !@#$£&()\-_`.+,"]*$/u, {
        message:
          'Alt Part Number can only contain alphanumeric characters plus some special characters..',
      })
      .nullable(),
  ),
  customerReference: z.optional(
    z
      .string()
      .regex(/^[\p{L}0-9 !@#$£&()\-_`.+,"]*$/u, {
        message:
          'Customer Reference can only contain alphanumeric characters plus some special characters..',
      })
      .nullable(),
  ),

  categoryId: z.number().min(1, { message: 'Category is required.' }),
  parentCategoryId: z.number().min(1, { message: 'Parent Category is required.' }),
  partTypeId: z.number().min(1, { message: 'Part Type is required.' }),
  facings: z.number().min(1, { message: 'Facings is required.' }),
  height: z.number().min(1, { message: 'Height is required.' }),
  width: z.number().min(1, { message: 'Width is required.' }),
  stock: z.number().min(1, { message: 'Stock is required.' }),
  products: z.array(z.number()).min(1, { message: 'At least one product is required.' }),
  standTypes: z.array(z.number()).min(1, { message: 'At least one stand type is required.' }),
  // shoppingHeight: z.number().min(0, { message: 'Shopping Height is required.' }),
  // depth: z.number().min(0, { message: 'Depth is required.' }),
  // unitCost: z.number().min(0, { message: 'Unit Cost is required.' }),

  // msregions: z.array(z.number()).min(1, { message: 'At least one region is required.' }),
  // countries: z.array(z.number()).min(1, { message: 'At least one country is required.' }),
})

function validateCountrySelection() {
  const countriesField = partFormTemplateRef.value.fields?.['countries']
  if (!ms_selectedCountries || ms_selectedCountries.value.length === 0) {
    countriesField.states.invalid = true
    countriesField.states.error = { message: 'At least one country is required' }
    return false
  }
  return true
}
const resolver = ref(zodResolver(baseSchema))

async function onFormSubmit({ valid }: any) {
  if (!validateCountrySelection()) {
    return
  }
  if (valid) {
    //manage file uploads
    let partData = partForm.createPartFormData(partModel)

    if (packShotFile.value) {
      partData.append('packShotFile', packShotFile.value)
    }
    if (renderFile.value) {
      partData.append('renderFile', renderFile.value)
    }
    if (iconFile.value) {
      partData.append('iconFile', iconFile.value)
    }
    await partStore
      .savePart(partData, partModel.value.id ?? 0)
      .then(() => {
        toast.add({
          severity: 'success',
          summary: 'Form is submitted.',
          life: 3000,
        })
        router.push({ name: 'parts' })
      })
      .catch((error) => {
        toast.add({
          severity: 'error',
          summary: 'Error saving part: ' + error,
          life: 5000,
        })
      })
  }
}
function onCancel() {
  router.back()
}
const textFieldInputTooltip = `<div class="flex items-center">
  <span>Text fields can only contain alphanumeric characters plus some special characters.</span>
  <div class="ml-2">! @ # $ £ & ( ) - _ \` . + , </div>
  </div>`
</script>
<style>
.cassette-icon svg {
  max-width: 100%;
  max-height: 200px;
}
</style>
<template>
  <div class="edit-part-view">
    <Toast position="top-right" group="tr" />
    <Toast position="bottom-center" group="bc" />
    <div class="mb-5">
      <Button
        label="Back to Parts"
        icon="pi pi-arrow-left"
        class="p-button-text"
        @click="router.back()"
      />
    </div>
    <div class="w-full sticky bg-white top-16 block p-10 z-10">
      <h2>
        <span v-if="editPart">Edit Part</span>
        <span v-else-if="copyPart">Copy Part</span>
        <span v-else-if="newPart">New Part</span>
      </h2>
      <div class="card flex flex-col gap-2">
        <span v-if="router.currentRoute.value.name !== 'newPart'" class="font-bold text-xl">{{
          part?.name
        }}</span>
        <span v-else class="font-bold text-xl">{{ partModel?.name }}</span>
        <span class="text-gray-600">Part No: {{ partModel?.partNumber }}</span>
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
                  v-tooltip="{ value: textFieldInputTooltip, escape: false }"
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
                  v-tooltip.bottom="{ value: textFieldInputTooltip, escape: false }"
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
                  v-tooltip="{ value: textFieldInputTooltip, escape: false }"
                  v-model="partModel.partNumber"
                  name="partNumber"
                  type="text"
                  placeholder="Part Number"
                  fluid
                  length="255"
                />
                <Message
                  v-if="$form.partNumber?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.partNumber.error?.message }}</Message
                >
              </div>
              <div class="flex flex-col gap-1">
                <label for="altPartNumber">Alt Part Number:</label>
                <InputText
                  v-tooltip.bottom="{ value: textFieldInputTooltip, escape: false }"
                  v-model="partModel.altPartNumber"
                  name="altPartNumber"
                  type="text"
                  placeholder="Alt Part Number"
                  fluid
                  length="255"
                />
                <Message
                  v-if="$form.altPartNumber?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.altPartNumber.error?.message }}</Message
                >
              </div>

              <div class="flex flex-col gap-1">
                <label for="description">Description:</label>
                <InputText
                  v-tooltip="{ value: textFieldInputTooltip, escape: false }"
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
                <!-- <div v-html="cassetteIconSrc" class="cassette-icon max-w-40"></div> -->
                <!-- <img :src="cassetteIconSrc" class="cassette-icon max-w-40"></img> -->
                <span
                  v-if="partModel.svgLineGraphic != null && partModel.svgLineGraphic.length != 0"
                  class="cassette-icon max-w-40"
                  v-html="partModel.svgLineGraphic"
                >
                </span>

                <Skeleton
                  v-if="partModel.svgLineGraphic == null || partModel.svgLineGraphic.length == 0"
                  height="8rem"
                  width="8rem"
                  class="mb-2"
                ></Skeleton>
                <FileUpload
                  name="iconFile"
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
                  v-if="cassettePhotoSrc != null && cassettePhotoSrc.length != 0"
                  :src="cassettePhotoSrc"
                  alt="Pack Shot"
                  class="block m-auto pb-4 max-h-60"
                />
                <Skeleton
                  v-if="cassettePhotoSrc == null || cassettePhotoSrc.length == 0"
                  height="8rem"
                  width="8rem"
                  class="mb-2"
                ></Skeleton>

                <FileUpload
                  name="packShotFile"
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
                  v-if="cassetteRenderSrc != null && cassetteRenderSrc.length != 0"
                  :src="cassetteRenderSrc"
                  alt="Pack Shot"
                  class="block m-auto pb-4 max-h-60"
                />
                <Skeleton
                  v-if="cassetteRenderSrc == null || cassetteRenderSrc.length == 0"
                  height="8rem"
                  width="8rem"
                  class="mb-2"
                ></Skeleton>

                <FileUpload
                  name="renderFile"
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
                  name="region"
                  multiple
                  v-model="ms_selectedRegions"
                  :options="regionSelectList ?? []"
                  id="region"
                  class="w-full"
                  option-label="name"
                  option-value="id"
                  @change="onRegionChange"
                >
                  <template #header>
                    <div class="flex items-center gap-1 px-3 pt-2">
                      <Button variant="text" size="small" @click="onSelectAllRegionsChange()"
                        >Select All</Button
                      >
                      <Button
                        variant="text"
                        size="small"
                        severity="secondary"
                        @click="clearRegionSelection"
                        >Clear</Button
                      >
                    </div>
                  </template>
                  <template #option="slotProps">
                    <div class="flex items-center gap-2">
                      <Checkbox
                        :modelValue="isRegionSelected(slotProps.option)"
                        binary
                        :tabindex="-1"
                        readonly
                      />
                      <span>{{ slotProps.option.name }}</span>
                    </div>
                  </template>
                </Select>
              </div>
              <div class="flex flex-col gap-2">
                <label for="country">Country:</label>
                <Select
                  placeholder="Select a region first"
                  multiple
                  name="countries"
                  v-model="ms_selectedCountries"
                  :options="countrySelectList ?? []"
                  id="country"
                  class="w-full"
                  option-label="name"
                  option-value="id"
                  @change="onCountryChange"
                >
                  <template #header>
                    <div class="flex items-center gap-1 px-3 pt-2">
                      <Button
                        variant="text"
                        size="small"
                        @click="onSelectAllCountriesChange($event)"
                        >Select All</Button
                      >
                      <Button
                        variant="text"
                        size="small"
                        severity="secondary"
                        @click="clearCountrySelection"
                        >Clear</Button
                      >
                    </div>
                  </template>
                  <template #option="slotProps">
                    <div class="flex items-center gap-2">
                      <Checkbox
                        :modelValue="isCountrySelected(slotProps.option)"
                        binary
                        :tabindex="-1"
                        readonly
                      />
                      <span>{{ slotProps.option.name }}</span>
                    </div>
                  </template>
                </Select>
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
                <Select
                  multiple
                  name="products"
                  v-model="selectedProducts"
                  :options="ms_productList ?? []"
                  id="products"
                  class="w-full"
                  option-label="name"
                  option-value="id"
                  @change="onProductChange"
                >
                  <template #header>
                    <div class="flex items-center gap-1 px-3 pt-2">
                      <Button variant="text" size="small" @click="onSelectAllProductsChange($event)"
                        >Select All</Button
                      >
                      <Button
                        variant="text"
                        size="small"
                        severity="secondary"
                        @click="clearProductSelection"
                        >Clear</Button
                      >
                    </div>
                  </template>
                  <template #option="slotProps">
                    <div class="flex items-center gap-2">
                      <Checkbox
                        :modelValue="isProductSelected(slotProps.option)"
                        binary
                        :tabindex="-1"
                        readonly
                      />
                      <span>{{ slotProps.option.name }}</span>
                    </div>
                  </template>
                </Select>
                <Message
                  v-if="$form.products?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.products.error?.message }}</Message
                >
                <!-- <MultiSelect
                  v-model="selectedProducts"
                  :options="ms_productList ?? []"
                  id="products"
                  name="products"
                  class="w-full"
                  option-label="name"
                  option-value="id"

                  @show="onShowProducts()"
                  :selectAll="selectAllProducts"
                  @selectall-change="onSelectAllProductsChange($event)"
                >
                  <template #option="option">
                    <div class="flex align-items-center">
                      <span>{{ option.option.name }}</span>
                    </div>
                  </template>
                </MultiSelect> -->
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
                <Select
                  multiple
                  name="standTypes"
                  v-model="selectedStandTypes"
                  :options="standTypesList ?? []"
                  id="standTypes"
                  class="w-full"
                  option-label="name"
                  option-value="id"
                  @change="onStandTypeChange"
                >
                  <template #header>
                    <div class="flex items-center gap-1 px-3 pt-2">
                      <Button
                        variant="text"
                        size="small"
                        @click="onSelectAllStandTypesChange($event)"
                        >Select All</Button
                      >
                      <Button
                        variant="text"
                        size="small"
                        severity="secondary"
                        @click="clearStandTypeSelection"
                        >Clear</Button
                      >
                    </div>
                  </template>
                  <template #option="slotProps">
                    <div class="flex items-center gap-2">
                      <Checkbox
                        :modelValue="isStandTypeSelected(slotProps.option)"
                        binary
                        :tabindex="-1"
                        readonly
                      />
                      <span>{{ slotProps.option.name }}</span>
                    </div>
                  </template>
                </Select>
                <Message
                  v-if="$form.standTypes?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.standTypes.error?.message }}</Message
                >
                <!-- <MultiSelect
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
                </MultiSelect> -->
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
                  dateFormat="dd/mm/yy"
                  placeholder="Created Date"
                  fluid
                  v-model="partModel.dateCreated"
                  disabled
                  @date-select="updateDateCreated($event)"
                />
              </div>
            </div>
          </fieldset>
        </div>

        <Button
          type="button"
          label="Cancel"
          severity="secondary"
          class="mr-2 w-1/2"
          @click="onCancel"
        />
        <Button type="submit" severity="primary" label="Submit" class="w-1/2 ml-2" />
      </Form>
    </div>
  </div>
</template>
