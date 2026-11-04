# Launch Guide: Running ThriftMedia in Visual Studio

How to run the full app (Postgres, migrations, Api, Admin, Web) from Visual Studio through the Aspire AppHost, and which secrets and settings must be in place first.

Related: [oauth-provider-registration.md](oauth-provider-registration.md) (how to get the Google and Microsoft client IDs), [punch-list.md](punch-list.md) (known gaps), [requirements/features/authentication-authorization.md](requirements/features/authentication-authorization.md).

## 1. Prerequisites

- Visual Studio 2022 17.14 or newer (or Visual Studio 2026) with the **ASP.NET and web development** workload.
- **.NET 10 SDK** (the solution targets `net10.0`).
- **Docker Desktop**, running. Postgres runs in a container.
- The data folder `c:\ThriftMediaDb` is bind-mounted into the Postgres container. Create it if it does not exist and make sure Docker Desktop is allowed to share the `C:` drive.
- Google and Microsoft OAuth client registrations (see the registration guide). Without them the app starts, but nobody can sign in.

## 2. Startup project

1. Open `ThriftMedia.sln`.
2. Right-click **ThriftMedia.AppHost** and choose **Set as Startup Project**.
3. Select the **https** launch profile and press F5 (or Ctrl+F5).

The AppHost starts Postgres, runs `db-migrator`, then starts `api`, `admin` and `web`. The Aspire dashboard opens at `https://localhost:17209`.

Expected URLs (from each project's launch profile; confirm in the dashboard):

| Service | URL |
|---|---|
| Aspire dashboard | `https://localhost:17209` |
| Admin | `https://localhost:7031` |
| Web (public) | `https://localhost:7005` |
| Api | assigned by Aspire; internal, reached by the apps through service discovery as `api` |

The Admin URL matters: it is the base of the redirect URIs you register with Google and Microsoft.

## 3. Secrets and settings

Nothing secret is committed. All values live in the **AppHost user-secrets** and are passed to the services as environment variables. The System Admin email is deliberately not in source control.

### 3.1 Setting the values

Either use Visual Studio: right-click **ThriftMedia.AppHost** → **Manage User Secrets** and paste the JSON below, or use the CLI from the repo root:

```sh
dotnet user-secrets set "Parameters:google-client-id"          "<google client id>"     --project src/ThriftMedia.AppHost
dotnet user-secrets set "Parameters:google-client-secret"      "<google client secret>" --project src/ThriftMedia.AppHost
dotnet user-secrets set "Parameters:microsoft-client-id"       "<application (client) id>" --project src/ThriftMedia.AppHost
dotnet user-secrets set "Parameters:microsoft-client-secret"   "<client secret VALUE>"  --project src/ThriftMedia.AppHost
dotnet user-secrets set "Parameters:system-admin-email"        "<system admin email>"   --project src/ThriftMedia.AppHost
dotnet user-secrets set "Parameters:system-admin-provider"     "Microsoft"              --project src/ThriftMedia.AppHost
```

Equivalent `secrets.json` content:

```json
{
  "Parameters:google-client-id": "<google client id>",
  "Parameters:google-client-secret": "<google client secret>",
  "Parameters:microsoft-client-id": "<application (client) id>",
  "Parameters:microsoft-client-secret": "<client secret value>",
  "Parameters:system-admin-email": "<system admin email>",
  "Parameters:system-admin-provider": "Microsoft"
}
```

### 3.2 What each setting does

| User-secret key | Required? | Passed to | As | Purpose |
|---|---|---|---|---|
| `Parameters:auth-signing-key` | Auto-generated | Api, Admin | `Auth__SigningKey` | Shared HS256 key. The Admin app signs 5-minute API tokens with it and the Api validates them. Generated on first run (48 characters) and saved to user-secrets. Do not set by hand unless rotating. |
| `Parameters:google-client-id` / `google-client-secret` | Only for Google sign-in | Admin | `Authentication__Google__ClientId` / `ClientSecret` | Google appears on the sign-in page only when **both** are set. |
| `Parameters:microsoft-client-id` / `microsoft-client-secret` | Only for Microsoft sign-in | Admin | `Authentication__Microsoft__ClientId` / `ClientSecret` | Microsoft appears only when **both** are set. Use the secret **Value**, not the secret ID. |
| `Parameters:system-admin-email` | Yes, to have a System Admin | Api | `Auth__SystemAdminEmail` | The one account allowed to become System Admin. Without it nobody can sign in as System Admin. |
| `Parameters:system-admin-provider` | Optional (default `Microsoft`) | Api | `Auth__SystemAdminProvider` | The provider that email must sign in with. Case-sensitive: `Microsoft` or `Google`. |
| `Parameters:postgres-password` | Only if needed | Postgres | n/a | Pin this if Postgres rejects the password on a second run against the existing `c:\ThriftMediaDb` data (see Troubleshooting). |

Defaults that need no setting: issuer `thriftmedia-apps`, API audience `thriftmedia-api`, sign-in audience `thriftmedia-api-signin`, and the database connection string (injected by Aspire as `ConnectionStrings__ThriftMediaDb`).

### 3.3 Who is the System Admin

The first time the account matching `system-admin-email` signs in through `system-admin-provider`, the Api creates the single System Admin record. Afterwards the allowlist is re-checked on every sign-in, so changing or clearing the setting removes access on the next check (within about a minute for open sessions).

Turn on multi-factor authentication for that provider account. The app cannot verify MFA for Google or Microsoft personal accounts; it relies on the provider.

## 4. First-run checklist

1. Docker Desktop is running.
2. User-secrets are set (section 3).
3. Start the AppHost. In the dashboard, wait for `postgres` and `db-migrator` to finish (db-migrator should end as *Finished*, not *Failed*) and for `api`, `admin`, `web` to be *Running*.
4. Open `https://localhost:7031`. You should see **Sign in with Google** and/or **Sign in with Microsoft** (one button per configured provider). If you see "No sign-in providers are configured", the client id/secret pair for that provider is missing.
5. Sign in as the System Admin (the configured provider and email). You should land on `/system` showing "Pending store applications: 0".
6. Open `https://localhost:7031/whoami` (Development only). The session claims should show `role = SystemAdmin`, and the freshly minted API token claims should show the same role. If the token section is empty, the Api rejected or could not be reached for the status re-check.
7. Sign out from the navigation menu. Visiting `/system`, `/store` or `/media` while signed out should redirect to `/signin`.
8. Optional: submit a test application to the Api (the public Apply page does not exist yet), for example with the Api's Swagger UI in the dashboard (Development only) calling `POST /api/store-applications`.

## 5. Running a single project outside the AppHost

Not recommended, because the apps expect Aspire for the database connection and service discovery. If you must, set these environment variables yourself:

| Project | Variables |
|---|---|
| Api | `ConnectionStrings__ThriftMediaDb`, `Auth__SigningKey` (32+ characters), `Auth__SystemAdminEmail`, `Auth__SystemAdminProvider` |
| Admin | `Auth__SigningKey` (identical to the Api's), `Authentication__Google__*`, `Authentication__Microsoft__*`, and `services__api__https__0` (or `__http__0`) pointing at the running Api |

The Api refuses to start if `Auth:SigningKey` is missing or shorter than 32 characters.

## 6. Troubleshooting

| Symptom | Likely cause and fix |
|---|---|
| Api fails at startup with an options validation error about `SigningKey` | `Auth__SigningKey` did not reach the Api. Start through the AppHost, or set it. |
| Admin sign-in page says no providers configured | Set both the client id **and** secret for at least one provider, then restart the AppHost. |
| Google error `redirect_uri_mismatch` or Microsoft `AADSTS50011` | The registered redirect URI does not exactly match `https://localhost:7031/signin-google` or `/signin-microsoft`. Compare against the actual Admin URL in the dashboard. |
| After login you land on **We couldn't sign you in** | The sign-in was refused. Common cause: your email is not an approved Store Admin and is not the configured System Admin. Check `system-admin-email` and `system-admin-provider` (case-sensitive provider). |
| Message about email not verified | Google reported `email_verified = false`, or Microsoft returned no email address. Use an account with a verified address. |
| `db-migrator` fails | Read its log in the dashboard. The 0005/0006 scripts have not been run against Postgres before; report or fix the SQL error. |
| Postgres authentication failed on the second start | The data in `c:\ThriftMediaDb` was initialised with an earlier generated password. Set `Parameters:postgres-password` to a fixed value and, if it still fails, stop the AppHost and clear the folder (this deletes all local data). |
| Browser warns about the dev HTTPS certificate | Run `dotnet dev-certs https --trust`. |
| Signed in but `/whoami` shows no token claims | The Api is unreachable, or the account is not allowed (suspended, removed from the allowlist). Check the Api log. |
