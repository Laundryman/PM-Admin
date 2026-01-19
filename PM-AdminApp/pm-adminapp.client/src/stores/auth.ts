import type { MaybeAccount } from '@/services/Identity/auth'
import { Auth } from '@/services/Identity/auth'
import userService from '@/services/Identity/UserService'
import type { NavigationClient } from '@azure/msal-browser'
import type { User } from '@microsoft/microsoft-graph-types-beta'
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useRouter } from 'vue-router'

export const initialized = ref(false)
export const account = ref<MaybeAccount>(null)
export const error = ref<string>()
export const userInfo = ref<User | null>(null)

export const useAuthStore = defineStore('auth', () => {
  const error = ref<string>()
  const account = ref<MaybeAccount>(null)
  const initialized = ref(false)
  const userInfo = ref<User | null>(null)
  const router = useRouter()
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
        await userService.initialise()
        await userService.getCurrentUserInfo()
        initialized.value = true
        error.value = ''
        router.push({ name: 'home' })
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

  async function setCurrentlyLoggedInUserInfo(data: User | null) {
    userInfo.value = data
  }

  async function GetToken(): Promise<string> {
    return Auth.getToken()
  }

  async function GetGraphToken(): Promise<string> {
    return Auth.getGraphToken()
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
