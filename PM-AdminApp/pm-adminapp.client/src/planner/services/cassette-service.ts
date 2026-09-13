import { Auth } from '@/services/Identity/auth';
import { useAuthStore } from '@/stores/auth';
import axios from 'axios';
import { ref } from 'vue';
import { GetMenuParams } from '../models/call-models/GetMenuParams.model';

const token = ref();
const idToken = ref();
const initialized = ref(false);
const authStore = useAuthStore();
if (!authStore.initialized) {
    await authStore.initialize();
}
const apiClient = axios.create({
    baseURL: import.meta.env.VITE_API_ROOT + '/api/planograms',
    withCredentials: false,
    headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token.value}`,
        ClaimsAuth: idToken.value || ''
    }
});

export class CassetteService {
    //get Image Location
    async getImageLocation() {
        if (token.value) {
            apiClient.defaults.headers.Authorization = `Bearer ${token.value}`;
            apiClient.defaults.headers['ClaimsAuth'] = idToken.value || '';
        }

        // const params: GetMenuParams = new GetMenuParams();

        //params.brandId = brandId;
        //params.standTypeId = standTypeId;
        //params.countryId = countryId;
        // params.planogramId = planogramId;

        // if (token.value) {
        //     apiClient.defaults.headers.Authorization = `Bearer ${token.value}`;
        // }
        // const response = await apiClient
        //     .get('/edit/getmenucategories', { params })
        //     .then((res) => {
        //         return res.data;
        //     })
        //     .catch((error) => {
        //         throw error;
        //     });
        // return response;
        // return $.ajax({
        //     type: 'GET',
        //     url: '/umbraco/api/planxapi/getImageLocation',
        //     contentType: 'application/json'
        // })
        //     .done((data) => data)
        //     .fail((data) => data);
    }

    //get Products for the Part
    async getPartProducts(partId: number, planogramId: number) {
        if (token.value) {
            apiClient.defaults.headers.Authorization = `Bearer ${token.value}`;
            apiClient.defaults.headers['ClaimsAuth'] = idToken.value || '';
        }

        const params: GetMenuParams = new GetMenuParams();

        //params.brandId = brandId;
        //params.standTypeId = standTypeId;
        //params.countryId = countryId;
        // params.planogramId = planogramId;

        if (token.value) {
            apiClient.defaults.headers.Authorization = `Bearer ${token.value}`;
        }
        const response = await apiClient
            .get('/edit/getpartProducts', { params: { partId: partId, planogramId: planogramId } })
            .then((res) => {
                return res.data;
            })
            .catch((error) => {
                throw error;
            });
        return response;

        // return $.ajax({
        //     type: 'GET',
        //     url: '/umbraco/api/planxapi/getpartProducts?partId=' + partId + '&planogramId=' + planogramId,
        //     contentType: 'application/json'
        // })
        //     .done((data) => data)
        //     .fail((data) => data);
    }

    //get Shades for the Product
    async getProductShades(productId: number) {
        if (token.value) {
            apiClient.defaults.headers.Authorization = `Bearer ${token.value}`;
            apiClient.defaults.headers['ClaimsAuth'] = idToken.value || '';
        }

        const response = await apiClient
            .get('/edit/getProductShades', { params: { productId: productId } })
            .then((res) => {
                return res.data;
            })
            .catch((error) => {
                throw error;
            });
        return response;
    }

    async getNonMarketParts(planogramId: number) {
        if (token.value) {
            apiClient.defaults.headers.Authorization = `Bearer ${token.value}`;
            apiClient.defaults.headers['ClaimsAuth'] = idToken.value || '';
        }

        const response = await apiClient
            .get('/edit/getNonMarketParts', { params: { planogramId: planogramId } })
            .then((res) => {
                return res.data;
            })
            .catch((error) => {
                throw error;
            });
        return response;
    }

    async initialise() {
        const authStore = useAuthStore();
        if (!authStore.initialized) {
            await authStore.initialize();
        }
        const t = await Auth.getToken();
        token.value = t;
        const idT = await Auth.getIdToken();
        idToken.value = idT;
        initialized.value = true;
    }
}
