import { PlanogramFilter } from '@/models/Planograms/planogramFilter.model'
import { searchPlanogramInfo } from '@/models/Planograms/searchPlanogramnfo.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'

await msal.initialize()

const token = ref()
const initialized = ref(false)
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/planograms',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token.value}`,
  },
})
export default {
  async searchPlanograms(filter: PlanogramFilter): Promise<searchPlanogramInfo[]> {
    // if (initialized.value !== false) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient
      .post('/searchPlanograms', filter)
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response
    // } else {
    //   throw new Error('PlanogramService not initialized')
    // }
  },

  async unlockPlanogram(planogramId: number): Promise<void> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    await apiClient
      .get('/unlock/', { params: { id: planogramId } })
      .then(() => {
        return
      })
      .catch((error) => {
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
    console.log('PlanogramService initialized with token:', token.value)
    initialized.value = true
  },
}
