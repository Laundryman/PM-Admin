<script setup lang="ts">
import { useLocationFilters } from '@/components/composables/locationFilters'
import { Brand } from '@/models/Brands/brand.model'
import type { Country } from '@/models/Countries/country.model'
import { Region } from '@/models/Countries/region.model'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { Role } from '@/models/Identity/role.model'
import { User } from '@/models/Identity/user.model'
import CountryService from '@/services/Countries/CountryService'
import RoleService from '@/services/Identity/RoleService'
import UserService from '@/services/Identity/UserService.js'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'
import { useUserStore } from '@/stores/userStore'
import { useToast } from 'primevue/usetoast'
import { defineAsyncComponent, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const userId = ref('')
const systemStore = useSystemStore()
const brandStore = useBrandStore()
const userStore = useUserStore()
const locationFilters = useLocationFilters()
const toast = useToast()
const user = ref<User>(new User())
const brands = ref<Brand[]>([])
const countries = ref<Country[]>([])
const roles = ref<Role[]>([])

const ms_selectedRegions = ref<number[] | null>(null) // MultiSelect binding
const ms_selectedCountries = ref<number[] | null>(null) // MultiSelect binding
const selectAllCountries = ref(false)
const selectAllProducts = ref(false)
const regionSelectList = ref<Region[] | null>(null)
const countrySelectList = ref<Country[] | null>(null)

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
const currentUser = ref<User>(new User())
const submitted = ref(false)
const newPassword = ref(null)
const initialValues = ref(new User())
const userModel = ref<User>(new User())
const brandTabIndex = ref('0')
const brandTabs = ref([] as { title: string; content: string; value: string }[])
const currentBrandTabProperties = ref({
  brandId: 0,
  regions: [] as Region[] | null,
  countries: [] as Country[] | null,
  selectedRegions: [] as number[] | null,
  selectedCountries: [] as number[] | null,
})
const tabComponent = defineAsyncComponent(() => import('@/components/userBrandTab.vue'))

onMounted(async () => {
  loading.value = true
  systemStore.layoutState.disableBrandSelect = false
  userId.value = userStore.selectedUser.id || ''
  if (!userId.value) {
    toast.add({ severity: 'error', summary: 'Error', detail: 'No user selected', life: 3000 })
    router.push({ name: 'users' })
    return
  }
  await UserService.initialise()
  await UserService.getUser(userId.value)
    .then((response: User) => {
      userModel.value = response
      loading.value = false
    })
    .catch((error) => {
      console.log(error)
    })

  regionSelectList.value = await locationFilters.getRegions(new regionFilter())
  //setup brand tabs
  for (const brand of brandStore.brands) {
    if (userModel.value.brandIds?.includes(brand.id)) {
      brandTabs.value.push({
        title: brand.name,
        content: `Content for ${brand.name}`,
        value: brand.id.toString(),
      })
    }
  }

  brands.value = brandStore.brands

  let brandid = brandStore.activeBrand?.id ?? 0
  let rFilter = new regionFilter()
  rFilter.brandId = brandid

  await locationFilters.getRegions(rFilter).then((response) => {
    regionSelectList.value = response
  })

  await CountryService.initialise()
  await CountryService.getCountries().then((data: Country[]) => {
    countries.value = data
  })

  await RoleService.initialise()
  await RoleService.getRoles().then((data: Role[]) => {
    roles.value = data
  })
})

async function onTabChange(index: string) {
  let brandId = parseInt(index)
  brandTabIndex.value = index
  selectedBrands.value.push(brandId)
  let rFilter = new regionFilter()
  rFilter.brandId = brandId
  console.log('Getting regions for brand', brandId)
  await locationFilters.getRegions(rFilter).then((response) => {
    currentBrandTabProperties.value.regions = response
  })

  regionSelectList.value = currentBrandTabProperties.value.regions
  var brandSelectedRegions = regionSelectList.value?.filter((r) =>
    userModel.value.regions?.some((ur) => ur.id === r.id),
  )
  currentBrandTabProperties.value.selectedRegions = brandSelectedRegions?.map((r) => r.id) ?? []

  currentBrandTabProperties.value.brandId = brandId

  if (userModel.value.countryList) {
    currentBrandTabProperties.value.selectedCountries = userModel.value.countryList
      .split(',')
      .map((c) => parseInt(c))
  } else {
    currentBrandTabProperties.value.selectedCountries = []
  }
  currentBrandTabProperties.value.countries = userModel.value.countries || []
}

async function onBrandChange(evt: any) {
  let brandid = evt.value ? evt.value[0] : 0
  selectedBrands.value = evt.value
  //clear brandTabs
  brandTabs.value = []
  for (const brandId of selectedBrands.value) {
    brandTabs.value.push({
      title: brands.value.find((b) => b.id === brandId)?.name ?? '',
      content: `Content for ${brands.value.find((b) => b.id === brandId)?.name ?? ''}`,
      value: brandId.toString(),
    })
  }
}

async function saveUser() {
  submitted.value = true
  if (currentUser?.value?.newUserName?.trim()) {
    currentUser.value.userName = currentUser.value.newUserName
  }
}

////////////////////////////////////////////////////
// Generic Multi-Select Handler
///////////////////////////////////////////////////

function manageSelectedValues(
  selectedValues: number[],
  availableValues: any[],
  targetArray: any[],
) {
  if (selectedValues.length > 0) {
    for (const valueId of selectedValues) {
      let foundValue = targetArray.find((st) => st.id === valueId)
      if (!foundValue) {
        //handling unpublished items in the target array
        let item = availableValues?.find((st) => st.id === valueId)
        if (item) targetArray?.push(item)
      }
    }
  }
  let selectedValuesToRemove = new Array<number>()
  for (const item of targetArray ?? []) {
    let index = selectedValues.indexOf(item.id) //if it's been removed
    if (index == -1) {
      // let foundItem = availableValues.find((st) => st.id === item.id)
      // if (!foundItem)
      var published = item.published ?? true
      if (published === true) {
        selectedValuesToRemove.push(item.id)
      }
    }
  }
  if (selectedValuesToRemove.length > 0) {
    for (const stId of selectedValuesToRemove) {
      let removeIndex = targetArray.findIndex((st) => st.id === stId)
      if (removeIndex !== -1) {
        targetArray.splice(removeIndex, 1)
      }
    }
  }
}

////////////////////////////////////////////////////
// Location Handlers
////////////////////////////////////////////////////

async function onRegionChange(evt: any) {
  regionSelectList.value = currentBrandTabProperties.value.regions
  var brandId = parseInt(brandTabIndex.value) ?? 0
  var brandSelectedRegions = regionSelectList.value?.filter((r) =>
    userModel.value.regions?.some((ur) => ur.id === r.id),
  )

  var currentSelectList = userModel.value.regions
  // need to only remove or add regions from the current selected brand
  // first remove all regions from the user model that match the current brand
  var regionsToKeep = userModel.value.regions?.filter((r) => r.brandId != brandId) ?? []
  if (regionsToKeep.length > 0) {
    for (const r of regionsToKeep) {
      let index = userModel.value.regions?.findIndex((ur) => ur.id === r.id) ?? -1
      if (index !== -1) {
        userModel.value.regions?.splice(index, 1)
      }
    }
  }

  var userRegionList = userModel.value.regions?.map((r) => r.id).join(',') || ''
  var userRegionIdArray = userRegionList ? userRegionList.split(',').map((id) => parseInt(id)) : []
  userRegionIdArray.push(...(evt as number[]))

  manageSelectedValues(evt, regionSelectList.value ?? [], userModel.value.regions ?? [])
  userModel.value.regions.splice(0, 0, ...(regionsToKeep ?? [])) // add back any regions to keep at the start of the array to preserve any ordering if needed

  userModel.value.regionList = userModel.value.regions?.map((r) => r.id).join(',') || ''
  console.log('Regions Changed', userModel.value.regions)
}

async function onCountryChange(evt: any) {
  // manageSelectedCountries(evt.value)
  manageSelectedValues(evt.value, countrySelectList.value ?? [], userModel.value.countries ?? [])
  console.log('Selected Countries after region change', ms_selectedCountries.value)
  console.log('Part Model Countries after region change', userModel.value.countries)

  userModel.value.countryList = userModel.value.countries?.map((c) => c.id).join(',') || ''
}

function onSelectAllCountriesChange(event: any) {
  ms_selectedCountries.value = event.checked
    ? (countrySelectList.value?.map((item) => item.id) ?? [])
    : []
  selectAllCountries.value = event.checked
  manageSelectedValues(
    ms_selectedCountries.value,
    countrySelectList.value ?? [],
    userModel.value.countries ?? [],
  )
  userModel.value.countryList = userModel.value.countries?.map((c) => c.id).join(',') || ''
}

function clearCountrySelection() {
  ms_selectedCountries.value = []
  userModel.value.countries = []
  userModel.value.countryList = ''
}

/////////////////////////////////////////////////////
// Form Handlers
/////////////////////////////////////////////////////
const resolver = ({ values }: any) => {
  const errors = {} as any

  // if (!values.name) {
  //   errors.name = [{ message: 'Name is required.' }]
  // }
  // if (!values.partNumber) {
  //   errors.partNumber = [{ message: 'Part Number is required.' }]
  // }
  // if (!values.description) {
  //   errors.description = [{ message: 'Description is required.' }]
  // }
  // if (!values.categoryId) {
  //   errors.categoryId = [{ message: 'Category is required.' }]
  // }
  // if (!values.parentCategoryId) {
  //   errors.parentCategoryId = [{ message: 'Parent Category is required.' }]
  // }
  // if (!values.facings && values.facings !== 0) {
  //   errors.facings = [{ message: 'Facings is required.' }]
  // }
  // if (!values.height && values.height !== 0) {
  //   errors.height = [{ message: 'Height is required.' }]
  // }
  // if (!values.width && values.width !== 0) {
  //   errors.width = [{ message: 'Width is required.' }]
  // }
  // if (!values.stock && values.stock !== 0) {
  //   errors.stock = [{ message: 'Stock is required.' }]
  // }
  // if (!values.depth && values.depth !== 0) {
  //   errors.depth = [{ message: 'Depth is required.' }]
  // }
  // if (!values.unitCost && values.unitCost !== 0) {
  //   errors.unitCost = [{ message: 'Unit Cost is required.' }]
  // }
  return {
    values, // (Optional) Used to pass current form values to submit event.
    errors,
  }
}

async function onFormSubmit({ valid }: any) {
  if (valid) {
    await UserService.saveUser(user.value)
      .then(() => {
        toast.add({ severity: 'success', summary: 'Success', detail: 'User updated', life: 3000 })
        router.push({ name: 'userList' })
      })
      .catch((error) => {
        console.log(error)
        toast.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to update user',
          life: 3000,
        })
      })
  }
}
</script>
<template>
  <!-- <userBrandTab></userBrandTab> -->
  <div class="edit-part-view">
    <Toast position="top-right" group="tr" />
    <Toast position="bottom-center" group="bc" />
    <div class="mb-5">
      <Button
        label="Back to Users"
        icon="pi pi-arrow-left"
        class="p-button-text"
        @click="router.back()"
      />
    </div>
    <div class="w-full sticky bg-white top-16 block p-10 z-10">
      <h2>Edit User</h2>
      <div class="card flex flex-col gap-2">
        <span class="font-bold text-xl">{{ user.givenName }} {{ user.surname }}</span>
        <!-- <span class="text-gray-600">{{ user.surname }}</span> -->
      </div>
    </div>
    <div class="card grid grid-cols-1 gap-4 justify-center">
      <Form
        ref="part-form"
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
                <label for="name">First Name:</label>
                <InputText
                  v-model="userModel.givenName"
                  name="givenName"
                  :value="userModel.givenName"
                  type="text"
                  length="255"
                  placeholder="Part Name"
                  fluid
                />
                <Message
                  v-if="$form.givenName?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.givenName.error?.message }}</Message
                >
              </div>
              <div class="flex flex-col gap-1">
                <label for="surname">Surname:</label>
                <InputText
                  v-model="userModel.surname"
                  name="surname"
                  type="text"
                  placeholder="Customer Reference"
                  fluid
                  length="255"
                />
                <Message
                  v-if="$form.surname?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.surname.error?.message }}</Message
                >
              </div>
              <div class="flex flex-col gap-1">
                <label for="partNumber">User Name:</label>
                <InputText
                  v-model="userModel.givenName"
                  name="partNumber"
                  type="text"
                  placeholder="Part Number"
                  fluid
                  length="255"
                />
              </div>
              <div class="flex flex-col gap-1">
                <label for="partNumber">Email:</label>
                <InputText
                  v-model="userModel.userEmailAddress"
                  name="partNumber"
                  type="text"
                  placeholder="Part Number"
                  fluid
                  length="255"
                />
              </div>
              <div class="flex flex-col gap-1">
                <label for="ddCountries" class="font-semibold">User Country</label>
                <Select
                  id="ddCountries"
                  v-model="userModel.diamCountryId"
                  :options="countries"
                  optionValue="id"
                  optionLabel="name"
                  placeholder="Select a Country"
                  :invalid="submitted && !currentUser.diamCountryId"
                >
                </Select>
              </div>

              <div class="flex flex-col gap-1" v-if="!currentUser.id">
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
                  <small class="p-error" v-if="submitted && !newPassword"
                    >Password is required.</small
                  >
                </div>
              </div>
            </div>
          </fieldset>
        </div>
        <div class="bg-gray-50 col-span-2 p-10 mb-5">
          <fieldset legend="Location" class="col-span-2">
            <legend class="text-lg font-bold mb-2">Access</legend>
            <div class="grid grid-cols-2 gap-10">
              <div class="flex flex-col gap-1">
                <label for="ddBrands" class="font-semibold w-24 mb-3">Brands</label>

                <MultiSelect
                  id="ddBrands"
                  v-model="userModel.brandIds"
                  :options="brands"
                  optionLabel="name"
                  optionValue="id"
                  placeholder="Select Brands"
                  :maxSelectedLabels="3"
                  @change="onBrandChange"
                >
                </MultiSelect>
              </div>
              <div class="flex flex-col gap-1">
                <label for="ddRoles" class="font-semibold w-24 mb-3">Roles</label>

                <MultiSelect
                  id="ddRoles"
                  v-model="userModel.roleIds"
                  :options="roles"
                  optionLabel="name"
                  optionValue="id"
                  placeholder="Select Roles"
                  :maxSelectedLabels="3"
                />
              </div>
            </div>
          </fieldset>
        </div>

        <Tabs v-model:value="brandTabIndex">
          <ul class="flex gap-2 mb-5 border-b">
            <li v-for="tab in brandTabs" :key="tab.value" class="mr-2">
              <Button @click="onTabChange(tab.value)">
                {{ tab.title }}
              </Button>
              <!-- <Tab v-for="tab in brandTabs" :key="tab.title" :value="tab.value">
              {{ tab.title }}
            </Tab> -->
            </li>
          </ul>
          <TabPanels>
            <TabPanel v-for="tab in brandTabs" :key="tab.content" :value="tab.value">
              <p class="m-0">{{ tab.content }}</p>
              <component
                :is="tabComponent"
                :key="tab.value"
                :brandId="parseInt(tab.value)"
                :regions="currentBrandTabProperties.regions"
                :countries="currentBrandTabProperties.countries"
                :selectedRegions="currentBrandTabProperties.selectedRegions"
                :selectedCountries="currentBrandTabProperties.selectedCountries"
                @update:selectedRegions="onRegionChange"
                @update:selectedCountries="onCountryChange"
              ></component>
            </TabPanel>
          </TabPanels>
        </Tabs>

        <!-- <div class="bg-gray-50 col-span-2 p-10 mb-5">
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
                <template v-for="country in userModel.countries">
                  <Chip class="flex-wrap" :label="country.name"></Chip>
                </template>
              </div>
            </div>
          </fieldset>
        </div> -->
      </Form>
    </div>
  </div>

  <div class="card">
    <h1>Manage User</h1>
    <p>Here you can manage users.</p>
    <div class="field">
      <label for="username">Username</label>
      <InputText id="username" type="text" v-model="user.userName" />
    </div>
    <div class="field">
      <label for="email">Email</label>
      <InputText id="email" type="text" v-model="user.mail" />
    </div>
    <div class="field">
      <label for="firstName">First Name</label>
      <InputText id="firstName" type="text" v-model="user.displayName" />
    </div>
    <div class="field">
      <label for="lastName">Last Name</label>
      <InputText id="lastName" type="text" v-model="user.surname" />
    </div>
    <!-- <div class="field">
      <label for="country">Country</label>
      <Dropdown
        id="country"
        :options="countries"
        optionLabel="name"
        optionValue="id"
        v-model="user.diamCountryId"
        placeholder="Select a Country"
      />
    </div> -->
    <!-- <div class="field">
      <label for="brand">Brand</label>
      <Dropdown
        id="brand"
        :options="brands"
        optionLabel="name"
        optionValue="id"
        v-model="user.brandIds"
        placeholder="Select a Brand"
      />
    </div>
    <div class="field">
      <label for="role">Role</label>
      <Dropdown
        id="role"
        :options="roles"
        optionLabel="name"
        optionValue="id"
        v-model="user.roleIds"
        placeholder="Select a Role"
      />
    </div> -->
    <Button label="Save" @click="saveUser" />
  </div>
</template>
