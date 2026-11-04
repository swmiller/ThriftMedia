namespace ThriftMedia.Contracts.Dto;

public enum SignInOutcome
{
    Succeeded = 0,

    /// <summary>No approved account for this identity (also covers pending, rejected and unknown users).</summary>
    NotRegistered = 1,

    Suspended = 2,

    /// <summary>The provider did not assert a verified email address.</summary>
    EmailNotVerified = 3,

    /// <summary>The System Admin signed in without MFA although the provider reported it was not used.</summary>
    MfaRequired = 4
}

/// <param name="Outcome">Result of the sign-in decision.</param>
/// <param name="AppUserId">Set when <see cref="Outcome"/> is <see cref="SignInOutcome.Succeeded"/>.</param>
/// <param name="Role">"SystemAdmin" or "StoreAdmin"; set on success.</param>
/// <param name="StoreId">The Store Admin's store id; null for the System Admin or if no store exists.</param>
public record SignInResultDto(SignInOutcome Outcome, int? AppUserId = null, string? Role = null, int? StoreId = null);
