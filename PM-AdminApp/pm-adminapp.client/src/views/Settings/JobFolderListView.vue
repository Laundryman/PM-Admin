<script setup lang="ts">
import { useFolderManagement } from '@/components/composables/jobManagement.composable'
import { useLocationFilters } from '@/components/composables/locationFilters'
import { useMultiSelectLists } from '@/components/composables/multiSelectList.composable'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { JobFolderFilter } from '@/models/Jobs/JobFolderFilter.model'

import { Country } from '@/models/Countries/country.model'
import { Region } from '@/models/Countries/region.model'
import { Job } from '@/models/Jobs/Job.model'
import { JobFolder } from '@/models/Jobs/JobFolder.model'
import { default as countryService } from '@/services/Countries/CountryService'
import { default as jobsService } from '@/services/Jobs/JobsService'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'

import JobForm from '@/components/Forms/JobForm.component.vue'
import { FilterMatchMode } from '@primevue/core/api/'
import type { FormInstance } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { onMounted, ref, useTemplateRef, watch } from 'vue'
import { useRouter } from 'vue-router'
import { z } from 'zod'

const jobFolderFilter = ref(new JobFolderFilter())
const selectedJobFolder = ref<JobFolder | null>(null)
const jobFolders = ref<JobFolder[] | null>(null)
const selectedJobFolders = ref()
const expandedRows = ref()

const router = useRouter()
const { regions, countries } = useLocationFilters()
const multiSelectLists = useMultiSelectLists()

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
const toast = useToast()
const loading = ref(true)
const layout = useSystemStore()
const brandStore = useBrandStore()
const brand = storeToRefs(brandStore).activeBrand

const jobDialog = ref(false)
const submitted = ref(false)
const job = ref()
const locationFilters = useLocationFilters()
const selectedRegion = ref()
const selectedCountry = ref()
const countrySelectList = ref<Country[] | null>(null)
const regionSelectList = ref<Region[] | null>(null)
const ms_selectedRegions = ref<number[] | null>(null) // MultiSelect binding
const ms_selectedCountries = ref<number[] | null>(null) // MultiSelect binding
const selectAllCountries = ref(false)

const jobDateRange = ref<Date[] | null>(null)

const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  name: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
  description: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
})
const initialJobValues = ref({
  jobCode: '',
  jobDescription: '',
  jobFolderId: null,
  jobFolderName: '',
  brandId: null,
  jobDateRange: [] as Date[] | null,
})

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
watch(brand, async (newBrand) => {
  if (newBrand) {
    loading.value = true
    let filter = new JobFolderFilter()
    filter.includeChildren = true

    filter.brandId = newBrand.id
    await jobsService.searchJobFolders(filter).then((response) => {
      jobFolders.value = response.data
      console.log('Job Folders loaded for brand change', jobFolders.value)
      loading.value = false
    })
    let rFilter = new regionFilter()
    rFilter.brandId = newBrand.id
    await useLocationFilters()
      .getRegions(rFilter)
      .then((response) => {
        regions.value = response
      })
  }
})

onMounted(async () => {
  loading.value = true
  layout.layoutState.disableBrandSelect = false
  await jobsService.initialise()
  await countryService.initialise()

  let brandid = brandStore.activeBrand?.id ?? 0

  let rFilter = new regionFilter()
  rFilter.brandId = brandid

  await useLocationFilters()
    .getRegions(rFilter)
    .then((response) => {
      regions.value = response
    })

  var filter = new JobFolderFilter()
  filter.brandId = brandid
  filter.includeChildren = true
  await jobsService.searchJobFolders(filter).then((response) => {
    jobFolders.value = response.data
    loading.value = false
  })
})

async function clearFilters() {
  selectedRegion.value = null
  selectedCountry.value = null
  countries.value = []
  let filter = new JobFolderFilter()
  filter.brandId = brandStore.activeBrand?.id ?? 0
  filter.includeChildren = true

  await jobsService.searchJobFolders(filter).then((response) => {
    jobFolders.value = response.data
    console.log('Job Folders loaded', jobFolders.value)
  })
  let rFilter = new regionFilter()
  rFilter.brandId = filter.brandId
  await countryService.getRegions(rFilter).then((response) => {
    regions.value = response
    console.log('Regions loaded', regions.value)
  })
}

////////////////////////////////////////////////////
// row Expansion handlers
////////////////////////////////////////////////////
function onRowExpand(event: any) {
  console.log('Row expanded', event.data)
  jobFolder.value = event.data
}

////////////////////////////////////////////////////
// Location Filter Handlers
////////////////////////////////////////////////////
async function onRegionFilterChange() {
  if (selectedRegion.value) {
    countries.value = await useLocationFilters().onRegionChange(selectedRegion.value)
    let filter = new JobFolderFilter()
    filter.brandId = brandStore.activeBrand?.id ?? 0
    filter.regionId = selectedRegion.value
    filter.includeChildren = true

    await jobsService.searchJobFolders(filter).then((response) => {
      jobFolders.value = response.data
      console.log('Job Folders loaded', jobFolders.value)
    })
  } else {
    countries.value = []
  }
}

async function onCountryFilterChange() {
  if (selectedCountry.value) {
    let filter = new JobFolderFilter()
    filter.brandId = brandStore.activeBrand?.id ?? 0
    filter.countryId = selectedCountry.value
    filter.includeChildren = true

    await jobsService.searchJobFolders(filter).then((response) => {
      jobFolders.value = response.data
      console.log('Job Folders loaded', jobFolders.value)
    })
  } else {
    countries.value = []
  }
}

////////////////////////////////////////////////
// Job Folder Edit Handlers
////////////////////////////////////////////////

// function onSelectAllCountriesChange(event: any) {
//   ms_selectedCountries.value = event.checked
//     ? (countrySelectList.value?.map((item) => item.id) ?? [])
//     : []
//   selectAllCountries.value = event.checked
//   multiSelectLists.manageSelectedValues(
//     ms_selectedCountries.value,
//     countrySelectList.value ?? [],
//     jobFolder.value.countries ?? [],
//   )
//   // jobFolder.value.countriesList = jobFolder.value.countries?.map((c: any) => c.id).join(',') || ''
// }

function clearCountryFilterSelection() {
  ms_selectedCountries.value = []
  jobFolder.value.countries = []
  // jobFolder.value.countriesList = ''
}

////////////////////////////////////////////
// Folder Dialog functions
////////////////////////////////////////////
const hideFolderDialog = () => {
  folderDialog.value = false
  submitted.value = false
}
async function saveFolder() {
  //folderManagement.saveUpdate()
  // jobFolder.value = jobFolderEdit.value
  // const isNewFolder = jobFolder.value.id === 0
  jobFolder.value.name = jobFolderEdit.value.name
  jobFolder.value.description = jobFolderEdit.value.description
  jobFolder.value.regionId = jobFolderEdit.value.regionId
  jobFolder.value.countries = jobFolderEdit.value.countries
  jobFolder.value.region = jobFolderEdit.value.region
  jobFolder.value.id = jobFolderEdit.value.id
  jobFolder.value.brandId = brandStore.activeBrand?.id ?? 0
  submitted.value = true

  if (jobFolder?.value.name?.trim()) {
    if (jobFolder?.value.id) {
      await jobsService
        .updateJobFolder(jobFolder.value)
        .then((response) => {
          toast.add({
            severity: 'success',
            summary: 'Successful',
            detail: 'Job Folder Updated',
            life: 3000,
          })
        })
        .catch((error) => {
          console.error('Error updating job folder:', error)
        })
    } else {
      jobFolder.value.id = 0

      await jobsService.createJobFolder(jobFolder.value).then((response) => {
        // console.log('Job Folder created', response)
        let newJobFolder = response
        const tempFolderList = [...jobFolders.value]

        tempFolderList.unshift(newJobFolder)
        jobFolders.value = tempFolderList
        toast.add({
          severity: 'success',
          summary: 'Successful',
          detail: 'Job Folder Created',
          life: 3000,
        })
      })
    }

    folderDialog.value = false
    jobFolder.value = new JobFolder()
  }
}
function editFolder(folder: JobFolder) {
  jobFolder.value = folder
  jobFolderEdit.value = Object.assign({}, folder)
  folderSelectedRegion.value = folder.regionId ?? null
  folder_selectedCountries.value = folder.countries?.map((c) => c.id) ?? []
  folderManagement.setRegionSelectList(regions.value ?? [])
  folderManagement.onFolderRegionChange(folder.regionId ?? null)
  folder_regionSelectList.value = regions.value ?? []
  folderDialog.value = true
}
function addJobFolder() {
  folderManagement.setRegionSelectList(regions.value ?? [])
  jobFolder.value = new JobFolder()
  jobFolderEdit.value = new JobFolder()
  jobFolder.value.brandId = brandStore.activeBrand?.id ?? 0
  jobFolderEdit.value.brandId = brandStore.activeBrand?.id ?? 0
  folderDialog.value = true
}
////////////////////////////////////////////
// Job Dialog functions
////////////////////////////////////////////
const hideJobDialog = () => {
  jobDialog.value = false
  submitted.value = false
}
async function saveJob(currentJob: Job) {
  submitted.value = true

  const formData = new FormData()

  if (currentJob.jobCode?.trim()) {
    currentJob.uploadedBy = 0 // Replace with actual user info
    currentJob.uploadedOn = new Date()
    currentJob.brandId = brandStore.activeBrand?.id ?? 0
    currentJob.brandName = brandStore.activeBrand?.name ?? ''

    if (currentJob.id) {
      await jobsService
        .updateJob(currentJob)
        .then((response) => {
          // console.log(response.data)
          job.value = { ...currentJob }
          const jfolder = jobFolders.value.find((jf) => jf.id == currentJob.jobFolderId)
          const updateJob = jfolder.jobs.find((j) => j.id === currentJob.id)
          if (updateJob) {
            updateJob.jobCode = currentJob.jobCode
            updateJob.description = currentJob.description
            updateJob.dateFrom = currentJob.dateFrom
            updateJob.dateTo = currentJob.dateTo
            updateJob.uploadedOn = currentJob.uploadedOn
          }

          toast.add({
            severity: 'success',
            summary: 'Successful',
            detail: 'Job Updated',
            life: 3000,
          })
        })
        .catch((error) => {
          console.log('Error updating job:', error)
        })
    } else {
      currentJob.id = 0
      var newJob = await jobsService.createJob(currentJob).then((response) => {
        return response
      })
      job.value = { ...currentJob }
      const jfolder = jobFolders.value.find((jf) => jf.id == newJob.jobFolderId)
      if (!jfolder.jobs) {
        jfolder.jobs = []
      }
      jfolder.jobs.push(currentJob)
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'Job Created',
        life: 3000,
      })
    }

    jobDialog.value = false
    job.value = new Job()
  }
}
function editJob(jobModel: Job) {
  job.value = jobModel
  const jobFolder = jobFolders.value.find((jf) => jf.id == jobModel.jobFolderId)
  if (jobModel.dateTo != null && jobModel.dateFrom != null) {
    // ukDateTo = formatDate(jobModel.dateTo)
    // ukDateFrom = formatDate(jobModel.dateFrom)
    jobDateRange.value = [jobModel.dateFrom, jobModel.dateTo]
  } else {
    // if (jobModel.uploadedOn != null) {
    //   const startDate = formatDateTime(jobModel.uploadedOn.toString())
    //   const endDateMonth = startDate.getMonth() + 1
    //   const endDate = startDate
    //   endDate.setMonth(endDateMonth)
    //   jobDateRange.value = [startDate, endDate]
    // } else {
    //   jobDateRange.value = [new Date(), new Date()]
    // }
  }
  job.value.dateFrom = jobModel.dateFrom
  job.value.dateTo = jobModel.dateTo
  job.value.jobFolderId = jobModel.id ?? 0
  job.value.jobFolderName = jobFolder?.name ?? ''
  jobDialog.value = true
}
function addJob(folder: JobFolder) {
  // const jobFolder = jobFolders.value.find((jf) => jf.id == jobModel.jobFolderId)

  job.value = new Job()
  job.value.jobFolderId = folder.id
  job.value.jobFolderName = folder.name
  job.value.brandId = brandStore.activeBrand?.id ?? 0
  jobDateRange.value = [new Date(), new Date()]
  // initialJobValues.value = { ...job.value }

  jobDialog.value = true
  setInitialJobFormValues()
}

const dateRangeSchema = z.object({
  jobDateRange: z.array(z.date()).refine(
    (data) => {
      return data[1] > data[0]
    },
    {
      message: 'Job date range requires a start and end date',
      path: ['jobDateRange'],
    },
  ),
})
const resolver = zodResolver(
  z.object({
    jobFolder: z.string().min(1, 'Name is required'),
    jobCode: z.string().nonempty({ message: 'Job code is required' }),
    jobDescription: z.string().optional(),
    jobDateRange: dateRangeSchema.shape.jobDateRange.optional(),
  }),
)
// const resolver = ({ values }: any) => {
//   const errors = {} as any

//   if (!values.jobCode) {
//     errors.jobCode = [{ message: 'Job Code is required.' }]
//   }
//   if (!values.jobDateRange) {
//     errors.jobDateRange = [{ message: 'Job Date Range is required.' }]
//   }
//   if (values.jobDateRange && values.jobDateRange.length === 2) {
//     if (values.jobDateRange[1] <= values.jobDateRange[0]) {
//       errors.jobDateRange = [
//         { message: 'Job Date Range must have an end date after the start date.' },
//       ]
//     }
//   }

//   return {
//     values, // (Optional) Used to pass current form values to submit event.
//     errors,
//   }
// }
// function submitJobForm({ valid, values }: any) {
//   // resolver.call(jobFormTemplateRef.value)
//   if (valid) {
//     saveJob()
//   }
// }

function setInitialJobFormValues() {
  const jobFormTemplateRef = useTemplateRef<FormInstance>('job-form')

  jobFormTemplateRef.value?.setValues({ ...initialJobValues.value })
  initialJobValues.value.jobCode = job.value.jobCode
  initialJobValues.value.jobDescription = job.value.jobDescription
  initialJobValues.value.jobFolderId = job.value.jobFolderId
  initialJobValues.value.jobFolderName = job.value.jobFolderName
  initialJobValues.value.brandId = job.value.brandId
  initialJobValues.value.jobDateRange = [job.value.dateFrom, job.value.dateTo]

  // jobFormTemplateRef.value.setFieldValue('jobCode', initialJobValues.value.jobCode)
  // jobFormTemplateRef.value.setFieldValue('jobDescription', initialJobValues.value.jobDescription)
  // jobFormTemplateRef.value.setFieldValue('jobFolderId', initialJobValues.value.jobFolderId)
  // jobFormTemplateRef.value.setFieldValue('jobFolderName', initialJobValues.value.jobFolderName)
  // jobFormTemplateRef.value.setFieldValue('brandId', initialJobValues.value.brandId)
  // jobFormTemplateRef.value.setFieldValue('jobDateRange', initialJobValues.value.jobDateRange)
}
</script>

<template>
  <div>
    <h1>JobFolders View</h1>
    <div class="w-full sticky top-16 block z-10 bg-slate-50">
      <Toolbar class="mb-6">
        <template #start>
          <Select
            v-model="selectedRegion"
            :options="regions ?? []"
            @change="onRegionFilterChange"
            option-label="name"
            option-value="id"
            placeholder="Select a region"
            class="mr-2"
          />

          <Select
            v-model="selectedCountry"
            :options="countries ?? []"
            @change="onCountryFilterChange"
            option-label="name"
            option-value="id"
            placeholder="Select a country"
            class="mr-2"
          />

          <!-- <Button label="Clear" icon="pi pi-filter" @click="clearFilters" /> -->
          <Button
            type="button"
            icon="pi pi-filter-slash"
            label="Clear"
            variant="outlined"
            @click="clearFilters()"
          />
        </template>

        <template #end>
          <Button label="New" icon="pi pi-plus" class="mr-2" @click="addJobFolder()" />
        </template>
      </Toolbar>
    </div>
    <div class="card">
      <DataTable
        ref="dt"
        v-model:filters="filters"
        v-model:expanded-rows="expandedRows"
        v-model:selection="selectedJobFolder"
        selectionMode="single"
        :loading="loading"
        :globalFilterFields="[
          //'categoryName',
          'name',
          'description',
          'regionName',
        ]"
        filterDisplay="row"
        :value="jobFolders"
        dataKey="id"
        :paginator="true"
        :rows="10"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rowsPerPageOptions="[5, 10, 25]"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} job folders"
        @row-expand="onRowExpand"
        tableClass="hide-row-border"
        scrollable
        scrollHeight="calc(70vh - 130px)"
      >
        <template #header>
          <div class="flex flex-wrap gap-2 items-center justify-between">
            <h4 class="m-0">Manage Job Folders</h4>
            <IconField>
              <InputIcon>
                <i class="pi pi-search" />
              </InputIcon>
              <InputText v-model="filters['global'].value" placeholder="Search..." />
            </IconField>
          </div>
        </template>
        <template #empty> No job folders found. </template>
        <Column expander style="width: 5rem" />
        <Column field="name" header="Name" sortable style="min-width: 12rem"></Column>
        <Column
          field="description"
          header="Description"
          filterField="description"
          style="min-width: 16rem"
        >
        </Column>
        <Column field="jobs" header="Jobs" style="min-width: 12rem">
          <template #body="slotProps">
            {{ slotProps.data.jobs.length }}
          </template>
        </Column>

        <Column header="Actions" :exportable="false" style="min-width: 12rem">
          <template #body="slotProps">
            <Button
              v-tooltip="'Edit Job Folder'"
              icon="pi pi-pencil"
              variant="outlined"
              rounded
              class="mr-2"
              @click="editFolder(slotProps.data)"
            />
            <Button
              v-tooltip="'Add Job'"
              icon="pi pi-plus"
              variant="outlined"
              rounded
              @click="addJob(slotProps.data)"
            />
          </template>
        </Column>

        <template #expansion="slotProps">
          <div class="p-4 border-solid border border-gray-300">
            <h5>Jobs for {{ slotProps.data.name }}</h5>
            <DataTable
              :value="slotProps.data.jobs"
              table-class="archive-jobs-expanded bg-emerald-50"
            >
              <Column field="jobCode" header="Job Code" sortable></Column>
              <Column field="description" header="Description" sortable></Column>
              <Column field="dateFrom" header="Date From" sortable>
                <template #body="slotProps">
                  {{ formatDate(slotProps.data.dateFrom) }}
                </template>
              </Column>
              <Column field="dateTo" header="Date To" sortable>
                <template #body="slotProps">
                  {{ formatDate(slotProps.data.dateTo) }}
                </template>
              </Column>
              <Column field="uploadedOn" header="Date Added" sortable>
                <template #body="slotProps">
                  {{ formatDate(slotProps.data.uploadedOn) }}
                </template>
              </Column>

              <Column header="Actions" headerStyle="width:4rem">
                <template #body="slotProps">
                  <Button
                    v-tooltip="'Edit Job'"
                    icon="pi pi-pencil"
                    variant="outlined"
                    rounded
                    class="mr-2"
                    @click="editJob(slotProps.data)"
                  />
                </template>
              </Column>
            </DataTable>
          </div>
        </template>
      </DataTable>
    </div>
    <Dialog
      v-model:visible="folderDialog"
      :style="{ width: '54rem' }"
      header="Job Folder Details"
      :modal="true"
    >
      <div class="flex flex-col gap-6">
        <div>
          <label for="name" class="block font-bold mb-3">Name</label>
          <InputText
            id="name"
            v-model.trim="jobFolderEdit.name"
            required="true"
            autofocus
            :invalid="submitted && !jobFolderEdit.name"
            fluid
          />
          <small v-if="submitted && !jobFolderEdit.name" class="text-red-500"
            >Name is required.</small
          >
        </div>
        <div>
          <label for="description" class="block font-bold mb-3">Description</label>
          <InputText
            id="description"
            v-model.trim="jobFolderEdit.description"
            required="true"
            autofocus
            :invalid="submitted && !jobFolderEdit.description"
            fluid
          />
          <small v-if="submitted && !jobFolderEdit.description" class="text-red-500"
            >Description is required.</small
          >
        </div>
        <div class="bg-gray-50 flex p-5 mb-5">
          <fieldset legend="Location" class="flex w-full">
            <legend class="text-lg font-bold mb-2">Location</legend>
            <div class="grid grid-flow-col grid-rows-3 gap-4 w-full">
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
              <div class="p-5 gap-2 grid grid-cols-12 w-full">
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
                <!-- <Message
                  v-if="$form.countries?.invalid"
                  severity="error"
                  size="small"
                  variant="simple"
                  >{{ $form.countries.error?.message }}</Message
                > -->
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
        <!-- <div>
          <label for="name" class="block font-bold mb-3">Disabled</label>
          <ToggleSwitch name="disabled" v-model="jobFolder.disabled" />
        </div> -->
      </div>
      <template #footer>
        <Button label="Cancel" icon="pi pi-times" text @click="hideFolderDialog" />
        <Button label="Save" icon="pi pi-check" @click="saveFolder" />
      </template>
    </Dialog>

    <Dialog
      v-model:visible="jobDialog"
      :style="{ width: '450px' }"
      header="Job Details"
      :modal="true"
      @show="setInitialJobFormValues"
    >
      <JobForm
        v-model:visible="jobDialog"
        :job="job"
        :jobFolder="selectedJobFolder"
        @save="saveJob"
        @cancel="hideJobDialog"
      />
      <!-- <Form v-slot="$form" @submit="submitJobForm" ref="job-form" :resolver>
        <div class="flex flex-col gap-6">
          <div>
            <label for="JobFolder" class="block font-bold mb-3">JobFolder</label>
            <InputText name="jobFolder" v-model.trim="job.jobFolderName" disabled fluid />
          </div>
          <div>
            <label for="jobCode" class="block font-bold mb-3">Job Code</label>
            <InputText name="jobCode" :value="job.jobCode" v-model="job.jobCode" autofocus fluid />
            <Message v-if="$form.jobCode?.invalid" severity="error" size="small" variant="simple">{{
              $form.jobCode.error?.message
            }}</Message>
          </div>
          <div>
            <label for="jobDescription" class="block font-bold mb-3">Description</label>
            <InputText
              name="jobDescription"
              :value="initialJobValues.jobDescription"
              autofocus
              :invalid="submitted && !job.description"
              fluid
            />
            <Message
              v-if="$form.description?.invalid"
              severity="error"
              size="small"
              variant="simple"
              >{{ $form.description.error?.message }}</Message
            >
          </div>
          <div>
            <label for="jobDateRange" class="w-full block font-bold mb-3">Date Range</label>
            <DatePicker
              name="jobDateRange"
              v-model="initialJobValues.jobDateRange"
              v-tooltip="'Select the date range by selecting the start date, then the end date'"
              selectionMode="range"
              :manualInput="false"
              dateFormat="dd/mm/yy"
              class="w-full"
              placeHolder="Select Date Range"
            />
            <Message
              v-if="$form.jobDateRange?.invalid"
              severity="error"
              size="small"
              variant="simple"
              >{{ $form.jobDateRange.error?.message }}</Message
            >
          </div>
        </div>
        <div class="mt-4 justify-end flex">
          <Button label="Cancel" icon="pi pi-times" text @click="hideJobDialog" />
          <Button label="Save" icon="pi pi-check" type="submit" />
        </div>
      </Form> -->
      <template #footer> </template>
    </Dialog>
  </div>
</template>
