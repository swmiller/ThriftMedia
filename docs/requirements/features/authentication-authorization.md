# Feature Requirements: Authentication and Authorization

## Status
Active

## Overview

ThriftMedia supports brick-and-mortar secondhand media stores. To protect store data and
administrative functions, all API and administration surfaces require authenticated and
authorized access. The only public surfaces are the public website (home and media search),
which are accessible anonymously.

Authentication is **federated**: users sign in with Google or Microsoft. ThriftMedia does not
store passwords or run its own credential store. ThriftMedia does own **authorization data**
(who is the System Admin, which store a user belongs to, and the user's status). See
`docs/adr/0001-authentication-authorization-approach.md`.

---

## Goals

1. Allow the verified Store Admin of each store to manage that store's inventory and data.
2. Allow the System Admin to oversee all stores, manage applications, and configure the platform.
3. Keep the public website, including media search, fully anonymous.
4. Keep users signed in with a secure cookie session in the Blazor apps, and call the API with
   short-lived access tokens minted from that session.
5. Support Google and Microsoft as the external identity providers. No local credentials.
6. Enforce multi-tenant isolation so each Store Admin can only access their own store's data.
7. Keep identity cost at or near zero (no paid identity broker).

---

## Out of Scope (Initial Slice)

1. Local email/password accounts, password policy, password reset, and account lockout
   (delegated to the identity providers).
2. Facebook and other identity providers.
3. Microsoft Entra (Azure AD) enterprise integration.
4. ThriftMedia-operated multi-factor authentication (MFA is enforced on the provider account;
   see System Admin below).
5. Fine-grained permission editor UI (beyond role assignment).
6. Third-party API clients (the API accepts only tokens minted by the ThriftMedia Blazor apps).
7. Store Workers and any multi-user team management (deferred; the only sign-in to a store's pages is its Store Admin). Users belonging to more than one store are also out of scope.

---

## Roles

| Role         | Description                                                                      |
|--------------|----------------------------------------------------------------------------------|
| SystemAdmin  | Exactly one person. Full platform access. Manages store applications.            |
| StoreAdmin   | One per store. Full access to their own store's profile, settings and media. |
| Anonymous    | Unauthenticated user. Read-only access to the public website and media search.   |

A StoreAdmin belongs to exactly one store (enforced by `retail."Store"."AppUserId"` being unique). A Worker role is deferred.

### System Admin Bootstrap
- The System Admin is identified by an **allowlisted email address** supplied through
  configuration (and bound to a specific provider account). No password is seeded.
- The allowlisted account must sign in through Google or Microsoft with MFA enabled on that
  provider account. This is an operational requirement; the API additionally requires the
  `amr` claim to indicate MFA when the provider supplies it.
- The System Admin record cannot be deleted through the UI.

---

## Access Control Matrix

| Surface                              | Anonymous | StoreAdmin                        | SystemAdmin |
|--------------------------------------|-----------|-----------------------------------|-------------|
| Public website and media search (Web) | ✅ Read   | ✅ Read                           | ✅ Read     |
| Admin site landing (`/`)             | ✅ Sign-in prompt | ✅                        | ✅          |
| Store pages in Admin site (`/store`, `/media`) | ❌ | ✅ Own store only | ✅ All |
| System pages in Admin site (`/system`) | ❌      | ❌                                | ✅          |
| All API endpoints (except search)    | ❌        | ✅ Own store only                 | ✅ All      |
| Media Search API endpoint            | ✅ Read   | ✅ Read                           | ✅ Read     |

---

## User Stories

### Authentication

**AUTH-1 — Federated Sign-In**
> As any user, I can sign in with Google or Microsoft so that I do not need to manage a
> separate password for ThriftMedia.

**AUTH-2 — Session**
> As an authenticated user, I stay signed in through a secure HttpOnly cookie session so that
> I do not need to sign in on every visit. When the session expires I am asked to sign in again.

**AUTH-3 — Sign-Out**
> As an authenticated user, I can sign out so that my session ends and I am no longer
> authenticated.

**AUTH-4 — API Access**
> As the system, the Blazor apps call the API with a short-lived signed access token minted from
> the user's session, so the API can enforce role and store scope without trusting the UI.

### Store Application Workflow

**APP-1 — Apply for a Store Account**
> As a prospective store owner, I can submit a store application from a public "Apply"
> page, providing the information required to verify my physical store location, so that
> the System Admin can review my request.

**APP-2 — Review Application**
> As the System Admin, I can view a list of pending store applications and approve or
> deny each one so that only legitimate brick-and-mortar stores gain access. When approving, I
> enter the license details I verified (type, issuing authority, issue and expiration dates); approval
> creates the Store Admin account and the store together.

**APP-3 — Application Status Notifications**
> As an applicant, I receive an email when my application is approved, denied, or when my
> account is later suspended so that I am always informed of my account status.

**APP-4 — Account Suspension**
> As the System Admin, I can suspend an active Store Admin so that a store that violates
> policy loses access without their data being deleted.

**APP-5 — Account Re-activation**
> As the System Admin, I can re-activate a suspended Store Admin so that a store that
> resolves a policy violation can regain access.

### Authorization

**AUTHZ-1 — Protected API Endpoints**
> As the system, all API endpoints (except media search) return `401 Unauthorized` when
> no valid token is present, and `403 Forbidden` when a valid token lacks the required
> role or store scope.

**AUTHZ-2 — Store Data Isolation**
> As a Store Admin, I can only read, create, update, or delete data belonging to my own
> store so that stores cannot access each other's inventory or settings.

**AUTHZ-3 — System Admin Unrestricted**
> As the System Admin, I can access data for any store so that I can provide support and
> oversight.

---

## Store Application — Required Fields

The public application form must capture sufficient information to verify a physical
storefront. Minimum required fields:

| Field                  | Notes                                        |
|------------------------|----------------------------------------------|
| Store name             |                                              |
| Owner full name        |                                              |
| Email address          | Must match the Google/Microsoft account the owner will sign in with |
| Phone number           |                                              |
| Physical street address |                                             |
| City, State, ZIP       |                                              |
| Country                | United States only                                             |
| Store website URL      | Optional but helps verification              |
| Brief store description| What types of secondhand media are sold    
| Business license number | Required
| Business license image upload | Required (PDF or image file) |
| Store front image upload | Optional (photo of the physical store front) |

---

## Application Status Lifecycle

```
Pending → Approved → Active
Pending → Denied   → Rejected (terminal)
Active  → Suspended
Suspended → Active  (re-activated by System Admin)
```

| Status    | Meaning                                                      |
|-----------|--------------------------------------------------------------|
| Pending   | Application submitted, awaiting System Admin review          |
| Approved  | System Admin approved; the user record exists and will be activated on first sign-in |
| Rejected  | System Admin denied the application                          |
| Active    | Store Admin can sign in and manage their store                |
| Suspended | Access disabled by System Admin; sign-in and API access blocked |

---

## Session and Token Policy

| Item           | Lifetime            | Notes                                                        |
|----------------|---------------------|--------------------------------------------------------------|
| Cookie session | Sliding, configurable (default 8 hours idle, 7 days absolute) | HttpOnly, Secure, SameSite=Lax |
| API access token | ~5 minutes        | Signed JWT minted by the Admin app from the cookie principal; never stored; carries user id, role and store id |

- Lifetimes are configurable via application settings.
- There are **no refresh tokens** and no token table. A new access token is minted when needed
  from the live session.
- On each mint, the user's status is re-checked against the database (with a short cache), so a
  suspension or rejection takes effect within minutes.
- The API validates issuer, audience, signature and expiry, and rejects tokens not minted by
  the ThriftMedia apps. The signing key is a secret supplied through configuration and rotated
  per the ADR.

---

## External Identity Provider Flow

1. User selects Google or Microsoft on the sign-in page.
2. User is redirected to the provider's OIDC consent page.
3. On callback, the system:
   - Matches the provider identity (provider + subject) to an existing ThriftMedia user, **or**
   - On first sign-in, links the provider identity to a pre-created, approved user by
     matching the **verified** email address (`email_verified` must be true), **or**
   - Rejects the sign-in with an "account not registered / pending" message if no matching
     approved user exists.
4. The System Admin signs in only when the provider identity matches the configured allowlist.

---

## Acceptance Criteria

| ID     | Criterion                                                                                 |
|--------|-------------------------------------------------------------------------------------------|
| AC-01  | A user with no matching approved account is denied sign-in with a generic message (no account-existence leak beyond "not registered or pending"). |
| AC-02  | Successful federated sign-in establishes a secure cookie session.                         |
| AC-03  | The Blazor apps call the API with a short-lived signed token; the API rejects expired, unsigned, or wrong-audience tokens with `401`. |
| AC-04  | An expired cookie session forces re-authentication.                                       |
| AC-05  | Sign-out ends the session; subsequent API calls from that session are not possible.       |
| AC-06  | Suspended accounts cannot sign in; API access ends within the status-cache window.        |
| AC-07  | Store Admin API calls can only affect data belonging to their own store (store ID claim).  |
| AC-08  | System Admin can access all store data.                                                   |
| AC-09  | Anonymous users can access the public website and media search endpoint; all other API endpoints return `401`. |
| AC-10  | Application form submission stores all required fields and sets status to `Pending`.     |
| AC-11  | Approval email is sent when status transitions to `Approved`.                             |
| AC-12  | Rejection email is sent when status transitions to `Rejected`.                            |
| AC-13  | Suspension email is sent when status transitions to `Suspended`.                          |
| AC-14  | The System Admin user is established from the configured allowlist; no password is seeded. |
| AC-15  | First sign-in links the provider identity to an approved user only by verified email.     |
| AC-16  | An account whose email is not the allowlisted System Admin cannot obtain the SystemAdmin role. |
| AC-17  | All role and store-scope checks are enforced server-side; UI-only checks are not sufficient. |

---

## Non-Functional Requirements

1. **Security**: ThriftMedia stores no passwords or provider tokens beyond what the cookie
   session needs. OAuth client secrets and the token signing key are never committed.
2. **Security**: Cookies are HttpOnly, Secure and SameSite; the access token is never exposed
   to the browser.
3. **Audit**: Sign-ins, denied sign-ins, sign-outs, and application status changes are logged
   (structured logging per the platform logging policy). Never log tokens.
4. **Performance**: API request validation must not require a database round-trip on every
   request (use JWT signature verification; only token minting re-checks user status, cached).

---

## Implementation Notes

> These are architectural hints for developers, not binding requirements.

- No ASP.NET Core Identity; no EF Core. Persistence is Dapper with DbUp scripts.
- Use ASP.NET Core's `AddCookie`, `AddGoogle`, and `AddMicrosoftAccount` handlers in the Admin app.
- Local tables (DbUp scripts 0002, 0003, 0005, 0006): `auth."AppUsers"` (provider, provider subject,
  email, role: 0 SystemAdmin / 1 StoreAdmin, status: 0 Approved / 1 Active / 2 Suspended; provider
  columns are null until first sign-in), `retail."Store"` (`AppUserId` unique links a store to its
  StoreAdmin), and `retail."StoreApplication"`. There is no separate membership table in v1.
- The Admin app mints the JWT via a `DelegatingHandler` on the typed API client, reading the
  user from `AuthenticationStateProvider` (not `HttpContext`, which is unreliable in Blazor
  Server circuits).
- Use the **storeId claim** inside the JWT to enforce multi-tenant isolation at the
  handler/query level (do not rely solely on the URL parameter).
- Application-layer code uses an `ICurrentUser` abstraction; the Api project supplies the
  implementation from claims. Handlers also re-check the System Admin role for admin-only operations
  (defense in depth).
- Sign-in resolution: after the provider callback, the Admin app sends the provider's claims (subject,
  email, `email_verified`, MFA indicator) to an API sign-in endpoint backed by `ResolveSignInCommand`,
  authenticated with a short-lived token signed by the app. The command links the identity to the
  approved account by verified email, provisions the allowlisted System Admin (provider and email from
  configuration, never committed), and returns the role and store id for the session.
- Public application submission always reports success, even for a duplicate email, so it cannot be
  used to discover which emails are registered.
- Use **ThriftMedia.Mediator** commands/queries for application workflow actions
  (e.g., `ApproveStoreApplicationCommand`, `SuspendStoreAdminCommand`).
- Email notifications use the platform's shared email service abstraction.

---

## Dependencies

1. Product-level requirements: `docs/requirements/product-requirements.md`
2. Applicable ADRs: `docs/adr/`
3. Platform logging policy (to be authored).
4. Platform email service abstraction (to be authored).
5. Google and Microsoft OAuth app registrations (manual setup outside the repo).

---

## Open Questions

| # | Question                                                                         | Owner         |
|---|----------------------------------------------------------------------------------|---------------|
| 1 | Should re-activation after suspension require a new application or simply admin action? | Product |
| 2 | Should the System Admin be notified in-app (badge/alert) when new applications arrive? | Product |
| 3 | How is the Store Admin's first sign-in tied to an approved application (invite link vs. email match only)? | Product |

## Local setup (secrets)

No secret or the System Admin email is committed. The AppHost reads them from its user-secrets and passes them to the Api and Admin as environment variables:

```sh
dotnet user-secrets set "Parameters:google-client-id" "<id>" --project src/ThriftMedia.AppHost
dotnet user-secrets set "Parameters:google-client-secret" "<secret>" --project src/ThriftMedia.AppHost
dotnet user-secrets set "Parameters:microsoft-client-id" "<id>" --project src/ThriftMedia.AppHost
dotnet user-secrets set "Parameters:microsoft-client-secret" "<secret>" --project src/ThriftMedia.AppHost
dotnet user-secrets set "Parameters:system-admin-email" "<your email>" --project src/ThriftMedia.AppHost
# optional; defaults to Microsoft
dotnet user-secrets set "Parameters:system-admin-provider" "Microsoft" --project src/ThriftMedia.AppHost
```

- The token signing key (`Auth:SigningKey`, shared by the Api and Admin) is generated on first run and persisted to the same user-secrets.
- A sign-in provider appears on the Admin sign-in page only when both its client id and secret are set.
- Register these redirect URIs with each provider: `https://<admin-host>/signin-google` and `https://<admin-host>/signin-microsoft`.
- Microsoft sign-in is limited to personal Microsoft accounts (the `consumers` tenant).
- Without `system-admin-email`, nobody can sign in as System Admin.
