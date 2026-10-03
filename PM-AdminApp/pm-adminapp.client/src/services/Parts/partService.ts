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
    apiClient.defaults.headers['Content-Type'] = 'application/json'
    let response = await apiClient
      .post('/searchParts', filter)
      .then((resp) => {
        return resp.data
      })
      .catch((error) => {
        throw error
      })
    return response
    // } else {
    //   throw new Error('PartService not initialized')
    // }
  },

  async getPart(partId: number): Promise<Part> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    apiClient.defaults.headers['Content-Type'] = 'application/json'

    let response = await apiClient.get('/getPart', { params: { id: partId } })
    return response.data
  },

  async savePart(part: FormData) {
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
    return response
  },

  async createPart(part: FormData) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    apiClient.defaults.headers['Content-Type'] = 'multipart/form-data'
    let response = await apiClient
      .post('/createPart', part)
      .then((resp) => {
        return resp.data
      })
      .catch((error) => {
        throw error
      })
    return response
  },

  async deletePart(partId: number): Promise<Part> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    apiClient.defaults.headers['Content-Type'] = 'application/json'

    let response = await apiClient
      .delete('/deletePart', {
        params: {
          id: partId,
        },
      })
      .then((response) => {
        return response.data
      })
      .catch((err) => {
        console.log('Error deleting part:', err)
        throw err
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
    console.log('PartService initialized with token:', token.value)
    initialized.value = true
  },
}
