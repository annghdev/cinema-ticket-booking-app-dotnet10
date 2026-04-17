using CinemaTicketBooking.Application;

namespace CinemaTicketBooking.IntegrationTests.Shared.Fixtures;

public sealed class FakeUserContext : IUserContext
{
    public bool IsAuthenticated { get; set; } = true;

    public Guid UserId { get; set; } = Guid.CreateVersion7();

    public string UserName { get; set; } = "integration-tester";

    public Guid? CustomerId { get; set; }

    public IReadOnlySet<string> Permissions { get; set; } = new HashSet<string>(StringComparer.Ordinal);

    public bool IsInRole(string role)
    {
        return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    public bool HasPermission(string permission) => Permissions.Contains(permission);
}
