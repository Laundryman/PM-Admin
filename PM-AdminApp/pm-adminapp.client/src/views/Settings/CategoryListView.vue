<script setup lang="ts">
import { Category } from '@/models/Categories/category.model'
import { default as categoryService } from '@/services/Categories/CategoryService'
import { useSystemStore } from '@/stores/systemStore'
import { FilterMatchMode } from '@primevue/core/api'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { onMounted, ref } from 'vue'

const categories = ref()
const selectedCategory = ref()
const categoryDialog = ref(false)
const submitted = ref(false)
const category = ref(new Category())
const expandedRows = ref()
const toast = useToast()
const confirm = useConfirm()
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

const hideDialog = () => {
  categoryDialog.value = false
  submitted.value = false
}
async function saveCategory(cat: Category) {
  submitted.value = true

  if (cat?.name?.trim()) {
    if (cat.id) {
      await categoryService
        .updateCategory(cat)
        .then((response) => {
          if (response && response.data) {
            console.log(response.data)
          }
        })
        .catch((error) => {
          console.error('Error updating category:', error)
        })
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'Category Updated',
        life: 3000,
      })
    } else {
      cat.id = 0
      await categoryService
        .addCategory(cat)
        .then((newCat) => {
          categories.value
            .find((c: Category) => c.id === newCat.parentCategoryId)
            ?.subCategories?.push(newCat)

          // console.log(response.data)
        })
        .catch((error) => {
          console.error('Error creating category:', error)
        })
      categories.value.push(cat)
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
function deleteCategory(cat: Category) {
  category.value = cat
  categoryService
    .deleteCategory(cat.id)
    .then((response) => {
      var subCats = categories.value.find(
        (c: Category) => c.id == cat.parentCategoryId,
      ).subCategories
      subCats.splice(
        subCats.findIndex((sc: Category) => sc.id == cat.id),
        1,
      )

      categories.value.find((c: Category) => c.id == cat.parentCategoryId).subCategories = subCats
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'Category Deleted',
        life: 6000,
      })
    })
    .catch((error) => {
      console.error('Error deleting category:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to delete category',
        life: 6000,
      })
    })
}
function addCategory() {
  category.value = {} as Category
  category.value.parentCategoryId = 0
  category.value.parentCategoryName = ''
  categoryDialog.value = true
}

function addChildCategory(cat: Category) {
  category.value = {} as Category
  category.value.parentCategoryId = cat.id
  category.value.parentCategoryName = cat.name
  categoryDialog.value = true
}

// function confirmSave(event: Event, cat: Category) {
//   confirm.require({
//     target: event.currentTarget as HTMLElement,
//     message: 'Are you sure you want to proceed?',
//     icon: 'pi pi-exclamation-triangle',
//     rejectProps: {
//       label: 'Cancel',
//       severity: 'secondary',
//       outlined: true,
//     },
//     acceptProps: {
//       label: 'Save',
//     },
//     accept: async () => {
//       // toast.add({ severity: 'info', summary: 'Confirmed', detail: 'You have accepted', life: 3000 })
//       var newCat = await saveCategory(cat)
//     },
//     reject: () => {
//       toast.add({ severity: 'error', summary: 'Rejected', detail: 'You have rejected', life: 3000 })
//     },
//   })
// }

function confirmDelete(event: Event, cat: Category) {
  confirm.require({
    target: event.currentTarget as HTMLElement,
    message: 'Are you sure you want to delete this category?',
    icon: 'pi pi-exclamation-triangle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true,
    },
    acceptProps: {
      label: 'Delete',
      severity: 'danger',
    },
    accept: () => {
      // toast.add({ severity: 'info', summary: 'Confirmed', detail: 'You have accepted', life: 3000 })
      deleteCategory(cat)
    },
    reject: () => {
      toast.add({ severity: 'error', summary: 'Rejected', detail: 'You have rejected', life: 3000 })
    },
  })
}
</script>

<template>
  <div>
    <div class="card">
      <Toolbar class="mb-6">
        <template #start>
          <Button
            label="New"
            icon="pi pi-plus"
            class="mr-2"
            @click="addCategory()"
            v-tooltip="'Add Parent Category'"
          />
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
              v-tooltip="'Edit Category'"
              @click="editCategory(slotProps.data)"
            />
            <Button
              icon="pi pi-plus"
              variant="outlined"
              rounded
              class="mr-2"
              v-tooltip="'Add Child Category'"
              @click="addChildCategory(slotProps.data)"
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
              <Column :exportable="false" style="min-width: 12rem">
                <template #body="childSlotProps">
                  <Button
                    icon="pi pi-trash"
                    variant="outlined"
                    rounded
                    class="mr-2"
                    v-tooltip="'Delete Category'"
                    @click="confirmDelete($event, childSlotProps.data)"
                  />
                </template>
              </Column>
            </DataTable>
          </div>
        </template>
      </DataTable>
    </div>
    <Dialog
      v-model:visible="categoryDialog"
      :style="{ width: '450px' }"
      header="Category Details"
      :modal="true"
    >
      <div class="flex flex-col gap-6">
        <div>
          <label for="name" class="block font-bold mb-3">Name</label>
          <InputText
            id="name"
            v-model.trim="category.name"
            required="true"
            autofocus
            :invalid="submitted && !category.name"
            fluid
          />
          <small v-if="submitted && !category.name" class="text-red-500">Name is required.</small>
        </div>
      </div>
      <template #footer>
        <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
        <Button label="Save" icon="pi pi-check" @click="saveCategory(category)" />
      </template>
    </Dialog>
  </div>
  <Toast></Toast>
  <ConfirmPopup></ConfirmPopup>
</template>
