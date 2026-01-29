import { User } from '@/models/Identity/user.model'
import { Auth, msal } from '@/services/Identity/auth'

import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'
const token = ref()
const initialized = ref(false)
const authStore = ref()

await msal.initialize()

const apiClient = axios.create({
  // baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/graph',
  baseURL: 'https://graph.microsoft.com/v1.0',
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
})

export default {
  async getCurrentUserInfo() {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    await apiClient
      .get('/me', {})
      .then((response) => {
        console.log('Graph user info response:', response.data)
        authStore.value.setCurrentlyLoggedInUserInfo(response.data)
      })
      .catch((error) => {
        throw error
      })
  },

  async getUser(id: string): Promise<User> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    return await apiClient
      .get('/users/' + id, {
        params: {
          $select:
            'identities,id,displayname,userName, givenName,surname,mail,mailNickname,userPrincipalName, ' +
            'extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamRoles, extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId,extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands, extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress',
        },
      })
      .then((response) => {
        let user = response.data
        this.updateExtensionFields(user)
        return user
      })
      .catch((error) => {
        throw error
      })
  },

  async getPMUsers(): Promise<User[]> {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    return await apiClient
      .get('/users', {
        params: {
          $select:
            'identities,id,displayname,userName, givenName,surname,mail,mailNickname,userPrincipalName, ' +
            'extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamRoles, extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId,extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands, extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress',
          $filter: "creationType eq 'LocalAccount'",
          $top: '100',
          // $orderBy: 'displayName',
        },
      })
      .then((response) => {
        console.log('Graph list users response:', response.data)
        const users = response.data.value
        users.forEach((user: any) => {
          this.updateExtensionFields(user)
          // if (user.identities) {
          //   let usernameId = user.identities.find(
          //     (identity: any) => identity.signInType === 'userName',
          //   )
          //   if (usernameId) {
          //     user.userName = usernameId.issuerAssignedId
          //   }
          // }
          // if (user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamRoles']) {
          //   user.roleIds = user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamRoles']
          //     .split(',')
          //     .map((id: any) => parseInt(id))
          // } else {
          //   user.roleIds = []
          // }
          // if (user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands']) {
          //   user.brandIds = user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands']
          //     .split(',')
          //     .map((id: any) => parseInt(id))
          // } else {
          //   user.brandIds = []
          // }
          // if (user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId']) {
          //   user.diamCountryId = user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId']
          // }
          // if (user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress']) {
          //   user.userEmailAddress =
          //     user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress']
          // }
        })
        return users
      })
      .catch((error) => {
        throw error
      })
  },

  async saveUser(user: User) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['Content-Type'] = 'application/json'
      apiClient.defaults.headers['prefer'] = 'return=representation'
    }
    let updatedUser = new Object() as any
    updatedUser = {
      Id: user.id,
      DisplayName: user.displayName,
      GivenName: user.givenName,
      Surname: user.surname,
      // MailNickname: user.mailNickName,
      AccountEnabled: true,
    }
    if (user.brandIds != null && user.brandIds.length > 0) {
      updatedUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands'] = user.brandIds.join(',')
    } else if (user.brandIds != null && user.brandIds.length == 1) {
      updatedUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands'] = user.brandIds
    }

    updatedUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId'] = user.diamCountryId

    if (user.roleIds != null && user.roleIds.length > 0) {
      updatedUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamRoles'] = user.roleIds.join(',')
    } else if (user.roleIds != null && user.roleIds.length == 1) {
      updatedUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamRoles'] = user.roleIds
    }
    updatedUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress'] =
      user.userEmailAddress

    await apiClient
      .patch('/users/' + user.id, updatedUser)
      .then((response) => {
        return response
      })
      .catch((error) => {
        throw error
      })
  },

  async createUser(user: User) {
    return await apiClient.put('/users', user)
  },

  deleteUser(id: string) {
    return apiClient.delete('/delete-user', {
      params: {
        id: id,
      },
    })
  },
  changePassword(user: User) {
    return apiClient.put('/change-password', user)
  },

  async initialise() {
    authStore.value = useAuthStore()
    if (!authStore.value.initialized) {
      await authStore.value.initialize()
    }
    const t = await Auth.getGraphToken()
    token.value = t
    console.log('UserService initialized with token:', token.value)
    initialized.value = true
  },

  updateExtensionFields(user: any) {
    if (user.identities) {
      let usernameId = user.identities.find((identity: any) => identity.signInType === 'userName')
      if (usernameId) {
        user.userName = usernameId.issuerAssignedId
      }
    }
    if (user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamRoles']) {
      user.roleIds = user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamRoles']
        .split(',')
        .map((id: any) => parseInt(id))
    } else {
      user.roleIds = []
    }
    if (user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands']) {
      user.brandIds = user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands']
        .split(',')
        .map((id: any) => parseInt(id))
    } else {
      user.brandIds = []
    }
    if (user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId']) {
      user.diamCountryId = user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId']
    }
    if (user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress']) {
      user.userEmailAddress = user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress']
    }
  },
}
