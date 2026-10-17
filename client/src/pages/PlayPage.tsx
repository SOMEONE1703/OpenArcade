import { useEffect, useRef, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { ApiError, useApi } from '../api/client'
import { Comments } from '../components/Comments'
import { GameActions, ReportButton } from '../components/GameActions'
import { useGame } from '../hooks'
import { NotFoundPage } from './NotFoundPage'

const LOAD_TIMEOUT_MS = 15_000

/**
 * openarcade.com/play/<slug>: the game in a sandboxed iframe served from the publisher's own domain.
 * - allow-same-origin is safe because the frame's origin is never ours, so it can't read our cookies or DOM.
 * - No allow-top-navigation, so a game can't redirect players away.
 * - No allow-popups by default (phishing risk, see "Gaps found in review"); add per game if one needs it.
 */
export function PlayPage() {
  const { slug = '' } = useParams()
  const api = useApi()
  const game = useGame(slug)
  const stageRef = useRef<HTMLDivElement>(null)
  const [loaded, setLoaded] = useState(false)
  const [timedOut, setTimedOut] = useState(false)
  const playRecorded = useRef(false)

  useEffect(() => {
    if (!game.data || loaded) return
    const t = setTimeout(() => setTimedOut(true), LOAD_TIMEOUT_MS)
    return () => clearTimeout(t)
  }, [game.data, loaded])

  if (game.error instanceof ApiError && game.error.status === 404) return <NotFoundPage />
  if (game.error) return <p className="error">{game.error.message}</p>
  if (!game.data) return <p className="muted">Loading…</p>
  const g = game.data

  const onLoad = () => {
    setLoaded(true)
    setTimedOut(false)
    // A play is counted when the iframe finishes loading; the API dedupes per visitor per day.
    if (!playRecorded.current) {
      playRecorded.current = true
      api.recordPlay(g.slug).catch(() => { /* counting is best-effort */ })
    }
  }

  return (
    <div className="play-page">
      <div className="play-bar">
        <Link to={`/games/${g.slug}`}>← Back to game page</Link>
        <span className="muted small">Game by {g.studio} · served from {g.domain}</span>
        <div className="row gap">
          <a href={g.playUrl} className="btn ghost small" target="_blank" rel="noopener noreferrer">Open game directly</a>
          <button className="btn ghost small" onClick={() => stageRef.current?.requestFullscreen?.()}>⛶ Fullscreen</button>
        </div>
      </div>

      <div className="stage" ref={stageRef}>
        <iframe
          src={g.playUrl}
          title={g.title}
          sandbox="allow-scripts allow-same-origin allow-pointer-lock"
          allow="fullscreen; gamepad; autoplay"
          referrerPolicy="strict-origin"
          onLoad={onLoad}
        />
        {timedOut && !loaded && (
          <div className="stage-overlay">
            <p>This game isn't responding.</p>
            <ReportButton onReport={(r) => api.reportGame(g.slug, r)} reasons={['Broken']} />
          </div>
        )}
      </div>

      <GameActions game={g} />
      <Comments slug={g.slug} canModerate={g.canModerate} />
    </div>
  )
}
