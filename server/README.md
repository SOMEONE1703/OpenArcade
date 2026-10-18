# OpenArcade API

ASP.NET Core minimal API on .NET 10, split into three projects. See the root README for running everything together.

```sh
docker compose up -d                                    # Postgres, Keycloak, sample games
dotnet run --project src/Presentation/OpenArcade.Api    # http://localhost:5080
```

## Projects

| Project | References | What's in it |
|---------|------------|--------------|
| `src/Domain/OpenArcade.Domain` | nothing | Entities and their rules (`Game`, `Comment`, `Vote`…), repository interfaces, read models, ranking (Wilson score, trending) |
| `src/Infrastructure/OpenArcade.Infrastructure` | Domain | EF Core `AppDbContext`, database records, mappers, repositories, migrations, dev seed, embed and domain checks, Hangfire jobs |
| `src/Presentation/OpenArcade.Api` | Domain, Infrastructure | `Program.cs`, Keycloak auth, and the vertical slices under `Features/` |

Each layer has its own models:

- **API DTOs** — each slice's `*Request` and `*Response` records. A few shared responses sit next to the slices
  that use them, such as `Features/Games/V1/GameCardResponse.cs`.
- **Domain entities** — plain classes in `OpenArcade.Domain` with the business rules, for example "changing the play
  URL sends a listed game back to review" or "publishers can't vote on their own games". A broken rule throws
  `DomainRuleException`, which the API returns as a 400.
- **Database records** — `*Record` classes in `Infrastructure/Persistence/Records`. Only EF Core and the mappers see
  them. Repositories map records to and from entities.

## Adding an endpoint

Slices live in `Features/<Feature>/V<n>/<Slice>/`:

```
Features/Games/V1/GetGame/
  GetGameEndpoint.cs    implements IEndpoint; maps a route relative to /api/v1
  GetGameHandler.cs     the logic; registered automatically (any *Handler class under Features)
  GetGameResponse.cs    the response DTO (and *Request.cs when the slice takes a body or query)
```

The `V<n>` folder sets the route prefix (`/api/v<n>`) and the feature folder sets the OpenAPI tag, so a V2 of a slice
is a new folder next to V1. Handlers return `Result<T>` (or `Result`), and the endpoint calls `.ToHttp()`.

## Migrations

```sh
dotnet tool restore
dotnet ef migrations add <Name> \
  --project src/Infrastructure/OpenArcade.Infrastructure \
  --startup-project src/Presentation/OpenArcade.Api \
  -o Persistence/Migrations
```

In Development the API applies migrations and seeds the sample games on startup.

## Auth

Roles come from Keycloak realm roles (`player`, `publisher`, `admin`) in the access token's `realm_access.roles`.
The `openarcade-web` client adds `openarcade-api` to the token audience, which the API validates.
`keycloak/openarcade-realm.json` is the realm export Keycloak imports on first start.
