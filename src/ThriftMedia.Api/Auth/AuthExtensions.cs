using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ThriftMedia.Application.Abstractions;
using ThriftMedia.Contracts.Auth;

namespace ThriftMedia.Api.Auth;

public static class AuthPolicies
{
    public const string SystemAdmin = "SystemAdmin";
    public const string StoreAdmin = "StoreAdmin";

    /// <summary>Any member of a store. Same as <see cref="StoreAdmin"/> until Workers are added.</summary>
    public const string StoreMember = "StoreMember";

    /// <summary>The sign-in resolution call, authenticated by an app-signed sign-in token only.</summary>
    public const string SignIn = "SignIn";
}

public static class AuthExtensions
{
    internal const string ApiScheme = "ApiToken";
    internal const string SignInScheme = "SignInToken";

    public static IServiceCollection AddApiAuthentication(this IServiceCollection services)
    {
        services.AddOptions<AuthOptions>()
            .BindConfiguration(AuthOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddSingleton<ISystemAdminAllowlist, ConfigSystemAdminAllowlist>();

        services.AddAuthentication(ApiScheme)
            .AddJwtBearer(ApiScheme, _ => { })
            .AddJwtBearer(SignInScheme, _ => { });

        // Resolved lazily so configuration (user-secrets, Aspire parameters, test overrides) is honoured.
        services.AddOptions<JwtBearerOptions>(ApiScheme)
            .Configure<IOptions<AuthOptions>>((jwt, auth) => Configure(jwt, auth.Value, auth.Value.ApiAudience));
        services.AddOptions<JwtBearerOptions>(SignInScheme)
            .Configure<IOptions<AuthOptions>>((jwt, auth) => Configure(jwt, auth.Value, auth.Value.SignInAudience));

        services.AddAuthorizationBuilder()
            // Deny by default: anything not explicitly opened up needs a valid API token.
            .SetFallbackPolicy(new AuthorizationPolicyBuilder(ApiScheme).RequireAuthenticatedUser().Build())
            .AddPolicy(AuthPolicies.SystemAdmin, p => p
                .AddAuthenticationSchemes(ApiScheme)
                .RequireAuthenticatedUser()
                .RequireClaim(ApiTokenDefaults.ClaimRole, ApiTokenDefaults.RoleSystemAdmin))
            .AddPolicy(AuthPolicies.StoreAdmin, StoreMemberPolicy)
            .AddPolicy(AuthPolicies.StoreMember, StoreMemberPolicy)
            .AddPolicy(AuthPolicies.SignIn, p => p
                .AddAuthenticationSchemes(SignInScheme)
                .RequireAuthenticatedUser()
                .RequireClaim(ApiTokenDefaults.ClaimPurpose, ApiTokenDefaults.PurposeSignIn));

        return services;
    }

    private static void StoreMemberPolicy(AuthorizationPolicyBuilder p) => p
        .AddAuthenticationSchemes(ApiScheme)
        .RequireAuthenticatedUser()
        .RequireClaim(ApiTokenDefaults.ClaimRole, ApiTokenDefaults.RoleStoreAdmin)
        .RequireClaim(ApiTokenDefaults.ClaimStoreId);

    private static void Configure(JwtBearerOptions jwt, AuthOptions auth, string audience)
    {
        jwt.MapInboundClaims = false; // keep "sub", "role", "storeId" as issued
        jwt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = auth.Issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(auth.SigningKey)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ApiTokenDefaults.ClaimSubject,
            RoleClaimType = ApiTokenDefaults.ClaimRole
        };
    }
}
