<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { RouterView } from 'vue-router'

const authStore = useAuthStore()
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
  <main>
    <RouterView />
  </main>

  <!-- error -->
  <div id="error" v-if="authStore.error">{{ authStore.error }}</div>
</template>

<style scoped></style>
