using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using ThriftMedia.Contracts.Auth;
using ThriftMedia.Contracts.Dto;

namespace ThriftMedia.Admin.Auth;

public static class AdminAuthExtensions
{
    public const string CookieScheme = "ThriftMediaAdmin";
    public const string ExternalScheme = "ThriftMediaExternal";

    /// <summary>
    /// Federated sign-in (Google, Microsoft) with a cookie session. A provider is registered only when its
    /// client id/secret are configured (<c>Authentication:Google:*</c>, <c>Authentication:Microsoft:*</c>).
    /// Redirect URIs to register with the providers: <c>{origin}/signin-google</c> and <c>{origin}/signin-microsoft</c>.
    /// </summary>
    public static WebApplicationBuilder AddAdminAuthentication(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddOptions<TokenOptions>()
            .BindConfiguration(TokenOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMemoryCache();
        services.AddSingleton<ApiTokenService>();
        services.AddTransient<AccountStatusChecker>();
        services.AddScoped<ApiTokenProvider>();

        services.AddHttpClient<SignInClient>(c => c.BaseAddress = new Uri("https+http://api"));
        services.AddHttpClient<AdminApiClient>(c => c.BaseAddress = new Uri("https+http://api"));

        var auth = services.AddAuthentication(CookieScheme)
            .AddCookie(CookieScheme, options =>
            {
                options.Cookie.Name = ".ThriftMedia.Admin";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = false;
                options.LoginPath = "/signin";
                options.AccessDeniedPath = "/access-denied";
                options.Events.OnValidatePrincipal = RevalidateAsync;
            })
            // Holds the provider's identity only for the moment between the provider callback and our own checks.
            .AddCookie(ExternalScheme, options =>
            {
                options.Cookie.Name = ".ThriftMedia.Admin.External";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            });

        var google = builder.Configuration.GetSection("Authentication:Google");
        if (!string.IsNullOrWhiteSpace(google["ClientId"]) && !string.IsNullOrWhiteSpace(google["ClientSecret"]))
        {
            auth.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = ExternalScheme;
                options.ClientId = google["ClientId"]!;
                options.ClientSecret = google["ClientSecret"]!;
                options.ClaimActions.MapJsonKey("email_verified", "email_verified");
            });
        }

        var microsoft = builder.Configuration.GetSection("Authentication:Microsoft");
        if (!string.IsNullOrWhiteSpace(microsoft["ClientId"]) && !string.IsNullOrWhiteSpace(microsoft["ClientSecret"]))
        {
            auth.AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = ExternalScheme;
                options.ClientId = microsoft["ClientId"]!;
                options.ClientSecret = microsoft["ClientSecret"]!;
                // Personal Microsoft accounts only. Work/school (Entra) accounts can carry unverified, mutable
                // email claims, which this app trusts for first-sign-in linking. Revisit if business accounts are needed.
                options.AuthorizationEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
                options.TokenEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
            });
        }

        services.AddAuthorizationBuilder()
            .AddPolicy(AdminPolicies.SystemAdmin, p => p
                .RequireAuthenticatedUser()
                .RequireClaim(AdminClaims.Role, ApiTokenDefaults.RoleSystemAdmin))
            .AddPolicy(AdminPolicies.StoreMember, p => p
                .RequireAuthenticatedUser()
                .RequireClaim(AdminClaims.Role, ApiTokenDefaults.RoleStoreAdmin));

        services.AddCascadingAuthenticationState();
        return builder;
    }

    /// <summary>Per request: if the Api says the account is no longer allowed (suspended etc.), end the session.</summary>
    private static async Task RevalidateAsync(CookieValidatePrincipalContext context)
    {
        var checker = context.HttpContext.RequestServices.GetRequiredService<AccountStatusChecker>();
        var status = await checker.RecheckAsync(context.Principal!, context.HttpContext.RequestAborted);

        // null means the Api is unreachable: keep the session rather than sign everyone out during an outage.
        if (status is not null && status.Outcome != SignInOutcome.Succeeded)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieScheme);
        }
    }
}
