<script setup lang="ts">
import { StandType } from '@/models/StandTypes/standType.model'
import { standTypeFilter } from '@/models/StandTypes/standTypeFilter.model'
import standTypeService from '@/services/StandTypes/StandTypeService'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'
import { FilterMatchMode } from '@primevue/core/api'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { onMounted, ref, watch } from 'vue'

const standImageUrl = import.meta.env.VITE_APP_STANDIMAGE_URL

const standTypes = ref()
const selectedStandType = ref()
const standTypeDialog = ref(false)
const submitted = ref(false)
const standType = ref(new StandType())
const expandedRows = ref()
const toast = useToast()
const layout = useSystemStore()
const brandStore = useBrandStore()
const brand = storeToRefs(brandStore).activeBrand
watch(brand, async (newBrand) => {
  if (newBrand) {
    let filter = new standTypeFilter()
    filter.brandId = newBrand.id
    await standTypeService.getParentStandTypes(filter).then((response) => {
      standTypes.value = response.data
      console.log('StandTypes loaded for brand change', response)
    })
  }
})

async function onRowExpand(event: any) {
  toast.add({
    severity: 'info',
    summary: 'StandType Expanded',
    detail: event.data.name,
    life: 3000,
  })
}
const onRowCollapse = (event: any) => {
  toast.add({
    severity: 'success',
    summary: 'StandType Collapsed',
    detail: event.data.name,
    life: 3000,
  })
}
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
})
const src = ref()
const file = ref()
onMounted(async () => {
  const layout = useSystemStore()
  layout.layoutState.disableBrandSelect = false
  await standTypeService.initialise()
  var filter = new standTypeFilter()
  let brandid = brandStore.activeBrand?.id ?? 0

  filter.brandId = brandid
  await standTypeService.getParentStandTypes(filter).then((data) => {
    // create categoryOptions array for select dropdown
    standTypes.value = data.data
    console.log('StandTypes loaded:', standTypes.value)
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
  standType.value = {} as StandType
  submitted.value = false
  standTypeDialog.value = true
}
const hideDialog = () => {
  standTypeDialog.value = false
  submitted.value = false
}
async function saveStandType() {
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

  if (standType?.value.name?.trim()) {
    formData.append('id', String(standType.value.id ?? 0))
    formData.append('name', standType.value.name)
    formData.append('description', String(standType.value.description))
    formData.append('lock', String(standType.value.lock))
    formData.append('hidePrices', String(standType.value.hidePrices))
    formData.append('file', file.value ?? '')
    if (standType.value.id) {
      await standTypeService
        .updateStandType(formData)
        .then((response) => {
          var updatedStandType = response
          standTypes.value
            .find((c: StandType) => c.id === updatedStandType.parentStandTypeId)
            .childStandTypes.splice(
              standTypes.value
                .find((c: StandType) => c.id === updatedStandType.parentStandTypeId)
                .childStandTypes.findIndex((cs: StandType) => cs.id === updatedStandType.id),
              1,
              updatedStandType,
            )
          standTypes.value
            .find((c: StandType) => c.id === updatedStandType.parentStandTypeId)
            .childStandTypes.push(updatedStandType)
        })
        .catch((error) => {
          console.log('Error updating standType:', error)
        })
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'StandType Updated',
        life: 3000,
      })
    } else {
      formData.append('brandId', standType.value.brandId)
      formData.append('parentStandTypeId', String(standType.value.parentStandTypeId))
      await standTypeService
        .addStandType(formData)
        .then((response) => {
          var updatedStandType = response
          standTypes.value
            .find((c: StandType) => c.id === updatedStandType.parentStandTypeId)
            .childStandTypes.push(updatedStandType)
        })
        .catch((error) => {
          console.log('Error creating standType:', error)
        })
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'StandType Created',
        life: 3000,
      })
    }

    file.value = null
    standTypeDialog.value = false
    standType.value = new StandType()
  }
}
function editStandType(cat: StandType) {
  standType.value = cat
  standTypeDialog.value = true
}

function addStandType(cat: StandType) {
  if (brand.value) {
    standType.value = new StandType()
    standType.value.brandId = brand.value.id
    standType.value.brandName = brand.value.name
    standType.value.parentStandTypeId = cat.parentStandTypeId
    standType.value.parentStandType = cat.parentStandType
    standType.value.hidePrices = false
    standType.value.lock = false
    standTypeDialog.value = true
  } else {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Please select a brand before adding a StandType.',
      life: 3000,
    })
    return
  }
}

const getLockIcon = (lock: boolean) => {
  return lock ? 'pi pi-lock' : 'pi pi-lock-open'
}

const getHideIcon = (hide: boolean) => {
  return hide ? 'pi pi-cross' : 'pi pi-tick'
}
</script>

<template>
  <div>
    <div class="card">
      <Toolbar class="mb-6">
        <template #start>
          <!-- <Button label="New" icon="pi pi-plus" class="mr-2" @click="openNew" /> -->
        </template>

        <template #end> </template>
      </Toolbar>

      <DataTable
        v-model:expandedRows="expandedRows"
        ref="dt"
        :value="standTypes"
        dataKey="id"
        rowHover
        tableStyle="min-width: 50rem"
        class="hover:bg-gray-100"
        @rowExpand="onRowExpand"
        @rowCollapse="onRowCollapse"
        sortMode="single"
        sortField="parentStandTypeName"
        :sortOrder="1"
      >
        <template #header>
          <div class="flex flex-wrap gap-2 items-center justify-between">
            <h4 class="m-0">Manage StandTypes</h4>
            <IconField>
              <InputIcon>
                <i class="pi pi-search" />
              </InputIcon>
              <InputText v-model="filters['global'].value" placeholder="Search..." />
            </IconField>
          </div>
        </template>
        <template #groupheader="slotProps">
          <span class="align-middle ml-2 font-bold leading-normal">{{
            slotProps.data.ParentStandTypeName
          }}</span>
        </template>
        <!-- <Column selectionMode="multiple" style="width: 3rem" :exportable="false"></Column> -->
        <Column expander style="width: 5rem" />

        <Column field="name" header="Name" sortable style="min-width: 16rem"></Column>

        <!-- <Column :exportable="false" style="min-width: 12rem">
          <template #body="slotProps">
            <Button
              icon="pi pi-pencil"
              variant="outlined"
              rounded
              class="mr-2"
              @click="editStandType(slotProps.data)"
            />
          </template>
        </Column> -->
        <template #expansion="slotProps">
          <div class="p-4">
            <!-- <h5>{{ slotProps.data.name }}</h5> -->
            <DataTable :value="slotProps.data.childStandTypes" tableStyle="min-width: 50rem">
              <Column
                field="parentStandType.name"
                header="Parent StandType"
                sortable
                style="min-width: 16rem"
              ></Column>
              <Column field="name" header="Name" sortable></Column>
              <Column field="brandName" header="Brand Name" sortable></Column>
              <Column field="description" header="Description" sortable></Column>
              <Column field="locked" header="Locked" sortable>
                <template #body="{ data }">
                  <div class="card flex flex-wrap justify-center gap-6">
                    <i :class="getLockIcon(data.locked)" style="font-size: 1rem" />
                  </div>
                </template>
              </Column>
              <Column field="hidePrices" header="Show Prices" sortable>
                <template #body="{ data }">
                  <i
                    class="pi"
                    :class="{
                      'pi-check-circle text-green-500 ': !data.hidePrices,
                      'pi-times-circle text-red-500': data.hidePrices,
                    }"
                  ></i>
                </template>
              </Column>
              <Column field="standCount" header="No. Stands" sortable></Column>
              <Column :exportable="false" style="min-width: 12rem">
                <template #body="slotProps">
                  <Button
                    icon="pi pi-pencil"
                    variant="outlined"
                    rounded
                    class="mr-2"
                    @click="editStandType(slotProps.data)"
                  />
                  <Button
                    icon="pi pi-plus"
                    variant="outlined"
                    rounded
                    class="mr-2"
                    @click="addStandType(slotProps.data)"
                  />
                </template>
              </Column>
            </DataTable>
          </div>
        </template>
      </DataTable>
    </div>
    <Dialog
      v-model:visible="standTypeDialog"
      :style="{ width: '450px' }"
      header="StandType Details"
      :modal="true"
    >
      <p v-if="brand" class="block font-bold mb-0" style="margin: 0 0">
        Brand: <span class="text-lg font-medium">{{ brand?.name }}</span>
      </p>
      <p v-if="standType.parentStandType" class="block font-bold mb-3">
        Parent StandType:
        <span class="text-lg font-medium">{{ standType.parentStandType?.name }}</span>
      </p>
      <div class="flex flex-col gap-6">
        <img
          v-if="standType.standImage"
          :src="`${standImageUrl}${standType.standImage}`"
          :alt="standType.standImage"
          class="block m-auto pb-4"
        />
        <div class="card flex flex-wrap gap-6 items-center justify-between mb-0">
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
        <!-- <label for="brandName" class="block font-bold mb-3">Brand</label>
        <InputText id="brandName" disabled v-model.trim="brand.name" fluid />

        <label for="parentType" class="block font-bold mb-3">Parent StandType</label>
        <InputText
          id="parentType"
          type="text"
          disabled
          v-model.trim="standType.parentStandTypeName"
          fluid
          class="border-0"
        /> -->

        <label for="name" class="block font-bold mb-3">Name</label>
        <InputText
          id="name"
          v-model.trim="standType.name"
          required="true"
          autofocus
          :invalid="submitted && !standType.name"
          fluid
        />
        <small v-if="submitted && !standType.name" class="text-red-500">Name is required.</small>
        <label for="description" class="block font-bold mb-3">Description</label>
        <InputText
          id="description"
          v-model.trim="standType.description"
          required="true"
          autofocus
          :invalid="submitted && !standType.description"
          fluid
        />
        <label for="locked" class="block font-bold mb-3">Lock</label>
        <ToggleButton v-model="standType.lock" onLabel="Locked" offLabel="Unlocked" />
        <label for="hidePrices" class="block font-bold mb-3">Hide Prices</label>
        <ToggleButton v-model="standType.hidePrices" onLabel="Yes" offLabel="No" />
      </div>
      <template #footer>
        <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
        <Button label="Save" icon="pi pi-check" @click="saveStandType()" />
      </template>
    </Dialog>
  </div>
  <Toast></Toast>
  <ConfirmPopup></ConfirmPopup>
</template>
