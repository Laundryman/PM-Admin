<script setup lang="ts">
import { useLayoutStore } from '@/layout/composables/layout'
import { Brand } from '@/models/Brands/brand.model'
import { default as brandService } from '@/services/Brands/BrandService'
import { FilterMatchMode } from '@primevue/core/api'
import { useToast } from 'primevue/usetoast'
import { onMounted, ref } from 'vue'

const brandImageUrl = import.meta.env.VITE_APP_BRANDIMAGE_URL
const brands = ref()
const brandDialog = ref(false)
const submitted = ref(false)
const brand = ref(new Brand())
const toast = useToast()
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
})
const src = ref()
const file = ref()
onMounted(async () => {
  const layout = useLayoutStore()
  await brandService.initialise()

  await brandService.getBrands().then((data) => {
    // create brandOptions array for select dropdown
    brands.value = data.data
    console.log('Brands loaded:', brands.value)
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
  brand.value = {} as Brand
  submitted.value = false
  brandDialog.value = true
}
const hideDialog = () => {
  brandDialog.value = false
  submitted.value = false
}
async function saveBrand() {
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

  if (brand?.value.name?.trim()) {
    formData.append('name', brand.value.name)
    formData.append('shelfLock', String(brand.value.shelfLock))
    formData.append('disabled', String(brand.value.disabled))
    formData.append('id', String(brand.value.id ?? 0))
    formData.append('file', file.value ?? '')
    if (brand.value.id) {
      await brandService
        .updateBrand(formData)
        .then((response) => {
          if (response && response.data) {
            console.log(response.data)
          }
        })
        .catch((error) => {
          console.error('Error updating brand:', error)
        })
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'Brand Updated',
        life: 3000,
      })
    } else {
      brand.value.id = 0
      brand.value.brandLogo = 'brand-placeholder.svg'
      brands.value.push(brand.value)
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'Brand Created',
        life: 3000,
      })
    }

    brandDialog.value = false
    brand.value = new Brand()
  }
}
function editBrand(br: Brand) {
  brand.value = br
  brandDialog.value = true
}

const getShelfLockIcon = (shelfLock: boolean) => {
  return shelfLock ? 'pi pi-lock' : 'pi pi-lock-open'
}

const getBrandImageUrl = (imageFileName: string) => {
  var baseURL = import.meta.env.VITE_APP_BRANDIMAGE_URL
  return baseURL + imageFileName
}
const findIndexById = (id: number) => {
  let index = -1
  for (let i = 0; i < brands.value.length; i++) {
    if (brands.value[i].id === id) {
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
        :value="brands"
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
            <h4 class="m-0">Manage Brands</h4>
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
        <Column field="shelfLock" header="Lock Shelves" sortable style="min-width: 12rem">
          <template #body="{ data }">
            <div class="card flex flex-wrap justify-center gap-6">
              <i :class="getShelfLockIcon(data.shelfLock)" style="font-size: 1rem" />
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
              @click="editBrand(slotProps.data)"
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
        <img
          v-if="brand.brandLogo"
          :src="`${brandImageUrl}${brand.brandLogo}`"
          :alt="brand.brandLogo"
          class="block m-auto pb-4"
        />
        <div class="card flex flex-wrap gap-6 items-center justify-between">
          <FileUpload
            ref="fileupload"
            mode="basic"
            @select="onFileSelect"
            customUpload
            accept="image/*"
            :maxFileSize="1000000"
          />
          <!-- <Button label="Upload" @click="upload" severity="secondary" /> -->
        </div>
        <div>
          <label for="name" class="block font-bold mb-3">Name</label>
          <InputText
            id="name"
            v-model.trim="brand.name"
            required="true"
            autofocus
            :invalid="submitted && !brand.name"
            fluid
          />
          <small v-if="submitted && !brand.name" class="text-red-500">Name is required.</small>
        </div>
        <div>
          <label for="name" class="block font-bold mb-3">Shelf Lock</label>
          <ToggleSwitch name="shelfLock" v-model="brand.shelfLock" />
        </div>
        <div>
          <label for="name" class="block font-bold mb-3">Disabled</label>
          <ToggleSwitch name="disabled" v-model="brand.disabled" />
        </div>
      </div>
      <template #footer>
        <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
        <Button label="Save" icon="pi pi-check" @click="saveBrand" />
      </template>
    </Dialog>
  </div>
</template>
