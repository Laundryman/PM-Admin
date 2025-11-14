import { type AccountInfo, PublicClientApplication } from "@azure/msal-browser";
import { reactive } from "vue";

export const msalConfig = {
  auth: {
    clientId: "7173f283-6276-4021-a2a8-e4c0a7510677",
    authority:
      "https://login.microsoftonline.com/aa779afa-e9ea-461b-b395-d2a7c17e5fa6",
    redirectUri: "https://localhost:53681/",
    postLogoutUri: "https://localhost:53681/",
  },
  cache: {
    cacheLocation: "localStorage",
    storeAuthStateInCookie: false,
  },
};

export const graphScopes = {
  scopes: ["api://7173f283-6276-4021-a2a8-e4c0a7510677"],
};
export const state = reactive({
  isAuthenticated: false,
  user: null as AccountInfo | null,
});
export const msalObj = new PublicClientApplication(msalConfig);
