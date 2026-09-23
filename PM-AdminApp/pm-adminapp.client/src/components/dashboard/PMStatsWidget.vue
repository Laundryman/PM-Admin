<script lang="ts" setup>
import type { Planogram } from '@/models/Planograms/planogram.model'
import { PlanoWidgetFilter } from '@/models/Widgets/planoWidgetFilter.model'
import { StatsDto } from '@/models/Widgets/statsDto'
import { PlanogramStatusEnum } from '@/planner/models/Enumerations'
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
const stats = ref<StatsDto>(null)
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
    .getStats()
    .then((response) => {
      stats.value = response
      console.log('Stats loaded', stats.value)
    })
    .catch((error) => {
      console.log('Error loading stats', error)
    })
  loading.value = false
})
</script>

<template>
  <div class="col-span-12 lg:col-span-6 xl:col-span-3">
    <div class="card mb-0">
      <div class="flex justify-between mb-4">
        <div>
          <span class="block text-muted-color font-medium mb-4">Planograms</span>
          <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
            {{ stats?.planogramsCount ?? 0 }}
          </div>
        </div>
        <div
          class="flex items-center justify-center bg-blue-100 dark:bg-blue-400/10 rounded-border"
          style="width: 2.5rem; height: 2.5rem"
        >
          <i class="pi pi-building text-blue-500 !text-xl"></i>
        </div>
      </div>
      <span class="text-primary font-medium">{{ stats?.recentPlanogramsCount ?? 0 }} new </span>
      <span class="text-muted-color">in last month</span>
    </div>
  </div>
  <div class="col-span-12 lg:col-span-6 xl:col-span-3">
    <div class="card mb-0">
      <div class="flex justify-between mb-4">
        <div>
          <span class="block text-muted-color font-medium mb-4">Parts</span>
          <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
            {{ stats?.partsCount ?? 0 }}
          </div>
        </div>
        <div
          class="flex items-center justify-center bg-orange-100 dark:bg-orange-400/10 rounded-border"
          style="width: 2.5rem; height: 2.5rem"
        >
          <i class="pi pi-objects-column text-orange-500 !text-xl"></i>
        </div>
      </div>
      <span class="text-primary font-medium">{{ stats?.recentPartsCount ?? 0 }} new </span>
      <span class="text-muted-color">in last month</span>
    </div>
  </div>
  <div class="col-span-12 lg:col-span-6 xl:col-span-3">
    <div class="card mb-0">
      <div class="flex justify-between mb-4">
        <div>
          <span class="block text-muted-color font-medium mb-4">Products</span>
          <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
            {{ stats?.productsCount ?? 0 }}
          </div>
        </div>
        <div
          class="flex items-center justify-center bg-cyan-100 dark:bg-cyan-400/10 rounded-border"
          style="width: 2.5rem; height: 2.5rem"
        >
          <i class="pi pi-briefcase text-cyan-500 !text-xl"></i>
        </div>
      </div>
      <span class="text-primary font-medium">{{ stats?.recentProductsCount ?? 0 }} added </span>
      <span class="text-muted-color">since last month</span>
    </div>
  </div>
  <div class="col-span-12 lg:col-span-6 xl:col-span-3">
    <div class="card mb-0">
      <div class="flex justify-between mb-4">
        <div>
          <span class="block text-muted-color font-medium mb-4">Shades</span>
          <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">
            {{ stats?.shadesCount ?? 0 }}
          </div>
        </div>
        <div
          class="flex items-center justify-center bg-purple-100 dark:bg-purple-400/10 rounded-border"
          style="width: 2.5rem; height: 2.5rem"
        >
          <i class="pi pi-circle-fill text-purple-500 !text-xl"></i>
        </div>
      </div>
      <span class="text-primary font-medium">{{ stats?.recentShadesCount ?? 0 }} new </span>
      <span class="text-muted-color">in last month</span>
    </div>
  </div>
</template>
