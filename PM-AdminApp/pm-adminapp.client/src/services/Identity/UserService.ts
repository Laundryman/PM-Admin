import User from '@/models/Identity/user.model'
import axios from 'axios'

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/users',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
})

export default {
  getUsers(searchText?: string, top?: number) {
    return apiClient.get('/get-users', {
      params: {
        searchText: searchText,
        top: top,
      },
    })
  },

  saveUser(user: User) {
    return apiClient.put('/update', user)
  },

  createUser(user: User) {
    return apiClient.put('/create', user)
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
}
