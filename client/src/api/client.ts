import { useMemo } from 'react'
import { useAuth } from 'react-oidc-context'
import type {
  Comment, CommentPage, GameDetail, GameSort, GameSummary, Me, Profile, PublisherGame, UpsertGame, VoteCounts,
} from './types'

export class ApiError extends Error {
  readonly status: number
  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

/**
 * Anonymous visitor id used to count plays once per visitor per game per day.
 * TODO: gate behind the cookie-consent banner for EU visitors (see "Legal and privacy" in the design doc).
 */
function visitorId(): string {
  const key = 'openarcade.visitor'
  let id = localStorage.getItem(key)
  if (!id) {
    id = crypto.randomUUID()
    localStorage.setItem(key, id)
  }
  return id
}

async function request<T>(path: string, token: string | undefined, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  if (token) headers.set('Authorization', `Bearer ${token}`)
  if (init.body) headers.set('Content-Type', 'application/json')
  headers.set('X-Visitor-Id', visitorId())

  const res = await fetch(path, { ...init, headers })
  if (!res.ok) {
    const text = await res.text()
    let message = text || res.statusText
    try {
      const json = JSON.parse(text)
      message = json.detail ?? json.title ?? message
    } catch { /* plain-text error body */ }
    throw new ApiError(res.status, message)
  }
  if (res.status === 204 || res.status === 202) return undefined as T
  return res.json() as Promise<T>
}

const json = (body: unknown) => JSON.stringify(body)

function createApi(token?: string) {
  const get = <T,>(path: string) => request<T>(path, token)
  const send = <T,>(method: string, path: string, body?: unknown) =>
    request<T>(path, token, { method, body: body === undefined ? undefined : json(body) })

  return {
    games: (params: { sort?: GameSort; tag?: string; q?: string; limit?: number } = {}) => {
      const qs = new URLSearchParams(Object.entries(params).filter(([, v]) => v != null && v !== '').map(([k, v]) => [k, String(v)]))
      return get<GameSummary[]>(`/api/v1/games?${qs}`)
    },
    tags: () => get<string[]>('/api/v1/games/tags'),
    game: (slug: string) => get<GameDetail>(`/api/v1/games/${slug}`),
    recordPlay: (slug: string) => send<{ counted: boolean }>('POST', `/api/v1/games/${slug}/plays`),
    vote: (slug: string, value: 'like' | 'dislike' | null) =>
      value ? send<VoteCounts>('PUT', `/api/v1/games/${slug}/vote`, { value }) : send<VoteCounts>('DELETE', `/api/v1/games/${slug}/vote`),
    favorite: (slug: string, on: boolean) => send<void>(on ? 'PUT' : 'DELETE', `/api/v1/games/${slug}/favorite`),
    reportGame: (slug: string, reason: string) => send<void>('POST', `/api/v1/games/${slug}/reports`, { reason }),

    comments: (slug: string, sort: 'new' | 'top') => get<CommentPage>(`/api/v1/games/${slug}/comments?sort=${sort}`),
    replies: (id: string) => get<Comment[]>(`/api/v1/comments/${id}/replies`),
    postComment: (slug: string, body: string, parentId?: string) =>
      send<Comment>('POST', `/api/v1/games/${slug}/comments`, { body, parentId }),
    deleteComment: (id: string) => send<void>('DELETE', `/api/v1/comments/${id}`),
    likeComment: (id: string, on: boolean) => send<void>(on ? 'PUT' : 'DELETE', `/api/v1/comments/${id}/like`),
    pinComment: (id: string) => send<void>('POST', `/api/v1/comments/${id}/pin`),
    hideComment: (id: string) => send<void>('POST', `/api/v1/comments/${id}/hide`),
    reportComment: (id: string, reason: string) => send<void>('POST', `/api/v1/comments/${id}/reports`, { reason }),

    me: () => get<Me>('/api/v1/me'),
    profile: (username: string) => get<Profile>(`/api/v1/users/${username}`),
    applyAsPublisher: (studioName: string, links: string[]) =>
      send<void>('POST', '/api/v1/me/publisher-request', { studioName, links }),

    myGames: () => get<PublisherGame[]>('/api/v1/publisher/games'),
    createGame: (game: UpsertGame) => send<PublisherGame>('POST', '/api/v1/publisher/games', game),
    verifyDomain: (id: string) => send<{ verified: boolean; instructions: string }>('POST', `/api/v1/publisher/games/${id}/verify-domain`),
    checkEmbed: (id: string) => send<{ allowed: boolean; detail: string }>('POST', `/api/v1/publisher/games/${id}/check-embed`),
    submitGame: (id: string) => send<PublisherGame>('POST', `/api/v1/publisher/games/${id}/submit`),

    reviewQueue: () => get<{ id: string; slug: string; title: string; playUrl: string; studio: string }[]>('/api/v1/admin/review-queue'),
    publisherRequests: () => get<{ id: string; username: string; studioName: string; links: string[] }[]>('/api/v1/admin/publisher-requests'),
    reviewPublisher: (id: string, approve: boolean) =>
      send<void>('POST', `/api/v1/admin/publisher-requests/${id}/${approve ? 'approve' : 'reject'}`),
    reviewGame: (id: string, action: 'approve' | 'reject' | 'delist') => send<void>('POST', `/api/v1/admin/games/${id}/${action}`),
  }
}

export type Api = ReturnType<typeof createApi>

/** API bound to the signed-in user's access token (or anonymous). */
export function useApi(): Api {
  const auth = useAuth()
  const token = auth.user?.access_token
  return useMemo(() => createApi(token), [token])
}
