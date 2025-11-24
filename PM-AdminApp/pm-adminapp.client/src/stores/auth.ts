import type { MaybeAccount } from '@/services/Identity/auth'
import { Auth } from '@/services/Identity/auth'
import type { NavigationClient } from '@azure/msal-browser'
import { defineStore } from 'pinia'
import { ref } from 'vue'

export const initialized = ref(false)
export const account = ref<MaybeAccount>(null)
export const error = ref<string>()

export const useAuthStore = defineStore('auth', () => {
  const error = ref<string>()
  const account = ref<MaybeAccount>(null)
  const initialized = ref(false)

  async function initialize(client?: NavigationClient) {
    if (initialized.value === true) {
      return account.value
    }
    return Auth.initialize(client).then((data) => {
      account.value = data
      initialized.value = true
      return data
    })
  }

  async function login() {
    error.value = ''
    return Auth.login()
      .then((data) => {
        account.value = data
        initialized.value = true
        error.value = ''
      })
      .catch((err) => {
        error.value = err.message
        throw err
      })
    // await Auth.login()
  }

  async function logout() {
    return Auth.logout().then(() => {
      account.value = null
    })
  }

  async function GetToken(): Promise<string> {
    return Auth.getToken()
  }
  return { initialized, account, error, initialize, login, logout, GetToken }
})
