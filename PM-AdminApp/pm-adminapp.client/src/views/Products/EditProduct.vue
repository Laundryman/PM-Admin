
<script setup lang="ts">
import { useCategoryFilters } from '@/components/composables/categoryFilters copy'
import { useLocationFilters } from '@/components/composables/locationFilters'
import { usePartTypes } from '@/components/composables/partTypes.composable'
import { useStandTypes } from '@/components/composables/standTypes.composable'
import { Category } from '@/models/Categories/category.model'
import { Country } from '@/models/Countries/country.model'
import { Region } from '@/models/Countries/region.model'
import { regionFilter } from '@/models/Countries/regionFilter.model'

import { useMultiSelectLists } from '@/components/composables/multiSelectList.composable'
import { useShadeManagement } from '@/components/composables/shademanagement.composable'
import { Product } from '@/models/Products/product.model'
import { ProductFilter } from '@/models/Products/productFilter.model'
import { Shade } from '@/models/Products/shade.model'
import { StandType } from '@/models/StandTypes/standType.model'
import shadeService from '@/services/Shades/ShadeService'
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


const brandStore = useBrandStore()
const partStore = usePartStore()
const productStore = useProductStore()
const systemStore = useSystemStore()
const locationFilters = useLocationFilters()
const categoryFilters = useCategoryFilters()
const layout = useSystemStore()
const router = useRouter()
const multiSelectLists = useMultiSelectLists()
const partTypeComposable = usePartTypes()
const standTypeComposable = useStandTypes()

const productform = useTemplateRef<FormInstance>('product-form')
const { product } = storeToRefs(productStore)
    const productModel = ref<Product>(new Product())

const initialValues = ref(new Product())

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

const productImageUrl = import.meta.env.VITE_APP_PRODUCTIMAGE_URL
const productImageSrc = ref()
const imageFile = ref<File | null>(null)
const tabId = ref('0')


// Shade Management
const shademanagement = useShadeManagement()

const { shades, shade, shadeDialog, submitted, shade_countrySelectList, shade_selectedCountries, shade_selectAllCountries } = shademanagement
/////////////////////////////////////////////////////
// Lifecycle Hooks
/////////////////////////////////////////////////////
onMounted(async () => {
  // await initialiseProductForm()
  layout.layoutState.disableBrandSelect = true
  var productFilter = new ProductFilter()
  productFilter.id = Number(router.currentRoute.value.params.id) || 0
   await productStore.initialize(productFilter.id)
   await productStore.getShadesForProduct(productFilter.id).then((response) => {
    shades.value = response
    console.log('Shades loaded', shades.value)
  })
  productModel.value = { ...product.value } as Product //clone(product.value)

  if (router.currentRoute.value.name === 'copyPart') {
    productModel.value.id = 0 //reset for copy
    productModel.value.name = productModel.value.name + ' - Copy'
    productModel.value.fullDescription = productModel.value.fullDescription + '-COPY'
    productModel.value.productImage = ''

    productImageSrc.value = ''
  }

  if (router.currentRoute.value.name === 'newProduct') {
    productModel.value.brandId = brandStore.activeBrand?.id ?? 0
  }

  if (productModel.value.productImage != null && productModel.value.productImage.length > 0) {
    productImageSrc.value = productImageUrl + productModel.value.productImage
  }
  selectedParentCategoryId.value = productModel.value.parentCategoryId || null
  if (router.currentRoute.value.name === 'editProduct') {
    ms_selectedRegions.value = productModel.value.regions.map((c) => c.id)
    countrySelectList.value = await locationFilters.getCountriesForRegions(ms_selectedRegions.value)
    shademanagement.setCountrySelectList(countrySelectList.value)
    ms_selectedCountries.value = productModel.value.countries.map((c) => c.id)
    shademanagement.setSelectedCountries(ms_selectedCountries.value)
    // dateCreated.value = new Date(productModel.value.dateCreated) //added to bind date picker
  }

  if (selectedParentCategoryId.value) {
    await categoryFilters.getChildCategories(selectedParentCategoryId.value).then((response) => {
      childCategories.value = response
      console.log('Child Categories loaded', childCategories.value)
    })
  }
  selectedChildCategory.value = productModel.value.categoryId || null
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




})

////////////////////////////////////////////////////
// Location Handlers
////////////////////////////////////////////////////

async function onRegionChange(evt: any) {
  countrySelectList.value = await locationFilters.getCountriesForRegions(ms_selectedRegions.value)
  shademanagement.setCountrySelectList(countrySelectList.value)
  //remove any countries no longer in the list
  let newSelectList = ms_selectedCountries.value?.filter((countryId) =>
    countrySelectList.value?.some((c) => c.id === countryId),
  )
  if (newSelectList) {
    ms_selectedCountries.value = newSelectList
  } else {
    ms_selectedCountries.value = []
  }
  multiSelectLists.manageSelectedValues(evt.value, regionSelectList.value ?? [], productModel.value.regions ?? [])
  multiSelectLists.manageSelectedValues(
    ms_selectedCountries.value ?? [],
    countrySelectList.value ?? [],
    productModel.value.countries ?? [],
  )

  productModel.value.regionsList = productModel.value.regions?.map((r) => r.id).join(',') || ''
}

async function onCountryChange(evt: any) {
  // manageSelectedCountries(evt.value)
  multiSelectLists.manageSelectedValues(evt.value, countrySelectList.value ?? [], productModel.value.countries ?? [])
  console.log('Selected Countries after region change', ms_selectedCountries.value)
  console.log('Part Model Countries after region change', productModel.value.countries)
  let someArray = productModel.value.countries ?? []

  productModel.value.countriesList = productModel.value.countries?.map((c) => c.id).join(',') || ''
}

function onSelectAllCountriesChange(event: any) {
  ms_selectedCountries.value = event.checked
    ? (countrySelectList.value?.map((item) => item.id) ?? [])
    : []
  selectAllCountries.value = event.checked
  multiSelectLists.manageSelectedValues(
    ms_selectedCountries.value,
    countrySelectList.value ?? [],
    productModel.value.countries ?? [],
  )
  productModel.value.countriesList = productModel.value.countries?.map((c) => c.id).join(',') || ''
}

function clearCountrySelection() {
  ms_selectedCountries.value = []
  productModel.value.countries = []
  productModel.value.countriesList = ''
}



//////////////////////////////////////////////////////
// Image Handlers
//////////////////////////////////////////////////////

function onProductImageSelect(event: any) {
  imageFile.value = event.files[0]
  const reader = new FileReader()
  reader.onload = async (e) => {
    // partModel.value.render2dImage = e.target?.result as string
    productModel.value.productImage = ''
    productImageSrc.value = e.target?.result
  }

  reader.readAsDataURL(imageFile.value!)
}

/////////////////////////////////////////////////////
// Category Handlers
/////////////////////////////////////////////////////
async function onParentCategoryChange() {
  if (selectedParentCategoryId.value) {
    productModel.value.parentCategoryId = selectedParentCategoryId.value
    productModel.value.parentCategoryName =
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
    productModel.value.categoryId = selectedChildCategory.value
    productModel.value.categoryName =
      childCategories.value?.find((cc) => cc.id === selectedChildCategory.value)?.name || ''
    // await getProducts()
  } else {
    ms_productList.value = []
  }
}

/////////////////////////////////////////////////////
// Form Handlers
/////////////////////////////////////////////////////
const resolver = ({ values }: any) => {
  const errors = {} as any

  if (!values.name) {
    errors.name = [{ message: 'Name is required.' }]
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
  return {
    values, // (Optional) Used to pass current form values to submit event.
    errors,
  }
}

async function onFormSubmit({ valid }: any) {
  if (valid) {
    //manage file uploads
    // let partData = partForm.createPartFormData(partModel)

    // await partStore
    //   .savePart(partData, partModel.value.id ?? 0)
    //   .then(() => {
    //     toast.add({
    //       severity: 'success',
    //       summary: 'Form is submitted.',
    //       life: 3000,
    //     })
    //     //router.push({ name: 'partList' })
    //   })
    //   .catch((error) => {
    //     toast.add({
    //       severity: 'error',
    //       summary: 'Error saving part: ' + error,
    //       life: 5000,
    //     })
    //   })
  }
}

///////////////////////////////////////////////////
// Shade Dialog Handlers
//
//////////////////////////////////////////////////

const openNew = () => {
  shade.value = {} as Shade
  submitted.value = false
  shadeDialog.value = true
}
const hideDialog = () => {
  shadeDialog.value = false
  submitted.value = false
}

function editShade(sh: Shade) {
  shade.value = sh
  shadeDialog.value = true
}


async function saveShade() {
  submitted.value = true

  const formData = new FormData()


  if (shade?.value.shadeNumber?.trim()) {
    formData.append('name', shade.value.shadeNumber)
    formData.append('shelfLock', String(shade.value.published))
    formData.append('id', String(shade.value.id ?? 0))
    if (shade.value.id) {
      await shadeService
        .updateShade(formData)
        .then((response) => {
          if (response && response.data) {
            console.log(response.data)
          }
        })
        .catch((error) => {
          console.error('Error updating shade:', error)
        })
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'Shade Updated',
        life: 3000,
      })
    } else {
      shade.value.id = 0
      // shades.value.push(shade.value)
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'Shade Created',
        life: 3000,
      })
    }

    shadeDialog.value = false
    shade.value = new Shade()
  }
}

</script>


<template>
  <div class="edit-product-view">
    <Toast position="top-right" group="tr" />
    <Toast position="bottom-center" group="bc" />
    <div class="mb-5">
      <Button
        label="Back to Products"
        icon="pi pi-arrow-left"
        class="p-button-text"
        @click="router.back()"
      />
    </div>
    <div class="w-full sticky bg-white top-16 block p-10 z-10">
      <h2>Edit Product</h2>
      <div class="card grid gap-2 grid-cols-2">
        <div class="flex flex-col gap-1">
          <span class="font-bold text-xl">{{ productModel.name }}</span>
          <span class="text-gray-600">Description: {{ productModel.fullDescription }}</span>
        </div>
        <div class="flex flex-col gap-1">
            <img :src="productImageSrc" class="cassette-icon max-w-40"></img>
        </div>
      </div>
    <div class="w-full p-10">

          <Button @click="tabId = '0'"  label="Product Details" class="" :outlined="tabId !== '0'" />
          <Button @click="tabId = '1'"  label="Shades" class="" :outlined="tabId !== '1'" />
    </div>
    </div>


    <div class="card grid grid-cols-1 gap-4 justify-center">
<Tabs v-model:value="tabId">
  <!-- <TabList>
        <Tab value="0">Part Details</Tab>
        <Tab value="1">Shades</Tab>
  </TabList> -->
  <TabPanels>
     <TabPanel value="0">
      <Form
        ref="product-form"
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
                <label for="published">Discontinued:</label>
                <ToggleButton
                  name="published"
                  v-model="productModel.discontinued"
                  onLabel="Yes"
                  offLabel="No"
                  onIcon="pi pi-check"
                  offIcon="pi pi-times"
                  class="w-24 mt-2"
                ></ToggleButton>
              </div>
              <div class="flex flex-col gap-1">
                <label for="productImage">Product Image:</label>
                <img :src="productImageSrc" class="cassette-icon max-w-40"></img>
                <Skeleton
                  v-if="productImageSrc == null || productImageSrc.length == 0"
                  height="8rem"
                  width="8rem"
                  class="mb-2"
                ></Skeleton>
                <FileUpload
                  name="iconFile"
                  ref="fileupload"
                  mode="basic"
                  @select="onProductImageSelect"
                  customUpload
                  accept="image/*"
                  :maxFileSize="1000000"
                />
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
                  v-model="productModel.name"
                  name="name"
                  :value="productModel.name"
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
                <label for="description">Description:</label>
                <InputText
                  v-model="productModel.fullDescription"
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
                <template v-for="country in productModel.countries">
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
            </div>
          </fieldset>
        </div>


        <Button type="submit" severity="secondary" label="Submit" />
      </Form>

     </TabPanel>
     <TabPanel value="1">
        <div class="p-10">
          <h3>Shades</h3>
              <div class="card">
                <DataTable :value="shades" scrollable scrollHeight="400px" tableStyle="min-width: 50rem">
                    <Column field="shadeNumber" header="Name"></Column>
                    <Column field="shadeDescription" header="Description"></Column>
                    <Column field="published" header="Published">
                      <template #body="slotProps">
                        <div class="flex align>-items-center gap-2">
                          <span>{{ slotProps.data.published ? 'Yes' : 'No' }}</span>
                          </div>
                      </template>
                    </Column>
                    <Column field="countries" header="Countries">
                    <template #body="slotProps">
                        <div class="flex align-items-center gap-2">
                          <Listbox :options="slotProps.data.countries" option-label="name" class="w-full" listStyle="max-height:100px">

                          </Listbox>
                        </div>
                      </template>
                    </Column>
                    <Column :exportable="false" style="min-width: 12rem">
                      <template #body="slotProps">
                        <Button
                          icon="pi pi-pencil"
                          variant="outlined"
                          rounded
                          class="mr-2"
                          @click="editShade(slotProps.data)"
                        />
                      </template>
                    </Column>
                </DataTable>
              </div>
        </div>
     </TabPanel>
  </TabPanels>
  </Tabs>

    <Dialog
      v-model:visible="shadeDialog"
      :style="{ width: '450px' }"
      header="Shade Details"
      :modal="true"
    >
      <div class="flex flex-col gap-6">
          <div>
          <label for="name" class="block font-bold mb-3">Name</label>
          <InputText
            id="name"
            v-model.trim="shade.shadeNumber"
            required="true"
            autofocus
            :invalid="submitted && !shade.shadeNumber"
            fluid
          />
          <small v-if="submitted && !shade.shadeNumber" class="text-red-500">Name is required.</small>
        </div>
                  <div>
          <label for="description" class="block font-bold mb-3">Description</label>
          <InputText
            id="description"
            v-model.trim="shade.shadeDescription"
            required="true"
            autofocus
            :invalid="submitted && !shade.shadeDescription"
            fluid
          />
          <small v-if="submitted && !shade.shadeDescription" class="text-red-500">Description is required.</small>
        </div>
        <div>
          <label for="published" class="block font-bold mb-3">Published</label>
          <ToggleSwitch name="published" v-model="shade.published" />
        </div>
              <div class="flex flex-col gap-2">
                <div>
                <label for="country">Country:</label>
                <MultiSelect
                  name="countries"
                  v-model="shade_selectedCountries"
                  :options="shade_countrySelectList ?? []"
                  id="country"
                  class="w-full"
                  option-label="name"
                  option-value="id"
                  @change="shademanagement.onCountryChange($event)"
                  :selectAll="shade_selectAllCountries"
                  @selectall-change="shademanagement.onSelectAllCountriesChange($event)"
                >
                  <template #option="option">
                    <div class="">
                      <span>{{ option.option.name }}</span>
                    </div>
                  </template>
                </MultiSelect>
                <!-- <Message
                  v-if="shade_selectedCountries?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.countries.error?.message }}</Message -->
                >
              </div>
              <div class="flex gap-2 flex-wrap max-h-40 overflow-auto">
                <Button
                  label="Clear Selection"
                  class="w-text-left"
                  @click="shademanagement.clearCountrySelection"
                />
                <template v-for="country in shade.countries">
                  <Chip class="flex-wrap" :label="country.name"></Chip>
                </template>
              </div>
            </div>

      </div>
      <template #footer>
        <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
        <Button label="Save" icon="pi pi-check" @click="saveShade" />
      </template>
    </Dialog>
    </div>
  </div>
</template>

