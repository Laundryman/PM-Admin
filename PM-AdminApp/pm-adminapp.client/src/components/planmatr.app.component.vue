<template>
  <div></div>
  <div
    id="planmatr-container"
    class="joint-app lm-planx-app joint-theme-planx-dark"
    data-cluster="true"
    data-theme="planx-dark"
  >
    <div class="app-header">
      <div class="app-title">
        <h1>Planner</h1>
      </div>
      <div class="toolbar-container"></div>
    </div>
    <div class="app-body">
      <div class="stencil-container"></div>
      <div class="paper-container"></div>
      <div class="inspector-container"></div>
      <div class="navigator-container"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import '@/assets/planner/css/rappid.css'

import '@/assets/planner/css/lmplanx.css'
import '@/assets/planner/css/style.css'
import '@/assets/planner/css/style.planx-dark.css'
import '@/assets/planner/css/theme-picker.css'
import { useLayoutStore } from '@/layout/composables/layout'
import { onBeforeMount, onMounted, onUnmounted } from 'vue'

const layoutStore = useLayoutStore()
function data() {
  return {
    currentDate: '',
  }
}

onBeforeMount(() => {
  layoutStore.togglePlannerLayout()
})
onMounted(() => {
  // Load Moment.js
  const script = document.createElement('script')
  script.src = '/public/planner/js/bundle.js'
  script.onload = () => {}
  document.head.appendChild(script)
})

onUnmounted(() => {
  layoutStore.togglePlannerLayout()
})
</script>
