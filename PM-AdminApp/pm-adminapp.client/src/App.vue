<script setup lang="ts">
import ClusterService from '@/services/Planner/ClusterService'
import { useAuthStore } from '@/stores/auth'
import { RouterView } from 'vue-router'
const authStore = useAuthStore()

async function loadClusterData(clusterId: number) {
  await ClusterService.initialise()
  const clusterData = await ClusterService.loadClusterData(clusterId)
  return clusterData
}

async function loadStandData(standId: number) {
  await ClusterService.initialise()
  const clusterData = await ClusterService.loadStandData(standId)
  return clusterData
}

async function loadClusterMenuData(clusterId: number) {
  await ClusterService.initialise()
  const clusterData = await ClusterService.loadClusterMenuData(clusterId)
  return clusterData
}
async function loadClusterMenuCategories(clusterId: number) {
  await ClusterService.initialise()
  const clusterData = await ClusterService.loadClusterMenuCategories(clusterId)
  return clusterData
}

async function loadClusterCategoryMenuData(clusterId: number, category: string) {
  await ClusterService.initialise()
  const clusterData = await ClusterService.loadClusterCategoryMenuData(clusterId, category)
  return clusterData
}

async function loadClusterShelves(clusterId: number) {
  await ClusterService.initialise()
  const clusterData = await ClusterService.loadClusterShelves(clusterId)
  return clusterData
}

async function loadClusterParts(clusterId: number) {
  await ClusterService.initialise()
  const clusterData = await ClusterService.loadClusterParts(clusterId)
  return clusterData
}

async function loadPartProducts(partId: number, clusterId: number) {
  await ClusterService.initialise()
  const clusterData = await ClusterService.loadPartProducts(partId, clusterId)
  return clusterData
}

defineExpose({
  loadClusterData,
  loadClusterMenuData,
  loadClusterMenuCategories,
  loadStandData,
  loadClusterCategoryMenuData,
  loadClusterShelves,
  loadClusterParts,
  loadPartProducts,
})
</script>

<template>
  <!-- menu -->
  <!-- <nav>
    <router-link to="/">Home</router-link>
    <template v-if="auth.account">
      <router-link to="/user">User</router-link>
      <router-link to="/logout">Logout</router-link>
    </template>
  </nav> -->

  <!-- view -->
  <main style="width: 100%; height: 100%">
    <RouterView />
  </main>

  <!-- error -->
  <div id="error" v-if="authStore.error">{{ authStore.error }}</div>
</template>

<style scoped></style>
