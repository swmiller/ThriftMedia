using ThriftMedia.Contracts.Dto;
using ThriftMedia.Domain.Entities;

namespace ThriftMedia.Application.Features.StoreApplications;

public static class StoreApplicationMappings
{
    public static StoreApplicationDto ToDto(this StoreApplication application) => new(
        application.Id,
        application.Status.ToString(),
        application.StoreName,
        application.OwnerFullName,
        application.Email,
        application.PhoneNumber,
        application.Address1,
        application.Address2,
        application.City,
        application.ProvinceState,
        application.PostalCode,
        application.Country,
        application.WebsiteUrl,
        application.Description,
        application.LicenseNumber,
        application.LicenseImageRef,
        application.StoreFrontImageRef,
        application.SubmittedAtUtc,
        application.ReviewNotes);
}
