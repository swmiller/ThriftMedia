# Punch List: Open Issues and Gaps

Status as of the federated-authentication work on branch `gap_cleanup` (plan steps 1–11 complete).
All code builds and 98 automated tests pass, but several pieces have **never run against real infrastructure**.

Legend: **P0** blocks a first real run, **P1** needed before any pilot users, **P2** needed before public launch, **P3** nice to have.

## 1. Unverified (never run for real)

| # | Pri | Item | Notes |
|---|-----|------|-------|
| 1.1 | P0 | DbUp scripts `0005_AddAppUserStatus.sql` and `0006_CreateStoreApplications.sql` | Written without a Postgres available. Run the DbMigrator against a real database and fix any SQL errors. |
| 1.2 | P0 | Dapper repositories (`AppUserRepository`, `StoreApplicationRepository`, `StoreRepository`) | Only exercised through in-memory fakes. Need integration tests against Postgres (for example Testcontainers). Includes the UTC `timestamp` handling in `DbTime`. |
| 1.3 | P0 | Real Google and Microsoft sign-in | Needs OAuth registrations ([oauth-provider-registration.md](oauth-provider-registration.md)). Confirm the Microsoft personal-account profile returns your address in `mail` or `userPrincipalName`, and that Google returns `email_verified`. |
| 1.4 | P0 | AppHost parameter wiring | Builds, but has not been started. Confirm the generated signing key is persisted to user-secrets (`persist: true`) and both the Api and Admin receive the same `Auth__SigningKey`. |
| 1.5 | P1 | Postgres password across restarts | Data is bind-mounted at `c:\ThriftMediaDb`. If Aspire generates a new random Postgres password on each run, the second run can fail to authenticate against the existing data directory. Verify; if it happens, pin `Parameters:postgres-password` in AppHost user-secrets. |
| 1.6 | P1 | Existing dev database rows | Migration 0002 defaulted `Role` to `0`, which is now `SystemAdmin`. If a dev database already has `auth."AppUsers"` rows, check none is wrongly a System Admin before running 0005. |

## 2. Missing functionality

| # | Pri | Item | Notes |
|---|-----|------|-------|
| 2.1 | P1 | Store handlers are commented out | The `/api/stores` endpoints compile but have no handlers (`TODO: EF Core removed`). Reimplement with Dapper and **scope every query to the caller's `storeId` claim** (replace the `TODO: replace with authenticated user` spots). |
| 2.2 | P1 | Store endpoint policies | Existing store routes are protected only by the deny-by-default fallback (any valid token, including the System Admin). Add `RequireAuthorization(AuthPolicies.StoreMember)` and tests for cross-store 403/404. |
| 2.3 | P1 | Public "Apply" page | The Api accepts `POST /api/store-applications`, but the public Web app has no form. |
| 2.4 | P1 | License image storage | Applications store only a reference (`LicenseImageRef`, `StoreFrontImageRef`). File upload, storage location, size/type limits and malware scanning are undecided. |
| 2.5 | P1 | System Admin UI | `/system` shows only the pending count. Needs a review list, approve (with license type, issuing authority, dates), reject, and suspend/reactivate screens. The Admin API client currently exposes only the pending-list call. |
| 2.6 | P1 | Real email provider | `LoggingEmailSender` only logs. Approval, rejection, suspension and reactivation emails are not actually sent. Pick a provider and configure sender domain/SPF/DKIM. |
| 2.7 | P2 | Media search endpoint | Does not exist yet. When added it must be explicitly `AllowAnonymous`; everything else is deny-by-default. The Admin `Media` page is still sample data. |
| 2.8 | P2 | Workers | Deferred by decision. One Store Admin per store; `StoreMember` currently equals `StoreAdmin`. |
| 2.9 | P2 | Store visibility on suspension | Suspending a Store Admin account does not touch `Store.IsSuspended`/`IsActive`. Decide whether suspending the user should also hide the store from public search. |
| 2.10 | P3 | Account changes | No flow for a Store Admin to change email or switch sign-in provider. A different provider with the same email currently gets "not registered" once an identity is linked. |
| 2.11 | P3 | Audit trail | Approve, reject, suspend and reactivate record reviewer/time on the application only. No general audit log. Confirm whether the requirements need one. |

## 3. Security and operations

| # | Pri | Item | Notes |
|---|-----|------|-------|
| 3.1 | P1 | Admin MFA is operational only | The Microsoft and Google paths report no `amr`, so the System Admin check allows the sign-in. Turn on MFA for the System Admin's Microsoft account and keep it on. |
| 3.2 | P1 | Microsoft work/school accounts unsupported | Sign-in is restricted to personal Microsoft accounts (`consumers` tenant) because Entra email claims can be unverified. A store owner with a Microsoft 365 business email must use Google. Decide if that is acceptable. |
| 3.3 | P1 | Signing key handling | One shared HS256 key between Api and Admin. No documented rotation procedure. Define how to rotate (generate new, deploy both, tokens live only 5 minutes) and where the production secret lives. |
| 3.4 | P1 | Client secret expiry | Microsoft client secrets expire (maximum 24 months). Set a reminder and rotation steps. |
| 3.5 | P1 | Blazor circuit revalidation | The cookie is rejected within about a minute of suspension on the next HTTP request, but an already-open interactive circuit (for example `/media`) keeps its original auth state until the page reloads. API calls still fail because the status re-check blocks token minting. Consider a revalidating `AuthenticationStateProvider`. |
| 3.6 | P2 | Rate limiting behind a proxy | The public submit endpoint is limited to 5 requests per minute per client IP. Behind a reverse proxy, configure forwarded headers or every caller shares the proxy's address. The limiter is also per instance, not shared. |
| 3.7 | P2 | Production secrets and hosting | No deployment target chosen. Secrets currently come from AppHost user-secrets only. |
| 3.8 | P2 | Cookie settings in production | The session cookie is `Secure` outside Development. Confirm HTTPS termination and HSTS at the real host. Session length is a fixed 8 hours. |
| 3.9 | P2 | Dependency warning | `NU1902` for `MessagePack 2.5.302` (moderate) comes in through the Aspire testing package referenced by the test project. Update or suppress deliberately. |
| 3.10 | P3 | Login throttling | Federated sign-in has no app-level throttle on `/signin/{provider}`. Providers enforce their own lockouts. |

## 4. Code quality and cleanup

| # | Pri | Item | Notes |
|---|-----|------|-------|
| 4.1 | P3 | Validation error field names | Responses show `Request.StoreName` instead of `StoreName` because validators target the command wrapper. Cosmetic; fix when building the UI forms. |
| 4.2 | P3 | Approve needs license details | `Store` requires license type, issuing authority and issue date, which the application form does not collect, so the System Admin types them at approval. Alternative: add the fields to the form (needs a new migration). |
| 4.3 | P3 | Doc paths | The feature doc and CLAUDE.md should stay aligned as items here close. Update the Known Gaps section of `CLAUDE.md` when finishing 2.1, 1.1–1.2. |

## 5. Test coverage gaps

| # | Pri | Item |
|---|-----|------|
| 5.1 | P0 | Repository/SQL integration tests against Postgres (see 1.1, 1.2). |
| 5.2 | P1 | Admin sign-in flow tests: callback handling, `email_verified` handling, access-denied reasons, sign-out antiforgery. |
| 5.3 | P1 | Cookie revalidation test (suspended user is signed out within the cache window). |
| 5.4 | P1 | Store data isolation tests (blocked by 2.1). |
| 5.5 | P2 | Rate-limit test for the public submit endpoint. |
| 5.6 | P2 | End-to-end smoke test through the AppHost (needs Docker). |

## Suggested order

1. Start Docker, run the migrations, fix SQL (1.1).
2. Register Google and Microsoft apps, set secrets, run the AppHost and walk the checklist in [launch-guide.md](launch-guide.md) (1.3, 1.4, 1.5).
3. Add Postgres integration tests for the repositories (1.2, 5.1).
4. Reimplement the Store handlers with scoping and policy (2.1, 2.2, 5.4).
5. Build the Apply page, upload storage and email provider (2.3, 2.4, 2.6).
6. Build the System Admin review screens (2.5).
