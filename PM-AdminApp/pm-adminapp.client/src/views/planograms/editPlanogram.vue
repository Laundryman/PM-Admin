<script setup lang="ts">
import PlanMatrPlanner from '@/components/Planner/PlanMatrPlanner.vue'
import { AppMode } from '@/planner/models/Enumerations'
import { default as planogramService } from '@/services/Planograms/PlanogramService'
import { usePlanogramStore } from '@/stores/planogramStore'
import { onBeforeRouteLeave, useRouter } from 'vue-router'

const router = useRouter()
const appMode = AppMode.Planogram
const planogramId = Number(router.currentRoute.value.params.id) || 0
const planogramStore = usePlanogramStore()

onBeforeRouteLeave((to, from, next) => {
  // Perform any necessary cleanup or actions before leaving the route
  if (planogramStore.dirty) {
    const confirmLeave = window.confirm(
      'You have unsaved changes. Are you sure you want to leave this page?',
    )
    if (!confirmLeave) {
      next(false) // Cancel navigation
      return
    }
  }
  planogramStore.dirty = false // Reset dirty state if user confirms navigation
  planogramService.unlockPlanogram(planogramStore.planogram.id)

  next()
})
</script>

<template>
  <div id="planner-app" class="planner-app">
    <PlanMatrPlanner
      :app-mode="AppMode.Planogram"
      :planogram-id="planogramId"
      :cluster-id="0"
      :dirty="false"
    />
  </div>
</template>

<style scoped>
header {
  line-height: 1.5;
}

.logo {
  display: block;
  margin: 0 auto 2rem;
}

@media (min-width: 1024px) {
  header {
    display: flex;
    place-items: center;
    padding-right: calc(var(--section-gap) / 2);
  }

  .logo {
    margin: 0 2rem 0 0;
  }

  header .wrapper {
    display: flex;
    place-items: flex-start;
    flex-wrap: wrap;
  }
}
</style>
