<script setup lang="ts">
import { useLocationFilters } from '@/components/composables/locationFilters'
import { Country } from '@/models/Countries/country.model'
import { Region } from '@/models/Countries/region.model'
import { onMounted, ref, watch } from 'vue'

const locationFilters = useLocationFilters()
const regionList = ref<Region[] | null>(null)
const emit = defineEmits(['update:selectedRegions', 'update:selectedCountries'])
const locationData = defineProps<{
  brandId: number | null
  regions: Region[] | null
  countries: Country[] | null
  selectedRegions: number[] | null
  selectedCountries: number[] | null
}>()

// const propSelectedRegions = defineProps<{ selectedRegions: number[] | null }>()
// const countryList = defineProps<{ countries: Country[] }>()
// const selectedRegions = defineProps<{ selectedRegions: string[] }>()
// const pre_selectedCountries = defineProps<{ selectedCountries: string[] }>()
//const regionList = locationData.regions
// const countryList = locationData.countries
// const selectedRegions = locationData.selectedRegions
// const pre_selectedCountries = locationData.selectedCountries
const parentTabId = ref<number | null>(null)
const ms_selectedRegions = ref<number[] | null>(null) // MultiSelect binding
const selectedCountries = ref<number[] | null>(null) // MultiSelect binding
const countrySelectList = ref<Country[] | null>(null)
const selectAllCountries = ref(false)

const allSelectedRegions = ref<Region[] | null>(null)
const allSelectedCountries = ref<Country[] | null>(null)
onMounted(async () => {
  parentTabId.value = locationData.brandId
  // let rFilter = new regionFilter()
  // console.log('Brand ID in userBrandTab', locationData.brandId)
  // if (locationData.brandId) rFilter.brandId = locationData.brandId
  // await locationFilters.getRegions(rFilter).then((response) => {
  //   regionList.value = response
  // })
  // if (selectedRegions) {
  //   ms_selectedRegions.value = selectedRegions.map((r) => parseInt(r))
  // }
  // if (pre_selectedCountries) {
  //   selectedCountries.value = pre_selectedCountries.map((c) => parseInt(c))
  // }
})

watch(locationData, async (newVal: any) => {
  //locationData.selectedRegions = locationData.selectedRegions
  console.log(
    'Location Data Changed',
    locationData.brandId,
    locationData.selectedRegions,
    locationData.regions,
  )
  // selectedRegions.value = locationData.selectedRegions
  if (locationData.regions && locationData.regions.length > 0) {
    if (locationData.regions[0]?.brandId == parentTabId.value) {
      ms_selectedRegions.value = locationData.selectedRegions
    } else {
      ms_selectedRegions.value = []
    }
  }
  selectedCountries.value = locationData.selectedCountries
  if (newVal && newVal.length > 0) {
    countrySelectList.value = await locationFilters.getCountriesForRegions(newVal)
  } else {
    countrySelectList.value = []
  }
})
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
  var selectedRegionIds = evt.value as number[]
  if (selectedRegionIds.length > 0) {
    countrySelectList.value = await locationFilters.getCountriesForRegions(
      locationData.selectedRegions ?? [],
    )
  }
  //remove any countries no longer in the list
  let newSelectList = selectedCountries.value?.filter((countryId) =>
    countrySelectList.value?.some((c) => c.id === countryId),
  )
  if (newSelectList) {
    selectedCountries.value = newSelectList
  } else {
    selectedCountries.value = []
  }

  emit('update:selectedRegions', selectedRegionIds)
  // userModel.value.regionsList = userModel.value.regions?.map((r) => r.id).join(',') || ''
}

async function onCountryChange(evt: any) {
  emit('update:selectedCountries', selectedCountries.value)
  // userModel.value.countriesList = userModel.value.countries?.map((c) => c.id).join(',') || ''
}

function onSelectAllCountriesChange(event: any) {
  selectedCountries.value = event.checked
    ? (countrySelectList.value?.map((item) => item.id) ?? [])
    : []
  selectAllCountries.value = event.checked
  // manageSelectedValues(
  //   ms_selectedCountries.value,
  //   countrySelectList.value ?? [],
  //   userModel.value.countries ?? [],
  // )
  // userModel.value.countriesList = userModel.value.countries?.map((c) => c.id).join(',') || ''
}

function clearCountrySelection() {
  selectedCountries.value = []
  // userModel.value.countries = []
  // userModel.value.countriesList = ''
}
</script>

<template>
  <div class="bg-gray-50 col-span-2 p-10 mb-5">
    <fieldset legend="Location" class="col-span-2">
      <legend class="text-lg font-bold mb-2">Location</legend>
      <div class="grid grid-cols-3 gap-10">
        <div class="flex flex-col gap-2">
          <label for="region">Region:</label>
          <MultiSelect
            name="region"
            v-model="ms_selectedRegions"
            :options="locationData.regions ?? []"
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
            v-model="selectedCountries"
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
          <!-- <Message v-if="$form.countries?.invalid" severity="error" size="small" variant="simple">{{
            $form.countries.error?.message
          }}</Message> -->
        </div>
        <div class="flex gap-2 flex-wrap max-h-40 overflow-auto">
          <Button label="Clear Selection" class="w-text-left" @click="clearCountrySelection" />
          <!-- <template v-for="country in userModel.countries">
            <Chip class="flex-wrap" :label="country.name"></Chip>
          </template> -->
        </div>
      </div>
    </fieldset>
  </div>
</template>
