import { GetMenuParams } from '@/models/Planner/GetMenuParams.model'
import { GetPlanogramParams } from '@/models/Planner/GetPlanogramParams.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'

await msal.initialize()

const token = ref()
const initialized = ref(false)
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/planner/cluster',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token.value}`,
  },
})

export default {
  async loadStandData(standId: number) {
    let params: GetPlanogramParams = new GetPlanogramParams()
    params.standId = standId

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getstand', params)
    return response.data
  },

  // Get the data to create the menu
  async loadClusterMenuCategories(clusterId: number) {
    let params: GetMenuParams = new GetMenuParams()
    params.clusterId = clusterId

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getmenucategories', params)
    return response.data
  },

  // Get the data to create the menu
  async loadClusterMenuData(clusterId: number) {
    try {
      let params: GetMenuParams = new GetMenuParams()
      params.clusterId = clusterId

      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      }
      let response = await apiClient.post('/getmenu', params)
      return response.data
    } catch (error) {
      console.error('Error loading cluster menu data:', error)
      throw error
    }
  },

  // Get the data to create the menu for a specific category
  async loadClusterCategoryMenuData(clusterId: number, category: string) {
    let params: GetMenuParams = new GetMenuParams()

    params.clusterId = clusterId
    params.category = category

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/get-category-menu', params)
    return response.data
  },

  async loadClusterData(clusterId: number) {
    let params: GetMenuParams = new GetMenuParams()
    params.clusterId = clusterId

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getcluster', params)
    return response.data
  },

  // Get the data to create the menu
  async loadClusterShelves(clusterId: number) {
    let params: GetMenuParams = new GetMenuParams()
    params.clusterId = clusterId

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getshelves', params)
    return response.data
  },

  // Get the data to create the menu
  async loadClusterParts(clusterId: number) {
    let params: GetMenuParams = new GetMenuParams()
    params.clusterId = clusterId

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getparts', params)
    return response.data
  },

  async initialise() {
    const authStore = useAuthStore()
    if (!authStore.initialized) {
      await authStore.initialize()
    }
    const t = await Auth.getToken()
    token.value = t
    console.log('ClusterService initialized with token:', token.value)
    initialized.value = true
  },
}
