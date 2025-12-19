// import { useAuthStore } from '@/stores/auth'
import { categoryFilter } from '@/models/Categories/categoryFilter.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'

await msal.initialize()

const token = ref()
const initialized = ref(false)
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/categories',
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

  async getAllCategories() {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      let catFilter = new categoryFilter()
      // catFilter.GetParents = true
      return apiClient.post('/getCategories', catFilter)
    } else {
      throw new Error('CategoryService not initialized')
    }
  },
  async getParentCategories() {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      let catFilter = new categoryFilter()
      catFilter.GetParents = true
      return apiClient.post('/getCategories', catFilter)
    } else {
      throw new Error('CategoryService not initialized')
    }
  },

  async getChildCategories(parentId: number) {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      let catFilter = new categoryFilter()
      catFilter.ParentCatId = parentId
      return apiClient.post('/getCategories', catFilter)
    } else {
      throw new Error('CategoryService not initialized')
    }
  },

  async getCategory(id: number) {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      return apiClient.get('/getCategory', {
        params: {
          id: id,
        },
      })
    } else {
      throw new Error('CategoryService not initialized')
    }
  },

  async updateCategory(formData: FormData): Promise<any> {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      apiClient.defaults.headers['Content-Type'] = 'multipart/form-data'
      await apiClient
        .post('/updateCategory', formData)
        .then((response) => {
          return response.data
        })
        .catch((error) => {
          throw error
        })
    } else {
      throw new Error('CategoryService not initialized')
    }
  },

  async initialise() {
    const authStore = useAuthStore()
    if (!authStore.initialized) {
      await authStore.initialize()
    }
    const t = await Auth.getToken()
    token.value = t
    console.log('CategoryService initialized with token:', token.value)
    initialized.value = true
  },
}
