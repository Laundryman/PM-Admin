<script setup lang="ts">
import { Region } from '@/models/Countries/region.model'
import { JobFolder } from '@/models/Jobs/JobFolder.model'
import { default as jobsService } from '@/services/Jobs/JobsService'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'

import { useFolderManagement } from '@/components/composables/jobManagement.composable'
import type { FormInstance } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { defineProps, onMounted, ref, useTemplateRef } from 'vue'
import { z } from 'zod'

const toast = useToast()
const layout = useSystemStore()
const brandStore = useBrandStore()
const brand = storeToRefs(brandStore).activeBrand

const submitted = ref(false)
const jobDateRange = ref<Date[] | null>(null)
const jobFolderFormTemplateRef = useTemplateRef<FormInstance>('jobFolderForm')

const initialJobFolderValues = ref({
  name: '',
  description: '',
  region: 0,
  countries: [] as number[] | null,
})

const props = defineProps<{
  jobFolder: JobFolder
  regions: Region[]
}>()

const currentJobFolder = ref<JobFolder | null>(null)
const folderManagement = useFolderManagement()
const {
  folderDialog,
  jobFolder,
  jobFolderEdit,
  folderSelectedRegion,
  folder_regionSelectList,
  folder_countrySelectList,
  folder_selectedCountries,
  folder_selectAllCountries,
} = folderManagement

const emits = defineEmits(['save', 'update', 'cancel', 'clearCountryFilterSelection'])

function formatDate(date: any): string {
  if (!date) return ''
  if (typeof date !== 'string') {
    // date = date.getDate() + '/' + (date.getMonth() + 1) + '/' + date.getFullYear()
    return date.toLocaleDateString('en-GB')
  }
  const [dateString, timeString] = date.split(' ')
  const [day, month, year] = dateString.split('/')
  const [hour, minute, second] = timeString.split(':')

  const dateObj = new Date(+year, +month - 1, +day, +hour, +minute, +second)

  return dateObj.toLocaleDateString('en-GB')
}

function formatDateTime(date: string): Date {
  // if (!date) return ''
  const [dateString, timeString] = date.split(' ')
  const [day, month, year] = dateString.split('/')
  const [hour, minute, second] = timeString.split(':')

  const dateObj = new Date(+year, +month - 1, +day, +hour, +minute, +second)

  return dateObj
}

onMounted(async () => {
  await jobsService.initialise()
  let brandid = brandStore.activeBrand?.id ?? 0
  setInitialJobFolderFormValues()
  if (props.jobFolder.id) {
    jobFolderEdit.value = Object.assign({}, props.jobFolder)
  } else {
    jobFolderEdit.value = new JobFolder()
    jobFolderEdit.value.countries = []
  }
})

function clearCountryFilterSelection() {
  // emits('clearCountryFilterSelection')
  folder_selectedCountries.value = []
  jobFolder.value.countries = []
  jobFolder.value.region = new Region()
}
function setInitialJobFolderFormValues() {
  // jobFolderEdit.value = Object.assign({}, props.jobFolder)
  folderManagement.setRegionSelectList(props.regions ?? [])
  folderManagement.onFolderRegionChange(props.jobFolder.regionId ?? null)
  folder_regionSelectList.value = props.regions ?? []

  if (props.jobFolder.id) {
    initialJobFolderValues.value.name = props.jobFolder.name ?? ''
    initialJobFolderValues.value.description = props.jobFolder.description ?? ''
    initialJobFolderValues.value.region = props.jobFolder.region.id ?? 0
    initialJobFolderValues.value.countries = props.jobFolder.countries.map((c) => c.id) ?? []
    jobFolderFormTemplateRef.value?.setValues({ ...initialJobFolderValues.value })
    jobFolder.value.countries = props.jobFolder.countries ?? []
    folder_selectedCountries.value = props.jobFolder.countries?.map((c) => c.id) ?? []
  } else {
    const jobFolderRegionField = jobFolderFormTemplateRef.value.fields?.['region']
    const jobFolderCountriesField = jobFolderFormTemplateRef.value.fields?.['countries']
    jobFolderRegionField.states.value = initialJobFolderValues.value.region ?? 0
    jobFolder.value.name = ''
    jobFolder.value.description = ''
    folderSelectedRegion.value = 0
    jobFolder.value.countries = []

    // jobFolderCountriesField.states.value = initialJobFolderValues.value.countries ?? []
  }
}
const baseSchema = z.object({
  name: z.string().nonempty({ message: 'Name is required' }),
  description: z.string().nonempty({ message: 'Description is required' }),
  region: z.number().nullable().optional(),
  // countries: z.array(z.number()).optional(),
})
// const countriesSchema = baseSchema.refine(
//   (data) => {
//     console.log(data.countries)
//     if (data.countries.length === 0) {
//       return false
//     }
//     return true
//   },
//   {
//     message: 'At least one country is required',
//     path: ['countries'],
//     when(payload) {
//       return baseSchema.pick({ countries: true }).safeParse(payload.value).success
//     },
//   },
// )

function validateCountrySelection() {
  const countriesField = jobFolderFormTemplateRef.value.fields?.['countries']
  if (!folder_selectedCountries || folder_selectedCountries.value.length === 0) {
    countriesField.states.invalid = true
    countriesField.states.error = { message: 'At least one country is required' }
    return false
  }
  return true
}

const resolver = zodResolver(baseSchema)
async function submitJobFolderForm({ valid }: any) {
  // const validationResult = dateRangeSchema.safeParse(formValues)
  if (!validateCountrySelection()) {
    return
  }
  if (valid) {
    //need to check the job date range validity before emitting the save event
    //return a new job object
    const nameField = jobFolderFormTemplateRef.value.fields?.['name']
    const descriptionField = jobFolderFormTemplateRef.value.fields?.['description']
    const jobFolderRegionField = jobFolderFormTemplateRef.value.fields?.['region']
    const jobFolderCountriesField = jobFolderFormTemplateRef.value.fields?.['countries']
    const newRegion = props.regions.find((r) => r.id === folderSelectedRegion.value) ?? null

    jobFolder.value.name = nameField.states.value ?? ''
    jobFolder.value.description = descriptionField.states.value ?? ''
    jobFolder.value.regionId = newRegion?.id ?? 0
    jobFolder.value.countryIds = folder_selectedCountries.value.join(',')
    jobFolder.value.countries = jobFolderEdit.value.countries
    // jobFolder.value.region = newRegion
    jobFolder.value.id = jobFolderEdit.value.id
    jobFolder.value.brandId = brandStore.activeBrand?.id ?? 0

    // const newJobFolder = {
    //   ...props.jobFolder,
    //   name: nameField.states.value ?? '',
    //   description: descriptionField.states.value ?? '',
    //   regionId: folderSelectedRegion.value ?? '',
    //   region: newRegion ?? null,
    //   brandId: newRegion?.brandId ?? null,
    //   countries:
    //     folder_countrySelectList.value.filter((c) =>
    //       folder_selectedCountries.value.includes(c.id),
    //     ) ?? [],
    //   // uploadedOn: props.job.uploadedOn
    // }
    // if (newJobFolder.id == null) {
    //   newJobFolder.id = 0 // Replace with your ID generation logic
    //   // parentFolderId = 0
    // }

    emits('save', jobFolder.value)
  }
}
</script>
<template>
  <Form
    v-slot="$form"
    @submit="submitJobFolderForm"
    ref="jobFolderForm"
    :initial-values="initialJobFolderValues"
    :resolver="resolver"
  >
    <div class="flex flex-col gap-6">
      <div>
        <label for="name" class="block font-bold mb-3">Name</label>
        <InputText id="name" name="name" v-model.trim="jobFolder.name" autofocus fluid />
        <Message v-if="$form.name?.invalid" severity="error" size="small" variant="simple">{{
          $form.name.error?.message
        }}</Message>
      </div>
      <div>
        <label for="description" class="block font-bold mb-3">Description</label>
        <InputText
          id="description"
          name="description"
          v-model.trim="jobFolder.description"
          autofocus
          fluid
        />
        <Message v-if="$form.description?.invalid" severity="error" size="small" variant="simple">{{
          $form.description.error?.message
        }}</Message>
      </div>
      <div class="bg-gray-50 flex p-5 mb-5">
        <fieldset legend="Location" class="flex w-full">
          <legend class="text-lg font-bold mb-2">Location</legend>
          <div class="flex flex-col gap-6">
            <div class="p-5 gap-2 grid grid-cols-12">
              <label for="region" class="grid mr-5 col-span-2">Region:</label>
              <Select
                name="region"
                v-model="folderSelectedRegion"
                :options="folder_regionSelectList ?? []"
                id="region"
                class="col-span-10"
                option-label="name"
                option-value="id"
                @change="folderManagement.onFolderRegionChange(folderSelectedRegion)"
              >
                <template #option="option">
                  <div class="flex align-items-center">
                    <span>{{ option.option.name }}</span>
                  </div>
                </template>
              </Select>
            </div>
            <Message
              v-if="$form.region?.invalid"
              severity="error"
              size="small"
              variant="simple"
              class="col-span-12"
              >{{ $form.region.error?.message }}</Message
            >
            <div class="p-5 gap-2 grid grid-cols-12">
              <label for="country" class="mr-5 col-span-2">Country:</label>
              <MultiSelect
                name="countries"
                v-model="folder_selectedCountries"
                :options="folder_countrySelectList ?? []"
                id="country"
                class="col-span-10"
                option-label="name"
                option-value="id"
                @change="folderManagement.onFolderCountryChange($event)"
                :selectAll="folder_selectAllCountries"
                @selectall-change="folderManagement.onSelectAllFolderCountriesChange($event)"
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
                class="col-span-10"
                >{{ $form.countries.error?.message }}</Message
              >
            </div>
            <div class="p-5 col-span-1">
              <Button
                label="Clear Selection"
                class="w-text-left"
                @click="clearCountryFilterSelection"
              />
              <template v-for="country in jobFolderEdit.countries">
                <Chip class="flex-wrap" :label="country.name"></Chip>
              </template>
            </div>
          </div>
        </fieldset>
      </div>
      <div class="mt-4 justify-end flex">
        <!-- Additional form fields can be added here -->
        <Button label="Cancel" icon="pi pi-times" text @click="emits('cancel')" />
        <Button label="Save" icon="pi pi-check" type="submit" />
      </div>
    </div>
  </Form>
  <!-- <template #footer>
        <Button label="Cancel" icon="pi pi-times" text @click="hideFolderDialog" />
        <Button label="Save" icon="pi pi-check" @click="saveFolder" />
      </template> -->
</template>
