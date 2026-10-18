import { Link, NavLink, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from 'react-oidc-context'
import { useMe } from '../hooks'

export function Layout() {
  return (
    <>
      <Header />
      <main className="container">
        <Outlet />
      </main>
    </>
  )
}

function Header() {
  const auth = useAuth()
  const me = useMe()
  const location = useLocation()

  const signIn = () => auth.signinRedirect({ state: { returnTo: location.pathname + location.search } })

  return (
    <header className="header">
      <div className="container header-inner">
        <Link to="/" className="brand">
          <span className="brand-mark" aria-hidden="true">▶</span>
          <span className="brand-name">OpenArcade</span>
        </Link>
        <nav className="nav">
          <NavLink to="/" end>Games</NavLink>
          {me.data && <NavLink to="/publish">Publish</NavLink>}
          {me.data?.roles.includes('admin') && <NavLink to="/admin">Admin</NavLink>}
        </nav>
        <div className="account">
          {auth.isLoading ? null : auth.isAuthenticated ? (
            <>
              {me.data && <Link to={`/u/${me.data.username}`}>{me.data.username}</Link>}
              <button className="btn ghost" onClick={() => auth.signoutRedirect()}>Sign out</button>
            </>
          ) : (
            <button className="btn" onClick={signIn}>Sign in</button>
          )}
        </div>
      </div>
    </header>
  )
}
