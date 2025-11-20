<script setup lang="ts">
import { msal } from '@/config/auth'
import { useAuthStore } from '@/stores/auth'
import { onMounted } from 'vue'

import FeaturesWidget from '@/components/landing/FeaturesWidget.vue'
import FooterWidget from '@/components/landing/FooterWidget.vue'
import HeroWidget from '@/components/landing/HeroWidget.vue'
import HighlightsWidget from '@/components/landing/HighlightsWidget.vue'
import PricingWidget from '@/components/landing/PricingWidget.vue'
import TopbarWidget from '@/components/landing/TopbarWidget.vue'

const authStore = useAuthStore()
const initialize = async () => {
  let account = authStore.account
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
  <div class="bg-surface-0 dark:bg-surface-900">
    <div id="home" class="landing-wrapper overflow-hidden">
      <div
        class="py-6 px-6 mx-0 md:mx-12 lg:mx-20 lg:px-20 flex items-center justify-between relative lg:static"
      >
        <TopbarWidget />
      </div>
      <HeroWidget />
      <FeaturesWidget />
      <HighlightsWidget />
      <PricingWidget />
      <FooterWidget />
    </div>
  </div>
</template>
