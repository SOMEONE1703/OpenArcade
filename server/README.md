# OpenArcade API

ASP.NET Core minimal API on .NET 10. See the root README for running everything together.

```sh
docker compose up -d                       # Postgres, Keycloak, sample games
dotnet run --project src/OpenArcade.Api    # http://localhost:5080
```

- `src/OpenArcade.Api/Data` — EF Core entities, `AppDbContext`, migrations, dev seed
- `src/OpenArcade.Api/Endpoints` — `/api/games`, comments, account, publisher and admin routes
- `src/OpenArcade.Api/Services` — embed checker, domain verifier, ranking (Wilson score, trending), profanity filter
- `src/OpenArcade.Api/Jobs` — Hangfire recurring jobs
- `keycloak/openarcade-realm.json` — realm export imported by Keycloak on first start

Add a migration after changing entities:

```sh
dotnet tool restore
dotnet ef migrations add <Name> --project src/OpenArcade.Api -o Data/Migrations
```

Roles come from Keycloak realm roles (`player`, `publisher`, `admin`) in the access token's `realm_access.roles`.
The `openarcade-web` client adds `openarcade-api` to the token audience, which the API validates.
