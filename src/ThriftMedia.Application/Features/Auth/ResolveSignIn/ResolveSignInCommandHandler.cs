using FluentValidation;
using ThriftMedia.Application.Abstractions;
using ThriftMedia.Contracts.Dto;
using ThriftMedia.Domain.Entities;
using ThriftMedia.Domain.Enums;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.Auth.ResolveSignIn;

/// <summary>
/// Decides whether a federated identity may use ThriftMedia, and links or provisions the local
/// account on first sign-in. The caller (a trusted Blazor app) supplies the provider's claims.
/// </summary>
public class ResolveSignInCommandHandler(
    IValidator<ResolveSignInCommand> validator,
    IAppUserRepository users,
    IStoreRepository stores,
    ISystemAdminAllowlist systemAdminAllowlist,
    TimeProvider timeProvider) : IRequestHandler<ResolveSignInCommand, SignInResultDto>
{
    public async Task<SignInResultDto> Handle(ResolveSignInCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var request = command.Request;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var email = request.Email?.Trim();
        var hasVerifiedEmail = request.EmailVerified && !string.IsNullOrWhiteSpace(email);

        var user = await users.GetByProviderAsync(request.Provider, request.ProviderSub, cancellationToken);
        if (user is not null)
            return await SignInKnownIdentityAsync(user, request.Provider, email, hasVerifiedEmail, request.MfaAsserted, now, cancellationToken);

        // First sign-in for this identity: it may only be linked through a verified email.
        if (!hasVerifiedEmail)
            return new SignInResultDto(SignInOutcome.EmailNotVerified);

        if (systemAdminAllowlist.IsSystemAdmin(request.Provider, email!))
            return await ProvisionSystemAdminAsync(request, email!, now, cancellationToken);

        var candidate = await users.GetByEmailAsync(email!, cancellationToken);
        if (candidate is null || candidate.Role != AppUserRole.StoreAdmin || candidate.HasLinkedIdentity)
            return new SignInResultDto(SignInOutcome.NotRegistered);

        if (!candidate.CanSignIn)
            return new SignInResultDto(SignInOutcome.Suspended);

        candidate.LinkIdentity(request.Provider, request.ProviderSub, request.DisplayName, now);
        await users.UpdateAsync(candidate, cancellationToken);
        return await SucceededAsync(candidate.Id, candidate.Role, cancellationToken);
    }

    private async Task<SignInResultDto> SignInKnownIdentityAsync(
        AppUser user, string provider, string? email, bool hasVerifiedEmail, bool? mfaAsserted, DateTime now, CancellationToken cancellationToken)
    {
        if (user.Role == AppUserRole.SystemAdmin)
        {
            // The allowlist is re-checked on every sign-in so removing it from configuration revokes access.
            if (!hasVerifiedEmail || !systemAdminAllowlist.IsSystemAdmin(provider, email!))
                return new SignInResultDto(SignInOutcome.NotRegistered);
            if (mfaAsserted == false)
                return new SignInResultDto(SignInOutcome.MfaRequired);
        }

        if (!user.CanSignIn)
            return new SignInResultDto(SignInOutcome.Suspended);

        user.RecordSignIn(now);
        await users.UpdateAsync(user, cancellationToken);
        return await SucceededAsync(user.Id, user.Role, cancellationToken);
    }

    private async Task<SignInResultDto> ProvisionSystemAdminAsync(
        Contracts.Requests.ResolveSignInRequest request, string email, DateTime now, CancellationToken cancellationToken)
    {
        if (request.MfaAsserted == false)
            return new SignInResultDto(SignInOutcome.MfaRequired);

        // Only one System Admin may exist; a different identity for the allowlisted email is not accepted.
        if (await users.GetSystemAdminAsync(cancellationToken) is not null)
            return new SignInResultDto(SignInOutcome.NotRegistered);

        var admin = AppUser.CreateSystemAdmin(request.Provider, request.ProviderSub, email, request.DisplayName, now);
        var id = await users.AddAsync(admin, cancellationToken);
        return new SignInResultDto(SignInOutcome.Succeeded, id, AppUserRole.SystemAdmin.ToString());
    }

    private async Task<SignInResultDto> SucceededAsync(int appUserId, AppUserRole role, CancellationToken cancellationToken)
    {
        int? storeId = role == AppUserRole.StoreAdmin
            ? await stores.GetIdByAppUserIdAsync(appUserId, cancellationToken)
            : null;

        return new SignInResultDto(SignInOutcome.Succeeded, appUserId, role.ToString(), storeId);
    }
}
