# OpenArcade web app

React + TypeScript on Vite. `npm run dev` serves http://localhost:5173 and proxies `/api` to the API on :5080.
Sign-in is OIDC authorization code + PKCE against the local Keycloak realm; override with `.env.local`
(see `.env.example`).

- `src/pages` — catalog, game page, play page (`/play/:slug`), profile, publish, admin
- `src/components` — header, game cards, like/favorite/report actions, comments
- `src/api` — fetch client bound to the signed-in user's token, and the API types
