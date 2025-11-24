<<<<<<< HEAD
import type { Country } from '@/models/Countries/country.model'
import type { Region } from '@/models/Countries/region.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'
const token = ref()
const initialized = ref(false)

await msal.initialize()
=======
import axios from 'axios'
>>>>>>> 02efb5c (working signin and api authorize)

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/countries',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
<<<<<<< HEAD
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
  async getRegions(brandId: number, searchText?: string): Promise<Region[]> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.get('/getAllRegions', {
      params: {
        brandId: brandId,
        searchText: searchText,
      },
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
=======
    'Content-Type': 'application/json'
  }
})

export default {
  getCountries(searchText?: string, top?: number) {
    return apiClient.get('/getall', {
      params: {
        searchText: searchText,
        top: top
      }
    })
  }
>>>>>>> 02efb5c (working signin and api authorize)
}
