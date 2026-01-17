<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
if (!auth.userInfo) {
  await auth.setCurrentlyLoggedInUserInfo()
}
</script>

<template>
  <div
    class="config-panel hidden absolute top-[3.25rem] right-0 w-64 p-4 bg-surface-0 dark:bg-surface-900 border border-surface rounded-border origin-top shadow-[0px_3px_5px_rgba(0,0,0,0.02),0px_0px_2px_rgba(0,0,0,0.05),0px_1px_4px_rgba(0,0,0,0.08)]"
  >
    <div class="flex flex-column align-items-center mb-3">
      <!-- <img
        src="../assets/images/avatar.jpg"
        alt="Image"
        class="w-20 h-20 mb-2 border-circle shadow-2"
      /> -->
      <div class="text-center">
        <div class="text-lg font-bold text-900 mb-1">{{ auth.userInfo?.name }}</div>
        <div class="text-sm text-600 mb-3">{{ auth.userInfo?.email }}</div>
        <button
          pButton
          type="button"
          label="View Profile"
          class="p-button-outlined p-button-sm"
          icon="pi pi-user"
        ></button>
      </div>
    </div>
    <div class="flex flex-column gap-3">
      <Button
        pButton
        type="button"
        label="Settings"
        class="p-button-text p-button-sm justify-content-start"
        icon="pi pi-cog"
      ></Button>
      <Button
        pButton
        type="button"
        label="Logout"
        class="p-button-text p-button-sm justify-content-start"
        icon="pi pi-sign-out"
      ></Button>
    </div>
  </div>
</template>
