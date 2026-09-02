<script lang="ts" setup>
import { onMounted, ref, watch } from 'vue'
// import UserService from '@/services/UserService.js'
import { useLocationFilters } from '@/components/composables/locationFilters'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { searchProductInfo } from '@/models/Products/searchProductInfo.model'
import { ActionTypeEnum } from '@/models/Reporting/actions.model'
import { ReportingFilter } from '@/models/Reporting/reportingFilter.model'
import { default as countryService } from '@/services/Countries/CountryService'
import { default as auditService } from '@/services/Reporting/AuditService'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { useRouter } from 'vue-router'

import { z } from 'zod'

const router = useRouter()
const { regions, countries } = useLocationFilters()
const selectedRegion = ref()
const selectedCountry = ref()
const selectedStatus = ref()

const statuses = ref([
  { label: 'Create Planogram', value: ActionTypeEnum.CreatePlano },
  { label: 'Submit Planogram', value: ActionTypeEnum.SubmitPlano },
  { label: 'Approve Planogram', value: ActionTypeEnum.ApprovePlano },
  { label: 'Edit Planogram', value: ActionTypeEnum.EditPlano },
  { label: 'Delete Planogram', value: ActionTypeEnum.DeletePlano },
  { label: 'Copy Planogram', value: ActionTypeEnum.CopyPlano },
  { label: 'Validate Planogram', value: ActionTypeEnum.ValidatePlano },
  { label: 'Unapprove Planogram', value: ActionTypeEnum.UnValidatePlano },
  // { label: 'Create Order', value: ActionTypeEnum.CreateOrder },
  // { label: 'Submit Order', value: ActionTypeEnum.SubmitOrder },
  // { label: 'Approve Order', value: ActionTypeEnum.ApproveOrder },
  // { label: 'Edit Order', value: ActionTypeEnum.EditOrder },
])

const products = ref<searchProductInfo[]>([])
const selectedProducts = ref<searchProductInfo[]>([])
const toast = useToast()
const loading = ref(true)
const layout = useSystemStore()
const brandStore = useBrandStore()
const brand = storeToRefs(brandStore).activeBrand
const showPublishedOnly = ref(false)
const searchText = ref('')
const startDate = ref(null)
const endDate = ref(null)
const reportData = ref(null)

const resolver = ref(
  zodResolver(
    z.object({
      startDate: z.date({ message: 'Start Date is required.' }),
      endDate: z.date({ message: 'End Date is required.' }),
    }),
  ),
)

watch(brand, async (newBrand) => {
  if (newBrand) {
    let filter = new ReportingFilter()
    filter.brandId = newBrand.id
    let rFilter = new regionFilter()
    rFilter.brandId = newBrand.id
    useLocationFilters()
      .getRegions(rFilter)
      .then((response) => {
        regions.value = response
      })
  }
})
onMounted(async () => {
  loading.value = true
  layout.layoutState.disableBrandSelect = false
  await auditService.initialise()

  let brandid = brandStore.activeBrand?.id ?? 0
  let rFilter = new regionFilter()
  rFilter.brandId = brandid
  await useLocationFilters()
    .getRegions(rFilter)
    .then((response) => {
      regions.value = response
    })
  let filter = new ReportingFilter()
  filter.brandId = brandid
})
async function onRegionChange() {
  if (selectedRegion.value) {
    countries.value = await useLocationFilters().onRegionChange(selectedRegion.value)
  } else {
    countries.value = []
  }
}

async function onCountryChange() {
  if (selectedCountry.value) {
  } else {
    countries.value = []
  }
}

async function clearFilters() {
  selectedRegion.value = null
  selectedCountry.value = null
  countries.value = []

  let rFilter = new regionFilter()
  rFilter.brandId = brandStore.activeBrand?.id ?? 0
  await countryService.getRegions(rFilter).then((response) => {
    regions.value = response
    console.log('Regions loaded', regions.value)
  })
}

function onStatusChange() {
  if (selectedStatus.value) {
    // let statusName = statuses.value.find((s) => s.value === selectedStatus.value)?.label;
    // filters.value.statusName.value = statusName ?? null;
    // filters.value.statusName.value = selectedStatus.value;
  }
}

async function getUserActionsReport() {
  let filter = new ReportingFilter()
  filter.brandId = brandStore.activeBrand?.id ?? 0
  filter.regionId = selectedRegion.value ?? null
  filter.countryId = selectedCountry.value ?? null
  filter.startDate = startDate.value ?? null
  filter.endDate = endDate.value ?? null
  await auditService.initialise()
  reportData.value = await auditService.getUserActionsReport(filter).then((response) => {
    if (response) {
      console.log('User Actions Report', response)
      toast.add({
        severity: 'success',
        summary: 'Report Generated',
        detail: 'User Actions Report generated successfully.',
        life: 3000,
      })
    } else {
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to generate User Actions Report.',
        life: 3000,
      })
    }
  })

  await auditService.getUserActionsReport(filter).then((response) => {
    if (response) {
      console.log('User Actions Report', response)
      toast.add({
        severity: 'success',
        summary: 'Report Generated',
        detail: 'User Actions Report generated successfully.',
        life: 3000,
      })
    } else {
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to generate User Actions Report.',
        life: 3000,
      })
    }
  })
}
</script>

<template>
  <div>
    <h1>User Action Report</h1>
    <!-- Product list content goes here -->
    <Form
      v-slot="$form"
      :resolver="resolver"
      @submit="getUserActionsReport"
      class="grid grid-cols-4 gap-4"
    >
      <div class="flex-1 flex-wrap gap-4">
        <Select
          v-model="selectedRegion"
          name="region"
          :options="regions ?? []"
          @change="onRegionChange"
          option-label="name"
          option-value="id"
          placeholder="Select a region"
          class="mr-2"
        />
      </div>
      <div class="flex flex-wrap gap-4">
        <Select
          v-model="selectedCountry"
          :options="countries ?? []"
          @change="onCountryChange"
          name="country"
          option-label="name"
          option-value="id"
          placeholder="Select a country"
          class="mr-2"
        />
      </div>
      <div class="flex flex-wrap gap-4">
        <Select
          v-model="selectedStatus"
          name="status"
          :options="statuses ?? []"
          @change="onStatusChange"
          option-label="label"
          option-value="label"
          placeholder="Select a status"
          class="mr-2"
        />
      </div>
      <div class="flex flex-wrap gap-4">
        <Button
          type="button"
          icon="pi pi-filter-slash"
          label="Clear"
          variant="outlined"
          @click="clearFilters()"
          v-tooltip="'Clear filters'"
        />
      </div>
      <div class="flex flex-wrap gap-4">
        <DatePicker
          v-model="startDate"
          name="startDate"
          showIcon
          fluid
          iconDisplay="input"
          inputId="icondisplay"
          placeholder="Start Date"
        />
        <Message
          class="flex"
          v-if="$form.startDate?.invalid"
          severity="error"
          size="small"
          variant="simple"
          >{{ $form.startDate.error?.message ?? '&nbsp;' }}</Message
        >
      </div>
      <div class="flex flex-wrap gap-4 col-span-2">
        <DatePicker
          v-model="endDate"
          na7me="endDate"
          showIcon
          fluid
          iconDisplay="input"
          inputId="icondisplay"
          placeholder="End Date"
        />
        <Message v-if="$form.endDate?.invalid" severity="error" size="small" variant="simple">{{
          $form.endDate.error?.message ?? '&nbsp;'
        }}</Message>
      </div>

      <div class="flex flex-wrap gap-4">
        <Button
          type="submit"
          label="Run"
          icon="pi pi-plus"
          severity="primary"
          class="mr-2"
          v-tooltip="'Run Report'"
        />
      </div>
    </Form>
    <div class="card"></div>
  </div>
</template>
47474/777
