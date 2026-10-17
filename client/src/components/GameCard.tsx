import { Link } from 'react-router-dom'
import type { GameSummary } from '../api/types'

export function GameCard({ game }: { game: GameSummary }) {
  return (
    <Link to={`/games/${game.slug}`} className="card">
      <div className="thumb">
        {game.thumbnailUrl ? <img src={game.thumbnailUrl} alt="" loading="lazy" /> : <span>{game.title[0]}</span>}
      </div>
      <div className="card-body">
        <strong>{game.title}</strong>
        <span className="muted">{game.studio}</span>
        <span className="muted small">
          {game.likePercent != null ? `${game.likePercent}% liked · ` : ''}
          {game.playCount.toLocaleString()} plays
        </span>
      </div>
    </Link>
  )
}

export function GameGrid({ games }: { games: GameSummary[] }) {
  if (games.length === 0) return <p className="muted">No games here yet.</p>
  return <div className="grid">{games.map((g) => <GameCard key={g.id} game={g} />)}</div>
}
