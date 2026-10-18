import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useApi } from '../api/client'
import { useMe } from '../hooks'
import { AdminGames } from '../components/AdminGames'

/** Admin queue: publisher requests, games waiting for review, and every game in the catalog. Reports and bans are API-only for now. */
export function AdminPage() {
  const me = useMe()
  const api = useApi()
  const qc = useQueryClient()
  const isAdmin = !!me.data?.roles.includes('admin')

  const requests = useQuery({ queryKey: ['admin', 'requests'], queryFn: api.publisherRequests, enabled: isAdmin })
  const queue = useQuery({ queryKey: ['admin', 'queue'], queryFn: api.reviewQueue, enabled: isAdmin })

  const reviewPublisher = useMutation({
    mutationFn: ({ id, approve }: { id: string; approve: boolean }) => api.reviewPublisher(id, approve),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin'] }),
  })
  const reviewGame = useMutation({
    mutationFn: ({ id, action }: { id: string; action: 'approve' | 'reject' }) => api.reviewGame(id, action),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin'] }),
  })

  if (!isAdmin) return <p className="muted">Admins only.</p>

  return (
    <>
      <h1>Admin</h1>
      <h2>Publisher requests</h2>
      {requests.data?.length === 0 && <p className="muted">None pending.</p>}
      {requests.data?.map((r) => (
        <div key={r.id} className="panel row">
          <span><strong>{r.studioName}</strong> · {r.username}</span>
          <span className="muted small">{r.links.join(' ')}</span>
          <div className="row gap">
            <button className="btn small" onClick={() => reviewPublisher.mutate({ id: r.id, approve: true })}>Approve</button>
            <button className="btn ghost small" onClick={() => reviewPublisher.mutate({ id: r.id, approve: false })}>Reject</button>
          </div>
        </div>
      ))}

      <h2>Games in review</h2>
      {queue.data?.length === 0 && <p className="muted">Nothing to review.</p>}
      {queue.data?.map((g) => (
        <div key={g.id} className="panel row">
          <span><strong>{g.title}</strong> · {g.studio}</span>
          <a href={g.playUrl} target="_blank" rel="noopener noreferrer" className="small">{g.playUrl}</a>
          <div className="row gap">
            <button className="btn small" onClick={() => reviewGame.mutate({ id: g.id, action: 'approve' })}>List</button>
            <button className="btn ghost small" onClick={() => reviewGame.mutate({ id: g.id, action: 'reject' })}>Send back</button>
          </div>
        </div>
      ))}

      <AdminGames />
    </>
  )
}
