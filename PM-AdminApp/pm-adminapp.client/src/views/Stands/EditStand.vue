<script setup lang="ts">
import { useLocationFilters } from '@/components/composables/locationFilters'
import { useMultiSelectLists } from '@/components/composables/multiSelectList.composable'
import { useStandTypes } from '@/components/composables/standTypes.composable'
import { Country } from '@/models/Countries/country.model'
import { Region } from '@/models/Countries/region.model'
import { StandType } from '@/models/StandTypes/standType.model'

import { regionFilter } from '@/models/Countries/regionFilter.model'
import { Column, Row, Upright } from '@/models/Stands/columns.model'
import { searchStandInfo } from '@/models/Stands/searchStandInfo.model'
import { Stand } from '@/models/Stands/stand.model'
import { StandFilter } from '@/models/Stands/standFilter.model'
import { useBrandStore } from '@/stores/brandStore'
import { useStandStore } from '@/stores/standStore'
import { useSystemStore } from '@/stores/systemStore'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'

/// consider this selector -> https://codepen.io/jmhmd/pen/JdPGgW
const router = useRouter()
const multiSelectLists = useMultiSelectLists()
const standTypes = useStandTypes()
const standModel = ref<Stand>(new Stand())
const stands = ref<searchStandInfo[]>([])
const selectedStands = ref<searchStandInfo[]>([])
const toast = useToast()
const loading = ref(true)
const layout = useSystemStore()
const brandStore = useBrandStore()
const brand = storeToRefs(brandStore).activeBrand
const standStore = useStandStore()
const colsCount = ref<number>()
const pitchCount = ref<number>()
const colsTable = ref<Column[] | null>(null)
const rowsCount = ref<number>()
const rowsTable = ref<Row[] | null>(null)

const locationFilters = useLocationFilters()
const selectedRegion = ref()
const selectedCountry = ref()
const countrySelectList = ref<Country[] | null>(null)
const regionSelectList = ref<Region[] | null>(null)
const ms_selectedRegions = ref<number[] | null>(null) // MultiSelect binding
const ms_selectedCountries = ref<number[] | null>(null) // MultiSelect binding
const selectAllCountries = ref(false)
const selectAllProducts = ref(false)
const standTypesList = ref<StandType[] | null>(null)
const selectedStandType = ref<number | null>(null)

const headerGraphicUrl = import.meta.env.VITE_APP_HEADERGRAPHIC_URL
const headerGraphicSrc = ref()
const imageFile = ref<File | null>(null)
const initialValues = ref(new Stand())
const tabId = ref('0')
const standLayoutStylesList = ref([
  { name: 'Column', id: 1 },
  { name: 'Pitch', id: 2 },
  { name: 'Column + Pitch', id: 3 },
])
/////////////////////////////////////////////////////
// Lifecycle Hooks
/////////////////////////////////////////////////////
onMounted(async () => {
  // await initialiseProductForm()
  layout.layoutState.disableBrandSelect = true
  var standFilter = new StandFilter()
  standFilter.id = Number(router.currentRoute.value.params.id) || 0
  await standStore.initialize(standFilter.id)
  standModel.value = { ...standStore.stand } as Stand //clone(product.value)
  colsTable.value = standModel.value.columnList ?? []
  rowsTable.value = standModel.value.rowList ?? []
  let brandid = brandStore.activeBrand?.id ?? 0
  let rFilter = new regionFilter()
  rFilter.brandId = brandid
  await locationFilters.getRegions(rFilter).then((response) => {
    regionSelectList.value = response
  })

  if (router.currentRoute.value.name === 'copyPart') {
    standModel.value.id = 0 //reset for copy
    standModel.value.name = standModel.value.name + ' - Copy'
    standModel.value.description = standModel.value.description + '-COPY'
    standModel.value.headerGraphic = ''

    headerGraphicSrc.value = ''
  }

  if (router.currentRoute.value.name === 'newProduct') {
    standModel.value.brandId = brandStore.activeBrand?.id ?? 0
  }

  if (standModel.value.headerGraphic != null && standModel.value.headerGraphic.length > 0) {
    headerGraphicSrc.value = headerGraphicUrl + standModel.value.headerGraphic
  }
  if (router.currentRoute.value.name === 'editStand') {
    ms_selectedRegions.value = standModel.value.regions.map((c) => c.id)
    countrySelectList.value = await locationFilters.getCountriesForRegions(ms_selectedRegions.value)
    ms_selectedCountries.value = standModel.value.countries.map((c) => c.id)
    // dateCreated.value = new Date(standModel.value.dateCreated) //added to bind date picker
  }

  await standTypes.getStandTypes().then((response) => {
    standTypesList.value = response
    standModel.value.standType = standTypesList.value?.find(
      (st) => st.id === standModel.value.standTypeId,
    ) as StandType
    if (router.currentRoute.value.name === 'editStand') {
      selectedStandType.value = standModel.value.standType.id ?? null
    }
  })
})

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
  multiSelectLists.manageSelectedValues(
    evt.value,
    regionSelectList.value ?? [],
    standModel.value.regions ?? [],
  )
  multiSelectLists.manageSelectedValues(
    ms_selectedCountries.value ?? [],
    countrySelectList.value ?? [],
    standModel.value.countries ?? [],
  )

  standModel.value.regionsList = standModel.value.regions?.map((r) => r.id).join(',') || ''
}

async function onCountryChange(evt: any) {
  // manageSelectedCountries(evt.value)
  multiSelectLists.manageSelectedValues(
    evt.value,
    countrySelectList.value ?? [],
    standModel.value.countries ?? [],
  )
  console.log('Selected Countries after region change', ms_selectedCountries.value)
  console.log('Part Model Countries after region change', standModel.value.countries)
  let someArray = standModel.value.countries ?? []

  standModel.value.countriesList = standModel.value.countries?.map((c) => c.id).join(',') || ''
}

function onSelectAllCountriesChange(event: any) {
  ms_selectedCountries.value = event.checked
    ? (countrySelectList.value?.map((item) => item.id) ?? [])
    : []
  selectAllCountries.value = event.checked
  multiSelectLists.manageSelectedValues(
    ms_selectedCountries.value,
    countrySelectList.value ?? [],
    standModel.value.countries ?? [],
  )
  standModel.value.countriesList = standModel.value.countries?.map((c) => c.id).join(',') || ''
}

function clearCountrySelection() {
  ms_selectedCountries.value = []
  standModel.value.countries = []
  standModel.value.countriesList = ''
}

////////////////////////////////////////////////////
// Stand Layout Handlers (rows/columns/uprights)
/////////////////////////////////////////////////////
watch(colsCount, async (newValue: number | undefined) => {
  //colsTable.value = []
  if (newValue == undefined) newValue = 0

  if (colsCount.value == null || colsTable.value == null) {
    colsTable.value = []
  }
  if (newValue < standModel.value.cols) {
    colsTable.value = standModel.value.columnList?.slice(0, newValue) ?? []
  }
  if (newValue > standModel.value.cols) {
    for (let i = standModel.value.cols - 1; i < (newValue ?? 0); i++) {
      var col = new Column()
      col.position = i + 1
      colsTable.value.push(col)
    }
    standModel.value.columnList = colsTable.value
  }

  standModel.value.cols = newValue ?? 0
  standModel.value.defaultColWidth = Math.floor(standModel.value.merchWidth / (newValue ?? 1))
  if (standModel.value.equalCols) {
    standModel.value.totalColWidth =
      (standModel.value.cols ?? 0) * (standModel.value.defaultColWidth ?? 0)
  } else {
    standModel.value.totalColWidth =
      standModel.value.columnList?.reduce((total, col) => total + (col.width ?? 0), 0) ?? 0
  }
})

watch(rowsCount, async (newValue: number | undefined) => {
  //colsTable.value = []
  if (newValue == undefined) newValue = 0

  if (rowsCount.value == null || rowsTable.value == null) {
    rowsTable.value = []
  }
  if (newValue < standModel.value.rows) {
    rowsTable.value = standModel.value.rowList?.slice(0, newValue) ?? []
  }
  if (newValue > standModel.value.rows) {
    for (let i = standModel.value.rows - 1; i < (newValue ?? 0); i++) {
      var row = new Row()
      row.position = i + 1
      rowsTable.value.push(row)
    }
    standModel.value.rowList = rowsTable.value
  }

  standModel.value.rows = newValue ?? 0
  standModel.value.defaultRowHeight = Math.floor(standModel.value.merchHeight / (newValue ?? 1))
  if (standModel.value.equalRows) {
    standModel.value.totalRowHeight =
      (standModel.value.rows ?? 0) * (standModel.value.defaultRowHeight ?? 0)
  } else {
    standModel.value.totalRowHeight =
      standModel.value.rowList?.reduce((total, row) => total + (row.height ?? 0), 0) ?? 0
  }
})

function addUpright(position: number) {
  let col = colsTable.value?.find((c) => c.position === position)
  if (col) {
    if (!col.uprights) {
      col.uprights = []
    }
    let upright = new Upright()
    upright.position = (col.uprights.length ?? 0) + 1

    col.uprights.push(upright)
  }
}

function delUpright(colPosition: number, position: number) {
  let col = colsTable.value?.find((c) => c.position === colPosition)
  let upright = col?.uprights?.find((u) => u.position === position)
  if (col && upright && col.uprights) {
    col.uprights = col.uprights.filter((u) => u.position !== position)
  } //
  //if (col && col.uprights && col.uprights.length > 0) { col.uprights.pop() } }
}
//Column Layout Change Handler
function changeColLayout(event: any) {
  if (event.value) {
    standModel.value.defaultColWidth = Math.floor(
      standModel.value.merchWidth / (standModel.value.cols ?? 1),
    )
  }
}

//Column Layout Change Handler
function changeRowLayout(event: any) {
  if (event.value) {
    standModel.value.defaultRowHeight = Math.floor(
      standModel.value.merchHeight / (standModel.value.rows ?? 1),
    )
  }
}

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
</script>

<template>
  <div>
    <h1>Edit Stand</h1>
    <div class="edit-stand-view">
      <Toast position="top-right" group="tr" />
      <Toast position="bottom-center" group="bc" />
      <div class="mb-5">
        <Button
          label="Back to Stands"
          icon="pi pi-arrow-left"
          class="p-button-text"
          @click="router.back()"
        />
      </div>
      <div class="w-full sticky bg-white top-16 block p-10 z-10">
        <h2>Edit Stand</h2>
        <div class="m-5 p-5 mb-0 pb-0 grid gap-2 grid-cols-3">
          <div class="flex flex-col gap-1">
            <span class="font-bold text-x inline">{{ standModel.name }}</span>
            <span class="text-gray-600 inline"
              >Stand Assembly Number: {{ standModel.standAssemblyNumber }}</span
            >
          </div>
          <div class="flex flex-col gap-1">
            <div class="flex gap-2">
              <span class="font-bold inline">{{ standModel.standType?.name }}</span>
            </div>
            <div class="flex gap-2">
              <span class="text-gray-600 inline">Layout Style: </span>
              <span class="text-gray-600 inline">{{
                standLayoutStylesList.find((style) => style.id === standModel.layoutStyle)?.name
              }}</span>
            </div>
          </div>
          <div class="flex flex-col gap-1">
            <div class="flex gap-2">
              <span class="font-bold inline"> Height x Width: </span>
              <span class="text-gray-600 inline"
                >{{ standModel.height }} x {{ standModel.width }}
              </span>
            </div>
            <div class="flex gap-2">
              <span class="font-bold inline"> Merch Space: </span>
              <span class="text-gray-600 inline">
                {{ standModel.merchHeight }} x{{ standModel.merchWidth }}
              </span>
            </div>
          </div>

          <!-- <div class="flex flex-col gap-1">
            <img :src="productImageSrc" class="cassette-icon max-w-40"></img>
        </div> -->
        </div>
        <div class="w-full p-10 pb-0">
          <Button @click="tabId = '0'" label="Details" class="" :outlined="tabId !== '0'" />
          <Button @click="tabId = '1'" label="Layout" class="" :outlined="tabId !== '1'" />
          <!-- <Button @click="tabId = '2'" label="Rows" class="" :outlined="tabId !== '2'" /> -->
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
                  <fieldset legend="Part Details" class="col-span-2 mb-12">
                    <legend class="text-lg font-bold mb-2">Details</legend>
                    <div class="grid grid-cols-2 gap-10">
                      <div class="flex flex-col gap-1">
                        <label for="name">Stand Name:</label>
                        <InputText
                          v-model="standModel.name"
                          name="name"
                          :value="standModel.name"
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
                        <label for="description">Stand Assembly Number:</label>
                        <InputText
                          v-model="standModel.standAssemblyNumber"
                          name="standAssemblyNumber"
                          type="text"
                          length="255"
                          placeholder="Stand Assembly Number"
                          fluid
                        />
                        <Message
                          v-if="$form.standAssemblyNumber?.invalid"
                          severity="error"
                          size="small"
                          variant="simple"
                          >{{ $form.standAssemblyNumber.error?.message }}</Message
                        >
                      </div>
                    </div>
                    <div class="grid grid-cols-2 gap-10 mt-10">
                      <div class="flex flex-col gap-2">
                        <label for="standType">Stand Type:</label>
                        <Select
                          v-model="selectedStandType"
                          :options="standTypesList ?? []"
                          id="standType"
                          name="standType"
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
                        <label for="layoutStyle">Layout Style:</label>
                        <Select
                          v-model="standModel.layoutStyle"
                          :options="standLayoutStylesList ?? []"
                          id="layoutStyle"
                          name="layoutStyle"
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
                    <div class="grid grid-cols-2 gap-10 mt-10">
                      <div class="flex flex-col gap-1">
                        <label for="discontinued">Discontinued:</label>
                        <ToggleButton
                          name="discontinued"
                          v-model="standModel.discontinued"
                          onLabel="Yes"
                          offLabel="No"
                          onIcon="pi pi-check"
                          offIcon="pi pi-times"
                          class="w-24 mt-2"
                        ></ToggleButton>
                      </div>
                      <div class="flex flex-col gap-1">
                        <label for="published">Published:</label>
                        <ToggleButton
                          name="published"
                          v-model="standModel.published"
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
                        <template v-for="country in standModel.countries">
                          <Chip class="flex-wrap" :label="country.name"></Chip>
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
                        <label for="Height">Height:</label>
                        <InputNumber
                          name="height"
                          placeholder="Height"
                          fluid
                          v-model="standModel.height"
                        />
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
                        <InputNumber
                          name="width"
                          placeholder="Width"
                          fluid
                          v-model="standModel.width"
                        />
                        <Message
                          v-if="$form.width?.invalid"
                          severity="error"
                          size="small"
                          variant="simple"
                          >{{ $form.width.error?.message }}</Message
                        >
                      </div>
                      <div class="flex flex-col gap-1">
                        <label for="merchHeight">Merchendising Height:</label>
                        <InputNumber
                          name="merchHeight"
                          placeholder="Merchendising Height"
                          fluid
                          v-model="standModel.merchHeight"
                        />
                      </div>

                      <div class="flex flex-col gap-1">
                        <label for="merchWidth">Merchendising Width:</label>
                        <InputNumber
                          name="merchWidth"
                          placeholder="Merchendising Width"
                          fluid
                          v-model="standModel.merchWidth"
                        />
                        <Message
                          v-if="$form.depth?.invalid"
                          severity="error"
                          size="small"
                          variant="simple"
                          >{{ $form.depth.error?.message }}</Message
                        >
                      </div>
                      <div class="flex flex-col gap-1">
                        <label for="headerHeight">Header Height:</label>
                        <InputNumber
                          name="headerHeight"
                          placeholder="Header Height"
                          fluid
                          v-model="standModel.headerHeight"
                        />
                      </div>
                      <div class="flex flex-col gap-1">
                        <label for="footerHeight">Footer Height:</label>
                        <InputNumber
                          name="footerHeight"
                          placeholder="Footer Height"
                          fluid
                          v-model="standModel.footerHeight"
                        />
                      </div>
                    </div>
                  </fieldset>
                </div>
                <div class="bg-gray-50 col-span-2 p-10 mb-5">
                  <fieldset legend="Dimensions" class="col-span-2">
                    <legend class="text-lg font-bold mb-2">Merchandising Dimensions</legend>
                    <div class="grid grid-cols-2 gap-10">
                      <div class="flex flex-col gap-1">
                        <label for="merchHeight">Merchendising Height:</label>
                        <InputNumber
                          name="merchHeight"
                          placeholder="Merchendising Height"
                          fluid
                          v-model="standModel.merchHeight"
                        />
                      </div>

                      <div class="flex flex-col gap-1">
                        <label for="merchWidth">Merchendising Width:</label>
                        <InputNumber
                          name="merchWidth"
                          placeholder="Merchendising Width"
                          fluid
                          v-model="standModel.merchWidth"
                        />
                        <Message
                          v-if="$form.depth?.invalid"
                          severity="error"
                          size="small"
                          variant="simple"
                          >{{ $form.depth.error?.message }}</Message
                        >
                      </div>
                    </div>
                  </fieldset>
                </div>

                <div class="bg-gray-50 col-span-2 p-10 mb-5">
                  <fieldset legend="Stands" class="col-span-2">
                    <legend class="text-lg font-bold mb-2">Settings</legend>
                    <div class="grid grid-cols-3 gap-10 mt-10">
                      <div class="flex flex-col gap-1">
                        <label for="spanShelves">Span Shelves:</label>
                        <ToggleButton
                          name="spanShelves"
                          v-model="standModel.spanShelves"
                          onLabel="Yes"
                          offLabel="No"
                          onIcon="pi pi-check"
                          offIcon="pi pi-times"
                          class="w-24 mt-2"
                        ></ToggleButton>
                      </div>
                      <div class="flex flex-col gap-1">
                        <label for="allowOverhang">Overhang:</label>
                        <ToggleButton
                          name="allowOverhang"
                          v-model="standModel.allowOverHang"
                          onLabel="Yes"
                          offLabel="No"
                          onIcon="pi pi-check"
                          offIcon="pi pi-times"
                          class="w-24 mt-2"
                        ></ToggleButton>
                      </div>

                      <div class="flex flex-col gap-1">
                        <label for="description">Stand Cost:</label>
                        <InputNumber
                          v-model="standModel.standCost"
                          name="standCost"
                          type="text"
                          length="255"
                          placeholder="Stand Cost"
                          fluid
                        />
                        <Message
                          v-if="$form.standCost?.invalid"
                          severity="error"
                          size="small"
                          variant="simple"
                          >{{ $form.standCost.error?.message }}</Message
                        >
                      </div>
                    </div>
                  </fieldset>
                </div>

                <Button type="submit" severity="secondary" label="Submit" />
              </Form>
            </TabPanel>
            <TabPanel value="1">
              <div class="card grid grid-cols-2 gap-4">
                <div class="col-span-2">
                  <h2>Layout</h2>
                  <div class="flex flex-col gap-2 max-w-sm">
                    <label for="layoutStyle">Layout Style:</label>
                    <Select
                      v-model="standModel.layoutStyle"
                      :options="standLayoutStylesList ?? []"
                      id="layoutStyle"
                      name="layoutStyle"
                      class="w-full border--500! border-2!"
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
                <div class="bg-gray-50 col-span-1 p-10 mb-5">
                  <fieldset legend="Dimensions" class="col-span-2">
                    <legend class="text-lg font-bold mb-2">Columns</legend>
                    <div v-if="standModel.layoutStyle == 1 || standModel.layoutStyle == 3">
                      <div class="grid grid-cols-4 gap-10">
                        <div class="flex flex-col gap-1">
                          <label for="cols">No. Columns:</label>
                          <InputNumber
                            name="cols"
                            placeholder="No. Columns"
                            fluid
                            v-model="standModel.cols"
                          />
                        </div>

                        <div class="flex flex-col gap-1">
                          <label for="equalCols">Equal Width:</label>
                          <ToggleButton
                            v-model="standModel.equalCols"
                            name="equalCols"
                            placeholder=""
                            onIcon="pi pi-check"
                            offIcon="pi pi-times"
                            @change="changeColLayout($event)"
                          />
                        </div>
                        <div class="flex flex-col gap-1">
                          <label for="defaultColWidth">Column Width:</label>
                          <InputNumber
                            name="defaultColWidth"
                            placeholder="Column Width"
                            fluid
                            v-model="standModel.defaultColWidth"
                          />
                        </div>
                      </div>

                      <div class="flex gap-2 flex-wrap pt-1">
                        <Card v-if="!standModel.equalCols">
                          <template #content>
                            <template v-for="col in colsTable">
                              <div class="grid grid-cols-3 gap-2 items-center mb-2">
                                <div class="flex flex-wrap items-end gap-1">
                                  <FloatLabel variant="in">
                                    <InputNumber
                                      type="number"
                                      v-model="col.position"
                                      inputId="col"
                                    />
                                    <label for="col">Column</label>
                                  </FloatLabel>
                                </div>
                                <div class="flex flex-col gap-1">
                                  <FloatLabel variant="in">
                                    <InputNumber
                                      type="number"
                                      v-model="col.width"
                                      inputId="col-w"
                                      class=""
                                    />
                                    <label for="col-w">Width</label>
                                  </FloatLabel>
                                </div>
                                <div class="flex flex-col gap-1 ml-10">
                                  <Button
                                    icon="pi pi-plus"
                                    tooltip="Add Upright"
                                    class="h-10 w-40"
                                    @click="addUpright(col.position ?? 0)"
                                  ></Button>
                                </div>
                              </div>
                              <div v-if="col.uprights" class="border p-5 mb-10">
                                <div class="font-bold mb-4">
                                  Uprights for Column {{ col.position }}
                                </div>
                                <div v-for="upright in col.uprights">
                                  <div class="flex-grid grid-cols-3 gap-2 items-center mb-2">
                                    <div class="flex flex-col gap-3" style="width">
                                      <FloatLabel variant="in">
                                        <InputNumber
                                          type="number"
                                          v-model="upright.position"
                                          inputId="upr-pos"
                                        />
                                        <label for="upr-pos">Upright</label>
                                      </FloatLabel>
                                    </div>
                                    <div class="flex flex-col gap-3">
                                      <FloatLabel variant="in">
                                        <InputNumber
                                          type="number"
                                          inputId="upr-w"
                                          v-model="upright.width"
                                          size="5"
                                        />
                                        <label for="upr-w">Width</label>
                                      </FloatLabel>
                                    </div>
                                    <div class="flex flex-col gap-1 ml-10">
                                      <Button
                                        icon="pi pi-trash"
                                        class="h-10 w-40"
                                        @click="
                                          delUpright(col.position ?? 0, upright.position ?? 0)
                                        "
                                      ></Button>
                                    </div>
                                  </div>
                                </div>
                              </div>
                            </template>
                          </template>
                        </Card>
                      </div>
                    </div>
                    <!-- Column layout container -->
                    <div v-if="standModel.layoutStyle == 2">
                      <div class="grid grid-cols-4 gap-10">
                        <div class="flex flex-col gap-1">
                          <label for="cols">No. Pitches:</label>
                          <InputNumber
                            name="cols"
                            placeholder="No. Pitches"
                            fluid
                            v-model="standModel.horizontalPitchCount"
                          />
                        </div>

                        <div class="flex flex-col gap-1">
                          <label for="defaultPitchSize">Pitch Size:</label>
                          <InputNumber
                            name="defaultPitchSize"
                            placeholder="Pitch Size"
                            fluid
                            v-model="standModel.horizontalPitchSize"
                          />
                        </div>
                      </div>
                    </div>
                  </fieldset>
                </div>
                <div class="bg-gray-50 col-span-1 p-10 mb-5">
                  <fieldset legend="Dimensions" class="col-span-2">
                    <legend class="text-lg font-bold mb-2">Rows</legend>
                    <div v-if="standModel.layoutStyle == 1">
                      <div class="grid grid-cols-4 gap-10">
                        <div class="flex flex-col gap-1">
                          <label for="rows">No. Rows:</label>
                          <InputNumber
                            name="rows"
                            placeholder="No. Rows"
                            fluid
                            v-model="standModel.rows"
                          />
                        </div>

                        <div class="flex flex-col gap-1">
                          <label for="equalRows">Equal Height:</label>
                          <ToggleButton
                            v-model="standModel.equalRows"
                            name="equalRows"
                            placeholder="Row Layout"
                            onIcon="pi pi-check"
                            offIcon="pi pi-times"
                            @change="changeRowLayout($event)"
                          />
                        </div>
                        <div class="flex flex-col gap-1">
                          <label for="defaultRowHeight">Row Height:</label>
                          <InputNumber
                            name="defaultRowHeight"
                            placeholder="Row Height"
                            fluid
                            v-model="standModel.defaultRowHeight"
                          />
                        </div>
                      </div>
                      <div class="flex gap-2 flex-wrap pt-1">
                        <Card v-if="!standModel.equalRows">
                          <template #content>
                            <template v-for="row in rowsTable">
                              <div class="grid grid-cols-3 gap-2 items-center mb-2">
                                <div class="flex flex-wrap items-end gap-1">
                                  <FloatLabel variant="in">
                                    <InputNumber
                                      type="number"
                                      v-model="row.position"
                                      inputId="row"
                                    />
                                    <label for="row">Row</label>
                                  </FloatLabel>
                                </div>
                                <div class="flex flex-col gap-1">
                                  <FloatLabel variant="in">
                                    <InputNumber
                                      type="number"
                                      v-model="row.height"
                                      inputId="row-h"
                                    />
                                    <label for="row-h">Height</label>
                                  </FloatLabel>
                                </div>
                              </div>
                            </template>
                          </template>
                        </Card>
                      </div>
                    </div>
                    <div v-if="standModel.layoutStyle == 2 || standModel.layoutStyle == 3">
                      <div class="grid grid-cols-4 gap-10">
                        <div class="flex flex-col gap-1">
                          <label for="defaultPitchSize">Vertical Pitch:</label>
                          <InputNumber
                            name="defaultPitchSize"
                            placeholder="Vertical Pitch"
                            fluid
                            v-model="standModel.shelfIncrement"
                          />
                        </div>
                      </div>
                    </div>
                  </fieldset>
                </div>
              </div>
            </TabPanel>
          </TabPanels>
        </Tabs>
      </div>
    </div>
  </div>
</template>
