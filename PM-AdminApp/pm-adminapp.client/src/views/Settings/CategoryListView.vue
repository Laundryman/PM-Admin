<script setup lang="ts">
import { Category } from '@/models/Categories/category.model'
import { default as categoryService } from '@/services/Categories/CategoryService'
import { useSystemStore } from '@/stores/systemStore'
import { FilterMatchMode } from '@primevue/core/api'
import { useToast } from 'primevue/usetoast'
import { onMounted, ref } from 'vue'

const categories = ref()
const selectedCategory = ref()
const categoryDialog = ref(false)
const submitted = ref(false)
const category = ref(new Category())
const expandedRows = ref()
const toast = useToast()
function onRowGroupExpand(event: any) {
  toast.add({
    severity: 'info',
    summary: 'Row Group Expanded',
    detail: 'Value: ' + event.data,
    life: 3000,
  })
}
function onRowGroupCollapse(event: any) {
  toast.add({
    severity: 'success',
    summary: 'Row Group Collapsed',
    detail: 'Value: ' + event.data,
    life: 3000,
  })
}

async function onRowExpand(event: any) {
  // await categoryService.getChildCategories(event.data.id).then((data) => {
  //   childCategories.value = data.data
  //   // event.data.children = childCategories.value
  // })
  toast.add({ severity: 'info', summary: 'Category Expanded', detail: event.data.name, life: 3000 })
}
const onRowCollapse = (event: any) => {
  toast.add({
    severity: 'success',
    summary: 'Category Collapsed',
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
  await categoryService.initialise()

  await categoryService.getParentCategories().then((data) => {
    // create categoryOptions array for select dropdown
    categories.value = data.data
    console.log('Categories loaded:', categories.value)
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
  category.value = {} as Category
  submitted.value = false
  categoryDialog.value = true
}
const hideDialog = () => {
  categoryDialog.value = false
  submitted.value = false
}
async function saveCategory() {
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

  if (category?.value.name?.trim()) {
    formData.append('name', category.value.name)
    // formData.append('shelfLock', String(category.value.shelfLock))
    // formData.append('disabled', String(category.value.disabled))
    formData.append('id', String(category.value.id ?? 0))
    formData.append('file', file.value ?? '')
    if (category.value.id) {
      await categoryService
        .updateCategory(formData)
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
      category.value.id = 0
      // category.value.categoryLogo = 'category-placeholder.svg'
      categories.value.push(category.value)
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'Category Created',
        life: 3000,
      })
    }

    categoryDialog.value = false
    category.value = new Category()
  }
}
function editCategory(cat: Category) {
  category.value = cat
  categoryDialog.value = true
}

function calculatePCatTotal(name: string) {
  let total = 0

  if (categories.value) {
    for (let cat of categories.value) {
      if (cat.parentCategoryName === name) {
        total++
      }
    }
  }

  return total
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
        :value="categories"
        dataKey="id"
        rowHover
        tableStyle="min-width: 50rem"
        class="hover:bg-gray-100"
        @rowExpand="onRowExpand"
        @rowCollapse="onRowCollapse"
        sortMode="single"
        sortField="parentCategoryName"
        :sortOrder="1"
      >
        <template #header>
          <div class="flex flex-wrap gap-2 items-center justify-between">
            <h4 class="m-0">Manage Categories</h4>
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
            slotProps.data.parentCategoryName
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
              @click="editCategory(slotProps.data)"
            />
          </template>
        </Column>
        <template #expansion="slotProps">
          <div class="p-4">
            <!-- <h5>{{ slotProps.data.name }}</h5> -->
            <DataTable :value="slotProps.data.subCategories" tableStyle="min-width: 50rem">
              <Column
                field="parentCategory.name"
                header="Parent Category"
                sortable
                style="min-width: 16rem"
              ></Column>
              <Column field="name" header="Name" sortable></Column>
            </DataTable>
          </div>
        </template>
      </DataTable>
    </div>
  </div>
</template>
