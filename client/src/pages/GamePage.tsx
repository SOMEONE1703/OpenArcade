import { Link, useParams } from 'react-router-dom'
import { ApiError } from '../api/client'
import { Comments } from '../components/Comments'
import { GameActions } from '../components/GameActions'
import { useGame } from '../hooks'
import { NotFoundPage } from './NotFoundPage'

export function GamePage() {
  const { slug = '' } = useParams()
  const game = useGame(slug)

  if (game.error instanceof ApiError && game.error.status === 404) return <NotFoundPage />
  if (game.error) return <p className="error">{game.error.message}</p>
  if (!game.data) return <p className="muted">Loading…</p>

  const g = game.data
  const thumbnail = g.media.find((m) => m.kind === 'thumbnail')
  const screenshots = g.media.filter((m) => m.kind === 'screenshot')

  return (
    <article className="game-page">
      <div className="game-hero">
        <div className="thumb large">{thumbnail ? <img src={thumbnail.url} alt="" /> : <span>{g.title[0]}</span>}</div>
        <div className="game-info">
          <h1>{g.title}</h1>
          <p className="muted">by {g.studio} · {g.domain}</p>
          <div className="chips">{g.tags.map((t) => <Link key={t} to={`/?tag=${t}`} className="chip">{t}</Link>)}</div>
          <Link to={`/play/${g.slug}`} className="btn play">▶ Play</Link>
          {!g.mobileFriendly && <p className="muted small">Best played on desktop.</p>}
          <p className="muted small">{g.playCount.toLocaleString()} plays</p>
        </div>
      </div>

      <GameActions game={g} />
      <p className="description">{g.description}</p>

      {screenshots.length > 0 && (
        <div className="screenshots">{screenshots.map((s) => <img key={s.url} src={s.url} alt="" loading="lazy" />)}</div>
      )}

      <Comments slug={g.slug} canModerate={g.canModerate} />
    </article>
  )
}
