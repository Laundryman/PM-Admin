import type { LayoutFilter } from '@/models/Layout/LayoutFilter.model'
import type { LayoutInfo } from '@/models/Layout/searchLayoutInfo.model'
import type { CreatePlanogramFilter } from '@/models/Planograms/createPlanogramFilter.model'
import type { Planogram } from '@/models/Planograms/planogram.model'
import { PlanogramFilter } from '@/models/Planograms/planogramFilter.model'
import { searchPlanogramInfo } from '@/models/Planograms/searchPlanogramnfo.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'

await msal.initialize()

const token = ref()
const idToken = ref()

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
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
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

  async getPlanogram(id: number): Promise<Planogram> {
    // if (initialized.value !== false) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
      apiClient.defaults.headers['Content-Type'] = 'application/json'
    }
    // apiClient.baseURL = import.meta.env.VITE_API_ROOT + '/api/planograms/edit';

    let response = await apiClient
      .get('/edit/getPlanogram', { params: { planogramId: id } })
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
  async lockPlanogram(planogramId: number): Promise<void> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    await apiClient
      .get('/lockplanogram/', { params: { id: planogramId } })
      .then(() => {
        return
      })
      .catch((error) => {
        throw error
      })
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

  async getLayouts(filter: LayoutFilter): Promise<LayoutInfo[]> {
    // if (initialized.value !== false) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
      apiClient.defaults.headers['Content-Type'] = 'application/json'
    }
    // apiClient.baseURL = import.meta.env.VITE_API_ROOT + '/api/planograms/edit';

    let response = await apiClient
      .post('/create/getLayouts', filter)
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
  async createPlanogram(filter: CreatePlanogramFilter): Promise<Planogram> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }
    let response = await apiClient
      .post('/create/createPlanogram', filter)
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response
  },

  async initialise() {
    const authStore = useAuthStore()
    if (!authStore.initialized) {
      await authStore.initialize()
    }
    const t = await Auth.getToken()
    token.value = t
    const idT = await Auth.getIdToken()
    idToken.value = idT
    initialized.value = true
  },
}
