<script setup lang="ts">
import ClusterService from '@/services/Planner/ClusterService'

async function loadClusterData(clusterId: number) {
  const clusterData = await ClusterService.loadClusterData(clusterId)
}
defineExpose({
  loadClusterData,
})

//provide('loadClusterData', loadClusterData)
</script>

<template></template>
