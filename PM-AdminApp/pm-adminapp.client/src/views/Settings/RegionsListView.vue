<script setup lang="ts">
import { useLayoutStore } from '@/layout/composables/layout'
import { Region } from '@/models/Countries/region.model'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import countryService from '@/services/Countries/CountryService'
import { FilterMatchMode } from '@primevue/core/api'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { onMounted, ref, watch } from 'vue'

const brandImageUrl = import.meta.env.VITE_APP_BRANDIMAGE_URL
const regions = ref()
const region = ref(new Region())
const brandDialog = ref(false)
const submitted = ref(false)
const toast = useToast()
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
})
const src = ref()
const file = ref()

const layout = useLayoutStore()
const brand = storeToRefs(layout).getActiveBrand

watch(brand, async (newBrand) => {
  if (newBrand) {
    let filter = new regionFilter()
    filter.brandId = newBrand.id
    await countryService.getRegions(filter).then((response) => {
      regions.value = response
      console.log('Regions loaded for brand change', response)
    })
  }
})

onMounted(async () => {
  const layout = useLayoutStore()
  await countryService.initialise()
  let brandid = layout.getActiveBrand?.id ?? 0
  let filter = new regionFilter()
  filter.brandId = brandid
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
  brandDialog.value = true
}
const hideDialog = () => {
  brandDialog.value = false
  submitted.value = false
}
async function saveRegion() {
  submitted.value = true

  const formData = new FormData()
  if (file.value) {
    formData.append('file', file.value)
    // await brandService.uploadBrandImage(formData).then((response) => {
    //   if (response && response.data) {
    //     brand.value.brandLogo = response.data.fileName
    //   }
    // })
  }

  if (region?.value.name?.trim()) {
    formData.append('name', region.value.name)
    // formData.append('shelfLock', String(brand.value.shelfLock))
    // formData.append('disabled', String(brand.value.disabled))
    // formData.append('id', String(brand.value.id ?? 0))
    // formData.append('file', file.value ?? '')
    if (region.value.id) {
      await countryService
        .updateRegion(formData)
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

    brandDialog.value = false
    region.value = new Region()
  }
}
function editRegion(rg: Region) {
  region.value = rg
  brandDialog.value = true
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
        <Column header="Image" style="min-width: 16rem">
          <template #body="slotProps">
            <img
              :src="`${brandImageUrl}${slotProps.data.brandLogo}`"
              :alt="slotProps.data.brandLogo"
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
              @click="editRegion(slotProps.data)"
            />
          </template>
        </Column>
      </DataTable>
    </div>

    <Dialog
      v-model:visible="brandDialog"
      :style="{ width: '450px' }"
      header="Brand Details"
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
      </div>
      <template #footer>
        <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
        <Button label="Save" icon="pi pi-check" @click="saveRegion" />
      </template>
    </Dialog>
  </div>
</template>
