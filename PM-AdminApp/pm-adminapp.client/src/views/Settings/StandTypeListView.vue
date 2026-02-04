<script setup lang="ts">
import { StandType } from '@/models/StandTypes/standType.model'
import { standTypeFilter } from '@/models/StandTypes/standTypeFilter.model'
import standTypeService from '@/services/StandTypes/StandTypeService'
import { useSystemStore } from '@/stores/systemStore'
import { FilterMatchMode } from '@primevue/core/api'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { onMounted, ref, watch } from 'vue'

const standTypes = ref()
const selectedStandType = ref()
const standTypeDialog = ref(false)
const submitted = ref(false)
const standType = ref(new StandType())
const expandedRows = ref()
const toast = useToast()
const layout = useSystemStore()
const brand = storeToRefs(layout).getActiveBrand

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
  let brandid = layout.getActiveBrand?.id ?? 0

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
    formData.append('name', standType.value.name)
    // formData.append('shelfLock', String(category.value.shelfLock))
    // formData.append('disabled', String(category.value.disabled))
    formData.append('id', String(standType.value.id ?? 0))
    formData.append('file', file.value ?? '')
    if (standType.value.id) {
      await standTypeService
        .updateStandType(formData)
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
      standType.value.id = 0
      // category.value.categoryLogo = 'category-placeholder.svg'
      standTypes.value.push(standType.value)
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'Category Created',
        life: 3000,
      })
    }

    standTypeDialog.value = false
    standType.value = new StandType()
  }
}
function editStandType(cat: StandType) {
  standType.value = cat
  standTypeDialog.value = true
}

const getLockIcon = (lock: boolean) => {
  return lock ? 'pi pi-lock' : 'pi pi-lock-open'
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

        <Column :exportable="false" style="min-width: 12rem">
          <template #body="slotProps">
            <Button
              icon="pi pi-pencil"
              variant="outlined"
              rounded
              class="mr-2"
              @click="editStandType(slotProps.data)"
            />
          </template>
        </Column>
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
                </template>
              </Column>
            </DataTable>
          </div>
        </template>
      </DataTable>
    </div>
  </div>
</template>
