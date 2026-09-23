<script setup lang="ts">
import BestSellingWidget from '@/components/dashboard/BestSellingWidget.vue'
import NotificationsWidget from '@/components/dashboard/NotificationsWidget.vue'
import RecentStatsWidget from '@/components/dashboard/PMStatsWidget.vue'
import RecentPlanogramWidget from '@/components/dashboard/RecentPlanogramWidget.vue'
import RevenueStreamWidget from '@/components/dashboard/RevenueStreamWidget.vue'
import { msal } from '@/services/Identity/auth'
import { onMounted } from 'vue'

const initialize = async () => {
  try {
    await msal.initialize()
  } catch (error) {
    console.error('Initialization error: ', error)
  }
}

onMounted(async () => {
  initialize()
})
</script>

<template>
  <div class="grid grid-cols-12 gap-8">
    <!-- <StatsWidget /> -->
    <RecentStatsWidget />

    <div class="col-span-12 xl:col-span-6">
      <RecentPlanogramWidget />
      <BestSellingWidget />
    </div>
    <div class="col-span-12 xl:col-span-6">
      <RevenueStreamWidget />
      <NotificationsWidget />
    </div>
  </div>
</template>
