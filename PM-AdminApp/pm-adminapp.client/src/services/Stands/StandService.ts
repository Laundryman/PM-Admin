// import { useAuthStore } from '@/stores/auth'
import type { searchStandInfo } from '@/models/Stands/searchStandInfo.model'
import type { Stand } from '@/models/Stands/stand.model'
import type { StandFilter } from '@/models/Stands/standFilter.model'
import type { StandType } from '@/models/StandTypes/standType.model'
import type { standTypeFilter } from '@/models/StandTypes/standTypeFilter.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'

await msal.initialize()

const token = ref()
const idToken = ref()
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
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }
    let response = await apiClient.post('/searchStands', filter)
    return response.data
    // } else {
    //   throw new Error('PartService not initialized')
    // }
  },

  async getStands(filter: StandFilter): Promise<Stand[]> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }
    let response = await apiClient
      .post('/searchStands', filter)
      .then((response) => {
        return response
      })
      .catch((err) => {
        console.error('Error fetching stand:', err)
        throw err
      })
    return response.data
  },

  async getAllStandTypes(filter: standTypeFilter): Promise<StandType[]> {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      apiClient.defaults.headers['Content-Type'] = 'application/json'

      // let catFilter = new standTypeFilter()
      // catFilter.GetParents = true
      let response = await apiClient.post('/getStandTypes', filter)
      return response.data
    } else {
      throw new Error('StandTypeService not initialized')
    }
  },

  async getStand(standId: number): Promise<Stand> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient
      .get('/getStand', { params: { id: standId } })
      .then((response) => {
        return response
      })
      .catch((err) => {
        console.error('Error fetching stand:', err)
        throw err
      })
    return response.data
  },

  async saveStand(standData: Stand): Promise<Stand> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient
      .post('/saveStand', standData)
      .then((response) => {
        return response
      })
      .catch((err) => {
        console.error('Error saving stand:', err)
        throw err
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
    const idT = await Auth.getIdToken()
    idToken.value = idT
    initialized.value = true
  },
}
