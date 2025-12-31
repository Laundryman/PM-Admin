import type { Country } from '@/models/Countries/country.model'
import type { Region } from '@/models/Countries/region.model'
import type { regionFilter } from '@/models/Countries/regionFilter.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'
const token = ref()
const initialized = ref(false)

await msal.initialize()

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/countries',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
})

export default {
  async getCountries(regionId?: number, searchText?: string): Promise<Country[]> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.get('/getallCountries', {
      params: {
        regionId: regionId,
        searchText: searchText,
      },
    })
    return response.data
  },
  async getRegions(filter: regionFilter): Promise<Region[]> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getRegions', filter)
    return response.data
  },

  async updateRegion(formData: FormData): Promise<Region> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.put('/updateRegion', formData)
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
