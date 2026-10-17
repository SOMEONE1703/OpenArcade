# OpenArcade

A curated catalog of browser games that publishers host on their own domains. Players browse, like, comment and
favorite on OpenArcade, and play at `/play/<slug>`, where the game runs in a sandboxed iframe served from the
publisher's domain. OpenArcade never serves game code.

| Folder     | What it is |
|------------|------------|
| `server/`  | C# ASP.NET Core API (.NET 10, EF Core + PostgreSQL, Hangfire, Keycloak JWT auth), plus Docker Compose for Postgres and Keycloak |
| `client/`  | React + TypeScript web app (Vite, React Router, TanStack Query, react-oidc-context) |
| `samples/` | Two tiny games served from a separate origin in development so the play page works end to end |

## Run it locally

Prerequisites: .NET 10 SDK, Node 20+, Docker.

```sh
# 1. Postgres, Keycloak and the sample games
cd server
docker compose up -d

# 2. API on http://localhost:5080 (applies migrations and seeds the two sample games in Development)
dotnet run --project src/OpenArcade.Api

# 3. Web app on http://localhost:5173 (proxies /api to the API)
cd ../client
npm install
npm run dev
```

Open http://localhost:5173. Dev accounts in the `openarcade` realm (local only):

| User     | Password | Roles |
|----------|----------|-------|
| `admin`  | `admin`  | admin, publisher, player |
| `player` | `player` | player |

You can also register a new account from the sign-in page. The Keycloak admin console is at
http://localhost:8080 (admin / admin). Other useful URLs: OpenAPI at http://localhost:5080/openapi/v1.json and the
Hangfire dashboard at http://localhost:5080/hangfire.

### Google and Discord sign-in

Both identity providers are in the realm export but disabled, because they need real OAuth credentials. In the
Keycloak console, open realm `openarcade` → Identity providers, set the client ID and secret for `google` and
`discord`, and enable them. Each provider's redirect URI is shown on its settings page.

## What's built

- **Catalog**: trending this week, most played, top rated (Wilson lower bound), new, tag filter and search.
- **Game and play pages**: likes/dislikes with "92% liked, 1,240 votes", favorites, reports, the sandboxed iframe
  (`allow-scripts allow-same-origin allow-pointer-lock`, no top navigation, no popups), fullscreen, "Open game
  directly", a 15-second "This game isn't responding" fallback, and a play counted once per visitor per game per day
  when the frame loads.
- **Comments**: newest/top, one level of replies collapsed after two, comment likes, `@mentions`, delete own,
  publisher pin/hide (hidden comments show a count), report, 5-per-minute rate limit and a word-list filter.
- **Publisher flow**: apply, admin approval, register a game, domain verification (DNS TXT or
  `/.well-known/openarcade.txt`), embedding test (X-Frame-Options and CSP `frame-ancestors`), submit for review.
  Changing the play URL sends a listed game back to review.
- **Admin**: approve publishers, list or send back games, delist kill switch, report queue and bans (API).
- **Background jobs (Hangfire)**: hourly stats and trending refresh, daily embed health check (three failures hides a
  game), monthly domain re-verification.

## Not built yet

- Playwright change detection (weekly screenshot and script hashes) and publisher emails.
- Granting the Keycloak `publisher` role on approval; the API currently checks the `publishers` table.
- Image uploads (media are URLs), notifications, cookie consent for the anonymous visitor ID, account deletion.
- Generating the client's API types from the OpenAPI spec (`client/src/api/types.ts` is hand-written for now).
