import { useState, type FormEvent } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { useApi } from '../api/client'
import type { PublisherGame } from '../api/types'
import { useMe } from '../hooks'

/** Publisher flow: apply once, then register a game, verify its domain, check embedding and submit for review. */
export function PublishPage() {
  const me = useMe()
  if (!me.data) return <p className="muted">Sign in to publish games.</p>
  if (!me.data.isPublisher) return <Apply status={me.data.publisherRequestStatus} />
  return <Dashboard />
}

function Apply({ status }: { status: string | null }) {
  const api = useApi()
  const qc = useQueryClient()
  const [studio, setStudio] = useState('')
  const [links, setLinks] = useState('')
  const apply = useMutation({
    mutationFn: () => api.applyAsPublisher(studio, links.split(/\s+/).filter(Boolean)),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['me'] }),
  })

  if (status === 'Pending') return <p>Your publisher request is waiting for review.</p>

  return (
    <form className="form" onSubmit={(e) => { e.preventDefault(); apply.mutate() }}>
      <h1>Become a publisher</h1>
      {status === 'Rejected' && <p className="error">Your last request was rejected. You can apply again.</p>}
      <label>Studio name<input value={studio} onChange={(e) => setStudio(e.target.value)} required /></label>
      <label>Links to your work (one per line)<textarea value={links} onChange={(e) => setLinks(e.target.value)} rows={3} /></label>
      <button className="btn" disabled={apply.isPending}>Request publisher access</button>
      {apply.error && <p className="error">{apply.error.message}</p>}
    </form>
  )
}

function Dashboard() {
  const api = useApi()
  const games = useQuery({ queryKey: ['my-games'], queryFn: api.myGames })
  return (
    <>
      <h1>Your games</h1>
      {games.data?.map((g) => <PublisherGameRow key={g.id} game={g} />)}
      {games.data?.length === 0 && <p className="muted">No games yet.</p>}
      <NewGameForm />
    </>
  )
}

function PublisherGameRow({ game }: { game: PublisherGame }) {
  const api = useApi()
  const qc = useQueryClient()
  const [message, setMessage] = useState<string>()
  const refresh = () => qc.invalidateQueries({ queryKey: ['my-games'] })

  const verify = useMutation({
    mutationFn: () => api.verifyDomain(game.id),
    onSuccess: (r) => { setMessage(r.verified ? 'Domain verified.' : r.instructions); refresh() },
  })
  const embed = useMutation({ mutationFn: () => api.checkEmbed(game.id), onSuccess: (r) => setMessage(r.detail) })
  const submit = useMutation({ mutationFn: () => api.submitGame(game.id), onSuccess: () => { setMessage('Submitted for review.'); refresh() } })
  const error = verify.error ?? embed.error ?? submit.error

  return (
    <div className="panel">
      <div className="row">
        <strong>{game.title}</strong>
        <span className="badge">{game.status}</span>
        {game.status === 'Listed' && <Link to={`/games/${game.slug}`}>View listing</Link>}
      </div>
      <p className="muted small">{game.playUrl}</p>
      {!game.domainVerified && (
        <p className="small">
          Verify <code>{game.domain}</code>: add a DNS TXT record <code>openarcade-verify={game.verificationToken}</code>,
          or serve <code>{game.verificationToken}</code> at <code>/.well-known/openarcade.txt</code>.
        </p>
      )}
      <div className="row gap">
        {!game.domainVerified && <button className="btn ghost small" onClick={() => verify.mutate()}>Verify domain</button>}
        <button className="btn ghost small" onClick={() => embed.mutate()}>Test embedding</button>
        {(game.status === 'Draft' || game.status === 'Hidden') && (
          <button className="btn small" disabled={!game.domainVerified} onClick={() => submit.mutate()}>Submit for review</button>
        )}
      </div>
      {message && <p className="small">{message}</p>}
      {error && <p className="error small">{error.message}</p>}
    </div>
  )
}

function NewGameForm() {
  const api = useApi()
  const qc = useQueryClient()
  const [form, setForm] = useState({ title: '', slug: '', playUrl: '', description: '', tags: '', thumbnailUrl: '', mobileFriendly: false })
  const set = (k: keyof typeof form) => (e: { target: { value: string } }) => setForm({ ...form, [k]: e.target.value })

  const create = useMutation({
    mutationFn: () => api.createGame({
      ...form,
      tags: form.tags.split(',').map((t) => t.trim()).filter(Boolean),
      thumbnailUrl: form.thumbnailUrl || undefined,
    }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['my-games'] })
      setForm({ title: '', slug: '', playUrl: '', description: '', tags: '', thumbnailUrl: '', mobileFriendly: false })
    },
  })

  const submit = (e: FormEvent) => { e.preventDefault(); create.mutate() }

  return (
    <form className="form" onSubmit={submit}>
      <h2>Register a game</h2>
      <label>Title<input value={form.title} onChange={set('title')} required /></label>
      <label>Slug<input value={form.slug} onChange={set('slug')} pattern="[a-z0-9]+(-[a-z0-9]+)*" required placeholder="space-miner" /></label>
      <label>Play URL<input type="url" value={form.playUrl} onChange={set('playUrl')} required placeholder="https://spaceminer.dev/play" /></label>
      <label>Description<textarea value={form.description} onChange={set('description')} rows={3} /></label>
      <label>Tags (comma separated)<input value={form.tags} onChange={set('tags')} /></label>
      <label>Thumbnail URL<input type="url" value={form.thumbnailUrl} onChange={set('thumbnailUrl')} /></label>
      <label className="check">
        <input type="checkbox" checked={form.mobileFriendly} onChange={(e) => setForm({ ...form, mobileFriendly: e.target.checked })} />
        Works on mobile
      </label>
      <p className="muted small">
        Your play URL must allow framing by OpenArcade: no <code>X-Frame-Options: DENY</code> or <code>SAMEORIGIN</code>,
        and <code>frame-ancestors</code> must include OpenArcade if you set a CSP. Browsers keep a framed game's storage
        separate from your own site, so saves won't carry over.
      </p>
      <button className="btn" disabled={create.isPending}>Create draft</button>
      {create.error && <p className="error">{create.error.message}</p>}
    </form>
  )
}
