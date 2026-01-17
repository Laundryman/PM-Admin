import User from '@/models/Identity/user.model'
import { Graph, msal } from '@/services/Identity/graph'

import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'
const token = ref()
const initialized = ref(false)
const authStore = ref()

await msal.initialize()

const apiClient = axios.create({
  // baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/graph',
  baseURL: 'https://graph.microsoft.com/v1.0',
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
})

export default {
  async getCurrentUserInfo() {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }

    // await apiClient
    //   .get('/get-currently-logged-in-user-info')
    //   .then((response) => {
    //     return response.data
    //   })
    //   .catch((error) => {
    //     console.log('Error fetching user info:', error)
    //     throw error
    //   })
    await apiClient
      .get('/me', {})
      .then((response) => {
        return response.data
      })
      .catch((error) => {
        throw error
      })
  },

  saveUser(user: User) {
    return apiClient.put('/update', user)
  },

  createUser(user: User) {
    return apiClient.put('/create', user)
  },

  deleteUser(id: string) {
    return apiClient.delete('/delete-user', {
      params: {
        id: id,
      },
    })
  },
  changePassword(user: User) {
    return apiClient.put('/change-password', user)
  },

  async initialise() {
    authStore.value = useAuthStore()
    if (!authStore.value.initialized) {
      await authStore.value.initialize()
    }
    const t = await Graph.getGraphToken()
    token.value = t
    console.log('UserService initialized with token:', token.value)
    initialized.value = true
  },
}
