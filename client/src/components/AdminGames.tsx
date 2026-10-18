import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useApi } from '../api/client'
import type { AdminGameRow, GameStatus } from '../api/types'

const statuses: GameStatus[] = ['Listed', 'Hidden', 'InReview', 'Draft', 'Delisted', 'Archived']
type StateAction = 'delist' | 'relist' | 'archive' | 'restore'

/** Every game in the catalog, with take offline, relist, edit and remove (a soft delete that can be restored). */
export function AdminGames() {
  const api = useApi()
  const qc = useQueryClient()
  const [status, setStatus] = useState<GameStatus | ''>('')
  const [q, setQ] = useState('')
  const [editing, setEditing] = useState<string | null>(null)

  const games = useQuery({
    queryKey: ['admin', 'games', status, q],
    queryFn: () => api.adminGames({ status: status || undefined, q }),
  })
  const setState = useMutation({
    mutationFn: ({ id, action }: { id: string; action: StateAction }) => api.setGameState(id, action),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin'] }),
  })

  const act = (game: AdminGameRow, action: StateAction) => {
    if (action === 'archive' && !confirm(`Remove "${game.title}"? It disappears everywhere, including the publisher's list. You can restore it from Archived.`)) return
    setState.mutate({ id: game.id, action })
  }

  return (
    <>
      <h2>Games</h2>
      <div className="row gap">
        <input type="search" placeholder="Search title, slug or studio" value={q} onChange={(e) => setQ(e.target.value)} />
        <select className="btn" value={status} onChange={(e) => setStatus(e.target.value as GameStatus | '')}>
          <option value="">All but archived</option>
          {statuses.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>
      {setState.error && <p className="error">{setState.error.message}</p>}
      {games.data?.length === 0 && <p className="muted">No games match.</p>}
      {games.data?.map((g) => (
        <div key={g.id} className="panel">
          <div className="row" style={{ justifyContent: 'space-between' }}>
            <span>
              <strong>{g.status === 'Listed' ? <Link to={`/games/${g.slug}`}>{g.title}</Link> : g.title}</strong> · {g.studio}{' '}
              <span className="badge">{g.status}</span>
            </span>
            <span className="muted small">
              {g.playCount} plays · {g.likeCount} likes · {g.dislikeCount} dislikes
              {g.consecutiveHealthFailures > 0 && <> · {g.consecutiveHealthFailures} failed checks</>}
              {!g.domainVerified && <> · domain not verified</>}
            </span>
            <div className="row gap">
              {g.status !== 'Archived' && (
                <button className="btn ghost small" onClick={() => setEditing(editing === g.id ? null : g.id)}>Edit</button>
              )}
              {(g.status === 'Listed' || g.status === 'Hidden' || g.status === 'InReview') && (
                <button className="btn ghost small" onClick={() => act(g, 'delist')}>Take offline</button>
              )}
              {g.status === 'Delisted' && (
                <>
                  <button className="btn small" onClick={() => act(g, 'relist')}>Relist</button>
                  <button className="btn ghost small" onClick={() => act(g, 'archive')}>Remove</button>
                </>
              )}
              {g.status === 'Archived' && <button className="btn ghost small" onClick={() => act(g, 'restore')}>Restore</button>}
            </div>
          </div>
          {editing === g.id && <EditDetails id={g.id} onDone={() => setEditing(null)} />}
        </div>
      ))}
    </>
  )
}

/** Catalog details only: the slug and play URL stay with the publisher, so the game keeps its status. */
function EditDetails({ id, onDone }: { id: string; onDone: () => void }) {
  const api = useApi()
  const qc = useQueryClient()
  const game = useQuery({ queryKey: ['admin', 'game', id], queryFn: () => api.adminGame(id) })

  const save = useMutation({
    mutationFn: (form: FormData) => api.editGameDetails(id, {
      title: String(form.get('title')),
      description: String(form.get('description')),
      tags: String(form.get('tags')).split(',').map((t) => t.trim()).filter(Boolean),
      thumbnailUrl: String(form.get('thumbnailUrl')) || undefined,
      screenshots: String(form.get('screenshots')).split('\n').map((s) => s.trim()).filter(Boolean),
      mobileFriendly: form.get('mobileFriendly') === 'on',
    }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin'] })
      onDone()
    },
  })

  if (!game.data) return <p className="muted small">Loading…</p>
  const g = game.data

  const submit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    save.mutate(new FormData(e.currentTarget))
  }

  return (
    <form className="form" onSubmit={submit}>
      <p className="muted small">Slug <code>{g.slug}</code> and play URL <code>{g.playUrl}</code> can only be changed by the publisher.</p>
      <label>Title<input name="title" defaultValue={g.title} required /></label>
      <label>Description<textarea name="description" defaultValue={g.description} rows={3} /></label>
      <label>Tags (comma separated)<input name="tags" defaultValue={g.tags.join(', ')} /></label>
      <label>Thumbnail URL<input name="thumbnailUrl" type="url" defaultValue={g.thumbnailUrl ?? ''} /></label>
      <label>Screenshot URLs (one per line)<textarea name="screenshots" defaultValue={g.screenshots.join('\n')} rows={3} /></label>
      <label className="check">
        <input name="mobileFriendly" type="checkbox" defaultChecked={g.mobileFriendly} />
        Works on mobile
      </label>
      {save.error && <p className="error">{save.error.message}</p>}
      <div className="row gap">
        <button className="btn small" disabled={save.isPending}>Save</button>
        <button type="button" className="btn ghost small" onClick={onDone}>Cancel</button>
      </div>
    </form>
  )
}
