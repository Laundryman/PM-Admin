import type { Planogram } from '@/planner/models/Planogram'
import { Auth } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { ref } from 'vue'
import { GetMenuParams } from '../models/call-models/GetMenuParams.model'
import { GetPlanogramParams } from '../models/call-models/GetPlanogramParams.model'
import { PartInfo } from '../models/PartInfo'

//await msal.initialize()

const token = ref()
const idToken = ref()
const initialized = ref(false)
const authStore = useAuthStore()
if (!authStore.initialized) {
  await authStore.initialize()
}
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_ROOT + '/api',
  withCredentials: false,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token.value}`,
    ClaimsAuth: idToken.value || '',
  },
})

export class MenuService {
  // Get the data to create the menu
  async loadMenuCategories(planogramId: number) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }

    const params: GetMenuParams = new GetMenuParams()

    //params.brandId = brandId;
    //params.standTypeId = standTypeId;
    //params.countryId = countryId;
    params.planogramId = planogramId

    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
    }
    const response = await apiClient
      .get('/planograms/edit/getmenucategories', { params })
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response

    // return $.ajax({
    //     type: "POST",
    //     url: '/umbraco/api/planxapi/getmenucategories',
    //     data: JSON.stringify(params),
    //     contentType: "application/json"
    //   }).done(data => data)
    //   .fail(data => data);
  }

  // Get the data to create the menu
  async loadMenuData(planogramId: number) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }

    const response = await apiClient
      .get('/planograms/edit/getmenu', { params: { planogramId: planogramId } })
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response

    // return $.ajax({
    //     type: "POST",
    //     url: '/getmenu',
    //     data: JSON.stringify(params),
    //     contentType: "application/json"
    //   }).done(data => data)
    //   .fail(data => data);
  }

  // Get the data to create the menu for a specific category
  async loadCategoryMenuData(planogramId: number, category: string) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }

    const params: GetMenuParams = new GetMenuParams()

    //params.brandId = brandId;
    //params.standTypeId = standTypeId;
    //params.countryId = countryId;
    params.planogramId = planogramId
    params.category = category

    const response = await apiClient
      .post('/planograms/getcategorymenu', params)
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response

    // return $.ajax({
    //     type: "POST",
    //     url: '/umbraco/api/planxapi/getcategorymenu',
    //     data: JSON.stringify(params),
    //     contentType: "application/json"
    //   }).done(data => data)
    //   .fail(data => data);
  }

  // Get the data to create the menu
  async loadPlanogramShelves(planogramId: number) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }

    // const params: GetPlanogramParams = new GetPlanogramParams();

    // params.planogramId = planogramId;

    const response = await apiClient
      .get('/planograms/edit/getPlanogramShelves', { params: { planogramId: planogramId } })
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response

    // return $.ajax({
    //     type: "GET",
    //     url: '/umbraco/api/planxapi/getPlanogramShelves?planogramId=' + planogramId,
    //     data: JSON.stringify(params),
    //     contentType: "application/json"
    //   }).done(data => data)
    //   .fail(function (jqXHR, textStatus, error) {
    //     console.log("GetShelves error: " + error);
    //   });
  }

  // Get the data to create the menu
  async loadPlanogramParts(planogramId: number) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }

    // const params: GetPlanogramParams = new GetPlanogramParams();

    // params.planogramId = planogramId;

    const response = await apiClient
      .get('/planograms/edit/getPlanogramParts', { params: { planogramId: planogramId } })
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response

    // return $.ajax({
    //     type: "GET",
    //     url: '/umbraco/api/planxapi/getPlanogramParts?planogramId=' + planogramId,
    //     //data: JSON.stringify(params),
    //     contentType: "application/json"
    //   }).done(data => data)
    //   .fail(data => data);
  }

  // Get the only newly added parts data
  async loadNewPlanogramParts(planogramId: number) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }

    const params: GetPlanogramParams = new GetPlanogramParams()

    params.planogramId = planogramId

    const response = await apiClient
      .get('/planograms/edit/getNewPlanogramParts', { params: { planogramId: planogramId } })
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response

    // return $.ajax({
    //   type: "GET",
    //   url: '/umbraco/api/planxapi/getNewPlanogramParts?planogramId=' + planogramId,
    //   //data: JSON.stringify(params),
    //   contentType: "application/json"
    // }).done(data => data)
    //   .fail(data => data);
  }

  async loadStandData(standId: number, isCluster: boolean) {
    // var apiUrl = '/umbraco/api/planxapi/getstand?standId=';
    // if (isCluster)
    //   apiUrl = '/api/v2/planx/cluster/get-stand/';

    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
        apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
      }
      return apiClient.get('/planograms/Edit/getStand', {
        params: {
          id: standId,
        },
      })
    } else {
      throw new Error('MenuService not initialized')
    }

    // return $.ajax({
    //     type: "GET",
    //     url: apiUrl + standId,
    //     //data: JSON.stringify(params),
    //     contentType: "application/json"
    //   }).done(data => data)
    //   .fail(data => data);
  }

  loadPlanogramData(planogramId: number): Promise<Planogram> {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
        apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
      }
      return apiClient.get('/planograms/edit/getPlanogram', {
        params: {
          planogramId: planogramId,
        },
      })
    } else {
      throw new Error('MenuService not initialized')
    }

    // return $.ajax({
    //     type: "GET",
    //     url: '/umbraco/api/planxapi/getplanogram?planogramId=' + planogramId,
    //     contentType: "application/json"
    //   }).done(data => data)
    //   .fail(data => data);
  }

  getPartData(partId: number): Promise<PartInfo> {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
        apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
      }
      return apiClient.get('/planograms/getPart', {
        params: {
          id: partId,
        },
      })
    } else {
      throw new Error('MenuService not initialized')
    }
    // return $.ajax({
    //     type: "GET",
    //     url: '/umbraco/api/planxapi/getpart?partId=' + partId,
    //     contentType: "application/json"
    //   }).then(data => data as PartInfo)
    //   .catch(error => { throw error; }) as unknown as Promise<PartInfo> & {[Symbol.toStringTag]: string};
  }

  /////////////////////////////////////////////////////////////////////////////////////////////////
  //// Cluster Service Calls
  /////////////////////////////////////////////////////////////////////////////////////////////////

  loadClusterData(clusterId: number): Promise<Planogram> {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
        apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
      }
      return apiClient.get('/clusters/getCluster', {
        params: {
          clusterId: clusterId,
        },
      })
    } else {
      throw new Error('MenuService not initialized')
    }
  }

  async loadClusterShelves(clusterId: number) {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
        apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
      }
      const response = await apiClient
        .get('/clusters/getShelves', {
          params: {
            id: clusterId,
          },
        })
        .then((res) => {
          return res.data
        })
        .catch((error) => {
          throw error
        })
      return response
    } else {
      throw new Error('MenuService not initialized')
    }
  }

  async loadClusterParts(clusterId: number) {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
        apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
      }
      const response = await apiClient.get('/clusters/getParts', {
        params: {
          id: clusterId,
        },
      })
      return response.data
    } else {
      throw new Error('MenuService not initialized')
    }
  }

  async loadClusterMenuCategories(clusterId: number) {
    if (initialized.value !== false) {
      if (token.value) {
        apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
        apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
      }
      const response = await apiClient.get('/clusters/getMenuCategories', {
        params: {
          id: clusterId,
        },
      })
      return response.data
    } else {
      throw new Error('MenuService not initialized')
    }
  }

  // Get the data to create the menu
  async loadClusterMenuData(clusterId: number) {
    if (token.value) {
      apiClient.defaults.headers.Authorization = `Bearer ${token.value}`
      apiClient.defaults.headers['ClaimsAuth'] = idToken.value || ''
    }

    const response = await apiClient
      .get('/clusters/getmenu', { params: { id: clusterId } })
      .then((res) => {
        return res.data
      })
      .catch((error) => {
        throw error
      })
    return response
  }

  // // Get the data to create the menu
  // async loadClusterMenuCategories(clusterId: number) {
  //   //ajax call to get json goes here
  //   // var self = this;
  //   // var menu = new Menu;

  //   const params: GetMenuParams = new GetMenuParams();

  //   //params.brandId = brandId;
  //   //params.standTypeId = standTypeId;
  //   //params.countryId = countryId;
  //   params.clusterId = clusterId;

  //   const response = await apiClient
  //     .post('/get-menu-categories', params)
  //     .then((res) => {
  //       return res.data
  //     })
  //     .catch((error) => {
  //       throw error
  //     })
  //   return response

  //   return $.ajax({
  //       type: "GET",
  //       url: '/api/v2/planx/cluster/get-menu-categories/' + clusterId,
  //       //data: JSON.stringify(params),
  //       contentType: "application/json"
  //     }).done(data => data)
  //     .fail(data => data);

  // }

  // // Get the data to create the menu
  // loadClusterMenuData(clusterId: number) {
  //   //ajax call to get json goes here
  //   // var self = this;
  //   // var menu = new Menu;

  //   let params: GetMenuParams = new GetMenuParams();

  //   //params.brandId = brandId;
  //   //params.standTypeId = standTypeId;
  //   //params.countryId = countryId;
  //   params.clusterId = clusterId;

  //   return $.ajax({
  //       type: "GET",
  //       url: '/api/v2/planx/cluster/get-menu/' + clusterId,
  //       //data: JSON.stringify(params),
  //       contentType: "application/json"
  //     }).done(data => data)
  //     .fail(data => data);

  // }

  // // Get the data to create the menu for a specific category
  // loadClusterCategoryMenuData(clusterId: number, category: string) {
  //   //ajax call to get json goes here
  //   // var self = this;
  //   // var menu = new Menu;

  //   let params: GetMenuParams = new GetMenuParams();

  //   //params.brandId = brandId;
  //   //params.standTypeId = standTypeId;
  //   //params.countryId = countryId;
  //   params.clusterId = clusterId;
  //   params.category = category;

  //   return $.ajax({
  //       type: "GET",
  //       url: '/api/v2/planx/cluster/get-category-menu/' + clusterId + '/' + category,
  //       //data: JSON.stringify(params),
  //       contentType: "application/json"
  //     }).done(data => data)
  //     .fail(data => data);

  // }

  // loadClusterData(clusterId: number) {
  //   return $.ajax({
  //       type: "GET",
  //       url: '/api/v2/planx/cluster/get-cluster/' + clusterId + '?clusterId=' + clusterId,
  //       contentType: "application/json"
  //   }).done(
  //       data => data)
  //     .fail(
  //       data => data);
  // }

  // // Get the data to create the menu
  // loadClusterShelves(clusterId: number) {
  //   //ajax call to get json goes here
  //   // var self = this;
  //   // var menu = new Menu;

  //   return $.ajax({
  //       type: "GET",
  //       url: '/api/v2/planx/cluster/get-shelves/' + clusterId + '?clusterId=' + clusterId,
  //       //data: JSON.stringify(params),
  //       contentType: "application/json"
  //     }).done(data => data)
  //     .fail(function (jqXHR, textStatus, error) {
  //       console.log("GetShelves error: " + error);
  //     });

  // }

  // // Get the data to create the menu
  // loadClusterParts(clusterId: number) {
  //   //ajax call to get json goes here
  //   // var self = this;
  //   // var menu = new Menu;

  //   //let params: GetPlanogramParams = new GetPlanogramParams();

  //   //params.clusterId = clusterId;

  //   return $.ajax({
  //       type: "GET",
  //       url: '/api/v2/planx/cluster/get-parts/' + clusterId + '?clusterId=' + clusterId,
  //       //data: JSON.stringify(params),
  //       contentType: "application/json"
  //     }).done(data => data)
  //     .fail(data => data);

  // }

  //loadClusterStandData(standId: number) {
  //  return $.ajax({
  //      type: "GET",
  //      url: '/api/v2/planx/cluster/get-stand/' + standId,
  //      //data: JSON.stringify(params),
  //      contentType: "application/json"
  //    }).done(data => data)
  //    .fail(data => data);
  //}

  async initialise() {
    const authStore = useAuthStore()
    if (!authStore.initialized) {
      await authStore.initialize()
    }
    const t = await Auth.getToken()
    token.value = t
    const idT = await Auth.getIdToken()
    idToken.value = idT
    console.log('BrandService initialized with token:', token.value)
    initialized.value = true
  }
}
