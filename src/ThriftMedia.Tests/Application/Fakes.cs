using ThriftMedia.Application.Abstractions;
using ThriftMedia.Domain.Entities;
using ThriftMedia.Domain.Enums;
using ThriftMedia.Domain.Events;

namespace ThriftMedia.Tests.Application;

internal sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc));
}

internal sealed class FakeCurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; init; }
    public int? AppUserId { get; init; }
    public AppUserRole? Role { get; init; }
    public int? StoreId { get; init; }

    public static FakeCurrentUser SystemAdmin(int id = 1) =>
        new() { IsAuthenticated = true, AppUserId = id, Role = AppUserRole.SystemAdmin };

    public static FakeCurrentUser StoreAdmin(int id = 5, int storeId = 10) =>
        new() { IsAuthenticated = true, AppUserId = id, Role = AppUserRole.StoreAdmin, StoreId = storeId };

    public static FakeCurrentUser Anonymous() => new();
}

internal sealed class FakeAllowlist(string provider, string email) : ISystemAdminAllowlist
{
    public bool IsSystemAdmin(string candidateProvider, string candidateEmail) =>
        string.Equals(provider, candidateProvider, StringComparison.Ordinal)
        && string.Equals(email, candidateEmail, StringComparison.OrdinalIgnoreCase);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int Executions { get; private set; }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken)
    {
        Executions++;
        await work(cancellationToken);
    }
}

internal sealed class FakeNotifier : IAccountNotifier
{
    public List<DomainEvent> Events { get; } = [];

    public Task NotifyAsync(IEnumerable<DomainEvent> events, CancellationToken cancellationToken)
    {
        Events.AddRange(events);
        return Task.CompletedTask;
    }
}

internal sealed class FakeAppUserRepository : IAppUserRepository
{
    private readonly Dictionary<int, AppUser> _users = [];
    private int _nextId = 100;

    public IReadOnlyCollection<AppUser> All => _users.Values;

    /// <summary>Seeds a user with a known id (the entity is copied with that id).</summary>
    public AppUser Seed(AppUser user, int id)
    {
        var stored = Copy(user, id);
        _users[id] = stored;
        return stored;
    }

    public Task<AppUser?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_users.GetValueOrDefault(id));

    public Task<AppUser?> GetByProviderAsync(string provider, string providerSub, CancellationToken cancellationToken) =>
        Task.FromResult(_users.Values.FirstOrDefault(u => u.Provider == provider && u.ProviderSub == providerSub));

    public Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(_users.Values.FirstOrDefault(u => u.EmailMatches(email)));

    public Task<AppUser?> GetSystemAdminAsync(CancellationToken cancellationToken) =>
        Task.FromResult(_users.Values.FirstOrDefault(u => u.Role == AppUserRole.SystemAdmin));

    public Task<int> AddAsync(AppUser user, CancellationToken cancellationToken)
    {
        var id = _nextId++;
        _users[id] = Copy(user, id);
        return Task.FromResult(id);
    }

    public Task UpdateAsync(AppUser user, CancellationToken cancellationToken)
    {
        _users[user.Id] = Copy(user, user.Id);
        return Task.CompletedTask;
    }

    private static AppUser Copy(AppUser u, int id) => AppUser.Rehydrate(
        id, u.Provider, u.ProviderSub, u.Email, u.DisplayName, u.Role, u.Status, u.CreatedAtUtc, u.LastSeenAtUtc);
}

internal sealed class FakeStoreApplicationRepository : IStoreApplicationRepository
{
    private readonly Dictionary<int, StoreApplication> _applications = [];
    private int _nextId = 1;

    public IReadOnlyCollection<StoreApplication> All => _applications.Values;

    public StoreApplication Seed(StoreApplication application)
    {
        var id = _nextId++;
        var stored = Copy(application, id);
        _applications[id] = stored;
        return stored;
    }

    public Task<StoreApplication?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_applications.GetValueOrDefault(id));

    public Task<IReadOnlyList<StoreApplication>> GetPendingAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<StoreApplication>>(
            _applications.Values.Where(a => a.Status == StoreApplicationStatus.Pending).OrderBy(a => a.SubmittedAtUtc).ToList());

    public Task<bool> HasPendingForEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(_applications.Values.Any(a =>
            a.Status == StoreApplicationStatus.Pending && string.Equals(a.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task<int> AddAsync(StoreApplication application, CancellationToken cancellationToken)
    {
        var id = _nextId++;
        _applications[id] = Copy(application, id);
        return Task.FromResult(id);
    }

    public Task UpdateAsync(StoreApplication application, CancellationToken cancellationToken)
    {
        _applications[application.Id] = Copy(application, application.Id);
        return Task.CompletedTask;
    }

    private static StoreApplication Copy(StoreApplication a, int id) => StoreApplication.Rehydrate(
        id, a.Status, a.StoreName, a.OwnerFullName, a.Email, a.PhoneNumber,
        a.Address1, a.Address2, a.City, a.ProvinceState, a.PostalCode, a.Country,
        a.WebsiteUrl, a.Description, a.LicenseNumber, a.LicenseImageRef, a.StoreFrontImageRef,
        a.SubmittedAtUtc, a.ReviewedByAppUserId, a.ReviewedAtUtc, a.ReviewNotes, a.AppUserId);
}

internal sealed class FakeStoreRepository : IStoreRepository
{
    private int _nextId = 50;

    public List<(int Id, Store Store)> Added { get; } = [];

    public Task<int> AddAsync(Store store, CancellationToken cancellationToken)
    {
        var id = _nextId++;
        Added.Add((id, store));
        return Task.FromResult(id);
    }

    public Task<int?> GetIdByAppUserIdAsync(int appUserId, CancellationToken cancellationToken) =>
        Task.FromResult(Added.Where(a => a.Store.AppUserId == appUserId).Select(a => (int?)a.Id).FirstOrDefault());
}
