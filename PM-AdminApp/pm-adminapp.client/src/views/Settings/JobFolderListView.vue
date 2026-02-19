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

import { FilterMatchMode } from '@primevue/core/api/'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'

import { onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'

const jobFolderFilter = ref(new JobFolderFilter())

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

watch(brand, async (newBrand) => {
  if (newBrand) {
    let filter = new JobFolderFilter()
    filter.brandId = newBrand.id
    await jobsService.searchJobFolders(filter).then((response) => {
      jobFolders.value = response.data
      console.log('Job Folders loaded for brand change', jobFolders.value)
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
  await jobsService.searchJobFolders(filter).then((response) => {
    jobFolders.value = response.data
    console.log('Job Folders loaded', jobFolders.value)
  })
})

async function clearFilters() {
  selectedRegion.value = null
  selectedCountry.value = null
  countries.value = []
  let filter = new JobFolderFilter()
  filter.brandId = layout.getActiveBrand?.id ?? 0
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
    filter.brandId = layout.getActiveBrand?.id ?? 0
    filter.regionId = selectedRegion.value
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
    filter.brandId = layout.getActiveBrand?.id ?? 0
    filter.countryId = selectedCountry.value
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
  submitted.value = true

  if (jobFolder?.value.name?.trim()) {
    if (jobFolder?.value.id) {
      await jobsService
        .updateJobFolder(jobFolder.value)
        .then((response) => {
          if (response && response.data) {
            console.log(response.data)
          }
        })
        .catch((error) => {
          console.error('Error updating job folder:', error)
        })
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'Job Folder Updated',
        life: 3000,
      })
    } else {
      jobFolder.value.id = 0

      await jobsService.createJobFolder(jobFolder.value).then((response) => {
        console.log('Job Folder created', response)
        let newJobFolder = response
        if (jobFolders.value) {
          jobFolders.value.push(newJobFolder)
        }
      })
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'Job Folder Created',
        life: 3000,
      })
    }

    folderDialog.value = false
    jobFolder.value = new JobFolder()
  }
}
function editFolder(folder: JobFolder) {
  jobFolder.value = folder
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
  jobFolder.value.brandId = brandStore.activeBrand?.id ?? 0
  folderDialog.value = true
}
////////////////////////////////////////////
// Job Dialog functions
////////////////////////////////////////////
const hideJobDialog = () => {
  jobDialog.value = false
  submitted.value = false
}
async function saveJob() {
  submitted.value = true

  const formData = new FormData()

  if (job?.value.jobCode?.trim()) {
    if (jobDateRange.value && jobDateRange.value.length === 2) {
      job.value.dateFrom = jobDateRange.value[0]
      job.value.dateTo = jobDateRange.value[1]
    }
    job.value.uploadedBy = 0 // Replace with actual user info
    job.value.uploadedOn = new Date()
    job.value.brandId = brandStore.activeBrand?.id ?? 0
    job.value.brandName = brandStore.activeBrand?.name ?? ''

    if (job.value.id) {
      await jobsService
        .updateJob(job.value)
        .then((response) => {
          if (response && response.data) {
            console.log(response.data)
          }
        })
        .catch((error) => {
          console.error('Error updating job:', error)
        })
      toast.add({
        severity: 'success',
        summary: 'Successful',
        detail: 'Job Updated',
        life: 3000,
      })
    } else {
      job.value.id = 0
      var newJob = await jobsService.createJob(job.value).then((response) => {
        return response
      })
      if (jobFolder.value.jobs) {
        jobFolder.value.jobs.push(newJob)
      }
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
  jobDateRange.value = [new Date(jobModel.dateFrom), new Date(jobModel.dateTo)]
  job.value.jobFolderId = jobFolder?.value.id ?? 0
  job.value.jobFolderName = jobFolder?.value.name ?? ''
  jobDialog.value = true
}
function addJob(folder: JobFolder) {
  job.value = new Job()
  job.value.jobFolderId = folder.id
  job.value.jobFolderName = folder.name
  job.value.brandId = brandStore.activeBrand?.id ?? 0
  jobDateRange.value = [new Date(), new Date()]
  jobDialog.value = true
}
</script>

<template>
  <div>
    <h1>JobFolders View</h1>
    <!-- JobFolders list content goes here -->
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
        <Button
          label="New"
          icon="pi pi-plus"
          severity="secondary"
          class="mr-2"
          @click="addJobFolder()"
        />
      </template>
    </Toolbar>
    <div class="card">
      <DataTable
        ref="dt"
        v-model:filters="filters"
        v-model:expanded-rows="expandedRows"
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
        <Column expander style="width: 5rem" />
        <Column field="name" header="Name" sortable style="min-width: 12rem"></Column>
        <Column
          field="description"
          header="Description"
          filterField="description"
          style="min-width: 16rem"
        >
          <template #filter="{ filterModel, filterCallback }">
            <InputText
              v-model="filterModel.value"
              type="text"
              @input="filterCallback()"
              placeholder="Search by description"
            />
          </template>
        </Column>
        <Column field="jobs" header="Jobs" style="min-width: 12rem">
          <template #body="slotProps">
            {{ slotProps.data.jobs.length }}
          </template>
        </Column>

        <Column :exportable="false" style="min-width: 12rem">
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
          <div class="p-4">
            <h5>Jobs for {{ slotProps.data.name }}</h5>
            <DataTable :value="slotProps.data.jobs">
              <Column field="jobCode" header="Job Code" sortable></Column>
              <Column field="description" header="Description" sortable></Column>
              <Column field="dateFrom" header="Date From" sortable></Column>
              <Column field="dateTo" header="Date To" sortable></Column>
              <Column field="uploadedOn" header="Date Added" sortable></Column>

              <Column headerStyle="width:4rem">
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
      :style="{ width: '450px' }"
      header="Job Folder Details"
      :modal="true"
    >
      <div class="flex flex-col gap-6">
        <div>
          <label for="name" class="block font-bold mb-3">Name</label>
          <InputText
            id="name"
            v-model.trim="jobFolder.name"
            required="true"
            autofocus
            :invalid="submitted && !jobFolder.name"
            fluid
          />
          <small v-if="submitted && !jobFolder.name" class="text-red-500">Name is required.</small>
        </div>
        <div>
          <label for="description" class="block font-bold mb-3">Description</label>
          <InputText
            id="description"
            v-model.trim="jobFolder.description"
            required="true"
            autofocus
            :invalid="submitted && !jobFolder.description"
            fluid
          />
          <small v-if="submitted && !jobFolder.description" class="text-red-500"
            >Description is required.</small
          >
        </div>
        <div class="bg-gray-50 col-span-2 p-10 mb-5">
          <fieldset legend="Location" class="col-span-2">
            <legend class="text-lg font-bold mb-2">Location</legend>
            <div class="flex flex-col gap-10">
              <div class="p-5 gap-2">
                <label for="region">Region:</label>
                <Select
                  name="region"
                  v-model="folderSelectedRegion"
                  :options="folder_regionSelectList ?? []"
                  id="region"
                  class="w-full"
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
              <div class="p-5 gap-2">
                <label for="country">Country:</label>
                <MultiSelect
                  name="countries"
                  v-model="folder_selectedCountries"
                  :options="folder_countrySelectList ?? []"
                  id="country"
                  class="w-full"
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
              <div class="p-5">
                <Button
                  label="Clear Selection"
                  class="w-text-left"
                  @click="clearCountryFilterSelection"
                />
                <template v-for="country in jobFolder.countries">
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
    >
      <div class="flex flex-col gap-6">
        <div>
          <label for="JobFolder" class="block font-bold mb-3">JobFolder</label>
          <InputText id="JobFolder" v-model.trim="job.jobFolderName" disabled fluid />
        </div>
        <div>
          <label for="jobCode" class="block font-bold mb-3">Job Code</label>
          <InputText
            id="jobCode"
            v-model.trim="job.jobCode"
            required="true"
            autofocus
            :invalid="submitted && !job.jobCode"
            fluid
          />
          <small v-if="submitted && !job.jobCode" class="text-red-500">Job Code is required.</small>
        </div>
        <div>
          <label for="description" class="block font-bold mb-3">Description</label>
          <InputText
            id="description"
            v-model.trim="job.description"
            required="true"
            autofocus
            :invalid="submitted && !job.description"
            fluid
          />
          <small v-if="submitted && !job.description" class="text-red-500"
            >Description is required.</small
          >
        </div>
        <div>
          <label for="description" class="block font-bold mb-3">Date Range</label>
          <DatePicker v-model="jobDateRange" selectionMode="range" :manualInput="false" />
          <small v-if="submitted && !jobDateRange" class="text-red-500"
            >Date Range is required.</small
          >
        </div>
      </div>
      <template #footer>
        <Button label="Cancel" icon="pi pi-times" text @click="hideJobDialog" />
        <Button label="Save" icon="pi pi-check" @click="saveJob" />
      </template>
    </Dialog>
  </div>
</template>
