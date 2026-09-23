<script lang="ts" setup>
import type { Planogram } from '@/models/Planograms/planogram.model'
import { PlanoWidgetFilter } from '@/models/Widgets/planoWidgetFilter.model'
import { PlanogramStatusEnum } from '@/planner/models/Enumerations'
import { default as countryService } from '@/services/Countries/CountryService'
import { default as planogramService } from '@/services/Planograms/PlanogramService'
import { default as widgetService } from '@/services/Widgets/widgetService'

import { useAuthStore } from '@/stores/auth'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'
import { storeToRefs } from 'pinia'
import { onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const layout = useSystemStore()
const brandStore = useBrandStore()
const authStore = useAuthStore()
const loading = ref(false)
const planograms = ref<Planogram[]>([])

const brand = storeToRefs(brandStore).activeBrand

const statuses = ref([
  { label: 'Editing', value: PlanogramStatusEnum.Editing },
  { label: 'Submitted', value: PlanogramStatusEnum.Submitted },
  { label: 'Validated', value: PlanogramStatusEnum.Validated },
  { label: 'Approved', value: PlanogramStatusEnum.Approved },
  // { label: 'Ordered', value: PlanogramStatusEnum.Ordered },
  { label: 'Deleted', value: PlanogramStatusEnum.Deleted },
])
watch(brand, async (newBrand) => {
  if (newBrand) {
    let filter = new PlanoWidgetFilter()
    filter.brandId = newBrand.id
    await widgetService.getRecentPlanograms(filter).then((response) => {
      planograms.value = response
      console.log('Planograms loaded for brand change', planograms.value)
    })
  }
})

onMounted(async () => {
  // const currentUser = ref(authStore.currentlyLoggedInUser)
  // if (!currentUser.value) {
  //   console.log('No current user found in authStore.')
  //   currentUser.value = await userService.getCurrentUserInfo()
  //   authStore.setCurrentlyLoggedInUserInfo(currentUser.value)
  // }
  loading.value = true
  layout.layoutState.disableBrandSelect = false
  await planogramService.initialise()
  await countryService.initialise()
  await widgetService.initialise()

  // let brandid = brandStore.activeBrand?.id ?? 0

  var filter = new PlanoWidgetFilter()
  filter.brandId = 0
  // filter.userId = currentUser.value?.id ?? ''
  filter.includeDeleted = true
  // filter.regionsList = regions.value?.map((r) => r.id).join(',') ?? ''
  // filter.countriesList = countries.value?.map((c) => c.id).join(',') ?? ''
  filter.archived = false // Only fetch non-archived planograms initially
  await widgetService
    .getRecentPlanograms(filter)
    .then((response) => {
      planograms.value = response
      console.log('Planograms loaded', planograms.value)
    })
    .catch((error) => {
      console.log('Error loading planograms', error)
    })
  loading.value = false
})

function getStatusSeverity(status: number): string {
  switch (status) {
    case 1:
      return 'info'
    case 2:
      return 'warn'
    case 3:
      return 'info'
    case 4:
      return 'danger'
    case 5:
      return 'success'
    case 6:
      return 'secondary'
    case 7:
      return 'success'
    default:
      return 'danger'
  }
}

function viewPlanogram(planogram) {
  router.push({ name: 'editPlanogram', params: { id: planogram.id } })
}
</script>
<template>
  <div class="card">
    <div class="font-semibold text-xl mb-4">Recent Planograms</div>
    <DataTable :value="planograms" :rows="5" :paginator="true" responsiveLayout="scroll">
      <Column style="width: 10%" header="Brand">
        <template #body="slotProps">
          <span v-if="slotProps.data.brand != null">{{ slotProps.data.brand.name }}</span>
        </template>
      </Column>
      <Column field="name" header="Name" :sortable="true" style="width: 20%"></Column>
      <Column field="status" header="Status" :sortable="true" style="width: 5%">
        <template #body="slotProps">
          <Tag
            :value="PlanogramStatusEnum[slotProps.data.statusId]"
            :severity="getStatusSeverity(slotProps.data.statusId)"
          />
        </template>
      </Column>
      <Column header="Last Updated By" :sortable="true" style="width: 25%">
        <template #body="slotProps">
          <span v-if="slotProps.data.lubName.trim() != ''">{{ slotProps.data.lubName }}</span>
          <span v-else>{{ slotProps.data.userName }}</span>
        </template>
      </Column>

      <!-- <Column style="width: 15%" header="View">
        <template #body="slotProps">
          <Button
            icon="pi pi-search"
            type="button"
            class="p-button-text"
            :onclick="() => viewPlanogram(slotProps.data)"
          ></Button>
        </template>
      </Column> -->
    </DataTable>
  </div>
</template>
