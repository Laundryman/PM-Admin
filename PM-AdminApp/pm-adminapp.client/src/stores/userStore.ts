import { User } from '@/models/Identity/user.model'
import userService from '@/services/Identity/UserService'
import { useLocalStorage } from '@vueuse/core'
import { acceptHMRUpdate, defineStore, skipHydrate } from 'pinia'

import { ref } from 'vue'

export const useUserStore = defineStore('userStore', () => {
  const selectedUser = ref<User>(new User())
  const userLoaded = ref<boolean>(false)
  const loadingUsers = ref<boolean>(true)
  const activeUser = useLocalStorage('activeUser', {} as User)

  async function loadUser(id: string) {
    if (!userService.isInitialised) await userService.initialise()
    console.log('Fetching users...')
    await userService
      .getUser(id)
      .then((data) => {
        // create brandOptions array for select dropdown
        selectedUser.value = data
        loadingUsers.value = false
        userLoaded.value = true
      })
      .catch((error) => {
        console.log('Error fetching user:', error)
      })
    console.log('Completed fetching user')
  }

  return {
    selectedUser,
    userLoaded,
    loadingUsers,
    activeUser: skipHydrate(activeUser),
    loadUser,
  }
})

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useUserStore, import.meta.hot))
}
