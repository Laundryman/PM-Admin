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
  selectedCountries: [] as number[] | null,
})

const currentRegionProps = ref({
  regions: [] as Region[] | null,
  selectedRegions: [] as number[] | null,
})
const tabComponent = defineAsyncComponent(() => import('@/components/userBrandTab.vue'))

onMounted(async () => {
  loading.value = true
  systemStore.layoutState.disableBrandSelect = false
  userId.value = userStore.selectedUser.id || ''
  // if (!userId.value) {
  //   toast.add({ severity: 'error', summary: 'Error', detail: 'No user selected', life: 3000 })
  //   router.push({ name: 'users' })
  //   return
  // }
  await UserService.initialise()

  if (!userId.value) {
    //creating new user
    userModel.value = new User()
    loading.value = false
  } else {
    await UserService.getUser(userId.value)
      .then((response: User) => {
        userModel.value = response
        loading.value = false
      })
      .catch((error) => {
        console.log(error)
      })
  }
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

  convertUserFKeys(userModel.value)
  console.log('Initial User Model after FKey conversion', userModel.value)
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
    currentRegionProps.value.regions = response
  })

  regionSelectList.value = currentBrandTabProperties.value.regions
  var brandSelectedRegions = regionSelectList.value?.filter((r) =>
    userModel.value.regions?.some((ur) => ur.id === r.id),
  )

  currentRegionProps.value.selectedRegions = brandSelectedRegions?.map((r) => r.id) ?? []

  currentBrandTabProperties.value.brandId = brandId

  //now get the countries for the selected regions
  if (
    currentRegionProps.value.selectedRegions &&
    currentRegionProps.value.selectedRegions.length > 0
  ) {
    countrySelectList.value = await locationFilters.getCountriesForRegions(
      currentRegionProps.value.selectedRegions,
    )
  } else {
    countrySelectList.value = []
  }
  currentBrandTabProperties.value.countries = countrySelectList.value

  if (userModel.value.countries) {
    var userCountryIds = userModel.value.countries.map((c) => c.id)
    for (const cId of userCountryIds) {
      if (countrySelectList.value?.some((c) => c.id === cId)) {
        currentBrandTabProperties.value.selectedCountries?.push(cId)
      }
    }
  } else {
    currentBrandTabProperties.value.selectedCountries = []
  }
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

async function convertUserFKeys(usr: User) {
  usr.brands = []
  usr.roles = []
  usr.brandNameList = []
  usr.roleNameList = []
  usr.countries = []
  usr.regions = []
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
  if (usr['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RegionList']) {
    usr.regionList = usr['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RegionList']
    var rFilter = new regionFilter()
    rFilter.idList = usr.regionList
    let regions = await locationFilters.getRegions(rFilter)
    if (regions) {
      usr.regions = regions
    }
  }
  if (usr['extension_ff5105e3fc0248fbad7979cfe9b62e1a_CountryList']) {
    usr.countryList = usr['extension_ff5105e3fc0248fbad7979cfe9b62e1a_CountryList']
    for (const cId of usr.countryList.split(',').map((id) => parseInt(id))) {
      let country = countries.value.find((c) => c.id === cId)
      if (country) {
        usr.countries.push(country)
      }
    }
  }
  console.log('Converted User FKeys', usr)
}

async function saveUser() {
  submitted.value = true
  if (userModel?.value?.newUserName?.trim()) {
    userModel.value.userName = userModel.value.newUserName
  }
  if (userModel?.value?.userName?.trim()) {
    // if (selectedBrands.value !== undefined) {
    //   userModel.value.brandIds = selectedBrands.value.join(',')
    // }
    // if (selectedRoles.value !== undefined) {
    //   userModel.value.roleIds = selectedRoles.value.join(',')
    // }
    if (userModel.value.id) {
      await UserService.initialise()
      await UserService.saveUser(userModel.value)
        .then(async (response) => {
          var updatedUser = userModel.value
          await convertUserFKeys(updatedUser)
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
      if (newPassword.value) {
        //validate password complexity
        const complexityRegex =
          /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/
        if (!complexityRegex.test(newPassword.value)) {
          toast.add({
            severity: 'error',
            summary: 'Validation Error',
            detail:
              'Password must be at least 8 characters long and include uppercase, lowercase, number, and special character.',
            life: 4000,
          })
          return
        }
      } else {
        toast.add({
          severity: 'error',
          summary: 'Validation Error',
          detail: 'Password is required for new users.',
          life: 4000,
        })
        return
      }
      userModel.value.password = newPassword.value
      UserService.createUser(userModel.value)
        .then((response) => {
          toast.add({
            severity: 'success',
            summary: 'Successful',
            detail: 'User Created',
            life: 4000,
          })
          //router.push({ name: 'userList' })
        })
        .catch((error) => {
          console.log(error)
          var errMessage = 'Could not create user'
          if (error.response.data.error.message) {
            errMessage = error.response.data.error.message
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
  if (regionsToKeep.length > 0) {
    userModel.value.regions?.splice(0, 0, ...(regionsToKeep ?? [])) // add back any regions to keep at the start of the array to preserve any ordering if needed
  }

  userModel.value.regionList = userModel.value.regions?.map((r) => r.id).join(',') || ''
  console.log('Regions Changed', userModel.value.regions)
}

async function onCountryChange(evt: any) {
  var selectedCountries = evt.countries
  var selectedRegions = evt.regions
  countrySelectList.value = await locationFilters.getCountriesForRegions(selectedRegions ?? [])

  //need to remove any countries from the user model that are not in the selected regions, and add any new ones
  var regionsToKeep = userModel.value.regions?.filter((r) => !selectedRegions?.includes(r.id)) ?? []
  var countriesToKeep = new Array<Country>()
  for (const r of regionsToKeep) {
    let cList = r.countryList ? r.countryList.split(',').map((id) => parseInt(id)) : []
    for (const cId of cList) {
      if (userModel.value.countries?.some((uc) => uc.id === cId)) {
        let country = countries.value.find((c) => c.id === cId)
        countriesToKeep.push(country as Country)
      }
    }
  }

  manageSelectedValues(
    evt.countries,
    countrySelectList.value ?? [],
    userModel.value.countries ?? [],
  )

  //add back the countries to keep that were removed in the manageSelectedValues function
  userModel.value.countries.splice(0, 0, ...(countriesToKeep ?? []))
  userModel.value.countryList = userModel.value.countries?.map((c) => c.id).join(',') || ''
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

  if (!values.FirstName) {
    errors.FirstName = [{ message: 'First Name is required.' }]
  }
  if (!values.LastName) {
    errors.LastName = [{ message: 'Last Name is required.' }]
  }
  if (!values.userEmailAddress) {
    errors.userEmailAddress = [{ message: 'Email is required.' }]
  }
  if (!values.userName) {
    errors.userName = [{ message: 'User Name is required.' }]
  }
  if (!values.diamCountryId) {
    errors.diamCountryId = [{ message: 'User Country is required.' }]
  }
  if (values.brandIds == null || values.brandIds.length === 0) {
    errors.brandIds = [{ message: 'At least one Brand must be selected.' }]
  }
  if (values.roleIds == null || values.roleIds.length === 0) {
    errors.roleIds = [{ message: 'At least one Role must be selected.' }]
  }
  if (!values.id) {
    if (!newPassword.value) {
      errors.password = [{ message: 'Password is required for new users.' }]
    } else {
      //validate password complexity
      const password = newPassword.value
      const complexityRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/
      if (!complexityRegex.test(password)) {
        errors.password = [
          {
            message:
              'Password must be at least 8 characters long and include uppercase, lowercase, number, and special character.',
          },
        ]
      }
    }
  }
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
                  placeholder="First Name"
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
                  placeholder="Last Name"
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
                  v-model="userModel.userName"
                  name="userName"
                  type="text"
                  placeholder="User Name"
                  fluid
                  length="255"
                />
              </div>
              <div class="flex flex-col gap-1">
                <label for="userEmailAddress">Email:</label>
                <InputText
                  v-model="userModel.userEmailAddress"
                  name="userEmailAddress"
                  type="text"
                  placeholder="Email Address"
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
                  :invalid="submitted && !userModel.diamCountryId"
                >
                </Select>
              </div>

              <div class="flex flex-col gap-1" v-if="!userModel.id">
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
        <div class="bg-gray-50 col-span-2 p-10 mb-5">
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
                  :countries="currentBrandTabProperties.countries"
                  :selectedCountries="currentBrandTabProperties.selectedCountries"
                  :currentBrandId="parseInt(tab.value)"
                  :regions="currentRegionProps.regions"
                  :selectedRegions="currentRegionProps.selectedRegions"
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
        </div>
        <Button label="Save" @click="saveUser" />
      </Form>
    </div>
  </div>
</template>
