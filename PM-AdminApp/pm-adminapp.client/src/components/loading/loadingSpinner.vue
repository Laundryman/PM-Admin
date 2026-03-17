<script setup lang="ts">
defineProps({ loading: { type: Boolean, required: true } })
</script>

<template>
  <div
    v-if="loading"
    class="fixed top-0 left-0loading object-cover w-full h-full flex items-center justify-center bg-gray-50/50 backdrop-opacity-90"
  >
    <div class="spinner">
      <ProgressSpinner />
    </div>
  </div>
</template>
