using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using ThriftMedia.Application.Common;
using ThriftMedia.Application.Features.StoreAdmins.ReactivateStoreAdmin;
using ThriftMedia.Application.Features.StoreAdmins.SuspendStoreAdmin;
using ThriftMedia.Application.Features.StoreApplications.ApproveStoreApplication;
using ThriftMedia.Application.Features.StoreApplications.GetPendingStoreApplications;
using ThriftMedia.Application.Features.StoreApplications.RejectStoreApplication;
using ThriftMedia.Application.Features.StoreApplications.SubmitStoreApplication;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Domain.Entities;
using ThriftMedia.Domain.Enums;
using ThriftMedia.Domain.Events;
using ThriftMedia.Domain.Exceptions;

namespace ThriftMedia.Tests.Application;

public class StoreApplicationHandlerTests
{
    private static readonly DateTime Now = new(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeAppUserRepository _users = new();
    private readonly FakeStoreApplicationRepository _applications = new();
    private readonly FakeStoreRepository _stores = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeNotifier _notifier = new();
    private readonly FixedTimeProvider _time = new(Now);

    private static SubmitStoreApplicationRequest ValidRequest(string email = "owner@example.com") => new(
        "Second Chapter Books", "Pat Q Owner", email, "555-0100",
        "1 Main St", null, "Springfield", "IL", "62701", null,
        null, "Used books and vinyl", "LIC-123", "uploads/license.pdf", null);

    private static ApproveStoreApplicationRequest ValidApproval() =>
        new("Retail", "State of Illinois", new DateTime(2025, 1, 1), null, "License checked");

    private StoreApplication SeedPending(string email = "owner@example.com")
    {
        var r = ValidRequest(email);
        return _applications.Seed(StoreApplication.Submit(
            r.StoreName, r.OwnerFullName, r.Email, r.PhoneNumber, r.Address1, r.Address2, r.City,
            r.ProvinceState, r.PostalCode, r.Country, r.WebsiteUrl, r.Description,
            r.LicenseNumber, r.LicenseImageRef, r.StoreFrontImageRef, Now));
    }

    private ApproveStoreApplicationCommandHandler ApproveHandler(FakeCurrentUser user) => new(
        user, new ApproveStoreApplicationValidator(), _applications, _users, _stores, _unitOfWork, _notifier, _time);

    // ---- Submit ----

    [Fact]
    public async Task Submit_StoresPendingApplication()
    {
        var handler = new SubmitStoreApplicationCommandHandler(
            new SubmitStoreApplicationValidator(), _applications, _users, _time,
            NullLogger<SubmitStoreApplicationCommandHandler>.Instance);

        await handler.Handle(new SubmitStoreApplicationCommand(ValidRequest()), CancellationToken.None);

        var stored = Assert.Single(_applications.All);
        Assert.Equal(StoreApplicationStatus.Pending, stored.Status);
    }

    [Fact]
    public async Task Submit_IgnoresDuplicatesSilently()
    {
        SeedPending("owner@example.com");
        _users.Seed(AppUser.CreateStoreAdmin("existing@example.com", null, Now), 9);
        var handler = new SubmitStoreApplicationCommandHandler(
            new SubmitStoreApplicationValidator(), _applications, _users, _time,
            NullLogger<SubmitStoreApplicationCommandHandler>.Instance);

        await handler.Handle(new SubmitStoreApplicationCommand(ValidRequest("OWNER@example.com")), CancellationToken.None);
        await handler.Handle(new SubmitStoreApplicationCommand(ValidRequest("existing@example.com")), CancellationToken.None);

        Assert.Single(_applications.All);
    }

    [Fact]
    public async Task Submit_RejectsInvalidInput()
    {
        var handler = new SubmitStoreApplicationCommandHandler(
            new SubmitStoreApplicationValidator(), _applications, _users, _time,
            NullLogger<SubmitStoreApplicationCommandHandler>.Instance);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new SubmitStoreApplicationCommand(ValidRequest("not-an-email")), CancellationToken.None));
    }

    // ---- Approve ----

    [Fact]
    public async Task Approve_CreatesStoreAdminAndStoreAtomicallyAndNotifies()
    {
        var application = SeedPending();

        var storeId = await ApproveHandler(FakeCurrentUser.SystemAdmin(1))
            .Handle(new ApproveStoreApplicationCommand(application.Id, ValidApproval()), CancellationToken.None);

        Assert.Equal(1, _unitOfWork.Executions);
        var user = Assert.Single(_users.All);
        Assert.Equal(AppUserRole.StoreAdmin, user.Role);
        Assert.Equal(AppUserStatus.Approved, user.Status);

        var (id, store) = Assert.Single(_stores.Added);
        Assert.Equal(storeId, id);
        Assert.Equal(100, store.AppUserId);
        Assert.Equal("Pat Q", store.OwnerFirstName);
        Assert.Equal("Owner", store.OwnerLastName);
        Assert.Equal("State of Illinois", store.IssueingAuthority);

        var stored = await _applications.GetByIdAsync(application.Id, CancellationToken.None);
        Assert.Equal(StoreApplicationStatus.Approved, stored!.Status);
        Assert.Equal(100, stored.AppUserId);
        Assert.IsType<StoreApplicationApprovedEvent>(Assert.Single(_notifier.Events));
    }

    [Fact]
    public async Task Approve_RequiresSystemAdmin()
    {
        var application = SeedPending();
        var command = new ApproveStoreApplicationCommand(application.Id, ValidApproval());

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            ApproveHandler(FakeCurrentUser.StoreAdmin()).Handle(command, CancellationToken.None));
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            ApproveHandler(FakeCurrentUser.Anonymous()).Handle(command, CancellationToken.None));

        Assert.Empty(_users.All);
        Assert.Empty(_stores.Added);
    }

    [Fact]
    public async Task Approve_UnknownApplication_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            ApproveHandler(FakeCurrentUser.SystemAdmin())
                .Handle(new ApproveStoreApplicationCommand(999, ValidApproval()), CancellationToken.None));
    }

    [Fact]
    public async Task Approve_WhenAccountForEmailExists_Throws()
    {
        var application = SeedPending();
        _users.Seed(AppUser.CreateStoreAdmin(application.Email, null, Now), 9);

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            ApproveHandler(FakeCurrentUser.SystemAdmin())
                .Handle(new ApproveStoreApplicationCommand(application.Id, ValidApproval()), CancellationToken.None));
    }

    [Fact]
    public async Task Approve_AlreadyReviewed_Throws()
    {
        var application = SeedPending();
        var handler = ApproveHandler(FakeCurrentUser.SystemAdmin());
        await handler.Handle(new ApproveStoreApplicationCommand(application.Id, ValidApproval()), CancellationToken.None);

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            handler.Handle(new ApproveStoreApplicationCommand(application.Id, ValidApproval()), CancellationToken.None));
    }

    // ---- Reject ----

    [Fact]
    public async Task Reject_MarksRejectedAndNotifiesWithReason()
    {
        var application = SeedPending();
        var handler = new RejectStoreApplicationCommandHandler(
            FakeCurrentUser.SystemAdmin(), new RejectStoreApplicationValidator(), _applications, _notifier, _time);

        await handler.Handle(
            new RejectStoreApplicationCommand(application.Id, new RejectStoreApplicationRequest("Blurry license")),
            CancellationToken.None);

        var stored = await _applications.GetByIdAsync(application.Id, CancellationToken.None);
        Assert.Equal(StoreApplicationStatus.Rejected, stored!.Status);
        var evt = Assert.IsType<StoreApplicationRejectedEvent>(Assert.Single(_notifier.Events));
        Assert.Equal("Blurry license", evt.Reason);
        Assert.Empty(_users.All);
    }

    [Fact]
    public async Task Reject_RequiresSystemAdmin()
    {
        var application = SeedPending();
        var handler = new RejectStoreApplicationCommandHandler(
            FakeCurrentUser.StoreAdmin(), new RejectStoreApplicationValidator(), _applications, _notifier, _time);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new RejectStoreApplicationCommand(application.Id, new RejectStoreApplicationRequest(null)), CancellationToken.None));
    }

    // ---- Pending list ----

    [Fact]
    public async Task GetPending_ReturnsOnlyPendingForSystemAdmin()
    {
        var pending = SeedPending("a@example.com");
        var reviewed = SeedPending("b@example.com");
        reviewed.Reject(1, null, Now);
        await _applications.UpdateAsync(reviewed, CancellationToken.None);

        var handler = new GetPendingStoreApplicationsQueryHandler(FakeCurrentUser.SystemAdmin(), _applications);
        var result = await handler.Handle(new GetPendingStoreApplicationsQuery(), CancellationToken.None);

        Assert.Equal(pending.Id, Assert.Single(result).Id);
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            new GetPendingStoreApplicationsQueryHandler(FakeCurrentUser.StoreAdmin(), _applications)
                .Handle(new GetPendingStoreApplicationsQuery(), CancellationToken.None));
    }

    // ---- Suspend / Reactivate ----

    private AppUser SeedActiveStoreAdmin(int id = 7)
    {
        var user = AppUser.CreateStoreAdmin("owner@example.com", null, Now);
        user.LinkIdentity("Google", "sub", null, Now);
        return _users.Seed(user, id);
    }

    [Fact]
    public async Task Suspend_SuspendsAndNotifies()
    {
        SeedActiveStoreAdmin(7);
        var handler = new SuspendStoreAdminCommandHandler(
            FakeCurrentUser.SystemAdmin(), new SuspendStoreAdminValidator(), _users, _notifier, _time);

        await handler.Handle(new SuspendStoreAdminCommand(7), CancellationToken.None);

        Assert.Equal(AppUserStatus.Suspended, (await _users.GetByIdAsync(7, CancellationToken.None))!.Status);
        Assert.IsType<AppUserSuspendedEvent>(Assert.Single(_notifier.Events));
    }

    [Fact]
    public async Task Suspend_RequiresSystemAdmin()
    {
        SeedActiveStoreAdmin(7);
        var handler = new SuspendStoreAdminCommandHandler(
            FakeCurrentUser.StoreAdmin(), new SuspendStoreAdminValidator(), _users, _notifier, _time);

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(new SuspendStoreAdminCommand(7), CancellationToken.None));
        Assert.Equal(AppUserStatus.Active, (await _users.GetByIdAsync(7, CancellationToken.None))!.Status);
    }

    [Fact]
    public async Task Suspend_UnknownUser_ThrowsNotFound()
    {
        var handler = new SuspendStoreAdminCommandHandler(
            FakeCurrentUser.SystemAdmin(), new SuspendStoreAdminValidator(), _users, _notifier, _time);

        await Assert.ThrowsAsync<DomainNotFoundException>(() => handler.Handle(new SuspendStoreAdminCommand(404), CancellationToken.None));
    }

    [Fact]
    public async Task Reactivate_RestoresAccessAndNotifies()
    {
        var user = SeedActiveStoreAdmin(7);
        user.Suspend(Now);
        user.ClearDomainEvents();
        await _users.UpdateAsync(user, CancellationToken.None);
        var handler = new ReactivateStoreAdminCommandHandler(
            FakeCurrentUser.SystemAdmin(), new ReactivateStoreAdminValidator(), _users, _notifier, _time);

        await handler.Handle(new ReactivateStoreAdminCommand(7), CancellationToken.None);

        Assert.Equal(AppUserStatus.Active, (await _users.GetByIdAsync(7, CancellationToken.None))!.Status);
        Assert.IsType<AppUserReactivatedEvent>(Assert.Single(_notifier.Events));
    }
}
