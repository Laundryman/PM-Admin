import type { AuditLog } from '@/models/Reporting/AuditLog.model'
import { ReportingFilter } from '@/models/Reporting/reportingFilter.model'
import { Auth, msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'

await msal.initialize()

const token = ref()
const idToken = ref()
const initialized = ref(false)
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_ROOT + '/api/reporting',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token.value}`,
    ClaimsAuth: idToken.value || '',
  },
})

export default {
  async getUserActionsReport(filter: ReportingFilter): Promise<AuditLog[]> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }
    const response = await apiClient
      .post('/getUserActionsReport', filter)
      .then((response) => {
        return response.data
      })
      .catch((error) => {
        throw error
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
