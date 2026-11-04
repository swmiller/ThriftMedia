# ThriftMedia

Distributed platform for discovering local thrift store inventory and driving in-store visits. Ad-supported, with **no e-commerce checkout**. It has a public site, a business owner admin site, and platform administration capabilities.

## Tech Stack

- .NET 10, C#, .NET Aspire 13 orchestration. Containers come from Aspire and SDK container publishing (`PublishContainer`); there are no hand-written Dockerfiles, so don't add any
- ASP.NET Core minimal APIs; Blazor (interactive server) with Bootstrap
- PostgreSQL in a Docker container, with data bind-mounted to `c:\ThriftMediaDb`
- **Dapper** for data access; **DbUp** for schema (see Known Gaps)
- **ThriftMedia.Mediator** for CQRS. This is a custom library, not MediatR; see `src/ThriftMedia.Mediator/README.md`
- FluentValidation; xUnit
- Auth (planned): federated sign-in only (Google, Microsoft via ASP.NET Core OIDC handlers), cookie sessions in the Blazor apps, short-lived JWTs minted by the apps for API calls. No ASP.NET Core Identity, no hosted broker, no local passwords; authorization data lives in our own Dapper tables. See `docs/requirements/features/authentication-authorization.md` and `docs/adr/0001-authentication-authorization-approach.md` (status: Accepted)

## Architecture

Vertical Slice organization inside Clean Architecture boundaries, with CQRS. New slices follow this layout:

```
src/
  ThriftMedia.Domain/          # Entities, value objects, domain events, rules; no framework dependencies
  ThriftMedia.Application/     # Features/<Feature>/<UseCase>/: command or query, validator, handler
  ThriftMedia.Infrastructure/  # Dapper persistence and external service implementations
  ThriftMedia.Contracts/       # Request records and DTOs shared across the API boundary
  ThriftMedia.Api/             # Features/<Feature>/<UseCase>/: thin minimal API endpoints only
  ThriftMedia.DbMigrator/      # DbUp runner; embedded scripts in Scripts/NNNN_Description.sql
  ThriftMedia.Web/             # Public Blazor site
  ThriftMedia.Admin/           # Business owner admin Blazor site
  ThriftMedia.AppHost/         # Aspire orchestration
  ThriftMedia.ServiceDefaults/ # Telemetry, resilience, health endpoints
  ThriftMedia.Tests/           # xUnit tests
```

Rules:

- Dependencies point inward. Domain and Application must not reference Infrastructure, Dapper, or ASP.NET Core.
- Endpoints only dispatch through `IMediator` and translate results to HTTP. Business logic belongs in handlers and Domain.
- Every feature's endpoints are exposed as a `Map<UseCase>Endpoint` extension on `IEndpointRouteBuilder` and registered in `Api/Program.cs`.
- Schema changes are new DbUp scripts. Never edit a script that has already been applied, and don't use EF migrations.
- Enforce authorization server-side at API boundaries, and scope business data to the caller's authorized store.

## Conventions

- Name slices `<UseCase>Command` or `<UseCase>Query`, with `<UseCase>Validator` and `<UseCase>CommandHandler` or `<UseCase>QueryHandler`.
- Don't use AutoMapper. Write `To...` methods when mapping between types.
- Pass `CancellationToken` through all async calls.
- Cover new behavior with focused, isolated, fast xUnit tests. Refactor only while tests pass.
- Follow OWASP-aligned secure coding practices.
- Keep `README.md` and the docs below aligned when requirements change.

## Documentation

- `doc/App Requirements.md`: primary requirements source
- `docs/requirements/`: product and feature requirements
- `docs/adr/`: architecture decisions; add one as `NNNN-short-title.md` for decisions that affect implementation direction
- `.github/copilot-instructions.md`: coding standards shared with Copilot; keep consistent with this file

## Known Gaps

The code doesn't match the architecture above yet. Follow the architecture for new work, and don't copy these patterns:

- **EF Core has been removed.** The `ThriftMediaDbContext`, scaffolded EF entities, and the Npgsql EF package are gone. The Store handlers are commented out (marked `TODO: EF Core removed`) until reimplemented with Dapper (now installed; `IDbConnectionFactory` in Infrastructure provides connections). Store endpoints compile but have no handlers.
- **Test coverage is partial.** `src/ThriftMedia.Tests` (xunit 2.x; `xunit.v3` removed) covers Domain, Application handlers and the Api's authentication/authorization pipeline (in-memory repositories via `WebApplicationFactory`). Nothing tests the Dapper repositories, the DbUp scripts or the Admin sign-in flow against real Postgres/providers yet (needs Docker).
- **Auth is not implemented yet.** The approach is decided (ADR 0001): no Identity, no stores for credentials. The Api (JWT validation, policies, account endpoints) and the Admin app (cookie sign-in, token minting, placeholder pages) are wired but not yet exercised end to end: provider credentials, signing key and System Admin settings are supplied in step 10.

## Commands

```sh
dotnet build ThriftMedia.sln
dotnet test ThriftMedia.sln
dotnet run --project src/ThriftMedia.AppHost/ThriftMedia.AppHost.csproj   # requires Docker
```

## Skills and Tools

- Skills to load as relevant: `vertical-slice`, `clean-architecture`, `minimal-api`, `aspire`, `authentication`, `testing`, `error-handling`. The `ef-core` skill doesn't apply to new work.
- `/scaffold` and the kit's templates default to EF Core and may assume a different slice layout. The architecture above takes precedence.
- Prefer the Roslyn navigator MCP tools (`find_symbol`, `find_references`, `get_project_graph`, `get_diagnostics`) over broad text searches when working with C# symbols.
