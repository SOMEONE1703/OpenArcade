import { useQuery } from '@tanstack/react-query'
import { useAuth } from 'react-oidc-context'
import { useLocation } from 'react-router-dom'
import { useApi } from './api/client'

/** The signed-in player's OpenArcade account, or undefined when signed out. */
export function useMe() {
  const auth = useAuth()
  const api = useApi()
  return useQuery({
    queryKey: ['me', auth.user?.profile.sub],
    queryFn: api.me,
    enabled: auth.isAuthenticated,
  })
}

/** Returns a function that runs `action` if signed in, otherwise sends the player to sign in. */
export function useRequireSignIn() {
  const auth = useAuth()
  const location = useLocation()
  return (action: () => void) => {
    if (auth.isAuthenticated) action()
    else auth.signinRedirect({ state: { returnTo: location.pathname + location.search } })
  }
}

export function useGame(slug: string) {
  const api = useApi()
  return useQuery({ queryKey: ['game', slug], queryFn: () => api.game(slug) })
}
