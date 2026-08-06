# Feature Requirements: Authentication and Authorization

## Status
Active

## Overview

ThriftMedia supports brick-and-mortar secondhand media stores. To protect store data and
administrative functions, all API and administration surfaces require authenticated and
authorized access. The only public surface is the media search page, which is accessible
anonymously.

---

## Goals

1. Allow verified store owners (Store Admins) to manage their store's inventory and data.
2. Allow the System Admin to oversee all stores, manage applications, and configure the platform.
3. Keep the public media search page fully anonymous.
4. Issue short-lived JWT access tokens and long-lived refresh tokens so authenticated users
   stay logged in without frequent re-authentication.
5. Support external identity providers (Google, Facebook, Microsoft Live) alongside
   local credentials.
6. Enforce multi-tenant isolation so each Store Admin can only access their own store's data.

---

## Out of Scope (Initial Slice)

1. Microsoft Entra (Azure AD) integration.
2. Multi-factor authentication (MFA).
3. Fine-grained permission editor UI (beyond role assignment).
4. Self-service password reset via SMS/2FA.

---

## Roles

| Role         | Description                                                                  |
|--------------|------------------------------------------------------------------------------|
| SystemAdmin  | Single bootstrapped account. Full platform access. Manages store applications.|
| StoreAdmin   | One account per approved store. Scoped to their own store's data only.       |
| Anonymous    | Unauthenticated user. Read-only access to the media search page.             |

### System Admin Bootstrap
- A single `SystemAdmin` account is **seeded at application startup** from environment
  variables / configuration (username + initial password).
- The System Admin account cannot be deleted through the UI.
- The System Admin can change their own password through the admin UI.

---

## Access Control Matrix

| Surface                              | Anonymous | StoreAdmin       | SystemAdmin |
|--------------------------------------|-----------|------------------|-------------|
| Media Search page (Web)              | ✅ Read   | ✅ Read          | ✅ Read     |
| Store Admin UI (`/admin/store/*`)    | ❌        | ✅ Own store only | ✅ All      |
| System Admin UI (`/admin/system/*`) | ❌        | ❌               | ✅          |
| All API endpoints (except search)   | ❌        | ✅ Own store only | ✅ All      |
| Media Search API endpoint            | ✅ Read   | ✅ Read          | ✅ Read     |

---

## User Stories

### Authentication

**AUTH-1 — Local Sign-In**
> As any user, I can sign in with my email address and password so that I receive a JWT
> access token and refresh token.

**AUTH-2 — External Provider Sign-In**
> As any user, I can sign in using Google, Facebook, or Microsoft Live so that I do not
> need to manage a separate password for ThriftMedia.

**AUTH-3 — Token Refresh**
> As an authenticated user, my client can exchange a valid refresh token for a new access
> token so that I stay logged in without re-entering credentials.

**AUTH-4 — Sign-Out**
> As an authenticated user, I can sign out so that my refresh token is revoked and I am
> no longer authenticated.

**AUTH-5 — Password Change**
> As an authenticated user, I can change my password so that I can maintain account
> security.

### Store Admin Application Workflow

**APP-1 — Apply for a Store Account**
> As a prospective store owner, I can submit a store application from a public "Apply"
> page, providing the information required to verify my physical store location, so that
> the System Admin can review my request.

**APP-2 — Review Application**
> As the System Admin, I can view a list of pending store applications and approve or
> deny each one so that only legitimate brick-and-mortar stores gain access.

**APP-3 — Application Status Notifications**
> As an applicant, I receive an email when my application is approved, denied, or when my
> account is later suspended so that I am always informed of my account status.

**APP-4 — Account Suspension**
> As the System Admin, I can suspend an active Store Admin account so that a store that
> violates policy loses access without their data being deleted.

**APP-5 — Account Re-activation**
> As the System Admin, I can re-activate a suspended Store Admin account so that a store
> that resolves a policy violation can regain access.

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
| Email address          | Used as the login credential                 |
| Phone number           |                                              |
| Physical street address |                                             |
| City, State, ZIP       |                                              |
| Country                |                                              |
| Store website URL      | Optional but helps verification              |
| Brief store description| What types of secondhand media are sold      |

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
| Approved  | System Admin approved; account created and credentials sent  |
| Rejected  | System Admin denied the application                          |
| Active    | Store Admin can log in and manage their store                |
| Suspended | Account disabled by System Admin; login blocked              |

---

## JWT Token Policy

| Token         | Lifetime      | Storage recommendation           |
|---------------|---------------|----------------------------------|
| Access Token  | 15 minutes    | In-memory / HTTP-only cookie     |
| Refresh Token | 7 days        | HTTP-only secure cookie          |

- Token lifetimes are configurable via application settings.
- Refresh tokens are **single-use** (rotation); a used refresh token is revoked.
- Refresh tokens are stored (hashed) server-side to support revocation on sign-out and
  account suspension.
- On suspension or rejection, all active refresh tokens for the user are immediately
  revoked.

---

## External Identity Provider Flow

1. User selects an external provider (Google, Facebook, Microsoft Live) on the sign-in page.
2. User is redirected to the provider's OAuth2/OIDC consent page.
3. On callback, the system either:
   - Matches the provider identity to an existing ThriftMedia account (by email) and
     issues tokens, **or**
   - Creates a new local identity linked to the external provider (for Store Admins this
     only happens after their application is approved and the System Admin creates the
     account — external login is linked at first sign-in).
4. The System Admin account does **not** support external provider login (local credentials
   only, for security).

---

## Acceptance Criteria

| ID     | Criterion                                                                                 |
|--------|-------------------------------------------------------------------------------------------|
| AC-01  | Invalid credentials return `401` with a generic error message (no user-enumeration leak). |
| AC-02  | Valid credentials return a signed JWT access token and a refresh token.                   |
| AC-03  | Expired access token + valid refresh token returns a new access token.                    |
| AC-04  | Expired or revoked refresh token forces re-authentication.                                |
| AC-05  | Sign-out revokes the refresh token server-side.                                           |
| AC-06  | Suspended accounts cannot authenticate; existing tokens are revoked.                      |
| AC-07  | Store Admin API calls can only affect data belonging to their own store (store ID claim). |
| AC-08  | System Admin can access all store data.                                                   |
| AC-09  | Anonymous users can access the media search endpoint; all others return `401`.            |
| AC-10  | Application form submission stores all required fields and sets status to `Pending`.      |
| AC-11  | Approval email is sent when status transitions to `Approved`.                             |
| AC-12  | Rejection email is sent when status transitions to `Rejected`.                            |
| AC-13  | Suspension email is sent when status transitions to `Suspended`.                          |
| AC-14  | System Admin seed account is created on first startup if it does not exist.               |
| AC-15  | External provider login links to an existing account by matching verified email.          |
| AC-16  | System Admin account does not support external provider login.                            |
| AC-17  | All role and store-scope checks are enforced server-side; UI-only checks are not sufficient. |

---

## Non-Functional Requirements

1. **Security**: Passwords are hashed using ASP.NET Core Identity's default PBKDF2 algorithm.
2. **Security**: Refresh tokens are stored as hashed values; plaintext tokens are never persisted.
3. **Audit**: Failed sign-in attempts, sign-outs, token revocations, and application status
   changes are logged (structured logging per the platform logging policy).
4. **Performance**: Token validation must not require a database round-trip on every request
   (use JWT signature verification; only refresh/revocation requires DB access).

---

## Implementation Notes

> These are architectural hints for developers, not binding requirements.

- Use **ASP.NET Core Identity** for user/role management and password hashing.
- Issue and validate **JWT Bearer tokens** in the API.
- Store **refresh tokens** (hashed) in a dedicated `RefreshTokens` table managed by DbUp.
- Store **store applications** in a `StoreApplications` table.
- Use the **storeId claim** inside the JWT to enforce multi-tenant isolation at the
  handler/query level (do not rely solely on the URL parameter).
- External provider integration via ASP.NET Core's `AddGoogle`, `AddFacebook`,
  `AddMicrosoftAccount` OAuth handlers.
- Use **ThriftMedia.Mediator** commands/queries for application workflow actions
  (e.g., `ApproveStoreApplicationCommand`, `SuspendStoreAdminCommand`).
- Email notifications use the platform's shared email service abstraction.

---

## Dependencies

1. Product-level requirements: `docs/requirements/product-requirements.md`
2. Applicable ADRs: `docs/adr/`
3. Platform logging policy (to be authored).
4. Platform email service abstraction (to be authored).

---

## Open Questions

| # | Question                                                                         | Owner         |
|---|----------------------------------------------------------------------------------|---------------|
| 1 | Should re-activation after suspension require a new application or simply admin action? | Product |
| 2 | Should the System Admin be notified in-app (badge/alert) when new applications arrive? | Product |
| 3 | Is a "forgot password" self-service flow required for the initial release?        | Product       |
| 4 | Should external provider accounts bypass any part of the store application workflow? | Product  |
