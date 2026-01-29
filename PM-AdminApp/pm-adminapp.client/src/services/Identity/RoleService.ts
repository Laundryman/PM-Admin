import { Role } from '@/models/Identity/role.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'
const token = ref()
const initialized = ref(false)

await msal.initialize()
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/roles',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
})

export default {
  async getRoles(searchText?: string, top?: number): Promise<Role[]> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    const response = await apiClient.get('/getRoles', {
      params: {
        searchText: searchText,
        top: top,
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
    console.log('RoleService initialized with token:', token.value)
    initialized.value = true
  },
}
