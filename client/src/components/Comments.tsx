import { useState, type FormEvent, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { useApi } from '../api/client'
import type { Comment } from '../api/types'
import { useRequireSignIn } from '../hooks'
import { ReportButton } from './GameActions'

/** Instagram-style comments: newest or top, one level of replies collapsed after two, likes and @mentions. */
export function Comments({ slug, canModerate }: { slug: string; canModerate: boolean }) {
  const api = useApi()
  const [sort, setSort] = useState<'new' | 'top'>('new')
  const comments = useQuery({ queryKey: ['comments', slug, sort], queryFn: () => api.comments(slug, sort) })

  return (
    <section className="comments">
      <div className="row">
        <h2>Comments</h2>
        <div className="tabs">
          <button className={sort === 'new' ? 'active' : ''} onClick={() => setSort('new')}>Newest</button>
          <button className={sort === 'top' ? 'active' : ''} onClick={() => setSort('top')}>Top</button>
        </div>
      </div>
      <CommentForm slug={slug} />
      {comments.isLoading && <p className="muted">Loading comments…</p>}
      {comments.data?.items.map((c) => <CommentItem key={c.id} slug={slug} comment={c} canModerate={canModerate} />)}
      {comments.data?.items.length === 0 && <p className="muted">Be the first to comment.</p>}
      {!!comments.data?.hiddenCount && (
        <p className="muted small">{comments.data.hiddenCount} comment(s) hidden by the publisher.</p>
      )}
    </section>
  )
}

function CommentForm({ slug, parentId, onDone }: { slug: string; parentId?: string; onDone?: () => void }) {
  const api = useApi()
  const qc = useQueryClient()
  const requireSignIn = useRequireSignIn()
  const [body, setBody] = useState('')

  const post = useMutation({
    mutationFn: () => api.postComment(slug, body, parentId),
    onSuccess: () => {
      setBody('')
      qc.invalidateQueries({ queryKey: ['comments', slug] })
      if (parentId) qc.invalidateQueries({ queryKey: ['replies', parentId] })
      onDone?.()
    },
  })

  const submit = (e: FormEvent) => {
    e.preventDefault()
    if (body.trim()) requireSignIn(() => post.mutate())
  }

  return (
    <form className="comment-form" onSubmit={submit}>
      <input
        value={body}
        onChange={(e) => setBody(e.target.value)}
        placeholder={parentId ? 'Write a reply…' : 'Add a comment…'}
        maxLength={2000}
        aria-label={parentId ? 'Reply' : 'Comment'}
      />
      <button className="btn" disabled={!body.trim() || post.isPending}>Post</button>
      {post.error && <span className="error small">{post.error.message}</span>}
    </form>
  )
}

function CommentItem({ slug, comment, canModerate, isReply }: {
  slug: string; comment: Comment; canModerate: boolean; isReply?: boolean
}) {
  const api = useApi()
  const qc = useQueryClient()
  const requireSignIn = useRequireSignIn()
  const [replying, setReplying] = useState(false)
  const [showAll, setShowAll] = useState(false)

  const replies = useQuery({
    queryKey: ['replies', comment.id],
    queryFn: () => api.replies(comment.id),
    enabled: !isReply && comment.replyCount > 0,
  })

  const refresh = () => {
    qc.invalidateQueries({ queryKey: ['comments', slug] })
    if (comment.parentId) qc.invalidateQueries({ queryKey: ['replies', comment.parentId] })
  }
  const like = useMutation({ mutationFn: () => api.likeComment(comment.id, !comment.likedByMe), onSuccess: refresh })
  const remove = useMutation({ mutationFn: () => api.deleteComment(comment.id), onSuccess: refresh })
  const pin = useMutation({ mutationFn: () => api.pinComment(comment.id), onSuccess: refresh })
  const hide = useMutation({ mutationFn: () => api.hideComment(comment.id), onSuccess: refresh })

  const visibleReplies = showAll ? replies.data : replies.data?.slice(0, 2)
  const moreCount = (replies.data?.length ?? 0) - (visibleReplies?.length ?? 0)

  return (
    <div className={`comment ${isReply ? 'reply' : ''}`}>
      <div className="comment-head">
        <Link to={`/u/${comment.username}`}><strong>{comment.username}</strong></Link>
        {comment.pinned && <span className="badge">Pinned</span>}
        <span className="muted small">{new Date(comment.createdAt).toLocaleString()}</span>
      </div>
      <p>{renderMentions(comment.body)}</p>
      <div className="comment-actions small">
        <button className={comment.likedByMe ? 'active' : ''} onClick={() => requireSignIn(() => like.mutate())}>
          ♥ {comment.likeCount}
        </button>
        {!isReply && <button onClick={() => requireSignIn(() => setReplying((r) => !r))}>Reply</button>}
        {comment.mine && <button onClick={() => remove.mutate()}>Delete</button>}
        {canModerate && !isReply && <button onClick={() => pin.mutate()}>{comment.pinned ? 'Unpin' : 'Pin'}</button>}
        {canModerate && <button onClick={() => hide.mutate()}>Hide</button>}
        {!comment.mine && (
          <ReportButton onReport={(r) => api.reportComment(comment.id, r)} reasons={['Spam', 'Harassment', 'Inappropriate']} />
        )}
      </div>
      {replying && <CommentForm slug={slug} parentId={comment.id} onDone={() => setReplying(false)} />}
      {visibleReplies?.map((r) => <CommentItem key={r.id} slug={slug} comment={r} canModerate={canModerate} isReply />)}
      {moreCount > 0 && (
        <button className="link small" onClick={() => setShowAll(true)}>View {moreCount} more repl{moreCount === 1 ? 'y' : 'ies'}</button>
      )}
    </div>
  )
}

/** Turns @username into a profile link. */
function renderMentions(body: string): ReactNode[] {
  return body.split(/(@[A-Za-z0-9_.-]+)/g).map((part, i) =>
    part.startsWith('@') ? <Link key={i} to={`/u/${part.slice(1)}`}>{part}</Link> : part)
}
