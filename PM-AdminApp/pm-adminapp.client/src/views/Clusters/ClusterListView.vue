<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
// import UserService from '@/services/UserService.js'
import { useLocationFilters } from '@/components/composables/locationFilters'
import { useManageCluster } from '@/components/composables/manageCluster.composable'
import { ClusterFilter } from '@/models/Clusters/clusterFilter.model'
import { SaveLayoutDto } from '@/models/Clusters/saveLayout.model'
import { searchClusterInfo } from '@/models/Clusters/searchClusterInfo.model'
import { regionFilter } from '@/models/Countries/regionFilter.model'
import { default as clusterService } from '@/services/Clusters/ClusterService'
import { default as countryService } from '@/services/Countries/CountryService'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'
import { FilterMatchMode } from '@primevue/core/api/'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { useRouter } from 'vue-router'
const manageCluster = useManageCluster()
const { regions, countries } = useLocationFilters()
const selectedRegion = ref()
const selectedCountry = ref()
const clusters = ref<searchClusterInfo[]>([])
const selectedClusters = ref<searchClusterInfo[]>([])
const selectedCluster = ref<searchClusterInfo | null>(null)
const showManageClusterDialog = ref(false)
const toast = useToast()
const loading = ref(true)
const loadingCluster = ref(false)
const layout = useSystemStore()
const brandStore = useBrandStore()
const brand = storeToRefs(brandStore).activeBrand
const searchText = ref('')
const router = useRouter()
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  standTypeName: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
})

const resolver = manageCluster.resolver.value
const selectedStand = manageCluster.selectedStand
const selectedStandType = manageCluster.selectedStandType
const mc_selectedCountries = manageCluster.ms_selectedCountries
const mcCountrySelectList = manageCluster.countrySelectList
const mcSelectAllCountries = manageCluster.selectAllCountries
const mcStands = manageCluster.stands
const mcStandTypes = manageCluster.standTypes
const clusterName = manageCluster.clusterName
const initialValues = ref({
  clusterName: '',
  published: false,
})
watch(brand, async (newBrand) => {
  if (newBrand) {
    let filter = new ClusterFilter()
    filter.brandId = newBrand.id
    await clusterService.searchClusters(filter).then((response) => {
      clusters.value = response
      console.log('Clusters loaded for brand change', clusters.value)
    })

    let rFilter = new regionFilter()
    rFilter.brandId = newBrand.id
    await useLocationFilters().getRegions(rFilter)
  }
})

watch(showManageClusterDialog, (newValue) => {
  if (!newValue) {
    selectedCluster.value = null
  }
})

onMounted(async () => {
  loading.value = true
  await clusterService.initialise()

  let brandid = brandStore.activeBrand?.id ?? 0
  let rFilter = new regionFilter()
  rFilter.brandId = brandid
  await useLocationFilters()
    .getRegions(rFilter)
    .then((response) => {
      regions.value = response
    })

  var filter = new ClusterFilter()
  filter.brandId = brandid
  await clusterService.searchClusters(filter).then((response) => {
    clusters.value = response
    console.log('Clusters loaded', clusters.value)
    loading.value = false
  })

  //   FilterService.register(part_FILTER.value, (value: any, filter: any) => {
  //     if (filter === undefined || filter === null || filter.trim() === '') {
  //       return true
  //     }

  //     if (value === undefined || value === null) {
  //       return false
  //     }

  //     return value.toString() === filter.toString()
  //   })
})

async function onRegionChange() {
  if (selectedRegion.value) {
    countries.value = await useLocationFilters().onRegionChange(selectedRegion.value)
    let filter = new ClusterFilter()
    filter.brandId = brandStore.activeBrand?.id ?? 0
    filter.regionId = selectedRegion.value
    await clusterService.searchClusters(filter).then((response) => {
      clusters.value = response
      console.log('Clusters loaded', clusters.value)
    })
  } else {
    countries.value = []
  }
}

async function onCountryChange() {
  if (selectedCountry.value) {
    let filter = new ClusterFilter()
    filter.brandId = brandStore.activeBrand?.id ?? 0
    filter.countryId = selectedCountry.value
    await clusterService.searchClusters(filter).then((response) => {
      clusters.value = response
      console.log('Clusters loaded', clusters.value)
    })
  } else {
    countries.value = []
  }
}

async function clearFilters() {
  selectedRegion.value = null
  selectedCountry.value = null
  countries.value = []
  let filter = new ClusterFilter()
  filter.brandId = brandStore.activeBrand?.id ?? 0
  await clusterService.searchClusters(filter).then((response) => {
    clusters.value = response
    console.log('Clusters loaded', clusters.value)
  })
  let rFilter = new regionFilter()
  rFilter.brandId = brandStore.activeBrand?.id ?? 0
  await countryService.getRegions(rFilter).then((response) => {
    regions.value = response
    console.log('Regions loaded', regions.value)
  })
}

function editCluster(cluster: searchClusterInfo) {
  console.log('Edit cluster', cluster)
  layout.setActiveCluster(cluster)
  // Navigate to edit page
  router.push({ name: 'editCluster', params: { id: cluster.id } })
}

async function onSelectCluster(event: any) {
  loadingCluster.value = true
  selectedCluster.value = event.data as searchClusterInfo

  initialValues.value.clusterName = selectedCluster.value.name
  initialValues.value.published = selectedCluster.value.published
  clusterName.value = selectedCluster.value.name
  // await getPlanogramPreviewImage(selectedCluster.value.id);
  showManageClusterDialog.value = true
  manageCluster.initialise(selectedCluster.value).then(() => {
    loadingCluster.value = false
  })
}

async function saveLayout({ valid }: any) {
  if (!valid) {
    return
  }

  // Implement the logic to create a cluster here
  let filter = new SaveLayoutDto()
  filter.name = clusterName.value
  filter.id = selectedCluster.value?.id ?? 0
  filter.published = selectedCluster.value?.published ?? false
  await clusterService.initialise()
  var saveStatus = await clusterService.saveLayout(filter)
  if (saveStatus === 200) {
    toast.add({
      severity: 'success',
      summary: 'Cluster Saved',
      detail: 'Cluster details saved successfully.',
      life: 3000,
    })
    selectedCluster.value!.dateCreated = new Date()
    showManageClusterDialog.value = false
    selectedCluster.value = null
  } else {
    toast.add({
      severity: 'error',
      summary: 'Error Saving Cluster',
      detail: 'An error occurred while saving the cluster details.',
      life: 3000,
    })
  }
}
</script>

<template>
  <div>
    <h1>Cluster List View</h1>
    <!-- Product list content goes here -->
    <Toolbar class="mb-6">
      <template #start>
        <Select
          v-model="selectedRegion"
          :options="regions ?? []"
          @change="onRegionChange"
          option-label="name"
          option-value="id"
          placeholder="Select a region"
          class="mr-2"
        />

        <Select
          v-model="selectedCountry"
          :options="countries ?? []"
          @change="onCountryChange"
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

      <template #end> </template>
    </Toolbar>
    <div class="card">
      <DataTable
        ref="dt"
        v-model:selection="selectedCluster"
        selectionMode="single"
        @row-select="onSelectCluster"
        v-model:filters="filters"
        :loading="loading"
        :globalFilterFields="[
          //'categoryName',
          'name',
          'description',
          'partNumber',
          'partTypeName',
          'facings',
          'stock',
        ]"
        filterDisplay="row"
        :value="clusters"
        dataKey="id"
        :paginator="true"
        :rows="10"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rowsPerPageOptions="[5, 10, 25]"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} Clusters"
      >
        <template #header>
          <div class="flex flex-wrap gap-2 items-center justify-between">
            <h4 class="m-0">Manage Clusters</h4>
            <IconField>
              <InputIcon>
                <i class="pi pi-search" />
              </InputIcon>
              <InputText v-model="filters['global'].value" placeholder="Search..." />
            </IconField>
          </div>
        </template>
        <!-- <Column selectionMode="multiple" style="width: 3rem" :exportable="false"></Column> -->
        <Column field="name" header="Name" sortable style="min-width: 6rem"></Column>
        <Column field="standName" header="Stand Name" sortable style="min-width: 12rem"></Column>
        <Column
          field="standTypeName"
          header="StandType"
          filterField="standTypeName"
          style="min-width: 16rem"
        >
          <template #filter="{ filterModel, filterCallback }">
            <InputText
              v-model="filterModel.value"
              type="text"
              @input="filterCallback()"
              placeholder="Search by stand type"
            />
          </template>
        </Column>
        <Column
          field="standAssemblyNumber"
          header="Assembly Number"
          sortable
          style="min-width: 12rem"
        ></Column>

        <Column field="height" header="Height" sortable style="min-width: 6rem"></Column>
        <Column field="width" header="Width" sortable style="min-width: 6rem"></Column>
        <Column field="published" header="Published" sortable style="min-width: 12rem"></Column>
        <Column field="dateCreated" header="Date Created" sortable style="min-width: 12rem">
          <template #body="slotProps">
            {{ new Date(slotProps.data.dateCreated).toLocaleDateString() }}
          </template>
        </Column>
        <Column field="dateUpdated" header="Last Updated" sortable style="min-width: 12rem">
          <template #body="slotProps">
            {{ new Date(slotProps.data.dateUpdated).toLocaleDateString() }}
          </template>
        </Column>
        <Column :exportable="false" style="min-width: 12rem">
          <template #body="slotProps">
            <Button
              icon="pi pi-pencil"
              variant="outlined"
              rounded
              class="mr-2"
              @click="editCluster(slotProps.data)"
            />
          </template>
        </Column>
      </DataTable>
    </div>
  </div>

  <Dialog
    v-model:visible="showManageClusterDialog"
    modal
    header="Manage Cluster"
    :style="{ width: '54rem' }"
  >
    <!-- <div class="flex flex-col gap-4 mb-2">
      <div class="flex flex-col gap-1.5">
        <label for="name">Cluster Preview Image</label>
        <img
          v-if="clusterPreviewImage"
          :src="decodeURIComponent(clusterPreviewImage)"
          alt="Cluster Preview"
          class="w-full h-auto rounded-lg border"
        />
        <div
          v-else
          class="w-full h-48 flex items-center justify-center rounded-lg border bg-surface-100 dark:bg-surface-800"
        >
          <span class="text-sm text-surface-500">No preview available</span>
        </div>
      </div>
    </div> -->
    <div class="flex flex-row gap-4 justify-center items-center">
      <BlockUI :blocked="loadingCluster">
        <div
          class="flex absolute w-full h-full justify-center items-center"
          :class="{ hidden: !loadingCluster }"
        >
          <ProgressSpinner v-if="loadingCluster" class="blockui-spinner z-1110" />
        </div>
        <!-- </BlockUI> -->
        <Form
          v-slot="$form"
          :initialValues="initialValues"
          :resolver="resolver"
          @submit="saveLayout"
          :class="{ 'z-10': !loadingCluster }"
        >
          <div class="flex flex-col md:flex-row gap-8">
            <div class="md:w-1/2">
              <div class="card flex flex-col gap-4">
                <div class="form-group">
                  <label for="planogramName">Cluster Name:</label>
                  <InputText
                    name="clusterName"
                    id="clusterName"
                    type="text"
                    v-model="selectedCluster.name"
                    :value="selectedCluster.name"
                    class="w-full"
                  />
                  <Message
                    v-if="$form.clusterName?.invalid"
                    severity="error"
                    size="small"
                    variant="simple"
                    >{{ $form.clusterName.error?.message ?? '&nbsp;' }}</Message
                  >
                </div>
                <div class="form-group">
                  <div class="flex items-center gap-2">
                    <label class="mr-2" for="publised">Published</label>
                    <Checkbox v-model="selectedCluster.published" binary size="large"> </Checkbox>
                  </div>
                </div>
                <div class="form-group">
                  <label for="layoutPartNumber">Assembly Number:</label>
                  <InputText
                    name="layoutPartNumber"
                    id="layoutPartNumber"
                    type="text"
                    class="w-full"
                    v-model="selectedCluster.standAssemblyNumber"
                  />
                  <Message
                    v-if="$form.layoutPartNumber?.invalid"
                    severity="error"
                    size="small"
                    variant="simple"
                    >{{ $form.layoutPartNumber.error?.message ?? '&nbsp;' }}</Message
                  >
                </div>

                <div class="flex flex-col gap-2">
                  <label for="regions">Regions:</label>
                  <MultiSelect
                    name="regions"
                    v-model="manageCluster.ms_selectedRegions"
                    :options="regions ?? []"
                    id="regions"
                    class="w-full"
                    option-label="name"
                    option-value="id"
                    @change="manageCluster.onRegionChange"
                  >
                    <template #option="option">
                      <div class="flex align-items-center">
                        <span>{{ option.option.name }}</span>
                      </div>
                    </template>
                  </MultiSelect>

                  <Message
                    v-if="$form.regions?.invalid"
                    severity="error"
                    size="small"
                    variant="simple"
                    >{{ $form.regions.error?.message ?? '&nbsp;' }}</Message
                  >
                </div>
                <div class="flex flex-col gap-2">
                  <!-- <Select
                  name="country"
                  v-model="selectedCountryId"
                  :options="countries ?? []"
                  @change="onCountryChange"
                  option-label="name"
                  option-value="id"
                  placeholder="Select a country"
                  class="mr-2"
                  fluid
                /> -->
                  <label for="countries">Countries:</label>
                  <MultiSelect
                    name="countries"
                    v-model="manageCluster.ms_selectedCountries"
                    :options="mcCountrySelectList ?? []"
                    id="countries"
                    class="w-full"
                    option-label="name"
                    option-value="id"
                    @change="manageCluster.onCountryChange"
                    :selectAll="mcSelectAllCountries"
                    @selectall-change="manageCluster.onSelectAllCountriesChange($event)"
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
                    >{{ $form.countries.error?.message ?? '&nbsp;' }}</Message
                  >
                </div>
                <div class="form-group">
                  <label for="standType">Stand Type:</label>
                  <InputText class="w-full" disabled :value="selectedStandType?.name"></InputText>
                </div>
                <div class="form-group">
                  <label for="stand">Stand:</label>
                  <InputText class="w-full" disabled :value="selectedStand?.name"></InputText>
                </div>

                <div class="flex gap-2 justify-between">
                  <Button type="submit" severity="secondary" class="w-60" :fluid="false"
                    >Save Cluster</Button
                  >
                </div>
              </div>
            </div>
            <div class="md:w-1/2">
              <div class="card flex flex-col gap-4">
                <div class="font-semibold text-xl">Cluster Details</div>
                <div class="grid grid-cols-12 gap-2">
                  <label for="name" class="flex items-center mb-2 md:col-span-2 md:mb-0 text-lg"
                    >Name</label
                  >
                  <div class="col-span-12 md:col-span-10">
                    <p class="font-bold text-lg">{{ selectedCluster.name ?? '' }}</p>
                  </div>
                </div>
                <div class="grid grid-cols-12 gap-2">
                  <div class="col-span-4 text-lg">Countries</div>
                  <div class="col-span-8 text-lg">
                    <div
                      v-for="country in mcCountrySelectList"
                      :key="country.id"
                      class="flex text-lg"
                    >
                      <template
                        v-if="
                          mc_selectedCountries != null && mc_selectedCountries.includes(country.id)
                        "
                      >
                        {{ country.name ?? '' }}
                      </template>
                    </div>
                  </div>
                </div>
                <div class="grid grid-cols-12 gap-2">
                  <div class="col-span-4 text-lg">Stand Type</div>
                  <div class="col-span-8">
                    <p class="text-lg">{{ selectedStandType?.name ?? '' }}</p>
                  </div>
                </div>
                <div class="grid grid-cols-12 gap-2">
                  <label
                    for="email3"
                    class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0 text-lg"
                    >Stand</label
                  >
                  <div class="col-span-12 md:col-span-10">
                    <p class="text-lg">{{ selectedStand?.name ?? '' }}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </Form>
      </BlockUI>
    </div>
    <template #footer> </template>
  </Dialog>
</template>
