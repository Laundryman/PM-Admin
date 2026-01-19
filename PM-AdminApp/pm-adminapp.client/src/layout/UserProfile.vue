<script setup lang="ts">
import userService from '@/services/Identity/UserService'
import { useAuthStore } from '@/stores/auth'
import { onMounted } from 'vue'

const auth = useAuthStore()

onMounted(async () => {
  if (auth.initialized === true) {
    // no need to load user info again
    if (!auth.userInfo) {
      await userService.initialise()
      await userService.getCurrentUserInfo()
    }

    console.log('User Info in UserProfile:', auth.userInfo)
  }
})
// const props = defineProps({
//   userInfo: {
//     type: Object as () => any,
//     required: true,
//   },
// })

function login() {
  // auth.login()
}

function logout() {
  auth.logout()
}
</script>

<template>
  <div
    class="config-panel hidden absolute top-[3.25rem] right-0 w-84 p-4 bg-surface-0 dark:bg-surface-900 border border-surface rounded-border origin-top shadow-[0px_3px_5px_rgba(0,0,0,0.02),0px_0px_2px_rgba(0,0,0,0.05),0px_1px_4px_rgba(0,0,0,0.08)]"
  >
    <div class="flex flex-column align-items-center mb-3">
      <!-- <img
        src="../assets/images/avatar.jpg"
        alt="Image"
        class="w-20 h-20 mb-2 border-circle shadow-2"
      /> -->
      <div class="flex flex-col gap-1">
        <div class="text-lg font-bold text-900 mb-1">{{ auth.userInfo?.displayName }}</div>
        <div class="text-sm text-600 mb-3">{{ auth.userInfo?.userPrincipalName }}</div>
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
        @click="logout"
      ></Button>
    </div>
  </div>
</template>
