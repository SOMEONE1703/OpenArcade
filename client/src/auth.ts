import type { AuthProviderProps } from 'react-oidc-context'
import { WebStorageStateStore } from 'oidc-client-ts'

// Authorization code + PKCE against Keycloak. Google, Discord and username/password are all offered on
// Keycloak's own login page, so the app only ever talks to one identity provider.
export const oidcConfig: AuthProviderProps = {
  authority: import.meta.env.VITE_OIDC_AUTHORITY ?? 'http://localhost:8080/realms/openarcade',
  client_id: import.meta.env.VITE_OIDC_CLIENT_ID ?? 'openarcade-web',
  redirect_uri: `${window.location.origin}/auth/callback`,
  post_logout_redirect_uri: window.location.origin,
  scope: 'openid profile email',
  userStore: new WebStorageStateStore({ store: window.localStorage }),
  automaticSilentRenew: true,
  // AuthCallbackPage sends the player back to where sign-in started, through React Router.
}
