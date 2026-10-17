import { createBrowserRouter } from 'react-router-dom'
import { Layout } from './components/Layout'
import { HomePage } from './pages/HomePage'
import { GamePage } from './pages/GamePage'
import { PlayPage } from './pages/PlayPage'
import { ProfilePage } from './pages/ProfilePage'
import { PublishPage } from './pages/PublishPage'
import { AdminPage } from './pages/AdminPage'
import { AuthCallbackPage } from './pages/AuthCallbackPage'
import { NotFoundPage } from './pages/NotFoundPage'

export const router = createBrowserRouter([
  {
    element: <Layout />,
    children: [
      { path: '/', element: <HomePage /> },
      { path: '/games/:slug', element: <GamePage /> },
      { path: '/play/:slug', element: <PlayPage /> },
      { path: '/u/:username', element: <ProfilePage /> },
      { path: '/publish', element: <PublishPage /> },
      { path: '/admin', element: <AdminPage /> },
      { path: '/auth/callback', element: <AuthCallbackPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
