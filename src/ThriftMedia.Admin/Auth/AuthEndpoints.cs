using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using ThriftMedia.Contracts.Auth;
using ThriftMedia.Contracts.Dto;
using ThriftMedia.Contracts.Requests;

namespace ThriftMedia.Admin.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // Starts the federated login. Only the two known providers are accepted.
        app.MapGet("/signin/{provider}", (string provider) =>
            ToScheme(provider) is { } scheme
                ? Results.Challenge(
                    new AuthenticationProperties { RedirectUri = $"/signin-complete/{provider.ToLowerInvariant()}" },
                    [scheme])
                : Results.NotFound())
            .AllowAnonymous();

        // Provider callback landing: ask the Api whether this identity may sign in, then start our own session.
        app.MapGet("/signin-complete/{provider}", CompleteSignInAsync).AllowAnonymous();

        app.MapPost("/signout", async (HttpContext context, IAntiforgery antiforgery) =>
        {
            try { await antiforgery.ValidateRequestAsync(context); }
            catch (AntiforgeryValidationException) { return Results.BadRequest(); }

            await context.SignOutAsync(AdminAuthExtensions.CookieScheme);
            return Results.Redirect("/");
        });

        return app;
    }

    private static string? ToScheme(string provider) => provider.ToLowerInvariant() switch
    {
        "google" => GoogleDefaults.AuthenticationScheme,
        "microsoft" => MicrosoftAccountDefaults.AuthenticationScheme,
        _ => null
    };

    private static async Task<IResult> CompleteSignInAsync(
        string provider, HttpContext context, SignInClient signInClient, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ThriftMedia.Admin.SignIn");
        var scheme = ToScheme(provider);
        var external = await context.AuthenticateAsync(AdminAuthExtensions.ExternalScheme);
        await context.SignOutAsync(AdminAuthExtensions.ExternalScheme); // single use

        if (scheme is null || !external.Succeeded || external.Principal?.Identity?.AuthenticationType != scheme)
            return Denied("Failed");

        var principal = external.Principal;
        var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue(ClaimTypes.Email);
        var name = principal.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(sub)) return Denied("Failed");

        // Google asserts email_verified. Personal Microsoft accounts only expose addresses they have verified.
        var emailVerified = scheme == MicrosoftAccountDefaults.AuthenticationScheme
            ? !string.IsNullOrWhiteSpace(email)
            : bool.TryParse(principal.FindFirstValue("email_verified"), out var v) && v;

        SignInResultDto result;
        try
        {
            result = await signInClient.ResolveAsync(
                new ResolveSignInRequest(scheme, sub, email, emailVerified, name, null), cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogError(ex, "Sign-in resolution call to the Api failed");
            return Denied("Unavailable");
        }

        if (result is not { Outcome: SignInOutcome.Succeeded, AppUserId: { } appUserId, Role: { } role })
            return Denied(result.Outcome.ToString());

        var claims = new List<Claim>
        {
            new(AdminClaims.AppUserId, appUserId.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new(AdminClaims.Role, role),
            new(AdminClaims.Provider, scheme),
            new(AdminClaims.ProviderSub, sub)
        };
        if (result.StoreId is { } storeId)
            claims.Add(new Claim(AdminClaims.StoreId, storeId.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        if (!string.IsNullOrWhiteSpace(name)) claims.Add(new Claim(AdminClaims.Name, name));
        if (!string.IsNullOrWhiteSpace(email)) claims.Add(new Claim(AdminClaims.Email, email));

        var identity = new ClaimsIdentity(claims, AdminAuthExtensions.CookieScheme, AdminClaims.Name, AdminClaims.Role);
        await context.SignInAsync(
            AdminAuthExtensions.CookieScheme, new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = false });

        return Results.Redirect(role == ApiTokenDefaults.RoleSystemAdmin ? "/system" : "/store");
    }

    private static IResult Denied(string reason) => Results.Redirect($"/access-denied?reason={Uri.EscapeDataString(reason)}");
}
