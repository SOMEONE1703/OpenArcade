import { useQuery } from '@tanstack/react-query'
import { useParams } from 'react-router-dom'
import { ApiError, useApi } from '../api/client'
import { GameGrid } from '../components/GameCard'
import { NotFoundPage } from './NotFoundPage'

export function ProfilePage() {
  const { username = '' } = useParams()
  const api = useApi()
  const profile = useQuery({ queryKey: ['profile', username], queryFn: () => api.profile(username) })

  if (profile.error instanceof ApiError && profile.error.status === 404) return <NotFoundPage />
  if (!profile.data) return <p className="muted">Loading…</p>

  const p = profile.data
  return (
    <>
      <div className="profile-head">
        <div className="avatar">{p.avatarUrl ? <img src={p.avatarUrl} alt="" /> : p.username[0].toUpperCase()}</div>
        <div>
          <h1>{p.username}</h1>
          <p className="muted small">Joined {new Date(p.joinedAt).toLocaleDateString()}</p>
        </div>
      </div>
      <h2>Favorites</h2>
      <GameGrid games={p.favorites} />
    </>
  )
}
