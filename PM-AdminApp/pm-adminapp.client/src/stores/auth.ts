import type { MaybeAccount } from '@/services/Identity/auth'
import { Auth } from '@/services/Identity/auth'
import { Graph } from '@/services/Identity/graph'
import userService from '@/services/Identity/UserService'
import type { NavigationClient } from '@azure/msal-browser'
import { defineStore } from 'pinia'
import { ref } from 'vue'

export const initialized = ref(false)
export const account = ref<MaybeAccount>(null)
export const error = ref<string>()
export const userInfo = ref<any>(null)

export const useAuthStore = defineStore('auth', () => {
  const error = ref<string>()
  const account = ref<MaybeAccount>(null)
  const initialized = ref(false)
  const userInfo = ref<any>(null)
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
      .then(async (data) => {
        account.value = data
        userInfo.value = await userService.getCurrentUserInfo()
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

  async function setCurrentlyLoggedInUserInfo() {
    await userService.initialise()
    userInfo.value = await userService.getCurrentUserInfo()
  }

  async function GetToken(): Promise<string> {
    return Auth.getToken()
  }

  async function GetGraphToken(): Promise<string> {
    return Graph.getGraphToken()
  }
  return {
    initialized,
    account,
    userInfo,
    error,
    initialize,
    login,
    logout,
    GetToken,
    GetGraphToken,
    setCurrentlyLoggedInUserInfo,
  }
})
