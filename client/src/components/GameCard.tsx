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
        <span className="card-stats small muted">
          {game.likePercent != null && <span className="good">▲ {game.likePercent}%</span>}
          <span>{game.playCount.toLocaleString()} plays</span>
        </span>
      </div>
    </Link>
  )
}

/** `scroll` lays the cards out as one sideways-scrolling row, used for the home page shelves. */
export function GameGrid({ games, scroll = false }: { games: GameSummary[]; scroll?: boolean }) {
  if (games.length === 0) return <p className="muted">No games here yet.</p>
  return <div className={scroll ? 'grid scroll' : 'grid'}>{games.map((g) => <GameCard key={g.id} game={g} />)}</div>
}
