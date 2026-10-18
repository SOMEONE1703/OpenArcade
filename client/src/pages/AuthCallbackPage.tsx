import { useAuth } from 'react-oidc-context'
import { Navigate } from 'react-router-dom'

/**
 * Keycloak redirects here. react-oidc-context finishes the code exchange, then this page returns the player to
 * the page sign-in started from (and drops ?code=&state= from the URL).
 */
export function AuthCallbackPage() {
  const auth = useAuth()
  if (auth.error) return <p className="error">Sign-in failed: {auth.error.message}</p>
  if (auth.isLoading) return <p className="muted">Signing you in…</p>

  const returnTo = (auth.user?.state as { returnTo?: string } | undefined)?.returnTo ?? '/'
  return <Navigate to={auth.isAuthenticated ? returnTo : '/'} replace />
}
