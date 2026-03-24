// import { useAuthStore } from '@/stores/auth'
import { standTypeFilter } from '@/models/StandTypes/standTypeFilter.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'

await msal.initialize()

const token = ref()
const initialized = ref(false)
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/standTypes',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token.value}`,
  },
})

export default {
  get isInitialised() {
    return initialized.value
  },

  async getAllStandTypes(filter: standTypeFilter) {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      apiClient.defaults.headers['Content-Type'] = 'application/json'

      // let catFilter = new standTypeFilter()
      // catFilter.GetParents = true
      return apiClient.post('/getStandTypes', filter)
    } else {
      throw new Error('StandTypeService not initialized')
    }
  },
  async getParentStandTypes(filter: standTypeFilter) {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      apiClient.defaults.headers['Content-Type'] = 'application/json'

      filter.getParents = true
      return apiClient.post('/getStandTypes', filter)
    } else {
      throw new Error('StandTypeService not initialized')
    }
  },

  async getChildStandTypes(parentId: number) {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      let catFilter = new standTypeFilter()
      catFilter.parentStandTypeId = parentId
      return apiClient.post('/getStandTypes', catFilter)
    } else {
      throw new Error('StandTypeService not initialized')
    }
  },

  async getStandType(id: number) {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      apiClient.defaults.headers['Content-Type'] = 'application/json'
      return apiClient.get('/getStandType', {
        params: {
          id: id,
        },
      })
    } else {
      throw new Error('StandTypeService not initialized')
    }
  },

  async addStandType(formData: FormData): Promise<any> {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      apiClient.defaults.headers['Content-Type'] = 'multipart/form-data'
      var response = await apiClient
        .post('/addStandType', formData)
        .then((response) => {
          return response.data
        })
        .catch((error) => {
          throw error
        })
      return response
    } else {
      throw new Error('StandTypeService not initialized')
    }
  },

  async updateStandType(formData: FormData): Promise<any> {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      apiClient.defaults.headers['Content-Type'] = 'multipart/form-data'
      var response = await apiClient
        .post('/updateStandType', formData)
        .then((response) => {
          return response.data
        })
        .catch((error) => {
          throw error
        })
      return response
    } else {
      throw new Error('StandTypeService not initialized')
    }
  },

  async initialise() {
    const authStore = useAuthStore()
    if (!authStore.initialized) {
      await authStore.initialize()
    }
    const t = await Auth.getToken()
    token.value = t
    console.log('StandTypeService initialized with token:', token.value)
    initialized.value = true
  },
}
