<script lang="ts" setup>
import { msal } from '@/config/auth'
import { useAuthStore } from '@/stores/auth'
import { onMounted } from 'vue'

const auth = useAuthStore()
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
  <h1>Home</h1>
  <p v-if="auth.account">Welcome!</p>
  <p v-else>Click to <router-link to="/login">login</router-link>.</p>
</template>
