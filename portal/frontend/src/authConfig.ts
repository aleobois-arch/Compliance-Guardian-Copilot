import { PublicClientApplication, Configuration, RedirectRequest } from '@azure/msal-browser';

/**
 * MSAL configuration for Azure AD / Entra ID authentication.
 * Values are read from environment variables (Vite VITE_ prefix).
 * Set these in a .env.local file:
 *   VITE_AZURE_CLIENT_ID=<your-app-client-id>
 *   VITE_AZURE_TENANT_ID=<your-tenant-id>
 *   VITE_API_SCOPE=<your-api-scope-uri>
 */
const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_CLIENT_ID ?? '',
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_TENANT_ID ?? 'common'}`,
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: 'sessionStorage', // More secure than localStorage
    storeAuthStateInCookie: false,
  },
};

export const loginRequest: RedirectRequest = {
  scopes: [import.meta.env.VITE_API_SCOPE ?? 'openid profile'],
};

export const msalInstance = new PublicClientApplication(msalConfig);

// Handle redirect response on page load
msalInstance.initialize().then(() => {
  msalInstance.handleRedirectPromise().catch(console.error);
});
