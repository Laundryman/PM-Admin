// import { useAuthStore } from '@/stores/auth'
import type { ProductFilter } from '@/models/Products/productFilter.model'
import type { searchProductInfo } from '@/models/Products/searchProductInfo.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'

await msal.initialize()

const token = ref()
const initialized = ref(false)
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/products',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token.value}`,
  },
})

export default {
  async searchProducts(filter: ProductFilter): Promise<searchProductInfo[]> {
    // if (initialized.value !== false) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/searchProducts', filter)
    return response.data
    // } else {
    //   throw new Error('PartService not initialized')
    // }
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
