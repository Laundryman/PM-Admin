import { config, graphScopes } from '@/config/auth'
import type { AccountInfo, AuthenticationResult, SilentRequest } from '@azure/msal-browser'
import {
  InteractionRequiredAuthError,
  NavigationClient,
  PublicClientApplication,
} from '@azure/msal-browser'
// type
export type MaybeAccount = AccountInfo | null

/**
 * MSAL instance
 */
// config.auth.authority = import.meta.env.VITE_GRAPH_AUTHORITY
export const msal = new PublicClientApplication(config)
// msal.initialize()
/**
 * Auth service
 */
export const Graph = {
  /**
   * Initialize and return active account
   */
  async initialize(client?: NavigationClient): Promise<MaybeAccount> {
    // start msal
    if (!msal) {
      throw new Error('MSAL not initialized. Call initializeMSAL() before using MSAL API')
    }

    try {
      // await msal.loginRedirect()
      await msal.handleRedirectPromise()

      // hook into application router
      if (client) {
        msal.setNavigationClient(client)
      }

      // grab and set account if in session
      const accounts = msal.getAllAccounts()
      if (accounts?.length && accounts[0]) {
        this.setAccount(accounts[0])
      }

      // return any active account
      return msal.getActiveAccount()
    } catch (error) {
      throw error
    }
  },

  /**
   * Get token for api
   */
  async getGraphToken() {
    const request: SilentRequest = {
      scopes: graphScopes,
    }
    return (
      msal
        // try getting the token silently
        .acquireTokenSilent(request)

        // attempt login popup if this fails
        .catch(async (error: unknown) => {
          if (error instanceof InteractionRequiredAuthError) {
            return msal.acquireTokenPopup(request)
          }
          throw error
        })
        .then((result: AuthenticationResult) => {
          return result.accessToken
        })
    )
  },

  /**
   * Set active account
   * @private
   */
  setAccount(account: MaybeAccount): MaybeAccount {
    msal.setActiveAccount(account)
    return account
  },

  /**
   * Escape hatch when msal gets stuck
   * @private
   */
  reset() {
    sessionStorage.clear()
  },
}
