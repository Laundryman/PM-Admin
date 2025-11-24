import axios from 'axios'

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/roles',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json'
  }
})

export default {
  getRoles(searchText?: string, top?: number) {
    return apiClient.get('/getall', {
      params: {
        searchText: searchText,
        top: top
      }
    })
  }
}
