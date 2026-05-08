using CinemaTicketBooking.Application.Abstractions;

namespace CinemaTicketBooking.Application.Features;

public class UpdateSystemAccountCommand : ICommand
{
    public Guid AccountId { get; set; }
    public List<string> Roles { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
    public string CorrelationId { get; set; } = string.Empty;
}

public class UpdateSystemAccountHandler(IIdentityAuthService auth)
{
    public async Task Handle(UpdateSystemAccountCommand cmd, CancellationToken ct)
    {
        var result = await auth.UpdateAccountRolesAndClaimsAsync(cmd.AccountId, cmd.Roles, cmd.Permissions, ct);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Không thể cập nhật tài khoản: {errors}");
        }
    }
}
