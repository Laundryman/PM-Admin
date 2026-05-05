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
  get isInitialised() {
    return initialized.value
  },
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
            'identities,id,displayname,userName, givenName,surname,mail,mailNickname,userPrincipalName,country,' +
            'extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId,extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands,' +
            'extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress,extension_ff5105e3fc0248fbad7979cfe9b62e1a_RegionList,extension_ff5105e3fc0248fbad7979cfe9b62e1a_CountryList,' +
            'extension_ff5105e3fc0248fbad7979cfe9b62e1a_RoleId, extension_ff5105e3fc0248fbad7979cfe9b62e1a_Shopper, extension_ff5105e3fc0248fbad7979cfe9b62e1a_OrderManager',
        },
      })
      .then((response) => {
        let user = response.data

        //this.updateExtensionFields(user)
        let mappedUser = this.mapUser(user)
        return mappedUser
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
            'extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId,extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands, ' +
            'extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress,extension_ff5105e3fc0248fbad7979cfe9b62e1a_RegionList, extension_ff5105e3fc0248fbad7979cfe9b62e1a_CountryList,' +
            'extension_ff5105e3fc0248fbad7979cfe9b62e1a_RoleId, extension_ff5105e3fc0248fbad7979cfe9b62e1a_Shopper, extension_ff5105e3fc0248fbad7979cfe9b62e1a_OrderManager',
          // $filter: "creationType eq 'LocalAccount'",
          $top: '100',
          // $orderBy: 'displayName',
        },
      })
      .then((response) => {
        console.log('Graph list users response:', response.data)
        const users = response.data.value
        users.forEach((user: any) => {
          this.updateExtensionFields(user)
          if (user.identities) {
            let usernameId = user.identities.find(
              (identity: any) => identity.signInType === 'userName',
            )
            if (usernameId) {
              user.userName = usernameId.issuerAssignedId
            } else {
              let usernameId = user.identities.find(
                (identity: any) => identity.signInType === 'emailAddress',
              )
              if (usernameId) {
                user.userName = usernameId.issuerAssignedId
              }
            }
            if (!user.userName) {
              user.userName = user.mailNickname || user.mail || user.userPrincipalName || 'Unknown'
            }
          }
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
      DisplayName: user.userName,
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
    updatedUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_CountryList'] = user.countryList
    updatedUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RegionList'] = user.regionList
    updatedUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_Shopper'] = user.shopper
    updatedUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_OrderManager'] = user.orderManager
    if (user.roleId != null) {
      updatedUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RoleId'] = user.roleId
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
    let newUser = new Object() as any
    let mailNickname = user.userEmailAddress.substring(0, user.userEmailAddress.indexOf('@'))
    newUser = {
      accountEnabled: true,
      displayName: user.userName,
      mailNickname: mailNickname,
      givenName: user.givenName,
      surname: user.surname,
      userPrincipalName: mailNickname + '@' + import.meta.env.VITE_APP_TENANT_NAME,
      identities: [
        {
          signInType: 'emailAddress',
          issuer: 'planmatr.onmicrosoft.com',
          issuerAssignedId: user.userEmailAddress,
        },
        {
          signInType: 'userName',
          issuer: 'planmatr.onmicrosoft.com',
          issuerAssignedId: user.userName,
        },
      ],
      mail: user.userEmailAddress,
      passwordProfile: {
        password: user.password,
        forceChangePasswordNextSignIn: false,
      },
      passwordPolicies: 'DisablePasswordExpiration',
    }
    if (user.brandIds != null && user.brandIds.length > 0) {
      newUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands'] = user.brandIds.join(',')
    } else if (user.brandIds != null && user.brandIds.length == 1) {
      newUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands'] = user.brandIds
    }

    newUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId'] = user.diamCountryId
    newUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_CountryList'] = user.countryList
    newUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RegionList'] = user.regionList

    if (user.roleId != null) {
      newUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RoleId'] = user.roleId
    }
    newUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress'] = user.userEmailAddress

    var resp = await apiClient
      .post('/users', newUser)
      .then((response) => {
        return response
      })
      .catch((error) => {
        throw error
      })
    return resp
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
    // if (user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamRoles']) {
    //   user.roleIds = user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamRoles']
    //     .split(',')
    //     .map((id: any) => parseInt(id))
    // } else {
    //   user.roleIds = []
    // }
    if (user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RoleId']) {
      user.roleId = user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RoleId']
    } else {
      user.roleId = 0
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
    if (user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_CountryList']) {
      user.countryList = user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_CountryList']
    } else {
      user.countryList = ''
    }
    if (user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RegionList']) {
      user.regionList = user['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RegionList']
    } else {
      user.regionList = ''
    }
  },

  mapUser(graphUser: any): User {
    var user = new User()
    user.country = graphUser.country
    user.displayName = graphUser.displayName
    user.givenName = graphUser.givenName
    user.surname = graphUser.surname
    user.mail = graphUser.mail
    user.mailNickName = graphUser.mailNickname
    user.userName = graphUser.userPrincipalName
    user.id = graphUser.id
    user.countries = []
    user.regions = []
    user.password = graphUser.password || ''
    user.roles = []
    user.brandIds = []

    if (graphUser.identities) {
      let usernameId = graphUser.identities.find(
        (identity: any) => identity.signInType === 'userName',
      )
      if (usernameId) {
        user.userName = usernameId.issuerAssignedId
      }
    }

    if (graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RoleId']) {
      user.extension_ff5105e3fc0248fbad7979cfe9b62e1a_RoleId =
        graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RoleId']
      user.roleId = graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RoleId'].map((id: any) =>
        parseInt(id),
      )
    } else {
      user.roleId = 0
    }
    if (graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands']) {
      user.extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands =
        graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands']
      user.brandIds = graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_Brands']
        .split(',')
        .map((id: any) => parseInt(id))
    } else {
      user.brandIds = []
    }

    if (graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress']) {
      user.extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress =
        graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress']
      user.userEmailAddress =
        graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_UserEmailAddress']
    } else {
      user.userEmailAddress = ''
    }
    if (graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_CountryList']) {
      user.extension_ff5105e3fc0248fbad7979cfe9b62e1a_CountryList =
        graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_CountryList']
      user.countryList = graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_CountryList']
    } else {
      user.countryList = ''
    }
    if (graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RegionList']) {
      user.extension_ff5105e3fc0248fbad7979cfe9b62e1a_RegionList =
        graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RegionList']
      user.regionList = graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_RegionList']
    } else {
      user.regionList = ''
    }

    if (graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_Shopper'] !== undefined) {
      user.extension_ff5105e3fc0248fbad7979cfe9b62e1a_Shopper =
        graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_Shopper']
      user.shopper = graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_Shopper']
    } else {
      user.shopper = false
    }
    if (graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_OrderManager'] !== undefined) {
      user.extension_ff5105e3fc0248fbad7979cfe9b62e1a_OrderManager =
        graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_OrderManager']
      user.orderManager = graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_OrderManager']
    } else {
      user.orderManager = false
    }

    //Deprecated fields mapping
    if (graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamRoles']) {
      user.extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamRoles =
        graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamRoles']
      user.roleIds = graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamRoles']
        .split(',')
        .map((id: any) => parseInt(id))
    } else {
      user.roleIds = []
    }
    if (graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId']) {
      user.extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId =
        graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId']
      user.diamCountryId = graphUser['extension_ff5105e3fc0248fbad7979cfe9b62e1a_DiamCountryId']
    } else {
      user.diamCountryId = 0
    }

    return user
  },
}
