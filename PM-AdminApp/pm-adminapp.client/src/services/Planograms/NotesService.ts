import { NewNote } from '@/models/Planograms/newnote.model';
import { PlanogramNote } from '@/models/Planograms/note.model';
import { Auth, msal } from '@/services/Identity/auth';
import { useAuthStore } from '@/stores/auth';
import axios from 'axios';
import { ref } from 'vue';

await msal.initialize();

const token = ref();
const idToken = ref();
const initialized = ref(false);
const apiClient = axios.create({
    baseURL: import.meta.env.VITE_API_ROOT + '/api/planogramNotes',
    withCredentials: false,
    headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token.value}`,
        ClaimsAuth: idToken.value || ''
    }
});

export default {
    getNotes(id: number): Promise<PlanogramNote[]> {
        if (token.value) {
            apiClient.defaults.headers.Authorization = `Bearer ${token.value}`;
            apiClient.defaults.headers['ClaimsAuth'] = idToken.value || '';
        }
        return apiClient
            .get('/getNotes', { params: { planogramId: id } })
            .then((response) => {
                return response.data;
            })
            .catch((error) => {
                throw error;
            });
    },
    addNote(newNote: NewNote): Promise<PlanogramNote> {
        if (token.value) {
            apiClient.defaults.headers.Authorization = `Bearer ${token.value}`;
            apiClient.defaults.headers['ClaimsAuth'] = idToken.value || '';
        }
        return apiClient
            .post('/addNote', newNote)
            .then((response) => {
                return response.data;
            })
            .catch((error) => {
                throw error;
            });
    },

    replyToNote(newNote: NewNote): Promise<PlanogramNote> {
        if (token.value) {
            apiClient.defaults.headers.Authorization = `Bearer ${token.value}`;
            apiClient.defaults.headers['ClaimsAuth'] = idToken.value || '';
        }
        return apiClient
            .post('/replyNote', newNote)
            .then((response) => {
                return response.data;
            })
            .catch((error) => {
                throw error;
            });
    },

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
};
