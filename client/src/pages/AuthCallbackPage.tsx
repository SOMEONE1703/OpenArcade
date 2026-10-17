import { useAuth } from 'react-oidc-context'
import { Navigate } from 'react-router-dom'

/** Keycloak redirects here; react-oidc-context finishes the code exchange and onSigninCallback navigates on. */
export function AuthCallbackPage() {
  const auth = useAuth()
  if (auth.error) return <p className="error">Sign-in failed: {auth.error.message}</p>
  if (!auth.isLoading && !auth.isAuthenticated) return <Navigate to="/" replace />
  return <p className="muted">Signing you in…</p>
}
