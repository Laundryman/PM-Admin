// import { useAuthStore } from '@/stores/auth'
import type { JobFolderFilter } from '@/models/Jobs/JobFolderFilter.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'

await msal.initialize()

const token = ref()
const initialized = ref(false)
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/jobs',
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
  async searchJobFolders(filter: JobFolderFilter) {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      await apiClient.post('/searchJobFolders', filter).then((response) => {
        return response.data
      })
    } else {
      throw new Error('JobsService not initialized')
    }
  },

  async getJobFolder(id: number) {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      return apiClient.get('/getJobFolder', {
        params: {
          id: id,
        },
      })
    } else {
      throw new Error('JobsService not initialized')
    }
  },

  async updateJobFolder(formData: FormData): Promise<any> {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      apiClient.defaults.headers['Content-Type'] = 'multipart/form-data'
      await apiClient
        .post('/updateJobFolder', formData)
        .then((response) => {
          return response.data
        })
        .catch((error) => {
          throw error
        })
    } else {
      throw new Error('JobService not initialized')
    }
  },

  async initialise() {
    const authStore = useAuthStore()
    if (!authStore.initialized) {
      await authStore.initialize()
    }
    const t = await Auth.getToken()
    token.value = t
    console.log('BrandService initialized with token:', token.value)
    initialized.value = true
  },
}
