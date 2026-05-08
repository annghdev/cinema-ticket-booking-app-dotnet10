using CinemaTicketBooking.Application.Abstractions;

namespace CinemaTicketBooking.Application.Features;

public record ResetAccountPasswordCommand(Guid AccountId) : ICommand
{
    public string CorrelationId { get; set; } = string.Empty;
}

public class ResetAccountPasswordHandler(IIdentityAuthService auth, IEmailSender emailSender)
{
    public async Task<string> Handle(ResetAccountPasswordCommand cmd, CancellationToken ct)
    {
        var details = await auth.GetAccountDetailsAsync(cmd.AccountId, ct);
        if (details is null) throw new Exception("Tài khoản không tồn tại.");

        var newPassword = await auth.AdminResetPasswordAsync(cmd.AccountId, ct);

        await emailSender.SendEmailAsync(
            details.Email,
            "Mật khẩu của bạn đã được đặt lại",
            $@"<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                <h2 style='color: #E21221;'>Thông báo đặt lại mật khẩu</h2>
                <p>Chào <strong>{details.UserName}</strong>,</p>
                <p>Mật khẩu tài khoản của bạn đã được quản trị viên đặt lại thành công.</p>
                <div style='background: #f9f9f9; padding: 15px; border-radius: 8px; margin: 20px 0; text-align: center;'>
                    <p style='margin: 0; color: #666; font-size: 12px; text-transform: uppercase;'>Mật khẩu mới của bạn là</p>
                    <p style='margin: 5px 0 0 0; font-family: monospace; font-size: 24px; color: #E21221; font-weight: bold;'>{newPassword}</p>
                </div>
                <p style='color: #666; font-size: 13px;'>Vì lý do bảo mật, vui lòng đổi mật khẩu ngay sau khi đăng nhập thành công.</p>
                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                <p style='font-size: 11px; color: #999;'>Đây là email tự động, vui lòng không phản hồi email này.</p>
            </div>",
            ct);

        return newPassword;
    }
}
