import { Part } from '@/models/Parts/part.model'
import { GetMenuParams } from '@/models/Planner/GetMenuParams.model'
import { GetPlanogramParams } from '@/models/Planner/GetPlanogramParams.model'
import { msal } from '@/services/Identity/auth'
import axios from 'axios'
import { ref } from 'vue'

await msal.initialize()

const token = ref()
const initialized = ref(false)
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_APP_SERVER_URL + '/api/planner/planogram',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token.value}`,
  },
})

export default {
  async getPlanoComCount(planogramId: number) {
    let params: GetMenuParams = new GetMenuParams()

    params.brandId = 0
    params.standTypeId = 0
    params.countryId = 0
    params.planogramId = planogramId
    params.category = ''

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getPlanoComCount', params)
    return response.data
  },

  // Get the data to create the menu
  async loadMenuCategories(planogramId: number) {
    let params: GetMenuParams = new GetMenuParams()

    params.brandId = 0
    params.standTypeId = 0
    params.countryId = 0
    params.planogramId = planogramId
    params.category = ''

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getMenuCategories', params)
    return response.data
  },

  // Get the data to create the menu
  async loadMenuData(planogramId: number) {
    let params: GetMenuParams = new GetMenuParams()

    params.brandId = 0
    params.standTypeId = 0
    params.countryId = 0
    params.planogramId = planogramId
    params.category = ''

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getMenu', params)
    return response.data
  },

  // Get the data to create the menu for a specific category
  async loadCategoryMenuData(planogramId: number, category: string) {
    //ajax call to get json goes here
    // var self = this;
    // var menu = new Menu;

    let params: GetMenuParams = new GetMenuParams()

    params.brandId = 0
    params.standTypeId = 0
    params.countryId = 0
    params.planogramId = planogramId
    params.category = category

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getCategoryMenu', params)
    return response.data
  },

  // Get the data to create the menu
  async loadPlanogramShelves(planogramId: number) {
    let params: GetPlanogramParams = new GetPlanogramParams()

    params.planogramId = planogramId

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getPlanogramShelves', params)
    return response.data
    // return $.ajax({
    //     type: "GET",
    //     url: API_DOMAIN + '/api/planxapi/getPlanogramShelves?planogramId=' + planogramId,
    //     data: JSON.stringify(params),
    //     contentType: "application/json"
    //   }).done(data => data)
    //   .fail(function (jqXHR, textStatus, error) {
    //     console.log("GetShelves error: " + error);
    //   });
  },

  // Get the data to create the menu
  async loadPlanogramParts(planogramId: number) {
    let params: GetPlanogramParams = new GetPlanogramParams()

    params.planogramId = planogramId

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getPlanogramParts', params)
    return response.data
    // return $.ajax({
    //     type: "GET",
    //     url: API_DOMAIN + '/api/planxapi/getPlanogramParts?planogramId=' + planogramId,
    //     //data: JSON.stringify(params),
    //     contentType: "application/json"
    //   }).done(data => data)
    //   .fail(data => data);
  },

  // Get the only newly added parts data
  async loadNewPlanogramParts(planogramId: number) {
    let params: GetPlanogramParams = new GetPlanogramParams()

    params.planogramId = planogramId

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getNewPlanogramParts', params)
    return response.data
    // return $.ajax({
    //   type: "GET",
    //   url: API_DOMAIN + '/api/planxapi/getNewPlanogramParts?planogramId=' + planogramId,
    //   //data: JSON.stringify(params),
    //   contentType: "application/json"
    // }).done(data => data)
    //   .fail(data => data);
  },

  async loadStandData(standId: number) {
    let params: GetPlanogramParams = new GetPlanogramParams()
    params.standId = standId

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getstand', params)
    return response.data
  },

  async loadPlanogramData(planogramId: number) {
    let params: GetPlanogramParams = new GetPlanogramParams()
    params.planogramId = planogramId

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getplanogram', params)
    return response.data
  },

  async getPartData(partId: number): Promise<Part> {
    let params: GetMenuParams = new GetMenuParams()
    params.partId = partId

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    let response = await apiClient.post('/getpart', params)
    return response.data
  },
}
