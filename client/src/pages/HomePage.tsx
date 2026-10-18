import { useQuery } from '@tanstack/react-query'
import { useSearchParams } from 'react-router-dom'
import { useApi } from '../api/client'
import type { GameSort } from '../api/types'
import { GameGrid } from '../components/GameCard'

export function HomePage() {
  const api = useApi()
  const [params, setParams] = useSearchParams()
  const tag = params.get('tag') ?? undefined
  const q = params.get('q') ?? undefined
  const tags = useQuery({ queryKey: ['tags'], queryFn: api.tags })

  const setParam = (key: string, value?: string) => {
    const next = new URLSearchParams(params)
    if (value) next.set(key, value)
    else next.delete(key)
    setParams(next, { replace: true })
  }

  return (
    <>
      <section className="hero">
        <h1>Play something new</h1>
        <p className="muted">Browser games from independent studios, free to play right here.</p>
          <div className="filters">
          <input
            type="search"
            placeholder="Search games"
            defaultValue={q}
            onKeyDown={(e) => e.key === 'Enter' && setParam('q', e.currentTarget.value.trim())}
            aria-label="Search games"
          />
          <div className="chips">
            <button className={!tag ? 'chip active' : 'chip'} onClick={() => setParam('tag')}>All</button>
            {tags.data?.map((t) => (
              <button key={t} className={tag === t ? 'chip active' : 'chip'} onClick={() => setParam('tag', t)}>{t}</button>
            ))}
        </div>
        </div>
      </section>

      {q ? (
        <Shelf title={`Results for "${q}"`} sort="new" tag={tag} q={q} />
      ) : (
        <>
          <Shelf title="Trending this week" sort="trending" tag={tag} />
          <Shelf title="Most played" sort="most-played" tag={tag} />
          <Shelf title="Top rated" sort="top" tag={tag} />
          <Shelf title="New" sort="new" tag={tag} />
        </>
      )}
    </>
  )
}

function Shelf({ title, sort, tag, q }: { title: string; sort: GameSort; tag?: string; q?: string }) {
  const api = useApi()
  const games = useQuery({
    queryKey: ['games', sort, tag, q],
    queryFn: () => api.games({ sort, tag, q, limit: 12 }),
  })

  return (
    <section className="shelf">
      <h2>{title}</h2>
      {games.isLoading && <p className="muted">Loading…</p>}
      {games.error && <p className="error">Couldn't load games: {games.error.message}</p>}
      {games.data && <GameGrid games={games.data} scroll={!q} />}
    </section>
  )
}
