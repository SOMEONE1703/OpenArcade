// Mirrors the response records under server/src/Presentation/OpenArcade.Api/Features/*/V1. Can be replaced by
// types generated from /openapi/v1.json.

export interface GameSummary {
  id: string
  slug: string
  title: string
  thumbnailUrl: string | null
  tags: string[]
  studio: string
  likeCount: number
  dislikeCount: number
  likePercent: number | null
  playCount: number
  playCount30d: number
  mobileFriendly: boolean
}

export interface GameDetail {
  id: string
  slug: string
  title: string
  description: string
  tags: string[]
  studio: string
  playUrl: string
  domain: string
  mobileFriendly: boolean
  likeCount: number
  dislikeCount: number
  likePercent: number | null
  playCount: number
  media: { kind: 'thumbnail' | 'screenshot' | 'video'; url: string }[]
  myVote: 'like' | 'dislike' | null
  isFavorite: boolean
  /** True for the game's publisher and admins: can pin and hide comments. */
  canModerate: boolean
}

export interface VoteCounts {
  likeCount: number
  dislikeCount: number
  likePercent: number | null
}

export interface Comment {
  id: string
  parentId: string | null
  username: string
  avatarUrl: string | null
  body: string
  likeCount: number
  pinned: boolean
  likedByMe: boolean
  mine: boolean
  replyCount: number
  createdAt: string
}

export interface CommentPage {
  items: Comment[]
  hiddenCount: number
}

export interface Me {
  id: string
  username: string
  avatarUrl: string | null
  roles: string[]
  isPublisher: boolean
  publisherRequestStatus: 'Pending' | 'Approved' | 'Rejected' | null
}

export interface Profile {
  username: string
  avatarUrl: string | null
  joinedAt: string
  favorites: GameSummary[]
}

export interface PublisherGame {
  id: string
  slug: string
  title: string
  description: string
  tags: string[]
  playUrl: string
  domain: string
  status: 'Draft' | 'InReview' | 'Listed' | 'Hidden' | 'Delisted'
  verificationToken: string
  domainVerified: boolean
  mobileFriendly: boolean
}

export type GameStatus = 'Draft' | 'InReview' | 'Listed' | 'Hidden' | 'Delisted' | 'Archived'

/** A row in the admin's list of every game. */
export interface AdminGameRow {
  id: string
  slug: string
  title: string
  thumbnailUrl: string | null
  studio: string
  status: GameStatus
  playUrl: string
  domain: string
  domainVerified: boolean
  playCount: number
  likeCount: number
  dislikeCount: number
  consecutiveHealthFailures: number
  createdAt: string
}

export interface AdminGame extends Omit<AdminGameRow, 'studio'> {
  description: string
  tags: string[]
  mobileFriendly: boolean
  screenshots: string[]
}

/** The catalog details an admin can edit. Slug and play URL stay with the publisher. */
export interface EditGameDetails {
  title: string
  description: string
  tags: string[]
  thumbnailUrl?: string
  screenshots: string[]
  mobileFriendly: boolean
}

export interface UpsertGame {
  slug: string
  title: string
  description: string
  tags: string[]
  playUrl: string
  thumbnailUrl?: string
  screenshots?: string[]
  mobileFriendly: boolean
}

export type GameSort = 'trending' | 'most-played' | 'most-played-30d' | 'top' | 'new'
