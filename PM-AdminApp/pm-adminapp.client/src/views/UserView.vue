<script lang="ts">
import { useAuthStore } from '@/stores/auth'
import { user } from '@/stores/user'
import { defineComponent } from 'vue'

// using options api as setup script doesn't seem to support before route enter?
const auth = useAuthStore()
export default defineComponent({
  computed: {
    user: () => user,
    auth: () => auth,
  },

  beforeRouteEnter(to, from, next) {
    !user.data ? user.load().then(next) : next()
  },
})
</script>

<template>
  <h1>User</h1>

  <!-- api data -->
  <h2>API data</h2>
  <pre>{{ user.data }}</pre>

  <!-- msal account data -->
  <h2>MSAL data</h2>
  <pre>{{ auth.account }}</pre>
</template>
