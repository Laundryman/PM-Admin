// import { useAuthStore } from '@/stores/auth'
import { Cluster } from '@/models/Clusters/cluster.model'
import type { ClusterFilter } from '@/models/Clusters/clusterFilter.model'
import { SaveLayoutDto } from '@/models/Clusters/saveLayout.model'
import type { searchClusterInfo } from '@/models/Clusters/searchClusterInfo.model'
import type { CreateLayoutFilter } from '@/models/Layout/createLayoutFilter.model'
import type { Stand } from '@/models/Stands/stand.model'
import type { StandFilter } from '@/models/Stands/standFilter.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'

await msal.initialize()

const token = ref()
const idToken = ref()
const initialized = ref(false)
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/clusters',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token.value}`,
  },
})

export default {
  async searchClusters(filter: ClusterFilter): Promise<searchClusterInfo[]> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }
    let response = await apiClient.post('/searchClusters', filter)
    return response.data
  },

  async getCluster(clusterId: number): Promise<Cluster> {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
        apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
      }
      return apiClient
        .get('/getCluster', {
          params: {
            id: clusterId,
          },
        })
        .then((response) => response.data)
    } else {
      throw new Error('ClusterService not initialized')
    }
  },

  async createLayout(filter: CreateLayoutFilter): Promise<number> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }
    let response = await apiClient
      .post('/create/createLayout', filter)
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response
  },

  async saveLayout(layoutData: SaveLayoutDto) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }
    let response = await apiClient
      .post('/saveLayoutDetails', layoutData)
      .then((res) => {
        return res.status
      })
      .catch((error) => {
        throw error
      })
    return response
  },

  async getStands(filter: StandFilter): Promise<Stand[]> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }
    let response = await apiClient
      .post('/create/getStands', filter)
      .then((response) => {
        return response.data
      })
      .catch((err) => {
        console.log('Error fetching stand:', err)
        throw err
      })
    return response
  },

  async deleteCluster(clusterId: number): Promise<any> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }
    let response = await apiClient
      .delete('/deleteCluster', {
        params: {
          id: clusterId,
        },
      })
      .then((response) => {
        return response.data
      })
      .catch((err) => {
        console.log('Error deleting cluster:', err)
        throw err
      })

    return response
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
