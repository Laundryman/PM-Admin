// import { useAuthStore } from '@/stores/auth'
import type { Job } from '@/models/Jobs/Job.model'
import type { JobFolder } from '@/models/Jobs/JobFolder.model'
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
      let response = await apiClient
        .post('/searchJobFolders', filter)
        .then((response) => {
          return response
        })
        .catch((error) => {
          throw error
        })
      return response
    } else {
      throw new Error('JobsService not initialized')
    }
  },

  async getJobFolder(id: number) {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      let response = await apiClient.get('/getJobFolder', {
        params: {
          id: id,
        },
      })
    } else {
      throw new Error('JobsService not initialized')
    }
  },

  async updateJobFolder(jobFolder: JobFolder): Promise<any> {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      apiClient.defaults.headers['Content-Type'] = 'application/json'
      let response = await apiClient
        .post('/saveJobFolder', jobFolder)
        .then((response) => {
          return response
        })
        .catch((error) => {
          throw error
        })
      return response.data
    } else {
      throw new Error('JobService not initialized')
    }
  },

  async createJobFolder(jobFolder: JobFolder): Promise<any> {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      apiClient.defaults.headers['Content-Type'] = 'application/json'
      await apiClient
        .post('/createJobFolder', jobFolder)
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

  async updateJob(job: Job): Promise<any> {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      apiClient.defaults.headers['Content-Type'] = 'application/json'
      await apiClient
        .post('/saveJob', job)
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

  async createJob(job: Job): Promise<any> {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      apiClient.defaults.headers['Content-Type'] = 'application/json'
      let response = await apiClient
        .post('/createJob', job)
        .then((response) => {
          return response.data
        })
        .catch((error) => {
          throw error
        })
      return response
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
