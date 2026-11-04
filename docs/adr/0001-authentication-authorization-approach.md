# 0001 - Authentication and Authorization Approach

## Status
Accepted

## Context
Authentication and authorization are the first vertical slice and establish security patterns for the rest of the system. We need a consistent approach across API and Blazor UI, aligned with Clean Architecture, VSA, and CQRS.

ThriftMedia is free to members and ad supported, so identity cost must be at or near zero. The data layer is Dapper + DbUp; EF Core has been removed, and ASP.NET Core Identity's default stores depend on EF Core.

## Decision
1. Implement authentication/authorization as a dedicated feature slice first.
2. **Federated sign-in only**: Google and Microsoft via ASP.NET Core's built-in OIDC handlers. No local passwords, no Facebook, no ASP.NET Core Identity, no hosted identity broker.
3. The Blazor apps use an **HttpOnly cookie session**. To call the API they mint a **short-lived (~5 minute) signed JWT** from the cookie principal. There are no refresh tokens and no token table.
4. ThriftMedia owns **authorization data** in PostgreSQL (Dapper + DbUp), building on the existing `auth."AppUsers"` and `retail."Store"` tables plus a new `retail."StoreApplication"` table. `AppUsers` gains a `Status` (Approved, Active, Suspended) and nullable provider columns (null until first sign-in links the identity by verified email). A Store Admin owns exactly one store (`Store.AppUserId` is unique).
5. Roles: `SystemAdmin` (a single allowlisted account, already enforced by a unique index) and `StoreAdmin`. The only sign-in to a store's pages is its Store Admin; Worker accounts and team management are deferred.
6. System Admin MFA is enforced on the provider account; the API checks the `amr` claim when present.
7. Enforce authorization checks server-side at API boundaries (policies plus store scoping from the `storeId` claim). UI checks are for experience only.
8. Keep feature logic in Application slices (using an `ICurrentUser` abstraction) and infrastructure concerns in Infrastructure.

## Alternatives Considered
- **Hosted broker (Entra External ID, Auth0)**: removes token code but adds recurring cost and vendor coupling; rejected on cost.
- **ASP.NET Core Identity with custom Dapper stores**: more code and a credential store to secure; rejected.
- **Self-hosted Keycloak**: free, but means running and securing a separate identity database; rejected.

## Consequences
1. Zero identity licensing cost and no credential storage.
2. We own the token-minting code and signing-key management; it must stay small, short-lived, audience-restricted and well tested.
3. Suspension takes effect within the status-cache window rather than at token expiry.
4. Users need a Google or Microsoft account; a provider outage blocks sign-in.
5. Third-party API clients are not supported until a token endpoint is designed (future ADR).
6. A broker can be introduced later without changing the API, since it only sees our token.

See `docs/requirements/features/authentication-authorization.md`.
