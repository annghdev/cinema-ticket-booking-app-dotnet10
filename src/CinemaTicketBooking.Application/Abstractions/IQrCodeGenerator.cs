namespace CinemaTicketBooking.Application.Abstractions;

public interface IQrCodeGenerator
{
    string GenerateCode(string input);
    Task<string> GenerateCodeAsync(string input);
}
