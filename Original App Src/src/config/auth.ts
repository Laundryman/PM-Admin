import { ref } from 'vue';
import { graphScopes, msalObj, state } from './authConfig';

export function useAuth() {
    const isAuthenticated = ref(false);

    const login = async () => {
        try {
            if (!msalObj) {
                throw new Error('MSAL not initialized. Call initializeMSAL() before using MSAL API');
            }
            await msalObj.loginRedirect();
            isAuthenticated.value = true;

            const loginResponse = await msalObj.loginRedirect();
            isAuthenticated.value = true;

            console.log('Login success: ', loginResponse);
        } catch (error) {
            console.error('Login error: ', error);
        }
    };

    const logout = () => {
        if (!msalObj) {
            throw new Error('MSAL not initialized. Call initializeMSAL() before using MSAL API');
        }

        msalObj.logoutRedirect({
            onRedirectNavigate: (url) => {
                // Return false if you would like to stop navigation after local logout
                return false;
            }
        });

        isAuthenticated.value = false;
        window.location.reload();
        console.log('Logged out');
    };

    const handleRedirect = async () => {
        try {
            await msalObj.handleRedirectPromise();
            state.isAuthenticated = msalObj.getAllAccounts().length > 0;
            state.user = msalObj.getAllAccounts()[0];
        } catch (error) {
            console.error('Redirect error: ', error);
        }
    };

    const getAccessToken = async () => {
        try {
            const accounts = msalObj.getAllAccounts();
            if (accounts.length >= 1) {
                const account = { account: accounts[0] };
                const request = Object.assign(graphScopes, account);
                const tokenResponse = await msalObj.acquireTokenSilent(request);
                return tokenResponse.accessToken;
            }
            return null;
        } catch (error) {
            logout();
        }
    };

    return { isAuthenticated, login, logout, handleRedirect, getAccessToken };
}
