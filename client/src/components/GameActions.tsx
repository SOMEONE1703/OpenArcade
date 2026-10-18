import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useApi } from '../api/client'
import type { GameDetail } from '../api/types'
import { useRequireSignIn } from '../hooks'

/** Thumbs up / down, favorite heart and Report, shown on the game page and under the play frame. */
export function GameActions({ game }: { game: GameDetail }) {
  const api = useApi()
  const qc = useQueryClient()
  const requireSignIn = useRequireSignIn()
  const key = ['game', game.slug]

  const vote = useMutation({
    mutationFn: (value: 'like' | 'dislike' | null) => api.vote(game.slug, value),
    onSuccess: (counts, value) => qc.setQueryData<GameDetail>(key, (g) => g && { ...g, ...counts, myVote: value }),
  })

  const favorite = useMutation({
    mutationFn: (on: boolean) => api.favorite(game.slug, on),
    onSuccess: (_, on) => qc.setQueryData<GameDetail>(key, (g) => g && { ...g, isFavorite: on }),
  })

  const total = game.likeCount + game.dislikeCount

  return (
    <div className="actions">
      <button
        className={`btn ghost ${game.myVote === 'like' ? 'active' : ''}`}
        aria-pressed={game.myVote === 'like'}
        onClick={() => requireSignIn(() => vote.mutate(game.myVote === 'like' ? null : 'like'))}
      >👍 {game.likeCount.toLocaleString()}</button>
      <button
        className={`btn ghost ${game.myVote === 'dislike' ? 'active' : ''}`}
        aria-pressed={game.myVote === 'dislike'}
        onClick={() => requireSignIn(() => vote.mutate(game.myVote === 'dislike' ? null : 'dislike'))}
      >👎 {game.dislikeCount.toLocaleString()}</button>
      <span className="muted small">
        {game.likePercent != null ? `${game.likePercent}% liked, ${total.toLocaleString()} ${total === 1 ? 'vote' : 'votes'}` : 'No votes yet'}
      </span>
      <button
        className={`btn ghost ${game.isFavorite ? 'active' : ''}`}
        aria-pressed={game.isFavorite}
        onClick={() => requireSignIn(() => favorite.mutate(!game.isFavorite))}
      >{game.isFavorite ? '♥ Favorited' : '♡ Favorite'}</button>
      <ReportButton onReport={(reason) => api.reportGame(game.slug, reason)}
        reasons={['Broken', 'Inappropriate', 'Not as described', 'Phishing or scam']} />
      {vote.error && <span className="error small">{vote.error.message}</span>}
    </div>
  )
}

export function ReportButton({ onReport, reasons, label = 'Report' }: {
  onReport: (reason: string) => Promise<void>
  reasons: string[]
  label?: string
}) {
  const requireSignIn = useRequireSignIn()
  const report = useMutation({ mutationFn: onReport })

  if (report.isSuccess) return <span className="muted small">Thanks, reported.</span>

  return (
    <select
      className="btn ghost small"
      aria-label={label}
      value=""
      onChange={(e) => {
        const reason = e.target.value
        requireSignIn(() => report.mutate(reason))
      }}
    >
      <option value="" disabled>{label}…</option>
      {reasons.map((r) => <option key={r} value={r}>{r}</option>)}
    </select>
  )
}
