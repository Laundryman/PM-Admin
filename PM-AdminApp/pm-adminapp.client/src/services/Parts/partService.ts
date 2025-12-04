// import { useAuthStore } from '@/stores/auth'
import type { PartFilter } from '@/models/Parts/partFilter.model'
import type { SearchPartInfo } from '@/models/Parts/searchPartInfo.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'

await msal.initialize()

const token = ref()
const initialized = ref(false)
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/part',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token.value}`,
  },
})

export default {
  async searchParts(filter: PartFilter): Promise<SearchPartInfo[]> {
    // if (initialized.value !== false) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/searchParts', filter)
    return response.data
    // } else {
    //   throw new Error('PartService not initialized')
    // }
  },

  async initialise() {
    const authStore = useAuthStore()
    if (!authStore.initialized) {
      await authStore.initialize()
    }
    const t = await Auth.getToken()
    token.value = t
    console.log('PartService initialized with token:', token.value)
    initialized.value = true
  },
}
