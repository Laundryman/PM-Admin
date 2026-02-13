// import { useAuthStore } from '@/stores/auth'
import type { searchStandInfo } from '@/models/Stands/searchStandInfo.model'
import type { Stand } from '@/models/Stands/stand.model'
import type { StandFilter } from '@/models/Stands/standFilter.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'

await msal.initialize()

const token = ref()
const initialized = ref(false)
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/stands',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token.value}`,
  },
})

export default {
  async searchStands(filter: StandFilter): Promise<searchStandInfo[]> {
    // if (initialized.value !== false) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/searchStands', filter)
    return response.data
    // } else {
    //   throw new Error('PartService not initialized')
    // }
  },

  async getStand(standId: number): Promise<Stand> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.get('/getStand', { params: { id: standId } })
    return response.data
  },

  async initialise() {
    const authStore = useAuthStore()
    if (!authStore.initialized) {
      await authStore.initialize()
    }
    const t = await Auth.getToken()
    token.value = t
    console.log('StandService initialized with token:', token.value)
    initialized.value = true
  },
}
