import type { Configuration } from '@azure/msal-browser';
import { LogLevel, PublicClientApplication } from '@azure/msal-browser';

export const scopes = [import.meta.env.VITE_AUTH_SCOPE];

export const config: Configuration = {
    // required
    auth: {
        // must match info in dashboard
        clientId: import.meta.env.VITE_AUTH_CLIENT_ID,
        authority: import.meta.env.VITE_AUTH_AUTHORITY, //  Replace the placeholder with your tenant info
        // EXTERNAL TENANT
        // authority: "https://Enter_the_Tenant_Subdomain_Here.ciamlogin.com/", // Replace the placeholder with your tenant subdomain
        redirectUri: import.meta.env.VITE_AUTH_REDIRECT_URI, // You must register this URI on App Registration. Defaults to window.location.href e.g. http://localhost:3000/
        navigateToLoginRequestUrl: true // If "true", will navigate back to the original request location before processing the auth code response.
        // knownAuthorities: [
        //   import.meta.env.VITE_AUTH_AUTHORITY,
        // ],
    },
    cache: {
        cacheLocation: 'sessionStorage', // Configures cache location. "sessionStorage" is more secure, but "localStorage" gives you SSO.
        storeAuthStateInCookie: false // set this to true if you have to support IE
    },
    // optional
    system: {
        loggerOptions: {
            logLevel: LogLevel.Verbose,
            loggerCallback
        }
    }
};

export const msal = new PublicClientApplication(config);

function loggerCallback(level: LogLevel, message: string, containsPii: boolean) {
    if (!containsPii) {
        const parts = message.split(' : ');
        const text = parts.pop();
        switch (level) {
            case LogLevel.Error:
                return console.error(text);

            case LogLevel.Warning:
                return console.warn(text);

            case LogLevel.Info:
                return console.info(text);

            case LogLevel.Verbose:
                return console.debug(text);
        }
    }
}
