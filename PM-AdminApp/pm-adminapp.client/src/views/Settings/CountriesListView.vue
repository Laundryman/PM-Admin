<script setup lang="ts">
import { Country } from '@/models/Countries/country.model'
import countryService from '@/services/Countries/CountryService'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'
import { FilterMatchMode } from '@primevue/core/api'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { onMounted, ref } from 'vue'

const flagImageUrl = import.meta.env.VITE_APP_FLAGIMAGE_URL
const countries = ref()
const country = ref(new Country())
const countryDialog = ref(false)
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

// watch(brand, async (newBrand) => {
//   if (newBrand) {
//     await countryService.getCountries().then((response) => {
//       countries.value = response
//       console.log('Countries loaded for brand change', response)
//     })
//   }
// })

onMounted(async () => {
  const layout = useSystemStore()
  await countryService.initialise()
  let brandid = brandStore.activeBrand?.id ?? 0
  await countryService.getCountries().then((response) => {
    countries.value = response
    console.log('Countries loaded', response)
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
  // region.value = {} as Region
  submitted.value = false
  countryDialog.value = true
}
const hideDialog = () => {
  countryDialog.value = false
  submitted.value = false
}
async function saveCountry() {
  submitted.value = true

  const formData = new FormData()
  if (file.value) {
    formData.append('file', file.value)
  }

  if (country?.value.name?.trim()) {
    formData.append('name', country.value.name)
    // formData.append('shelfLock', String(brand.value.shelfLock))
    // formData.append('disabled', String(brand.value.disabled))
    // formData.append('id', String(brand.value.id ?? 0))
    // formData.append('file', file.value ?? '')
    if (country.value.id) {
      await countryService
        .updateCountry(formData)
        .then((response) => {
          if (response && response) {
            console.log(response)
          }
        })
        .catch((error) => {
          console.error('Error updating country:', error)
        })
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'Country Updated',
        life: 3000,
      })
    } else {
      country.value.id = 0
      // country.value.brandLogo = 'brand-placeholder.svg'
      countries.value.push(country.value)
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'Country Created',
        life: 3000,
      })
    }

    countryDialog.value = false
    country.value = new Country()
  }
}
function editCountry(ct: Country) {
  country.value = ct
  countryDialog.value = true
}

const findIndexById = (id: number) => {
  let index = -1
  for (let i = 0; i < countries.value.length; i++) {
    if (countries.value[i].id === id) {
      index = i
      break
    }
  }

  return index
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
        :value="countries"
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
            <h4 class="m-0">Manage Countries</h4>
            <IconField>
              <InputIcon>
                <i class="pi pi-search" />
              </InputIcon>
              <InputText v-model="filters['global'].value" placeholder="Search..." />
            </IconField>
          </div>
        </template>

        <Column selectionMode="multiple" style="width: 3rem" :exportable="false"></Column>
        <Column header="Image" style="min-width: 16rem">
          <template #body="slotProps">
            <img
              :src="`${flagImageUrl}${slotProps.data.flagFileName}`"
              :alt="slotProps.data.name"
              class="rounded"
              style="width: 104px"
            />
          </template>
        </Column>
        <Column field="name" header="Name" sortable style="min-width: 16rem"></Column>

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
              @click="editCountry(slotProps.data)"
            />
          </template>
        </Column>
      </DataTable>
    </div>

    <Dialog
      v-model:visible="countryDialog"
      :style="{ width: '450px' }"
      header="Country Details"
      :modal="true"
    >
      <div class="flex flex-col gap-6">
        <div>
          <label for="name" class="block font-bold mb-3">Name</label>
          <InputText
            id="name"
            v-model.trim="country.name"
            required="true"
            autofocus
            :invalid="submitted && !country.name"
            fluid
          />
          <small v-if="submitted && !country.name" class="text-red-500">Name is required.</small>
        </div>
      </div>
      <template #footer>
        <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
        <Button label="Save" icon="pi pi-check" @click="saveCountry" />
      </template>
    </Dialog>
  </div>
</template>
