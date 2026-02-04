// import { useAuthStore } from '@/stores/auth'
import type { Part } from '@/models/Parts/part.model'
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

export const partService = {
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

  async getPart(partId: number): Promise<Part> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.get('/getPart', { params: { id: partId } })
    return response.data
  },

  async savePart(part: FormData): Promise<Part> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    apiClient.defaults.headers['Content-Type'] = 'multipart/form-data'

    let response = await apiClient
      .post('/savePart', part)
      .then((resp) => {
        return resp.data
      })
      .catch((error) => {
        throw error
      })
    return response.data
  },

  async createPart(part: FormData): Promise<Part> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    apiClient.defaults.headers['Content-Type'] = 'multipart/form-data'
    let response = await apiClient
      .post('/createPart', part)
      .then((resp) => {
        return resp
      })
      .catch((error) => {
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
    console.log('PartService initialized with token:', token.value)
    initialized.value = true
  },
}
