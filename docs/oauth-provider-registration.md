# Registering ThriftMedia with Google and Microsoft

ThriftMedia signs users in with Google and Microsoft only (no passwords are stored). Each provider needs an OAuth app registration that gives you a **client ID** and a **client secret**. This guide covers both, for local development and for production.

Only the **Admin app** signs people in. The Api and the public Web app never talk to the providers.

Related: [launch-guide.md](launch-guide.md) (where to put the values), [requirements/features/authentication-authorization.md](requirements/features/authentication-authorization.md).

Provider consoles change their layout from time to time. The steps below describe the intent; if a menu is named slightly differently, look for the same setting.

## 0. Values you need first

| Item | Local development | Production |
|---|---|---|
| Admin base URL | `https://localhost:7031` (confirm in the Aspire dashboard) | your real Admin host, for example `https://admin.example.com` |
| Google redirect URI | `https://localhost:7031/signin-google` | `https://<admin-host>/signin-google` |
| Microsoft redirect URI | `https://localhost:7031/signin-microsoft` | `https://<admin-host>/signin-microsoft` |

The redirect URI must match **exactly**: scheme, host, port, path and case, with no trailing slash. These paths are fixed by the ASP.NET Core handlers.

Use **separate registrations for development and production**. Never reuse a production secret locally.

## 1. Google

### 1.1 Create the project and consent screen

1. Go to <https://console.cloud.google.com> and sign in with the Google account that will own the registration.
2. Create a project (for example `ThriftMedia`) from the project picker.
3. Open **APIs & Services** → **OAuth consent screen** (also shown as **Google Auth Platform**) and start the setup.
4. **App information**: name `ThriftMedia`, and a support email address.
5. **Audience / User type**: choose **External**.
6. **Contact information**: a developer email address.
7. Scopes: ThriftMedia only needs the non-sensitive `openid`, `email` and `profile` scopes, which are included by default. No additional scopes are required, so Google's app verification review is not triggered by scopes.

### 1.2 Testing vs production publishing

- While the app is in **Testing** status, only accounts listed under **Test users** can sign in. Add your own Google account and any pilot store owners' addresses.
- Before real users sign in, set the publishing status to **In production**. With only basic scopes this normally does not require a lengthy verification, but you may be asked to provide a privacy policy URL and an authorized domain. Prepare these before launch.

### 1.3 Create the OAuth client

1. Open **Credentials** (or **Clients**) → **Create credentials** → **OAuth client ID**.
2. Application type: **Web application**. Name it for the environment, for example `ThriftMedia Admin (dev)`.
3. **Authorized redirect URIs**: add `https://localhost:7031/signin-google` (dev) or `https://<admin-host>/signin-google` (production). Authorized JavaScript origins are not needed.
4. Create. Copy the **Client ID** and **Client secret**. Store the secret immediately; treat it like a password.

### 1.4 Give the values to the app

```sh
dotnet user-secrets set "Parameters:google-client-id"     "<client id>"     --project src/ThriftMedia.AppHost
dotnet user-secrets set "Parameters:google-client-secret" "<client secret>" --project src/ThriftMedia.AppHost
```

The app requires Google to report `email_verified = true` before it will link an account.

## 2. Microsoft

ThriftMedia accepts **personal Microsoft accounts only** (outlook.com, hotmail.com, live.com and similar). Work and school accounts are intentionally excluded because their email claims can be unverified and changeable. The app is hard-wired to the `consumers` endpoint, so the registration must allow personal accounts.

### 2.1 Register the application

1. Go to the Microsoft Entra admin center, <https://entra.microsoft.com>, and sign in. App registrations require a directory (tenant). If your account has none, sign up for a free Azure account or create a free Entra tenant; registering an app costs nothing.
2. Open **Identity** → **Applications** → **App registrations** → **New registration**.
3. **Name**: `ThriftMedia Admin (dev)` (or production).
4. **Supported account types**: choose **Personal Microsoft accounts only**.
   - If that option is not offered, pick the multi-tenant-plus-personal option, register, then in **Manifest** set `signInAudience` to `PersonalMicrosoftAccount`.
5. **Redirect URI**: platform **Web**, value `https://localhost:7031/signin-microsoft` (dev) or `https://<admin-host>/signin-microsoft` (production).
6. Click **Register**.
7. On the **Overview** page copy the **Application (client) ID**.

### 2.2 Create a client secret

1. Open **Certificates & secrets** → **Client secrets** → **New client secret**.
2. Choose a description and an expiry. Maximum is 24 months.
3. Copy the secret **Value** immediately. It is shown only once. Do not copy the *Secret ID* by mistake.
4. **Put a calendar reminder before the expiry date.** When it expires, sign-in with Microsoft stops working until a new secret is configured. Create the replacement before the old one expires, update the setting, then delete the old one.

### 2.3 Permissions

The default delegated permission **Microsoft Graph → User.Read** is all that is needed (the app reads the user's id, display name and email from Graph). No admin consent is required for personal accounts.

### 2.4 Give the values to the app

```sh
dotnet user-secrets set "Parameters:microsoft-client-id"     "<application (client) id>" --project src/ThriftMedia.AppHost
dotnet user-secrets set "Parameters:microsoft-client-secret" "<secret value>"            --project src/ThriftMedia.AppHost
```

## 3. The System Admin account

The System Admin is a single person, identified by provider and email:

```sh
dotnet user-secrets set "Parameters:system-admin-email"    "<system admin email>" --project src/ThriftMedia.AppHost
dotnet user-secrets set "Parameters:system-admin-provider" "Microsoft"            --project src/ThriftMedia.AppHost
```

- The provider name must be exactly `Microsoft` or `Google`.
- The email must be the address that provider returns for the account (for a Microsoft account, usually the outlook.com or hotmail.com address).
- Turn on multi-factor authentication on that provider account. This is the control that protects the System Admin; the app cannot enforce it itself.
- The email is never committed to the repository.

## 4. Verifying the registration

1. Start the app (see the launch guide) and open the Admin app.
2. Click **Sign in with Google** or **Sign in with Microsoft**. You should be taken to the provider's consent page showing the app name you registered.
3. After consenting you return to the Admin app. The System Admin account lands on `/system`. Any other account with no approved store sees "We couldn't sign you in".

| Error | Cause |
|---|---|
| Google `redirect_uri_mismatch` | The URI in the Google client does not exactly match the one the app sends. Compare scheme, port and path. |
| Google `access_denied` or "app not verified" for your own account | App is in Testing status and the account is not in the Test users list, or the app needs to be published. |
| Microsoft `AADSTS50011` | Redirect URI mismatch. |
| Microsoft `AADSTS700016` or "application not found" | Wrong client ID, or the registration does not allow personal accounts. |
| Microsoft `AADSTS7000215` "Invalid client secret" | The secret **ID** was used instead of the **Value**, or the secret expired. |
| Microsoft `AADSTS50020` | A work or school account was used. Use a personal Microsoft account. |
| Signed in at the provider but refused by ThriftMedia | The account is not an approved Store Admin and does not match the System Admin settings, or the email is not verified. |

## 5. Production notes

- Create dedicated production registrations with the production redirect URIs; add the production host as an authorized domain in Google.
- Store secrets in the hosting platform's secret store (not in source control, not in appsettings files) as `Authentication__Google__ClientId`, `Authentication__Google__ClientSecret`, `Authentication__Microsoft__ClientId`, `Authentication__Microsoft__ClientSecret`, and the shared `Auth__SigningKey`.
- Production must serve the Admin app over HTTPS. Redirect URIs for non-localhost hosts must use `https`.
- Review the Google consent-screen branding (logo, privacy policy and terms links) before inviting real users.
- Keep a record of who owns each registration and when each secret expires.
