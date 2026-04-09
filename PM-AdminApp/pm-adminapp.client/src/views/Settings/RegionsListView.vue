<script setup lang="ts">
import { useMultiSelectLists } from '@/components/composables/multiSelectList.composable'
import { Country } from '@/models/Countries/country.model'
import { Region } from '@/models/Countries/region.model'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import countryService from '@/services/Countries/CountryService'

import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'
import { FilterMatchMode } from '@primevue/core/api'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { onMounted, ref, watch } from 'vue'

const regions = ref()
const region = ref(new Region())
const regionDialog = ref(false)
const submitted = ref(false)
const toast = useToast()
const brandStore = useBrandStore()
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
})
const src = ref()
const file = ref()

const layout = useSystemStore()
const brand = storeToRefs(brandStore).activeBrand
const multiSelectLists = useMultiSelectLists()
const countrySelectList = ref<Country[] | null>(null)
const selectedCountries = ref<number[] | null>(null) // MultiSelect binding
const selectAllCountries = ref(false)

watch(brand, async (newBrand) => {
  if (newBrand) {
    let filter = new regionFilter()
    filter.brandId = newBrand.id
    filter.loadChildren = true
    await countryService.getRegions(filter).then((response) => {
      regions.value = response
      console.log('Regions loaded for brand change', response)
    })
  }
})

onMounted(async () => {
  const layout = useSystemStore()
  await countryService.initialise()
  await countryService.getCountries().then((response) => {
    countrySelectList.value = response
    console.log('Countries loaded for region management', response)
  })
  let brandid = brandStore.activeBrand?.id ?? 0
  let filter = new regionFilter()
  filter.brandId = brandid
  filter.loadChildren = true
  await countryService.getRegions(filter).then((response) => {
    regions.value = response
    console.log('Regions loaded', response)
  })
})

function onFileSelect(event: any) {
  file.value = event.files[0]
  const reader = new FileReader()

  // reader.onload = async (e) => {
  //   if (e.target) {
  //     src.value = e.target.result
  //   }
  // }

  // reader.readAsDataURL(file)
}

const openNew = () => {
  region.value = {} as Region
  submitted.value = false
  regionDialog.value = true
}
const hideDialog = () => {
  regionDialog.value = false
  submitted.value = false
}
async function saveRegion(rg: Region) {
  submitted.value = true

  if (region.value.id) {
    await countryService
      .saveRegion(rg)
      .then((response) => {
        if (response && response) {
          console.log(response)
        }
      })
      .catch((error) => {
        console.error('Error updating region:', error)
      })
    toast.add({
      severity: 'success',
      summary: 'Successful',
      detail: 'Region Updated',
      life: 3000,
    })
  } else {
    region.value.id = 0
    // region.value.brandLogo = 'brand-placeholder.svg'
    regions.value.push(region.value)
    toast.add({
      severity: 'success',
      summary: 'Successful',
      detail: 'Region Created',
      life: 3000,
    })
  }

  regionDialog.value = false
  region.value = new Region()
}
function editRegion(rg: Region) {
  region.value = rg
  selectedCountries.value = region.value.countries?.map((c) => c.id) ?? []
  regionDialog.value = true
}

const findIndexById = (id: number) => {
  let index = -1
  for (let i = 0; i < regions.value.length; i++) {
    if (regions.value[i].id === id) {
      index = i
      break
    }
  }

  return index
}

async function onCountryChange(evt: any) {
  // manageSelectedCountries(evt.value)
  multiSelectLists.manageSelectedValues(
    evt.value,
    countrySelectList.value ?? [],
    region.value.countries ?? [],
  )
  console.log('Selected Countries after region change', selectedCountries.value)
  console.log('Part Model Countries after region change', region.value.countries)
  let someArray = region.value.countries ?? []

  region.value.countryList = region.value.countries?.map((c) => c.id).join(',') || ''
}

function onSelectAllCountriesChange(event: any) {
  selectedCountries.value = event.checked
    ? (countrySelectList.value?.map((item) => item.id) ?? [])
    : []
  selectAllCountries.value = event.checked
  multiSelectLists.manageSelectedValues(
    selectedCountries.value,
    countrySelectList.value ?? [],
    region.value.countries ?? [],
  )
  region.value.countryList = region.value.countries?.map((c) => c.id).join(',') || ''
}

function clearCountrySelection() {
  selectedCountries.value = []
  region.value.countries = []
  region.value.countryList = ''
}
</script>

<template>
  <div>
    <div class="card">
      <Toolbar class="mb-6">
        <template #start>
          <Button label="New" icon="pi pi-plus" class="mr-2" @click="openNew" />
        </template>

        <template #end> </template>
      </Toolbar>

      <DataTable
        ref="dt"
        :value="regions"
        dataKey="id"
        :paginator="true"
        :rows="10"
        :filters="filters"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rowsPerPageOptions="[5, 10, 25]"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} products"
      >
        <template #header>
          <div class="flex flex-wrap gap-2 items-center justify-between">
            <h4 class="m-0">Manage Regions</h4>
            <IconField>
              <InputIcon>
                <i class="pi pi-search" />
              </InputIcon>
              <InputText v-model="filters['global'].value" placeholder="Search..." />
            </IconField>
          </div>
        </template>

        <Column selectionMode="multiple" style="width: 3rem" :exportable="false"></Column>
        <Column field="name" header="Name" sortable style="min-width: 16rem"></Column>
        <Column field="countries" header="Countries">
          <template #body="slotProps">
            <div class="flex align-items-center gap-2">
              <Listbox
                :options="slotProps.data.countries"
                option-label="name"
                class="w-full"
                listStyle="max-height:100px"
              >
              </Listbox>
            </div>
          </template>
        </Column>
        <!-- <Column field="category" header="Category" sortable style="min-width: 10rem"></Column> -->
        <!-- <Column field="inventoryStatus" header="Status" sortable style="min-width: 12rem">
          <template #body="slotProps">
            <Tag
              :value="slotProps.data.inventoryStatus"
              :severity="getStatusLabel(slotProps.data.inventoryStatus)"
            />
          </template>
        </Column> -->
        <Column :exportable="false" style="min-width: 12rem">
          <template #body="slotProps">
            <Button
              icon="pi pi-pencil"
              variant="outlined"
              rounded
              class="mr-2"
              @click="editRegion(slotProps.data)"
            />
          </template>
        </Column>
      </DataTable>
    </div>

    <Dialog
      v-model:visible="regionDialog"
      :style="{ width: '450px' }"
      header="Region Details"
      :modal="true"
    >
      <div class="flex flex-col gap-6">
        <div>
          <label for="name" class="block font-bold mb-3">Name</label>
          <InputText
            id="name"
            v-model.trim="region.name"
            required="true"
            autofocus
            :invalid="submitted && !region.name"
            fluid
          />
          <small v-if="submitted && !region.name" class="text-red-500">Name is required.</small>
        </div>
        <div class="flex flex-col gap-2">
          <div>
            <label for="country">Country:</label>
            <MultiSelect
              name="countries"
              v-model="selectedCountries"
              :options="countrySelectList ?? []"
              id="country"
              class="w-full"
              option-label="name"
              option-value="id"
              @change="onCountryChange($event)"
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
            <Button label="Clear Selection" class="w-text-left" @click="clearCountrySelection" />
            <template v-for="country in region.countries ?? []" :key="country.id">
              <Chip class="flex-wrap" :label="country.name"></Chip>
            </template>
          </div>
        </div>
      </div>

      <template #footer>
        <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
        <Button label="Save" icon="pi pi-check" @click="saveRegion(region)" />
      </template>
    </Dialog>
  </div>
</template>
