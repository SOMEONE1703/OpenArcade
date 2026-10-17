import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <div className="empty">
      <h1>Not found</h1>
      <p className="muted">This page or game doesn't exist, or it's no longer listed.</p>
      <Link to="/" className="btn">Browse games</Link>
    </div>
  )
}
