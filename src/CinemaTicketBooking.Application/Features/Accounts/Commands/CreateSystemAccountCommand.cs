using CinemaTicketBooking.Application.Abstractions;

namespace CinemaTicketBooking.Application.Features;

public class CreateSystemAccountCommand : ICommand
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public string CorrelationId { get; set; } = string.Empty;
}

public class CreateSystemAccountHandler(IIdentityAuthService auth)
{
    public async Task Handle(CreateSystemAccountCommand cmd, CancellationToken ct)
    {
        var result = await auth.CreateAccountAsync(cmd.Email, cmd.UserName, cmd.Password, cmd.Roles, ct);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Không thể tạo tài khoản: {errors}");
        }
    }
}
