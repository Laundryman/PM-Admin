// import { useAuthStore } from '@/stores/auth'
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
  async updateShade(formData: FormData): Promise<any> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    apiClient.defaults.headers['Content-Type'] = 'multipart/form-data'
    await apiClient
      .post('/saveShade', formData)
      .then((response) => {
        console.log('Shade updated successfully')
        return response.data
      })
      .catch((error) => {
        console.error('Error updating shade:', error)
        throw error
      })
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
