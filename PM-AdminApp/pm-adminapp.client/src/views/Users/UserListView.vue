<script setup lang="ts">
import { Brand } from '@/models/Brands/brand.model'
import type { Country } from '@/models/Countries/country.model'
import { Role } from '@/models/Identity/role.model'
import { User } from '@/models/Identity/user.model'
import CountryService from '@/services/Countries/CountryService'
import RoleService from '@/services/Identity/RoleService'
import UserService from '@/services/Identity/UserService.js'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'
import { FilterMatchMode, FilterService } from '@primevue/core/api'
import { useToast } from 'primevue/usetoast'
import { onMounted, ref } from 'vue'
// import InputHTMLAttributes from "vue";
// import { RefSymbol } from '@vue/reactivity'
//import { User } from '@/models/user.model'
const systemStore = useSystemStore()
const brandStore = useBrandStore()
const toast = useToast()
const users = ref<User[]>([])
const brands = ref<Brand[]>([])
const countries = ref<Country[]>([])
const roles = ref<Role[]>([])
const userDialog = ref()
const passwordDialog = ref()
const deleteUserDialog = ref()
const deleteUsersDialog = ref()
const selectedUser = ref()
const selectedUsers = ref()
const selectedCountry = ref()
const selectedBrands = ref<number[]>([])
const selectedRoles = ref<number[]>([])
const lazyLoading = ref(false)
const loading = ref(true)
const currentUser: any = ref({})
const submitted = ref(false)
const newPassword = ref(null)
onMounted(async () => {
  loading.value = true
  systemStore.layoutState.disableBrandSelect = false
  await UserService.initialise()
  await UserService.getPMUsers()
    .then((response: User[]) => {
      users.value = response
      loading.value = false
    })
    .catch((error) => {
      console.log(error)
    })

  // BrandService.getBrands().then((data) => {
  //   brands.value = data.data
  // })

  brands.value = brandStore.brands

  await CountryService.initialise()
  await CountryService.getCountries().then((data: Country[]) => {
    countries.value = data
  })

  await RoleService.initialise()
  await RoleService.getRoles().then((data: Role[]) => {
    roles.value = data
  })

  convertUserFKeys(users.value)

  FilterService.register(Brand_FILTER.value, (value, filter) => {
    if (filter === undefined || filter === null || filter.trim() === '') {
      return true
    }

    if (value === undefined || value === null) {
      return false
    }

    return value.toString() === filter.toString()
  })
})

function convertUserFKeys(userList: User[]) {
  for (const usr of userList) {
    usr.brands = []
    usr.roles = []
    usr.brandNameList = []
    usr.roleNameList = []
    // usr.brandNameList = ''
    if (usr.roleIds) {
      for (const rid of usr.roleIds) {
        let foundRole = roles.value.find((r) => r.id === rid)
        if (foundRole !== undefined && foundRole !== null) {
          let role = new Role()
          role.id = foundRole.id
          role.name = foundRole.name
          usr.roles.push(role)
          usr.roleNameList.push(role.name)
        }
      }
    }
    if (usr.brandIds) {
      for (const bid of usr.brandIds) {
        let foundBrand = brands.value.find((b) => b.id === bid)
        if (foundBrand !== undefined && foundBrand !== null) {
          let brand = new Brand()
          brand.id = foundBrand.id
          brand.name = foundBrand.name
          usr.brands.push(brand)
          usr.brandNameList.push(brand.name)
        }
      }
    }
    if (usr['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId']) {
      usr.diamCountryId = usr['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId']
      let country = countries.value.find((c) => c.id === usr.diamCountryId)
      if (country) {
        usr.country = country
      }
    }
  }
}

var APPURL = import.meta.env.VITE_APP_SERVER_URL
const Brand_FILTER = ref('BRAND FILTER')
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  userName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
  surname: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
  givenName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
  // Email: { value: null, matchMode: FilterMatchMode.STARTS_WITH }
  'country.name': { value: null, matchMode: FilterMatchMode.STARTS_WITH },
  brandNameList: { value: null, matchMode: FilterMatchMode.CONTAINS },
  roleNameList: { value: null, matchMode: FilterMatchMode.CONTAINS },
})

const matchModeOptions = ref([
  // { label: 'Starts With', value: Brand_FILTER.value },
  { label: 'Starts With', value: FilterMatchMode.STARTS_WITH },
])

function editUser(usr: User) {
  currentUser.value = { ...usr }
  if (currentUser.value.Brands) {
    selectedBrands.value = currentUser.value.BrandIds.split(',').map((b: any) => parseInt(b))
  }
  if (currentUser.value.Roles) {
    selectedRoles.value = currentUser.value.RoleIds.split(',').map((r: any) => parseInt(r))
  }

  selectedCountry.value = currentUser.value.DiamCountryId ?? null

  userDialog.value = true
}

const createUser = () => {
  currentUser.value = {}
  selectedBrands.value = []
  selectedRoles.value = []
  selectedCountry.value = null
  newPassword.value = null
  userDialog.value = true
}

const changePassword = (usr: any) => {
  currentUser.value = { ...usr }
  selectedBrands.value = currentUser.value.BrandIds.split(',')
  selectedRoles.value = currentUser.value.RoleIds.split(',')
  selectedCountry.value = currentUser.value.DiamCountryId

  passwordDialog.value = true
}

const hideDialog = () => {
  userDialog.value = false
  submitted.value = false
}

const savePassword = () => {
  submitted.value = true
  if (currentUser?.value?.UserName?.trim()) {
    if (currentUser.value.Id) {
      currentUser.value.Password = newPassword.value
      UserService.changePassword(currentUser.value)
        .then((response) => {
          //users.value[findIndexById(currentUser.value.Id)] = currentUser.value
          //users.value = [...users.value]
          toast.add({
            severity: 'success',
            summary: 'Successful',
            detail: 'User Updated',
            life: 3000,
          })
        })
        .catch((error) => {
          console.log(error)
          toast.add({
            severity: 'error',
            summary: 'Update failed',
            detail: error.message || 'User not updated. Please try again.',
            life: 3000,
          })
        })
    } else {
      toast.add({
        severity: 'error',
        summary: 'Update failed',
        detail: 'Cannot find user',
        life: 3000,
      })
    }
    passwordDialog.value = false
    submitted.value = false
    currentUser.value = {}
    newPassword.value = null
  }
}
async function saveUser() {
  submitted.value = true
  if (currentUser?.value?.newUserName?.trim()) {
    currentUser.value.UserName = currentUser.value.newUserName
  }
  if (currentUser?.value?.userName?.trim()) {
    // if (selectedBrands.value !== undefined) {
    //   currentUser.value.brandIds = selectedBrands.value.join(',')
    // }
    // if (selectedRoles.value !== undefined) {
    //   currentUser.value.roleIds = selectedRoles.value.join(',')
    // }
    if (currentUser.value.id) {
      await UserService.initialise()
      await UserService.saveUser(currentUser.value)
      await UserService.saveUser(currentUser.value)
        .then(async (response) => {
          var updatedUser = await UserService.getUser(currentUser.value.id)
          convertUserFKeys([updatedUser])
          users.value[findIndexById(currentUser.value.id)] = updatedUser
          toast.add({
            severity: 'success',
            summary: 'Successful',
            detail: 'User Updated',
            life: 3000,
          })
          userDialog.value = false
          submitted.value = false
          // currentUser.value = {}
          // selectedBrands.value = []
          // selectedRoles.value = []
          // selectedCountry.value = null
        })
        .catch((error) => {
          console.log(error)
          toast.add({
            severity: 'error',
            summary: 'Update failed',
            detail: error.message || 'User not updated. Please try again.',
            life: 3000,
          })
        })
        .finally(() => {})
    } else {
      currentUser.value.Password = newPassword.value
      UserService.createUser(currentUser.value)
        .then((response) => {
          users.value.push(response.data)
          toast.add({
            severity: 'success',
            summary: 'Successful',
            detail: 'User Created',
            life: 4000,
          })
          userDialog.value = false
          submitted.value = false
          currentUser.value = {}
          selectedBrands.value = []
          selectedRoles.value = []
          selectedCountry.value = null
        })
        .catch((error) => {
          console.log(error)
          var errMessage = 'Could not create user'
          if (error.response.data.includes('User with the same username already exists')) {
            errMessage = 'A user with this username already exists'
          } else if (error.response.data.includes('password does not comply')) {
            errMessage = 'The password does not comply with password complexity requirements'
          }

          toast.add({
            severity: 'error',
            summary: 'Create failed',
            detail: errMessage,
            life: 4000,
          })
        })
        .finally(() => {})
    }
  }
}

const deleteUser = () => {
  users.value = users.value.filter((val: any) => val.id !== currentUser.value.id)
  deleteUserDialog.value = false
  currentUser.value = {}
  toast.add({ severity: 'success', summary: 'Successful', detail: 'Product Deleted', life: 3000 })
}

const deleteSelectedUsers = () => {
  users.value = users.value.filter((val: any) => !selectedUsers.value.includes(val))
  deleteUsersDialog.value = false
  selectedUsers.value = null
  toast.add({ severity: 'success', summary: 'Successful', detail: 'Products Deleted', life: 3000 })
}

const confirmDeleteUser = (usr: any) => {
  currentUser.value = usr
  deleteUserDialog.value = true
}

const confirmDeleteSelected = () => {
  deleteUsersDialog.value = true
}
const findIndexById = (id: string) => {
  let index = -1
  for (let i = 0; i < users.value.length; i++) {
    if (users.value[i] != null && users.value[i] != undefined) {
      if (users.value[i]?.id === id) {
        index = i
        break
      }
    }
  }

  return index
}

const getCountryName = (countryId: string) => {
  const country = countries.value?.find((c: any) => c.CountryId === countryId)
  return country ? country.name : ''
}
</script>

<template>
  <Toast />

  <div class="card">
    <Toolbar class="mb-4">
      <template #start>
        <Button label="New" icon="pi pi-plus" severity="success" class="mr-2" @click="createUser" />
        <Button
          label="Delete"
          icon="pi pi-trash"
          severity="danger"
          @click="confirmDeleteSelected"
          :disabled="!selectedUsers || !selectedUsers.length"
        />
      </template>
    </Toolbar>

    <DataTable
      v-model:filters="filters"
      filterDisplay="row"
      :globalFilterFields="[
        'givenName',
        'surname',
        'userName',
        'country.name',
        'brandNameList',
        'roleNameList',
      ]"
      ref="dt"
      :value="users"
      v-model:selection="selectedUsers"
      dataKey="id"
      :paginator="true"
      :rows="10"
      selectionMode="single"
      :loading="loading"
      scrollable
      scrollHeight="600px"
      tableStyle="min-width: 50rem"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
      :rowsPerPageOptions="[5, 10, 25]"
      currentPageReportTemplate="Showing {first} to {last} of {totalRecords} products"
    >
      <template #header>
        <div class="flex flex-wrap gap-2 align-items-center justify-content-between">
          <h4 class="m-0">Manage Users</h4>
          <IconField iconPosition="left">
            <InputIcon>
              <i class="pi pi-search" />
            </InputIcon>
            <InputText v-model="filters['global'].value" placeholder="Search..." />
          </IconField>
        </div>
      </template>
      <template #empty> No users found. </template>
      <template #loading>
        Loading users data. Please wait.
        <ProgressSpinner />
      </template>
      <!-- <Column selectionMode="multiple" style="width: 3rem" :exportable="false"></Column> -->
      <Column
        field="givenName"
        filterField="givenName"
        header="First Name"
        sortable
        :filterMatchModeOptions="matchModeOptions"
        style="max-width: 10rem"
      >
        <template #body="{ data }">
          {{ data.givenName }}
        </template>
        <template #filter="{ filterModel, filterCallback }">
          <InputText
            v-model="filterModel.value"
            type="text"
            @input="filterCallback()"
            class="p-column-filter"
            placeholder="Search by first name"
          />
        </template>
      </Column>
      <Column
        field="surname"
        filterField="surname"
        header="Surname"
        sortable
        :filterMatchModeOptions="matchModeOptions"
        style="max-width: 10rem"
      >
        <template #body="{ data }">
          {{ data.surname }}
        </template>
        <template #filter="{ filterModel, filterCallback }">
          <InputText
            v-model="filterModel.value"
            type="text"
            @input="filterCallback()"
            class="p-column-filter"
            placeholder="Search by Surname"
          />
        </template>
      </Column>
      <Column
        field="userName"
        filterField="userName"
        header="User Name"
        sortable
        :filterMatchModeOptions="matchModeOptions"
        style="max-width: 10rem"
      >
        <template #body="{ data }">
          {{ data.userName }}
        </template>
        <template #filter="{ filterModel, filterCallback }">
          <InputText
            v-model="filterModel.value"
            type="text"
            @input="filterCallback()"
            class="p-column-filter"
            autocomplete="one-time-code"
            placeholder="Filter columns"
          />
        </template>
      </Column>
      <Column
        field="country.name"
        filterField="country.name"
        sortField="country.name"
        header="Country"
        :filterMatchModeOptions="matchModeOptions"
        style="max-width: 10rem"
        sortable
      >
        <template #body="{ data }">
          <span v-if="data.country">
            {{ data.country.name }}
          </span>
        </template>
        <template #filter="{ filterModel, filterCallback }">
          <InputText
            v-model="filterModel.value"
            type="text"
            @input="filterCallback()"
            class="p-column-filter"
            placeholder="Search by Country"
          />
        </template>
      </Column>
      <Column
        header="Brands"
        style="max-width: 10rem"
        filterField="brandNameList"
        sortField="brandNameList"
      >
        <template #body="{ data }">
          <span v-if="data.brandNameList">
            {{ data.brandNameList.join(', ') }}
          </span>
        </template>
        <template #filter="{ filterModel, filterCallback }">
          <InputText
            v-model="filterModel.value"
            type="text"
            @input="filterCallback()"
            class="p-column-filter"
            placeholder="Search by Brand"
          />
        </template>
      </Column>

      <Column
        header="Roles"
        style="max-width: 10rem"
        filterField="roleNameList"
        sortField="roleNameList"
      >
        <template #body="{ data }">
          <span v-if="data.roleNameList">
            {{ data.roleNameList.join(', ') }}
          </span>
        </template>
        <template #filter="{ filterModel, filterCallback }">
          <InputText
            v-model="filterModel.value"
            type="text"
            autocomplete="one-time-code"
            @input="filterCallback()"
            class="p-column-filter"
            placeholder="Search by Role"
          />
        </template>
      </Column>

      <Column :exportable="false" style="min-width: 8rem">
        <template #body="slotProps">
          <span>&nbsp;</span>
          <Button
            v-tooltip="'edit user'"
            icon="pi pi-pencil"
            outlined
            rounded
            class="mr-2"
            @click="editUser(slotProps.data)"
          />
          <Button
            v-tooltip="'change password'"
            icon="pi pi-unlock"
            outlined
            rounded
            severity="warning"
            class="mr-2"
            @click="changePassword(slotProps.data)"
          />
        </template>
      </Column>
    </DataTable>
  </div>

  <Dialog v-model:visible="userDialog" :style="{ width: '50rem' }" :modal="true" class="p-fluid">
    <template #header>
      <div class="inline-flex items-center justify-center gap-2">
        <span class="font-bold whitespace-nowrap">{{
          currentUser.userName ?? currentUser.displayName
        }}</span>
      </div>
    </template>
    <span class="font-semibold block mb-8">Update user information.</span>
    <div class="flex items-center gap-4 mb-4" v-if="currentUser.userName">
      <label for="name">User Name</label>
      <InputText
        id="name"
        v-model.trim="currentUser.userName"
        required="false"
        autocomplete="one-time-code"
        autofocus
        disabled="true"
      />
      <small class="p-error" v-if="submitted && !currentUser.givenName">Name is required.</small>
    </div>
    <div class="flex items-center gap-4 mb-4" v-else>
      <label for="username" class="font-semibold w-24">Username</label>
      <InputText
        id="name"
        v-model.trim="currentUser.newUserName"
        required="true"
        autocomplete="one-time-code"
        autofocus
        class="w-3/4"
      />
      <small class="p-error" v-if="submitted && !currentUser.givenName">Name is required.</small>
    </div>

    <div class="flex items-center gap-4 mb-4">
      <!-- <label for="DiamUserId">Diam User Id</label>
      <Textarea
        id="diamUserId"
        v-model="currentUser.DiamUserId"
        required="true"
        rows="3"
        cols="20"
      /> -->
    </div>
    <div class="flex items-center gap-4 mb-4">
      <label for="GivenName" class="font-semibold w-24">First Name</label>
      <InputText id="givenName" v-model="currentUser.givenName" required="true" class="w-3/4" />
    </div>
    <div class="flex items-center gap-4 mb-4">
      <label for="Surname" class="font-semibold w-24">Surname</label>
      <InputText id="surname" v-model="currentUser.surname" required="true" class="w-3/4" />
    </div>
    <div class="flex items-center gap-4 mb-4">
      <label for="UserEmailAddress" class="font-semibold w-24">Email</label>
      <InputText
        id="UserEmailAddress"
        v-model="currentUser.userEmailAddress"
        required="true"
        class="w-3/4"
      />
    </div>
    <div class="flex items-center gap-4 mb-4" v-if="!currentUser.id">
      <label for="Password" class="font-semibold w-24">Password</label>
      <div class="flex w-3/4 flex-column">
        <Password
          v-model="newPassword"
          toggleMask
          :inputProps="{ autocomplete: 'one-time-code' }"
          :style="{ width: '100%' }"
          :inputStyle="{ width: '100%' }"
        >
          <template #header>
            <h6>Pick a password</h6>
          </template>
          <template #footer>
            <Divider />
            <p class="mt-2">Suggestions</p>
            <ul class="pl-2 ml-2 mt-0" style="line-height: 1.5">
              <li>At least one lowercase</li>
              <li>At least one uppercase</li>
              <li>At least one numeric</li>
              <li>Minimum 8 characters</li>
            </ul>
          </template>
        </Password>
        <small class="p-error" v-if="submitted && !newPassword">Password is required.</small>
      </div>
    </div>

    <div class="flex items-center gap-4 mb-4">
      <label for="ddCountries" class="font-semibold w-24 mb-3">Selected Country</label>
      <Select
        id="ddCountries"
        v-model="currentUser.diamCountryId"
        :options="countries"
        optionValue="id"
        optionLabel="name"
        placeholder="Select a Country"
        :invalid="submitted && !currentUser.diamCountryId"
      >
        <!-- <template #value="slotProps">
          <div v-if="slotProps.value">{{ getCountryName(slotProps.value) }}</div>
          <span v-else>
            {{ slotProps.placeholder }}
          </span>
        </template> -->
      </Select>
    </div>
    <div class="flex items-center gap-4 mb-4">
      <label for="ddBrands" class="font-semibold w-24 mb-3">Brands</label>

      <MultiSelect
        id="ddBrands"
        v-model="currentUser.brandIds"
        :options="brands"
        optionLabel="name"
        optionValue="id"
        placeholder="Select Brands"
        :maxSelectedLabels="3"
        class="w-3/4"
      >
      </MultiSelect>
    </div>
    <div class="flex items-center gap-4 mb-4 w-full">
      <label for="ddRoles" class="font-semibold w-24 mb-3">Roles</label>

      <MultiSelect
        id="ddRoles"
        v-model="currentUser.roleIds"
        :options="roles"
        optionLabel="name"
        optionValue="id"
        placeholder="Select Roles"
        :maxSelectedLabels="3"
        class="flex w-3/4"
      />
    </div>

    <template #footer>
      <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
      <Button label="Save" icon="pi pi-check" text @click="saveUser" />
    </template>
  </Dialog>

  <Dialog
    v-model:visible="passwordDialog"
    :style="{ width: '350px' }"
    header="Change Password"
    :modal="true"
    class="p-fluid"
  >
    <h2>Change Password for {{ currentUser.UserName }}</h2>
    <div class="field">
      <label for="Password">New Password</label>
      <Password v-model="newPassword" :inputProps="{ autocomplete: 'one-time-code' }" toggleMask>
        <template #header>
          <h6>Pick a password</h6>
        </template>
        <template #footer>
          <Divider />
          <p class="mt-2">Suggestions</p>
          <ul class="pl-2 ml-2 mt-0" style="line-height: 1.5">
            <li>At least one lowercase</li>
            <li>At least one uppercase</li>
            <li>At least one numeric</li>
            <li>Minimum 8 characters</li>
          </ul>
        </template>
      </Password>
      <small class="p-error" v-if="submitted && !newPassword">Password is required.</small>
    </div>

    <template #footer>
      <Button label="Cancel" icon="pi pi-times" text @click="hideDialog" />
      <Button label="Save" icon="pi pi-check" text @click="savePassword" />
    </template>
  </Dialog>

  <Dialog
    v-model:visible="deleteUserDialog"
    :style="{ width: '450px' }"
    header="Confirm"
    :modal="true"
  >
    <div class="confirmation-content">
      <i class="pi pi-exclamation-triangle mr-3" style="font-size: 2rem" />
      <span v-if="currentUser"
        >Are you sure you want to delete <b>{{ currentUser.UserName }}</b
        >?</span
      >
    </div>
    <template #footer>
      <Button label="No" icon="pi pi-times" text @click="deleteUserDialog = false" />
      <Button label="Yes" icon="pi pi-check" text @click="deleteUser" />
    </template>
  </Dialog>

  <Dialog
    v-model:visible="deleteUsersDialog"
    :style="{ width: '450px' }"
    header="Confirm"
    :modal="true"
  >
    <div class="confirmation-content">
      <i class="pi pi-exclamation-triangle mr-3" style="font-size: 2rem" />
      <span v-if="currentUser">Are you sure you want to delete the selected users?</span>
    </div>
    <template #footer>
      <Button label="No" icon="pi pi-times" text @click="deleteUsersDialog = false" />
      <Button label="Yes" icon="pi pi-check" text @click="deleteSelectedUsers" />
    </template>
  </Dialog>
</template>

<style scoped>
.events {
  display: flex;
  flex-direction: column;
  align-items: center;
}
</style>
