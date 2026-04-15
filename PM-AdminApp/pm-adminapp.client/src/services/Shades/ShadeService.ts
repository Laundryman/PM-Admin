// import { useAuthStore } from '@/stores/auth'
import type { Shade } from '@/models/Products/shade.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'

await msal.initialize()

const token = ref()
const initialized = ref(false)
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/shades',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token.value}`,
  },
})

export default {
  async updateShade(shade: Shade): Promise<Shade> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    // apiClient.defaults.headers['Content-Type'] = 'multipart/form-data'
    let response = await apiClient
      .post('/updateShade', shade)
      .then((response) => {
        console.log('Shade updated successfully')
        return response
      })
      .catch((error) => {
        console.error('Error updating shade:', error)
        throw error
      })

    return response.data
  },

  async createShade(shade: Shade): Promise<Shade> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    // apiClient.defaults.headers['Content-Type'] = 'multipart/form-data'
    let response = await apiClient
      .post('/createShade', shade)
      .then((response) => {
        console.log('Shade created successfully')
        return response
      })
      .catch((error) => {
        console.error('Error creating shade:', error)
        throw error
      })
    return response.data
  },

  async initialise() {
    const authStore = useAuthStore()
    if (!authStore.initialized) {
      await authStore.initialize()
    }
    const t = await Auth.getToken()
    token.value = t
    console.log('ShadeService initialized with token:', token.value)
    initialized.value = true
  },
}
