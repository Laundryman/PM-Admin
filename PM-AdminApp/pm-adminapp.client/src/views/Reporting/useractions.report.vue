<script lang="ts" setup>
import type { ActionReportDto } from '@/models/Reporting/ActionReportDto.model'
import { onMounted, ref, useTemplateRef, watch } from 'vue'
// import UserService from '@/services/UserService.js'
import { useLocationFilters } from '@/components/composables/locationFilters'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { ActionTypeEnum } from '@/models/Reporting/actions.model'
import { ReportingFilter } from '@/models/Reporting/reportingFilter.model'
import { default as countryService } from '@/services/Countries/CountryService'
import { default as auditService } from '@/services/Reporting/AuditService'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'
import { FilterMatchMode } from '@primevue/core/api/'
import type { FormInstance } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { useRouter } from 'vue-router'
import { utils, writeFileXLSX } from 'xlsx'
import { z } from 'zod'

const router = useRouter()
const { regions, countries } = useLocationFilters()
const selectedRegion = ref()
const selectedCountry = ref()
const selectedAction = ref()
const reportFilter = useTemplateRef<FormInstance>('reportFilter')
const dt = ref()
const actions = ref([
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

const toast = useToast()
const loading = ref(true)
const layout = useSystemStore()
const brandStore = useBrandStore()
const brand = storeToRefs(brandStore).activeBrand
const showPublishedOnly = ref(false)
const searchText = ref('')
const startDate = ref(null)
const endDate = ref(null)
// const reportData = ref(null)
const reportData = ref<ActionReportDto[]>([])
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  userName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
  actionName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
  planoName: { value: null, matchMode: FilterMatchMode.EQUALS },
  roleName: { value: null, matchMode: FilterMatchMode.EQUALS },
  regionName: { value: null, matchMode: FilterMatchMode.EQUALS },
  countryName: { value: null, matchMode: FilterMatchMode.EQUALS },
  permissions: { value: null, matchMode: FilterMatchMode.EQUALS },
})

const baseSchema = z.object({
  startDate: z.date({ message: 'Start Date is required.' }),
  endDate: z.date({ message: 'End Date is required.' }),
})

// const startDateSchema = baseSchema.refine(
//   (data) => {
//     if (data.startDate != null && data.endDate == null) {
//       return false
//     }
//     return true
//   },
//   {
//     message: 'Job date range requires a start and end date',
//     path: ['startDate'],
//     when(payload) {
//       return baseSchema.pick({ startDate: true }).safeParse(payload.value).success
//     },
//   },
// )

const resolver = ref(zodResolver(baseSchema))

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

  selectedAction.value = 0
  startDate.value = null
  endDate.value = null
  filters.value.global.value = null

  let rFilter = new regionFilter()
  rFilter.brandId = brandStore.activeBrand?.id ?? 0
  await countryService.getRegions(rFilter).then((response) => {
    regions.value = response
    console.log('Regions loaded', regions.value)
  })

  const startDateField = reportFilter.value.fields?.['startDate']
  const endDateField = reportFilter.value.fields?.['endDate']
  startDateField.states.value = null
  startDateField.states.invalid = false
  startDateField.states.error = null
  endDateField.states.value = null
  endDateField.states.invalid = false
  endDateField.states.error = null
}

function onActionChange() {
  if (selectedAction.value) {
    // let actionName = statuses.value.find((s) => s.value === selectedAction.value)?.label;
    // filters.value.actionName.value = actionName ?? null;
    // filters.value.actionName.value = selectedAction.value;
  }
}

async function getUserActionsReport() {
  if (!reportFilter.value?.valid) {
    toast.add({
      severity: 'error',
      summary: 'Invalid Filters',
      detail: 'Please correct the filter errors before generating the report.',
      life: 3000,
    })
    return
  }
  let filter = new ReportingFilter()
  filter.brandId = brandStore.activeBrand?.id ?? 0
  filter.regionId = selectedRegion.value ?? null
  filter.countryId = selectedCountry.value ?? null
  filter.startDate = startDate.value ?? null
  filter.endDate = endDate.value ?? null
  filter.actionId = selectedAction.value ?? null
  filter.actionType = 2
  await auditService.initialise()
  const response = await auditService.getUserActionsReport(filter)
  reportData.value = response
  if (response) {
    if (response.length === 0) {
      toast.add({
        severity: 'warn',
        summary: 'No Data',
        detail: 'No data available for the selected filters.',
        life: 3000,
      })
      return
    }
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
}

const exportCSV = () => {
  // dt.value.exportCSV()
  // const rows = reportData.value.map(({ objectName, ...row }) => row)
  const rows = reportData.value
  if (!rows || rows.length === 0) {
    toast.add({
      severity: 'warn',
      summary: 'No Data',
      detail: 'No data available to export.',
      life: 3000,
    })
    return
  }

  //format dates
  rows.map((row) => {
    if (row.date) {
      row.date = new Date(row.date)
    }
  })
  const worksheet = utils.json_to_sheet(rows)
  const workbook = utils.book_new()
  worksheet['!cols'] = Object.keys(rows[0]).map(() => ({ wch: 20 }))
  for (let i = 0; i <= rows.length; i++) {
    let cell = worksheet[`A${i + 1}`]
    worksheet[`A${i + 1}`].z = 'yyyy-mm-dd'
  }

  utils.book_append_sheet(workbook, worksheet, 'UserActionsReport')
  writeFileXLSX(workbook, `UserActionsReport.xlsx`)
}
</script>

<template>
  <div>
    <h1>User Action Report</h1>
    <!-- Product list content goes here -->
    <Form
      v-slot="$form"
      ref="reportFilter"
      :resolver="resolver"
      @submit="getUserActionsReport"
      class="grid grid-cols-4 gap-4 mb-10"
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
          v-model="selectedAction"
          name="action"
          :options="actions ?? []"
          @change="onActionChange"
          option-label="label"
          option-value="value"
          placeholder="Select an action"
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
          dateFormat="dd/mm/yy"
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
          name="endDate"
          showIcon
          fluid
          dateFormat="dd/mm/yy"
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
    <div class="card">
      <div class="flex items-center justify-end gap-3 mb-3">
        <!-- <span class="text-sm text-surface-500 dark:text-surface-400"
          >Export visible rows to CSV with custom column headers.</span
        > -->
        <Button type="button" size="small" @click="exportCSV()">
          <FileExport />
          Download Report
        </Button>
      </div>
      <DataTable
        ref="dt"
        :value="reportData"
        v-model:filters="filters"
        :globalFilterFields="[
          //'categoryName',
          'userName',
          'actionName',
          'date',
          'roleName',
          'regionName',
          'countryName',
          'permissions',
          'planoName',
        ]"
        exportFilename="usageReport"
        tableStyle="min-width: 50rem"
        :paginator="true"
        :rows="10"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rowsPerPageOptions="[5, 10, 25]"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} actions"
      >
        <template #header>
          <div class="flex flex-wrap gap-2 items-center justify-between">
            <h4 class="m-0">User Actions Report</h4>
            <IconField>
              <InputIcon>
                <i class="pi pi-search" />
              </InputIcon>
              <InputText v-model="filters['global'].value" placeholder="Search..." />
            </IconField>
          </div>
        </template>
        <template #empty>
          No results found for the current filters. Try widening the filters to include more
          results.
        </template>

        <Column
          filterField="userName"
          field="userName"
          header="User Name"
          sortable
          style="min-width: 12rem"
        ></Column>
        <Column
          filterField="actionName"
          field="actionName"
          header="Action Name"
          sortable
          style="min-width: 12rem"
        ></Column>
        <Column field="date" header="Timestamp" sortable style="min-width: 12rem">
          <template #body="slotProps">
            {{ new Date(slotProps.data.date).toLocaleDateString() }}
          </template>
        </Column>
        <Column
          filterField="roleName"
          field="roleName"
          header="Role Name"
          sortable
          style="min-width: 12rem"
        ></Column>
        <Column
          filterField="regionName"
          field="regionName"
          header="Region Name"
          sortable
          style="min-width: 12rem"
        ></Column>
        <Column
          filterField="countryName"
          field="countryName"
          header="Country Name"
          sortable
          style="min-width: 12rem"
        ></Column>
        <Column
          filterField="permissions"
          field="permissions"
          header="Permissions"
          sortable
          style="min-width: 12rem"
        ></Column>
        <!-- <Column field="message" header="Message" sortable style="min-width: 12rem"></Column> -->
        <Column
          filterField="planoName"
          field="planoName"
          header="Plano Name"
          sortable
          style="min-width: 12rem"
        ></Column>

        <!-- <Column field="orderName" header="orderName" sortable style="min-width: 12rem"></Column> -->
      </DataTable>
    </div>
  </div>
</template>
47474/777
