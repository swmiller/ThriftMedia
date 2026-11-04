# ThriftMedia

Distributed platform for discovering local thrift store inventory and driving in-store visits. Ad-supported, with **no e-commerce checkout**. It has a public site, a business owner admin site, and platform administration capabilities.

## Tech Stack

- .NET 10, C#, .NET Aspire 13 orchestration
- ASP.NET Core minimal APIs; Blazor (interactive server) with Bootstrap
- PostgreSQL in a Docker container, with data bind-mounted to `c:\ThriftMediaDb`
- **Dapper** for data access; **DbUp** for schema (see Known Gaps)
- **ThriftMedia.Mediator** for CQRS. This is a custom library, not MediatR; see `src/ThriftMedia.Mediator/README.md`
- FluentValidation; xUnit
- Auth (planned): ASP.NET Core Identity, JWT bearer, external OAuth providers; see `docs/requirements/features/authentication-authorization.md` and `docs/adr/0001-authentication-authorization-approach.md` (status: Proposed)

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

- **EF Core has been removed.** The `ThriftMediaDbContext`, scaffolded EF entities, and the Npgsql EF package are gone. The Store handlers are commented out (marked `TODO: EF Core removed`) until reimplemented with Dapper, which isn't installed yet. Store endpoints compile but have no handlers.
- **Store slices are in the wrong place.** Their handlers and validators live in `ThriftMedia.Api/Features/Stores` and should move to Application.
- **`ThriftMedia.Application` isn't wired in.** It has no code, isn't in `ThriftMedia.sln`, and isn't referenced by Api.
- **Api startup is incomplete.** `Api/Program.cs` doesn't call `AddServiceDefaults`, `AddMediator`, or register validators or a data access service.
- **`src/ThriftMedia.Tests` has no tests yet.** It references both `xunit` and `xunit.v3`.
- **No Dockerfiles exist yet**, although containerized deployment is the documented target.
- **Identity storage is undecided.** ASP.NET Core Identity's default stores use EF Core, so using Identity with Dapper requires custom stores or a documented exception. Ask before implementing.

## Commands

```sh
dotnet build ThriftMedia.sln
dotnet test ThriftMedia.sln
dotnet run --project src/ThriftMedia.AppHost/ThriftMedia.AppHost.csproj   # requires Docker
```

## Skills and Tools

- Skills to load as relevant: `vertical-slice`, `clean-architecture`, `minimal-api`, `aspire`, `authentication`, `testing`, `error-handling`. The `ef-core` skill doesn't apply to new work.
- `/scaffold` and the kit's templates default to EF Core and may assume a different slice layout. The architecture above takes precedence.
- Prefer the Roslyn navigator MCP tools (`find_symbol`, `find_references`, `get_project_graph`, `get_diagnostics`) over broad text searches when working with C# symbols. It won't see `ThriftMedia.Application` until the project is added to the solution.
