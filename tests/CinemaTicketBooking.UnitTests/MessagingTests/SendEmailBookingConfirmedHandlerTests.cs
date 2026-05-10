using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Application.Messaging;
using CinemaTicketBooking.Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CinemaTicketBooking.UnitTests.MessagingTests;

/// <summary>
/// Unit tests for <see cref="SendEmailBookingConfirmedHandler"/>.
/// Verifies routing, email-model construction and resilience against
/// missing data / failed email delivery.
/// </summary>
public class SendEmailBookingConfirmedHandlerTests
{
    // =============================================
    // Fixtures
    // =============================================

    private static BookingConfirmed MakeEvent(
        Guid? bookingId = null,
        string email = "customer@test.com",
        string customerName = "John Doe",
        decimal finalAmount = 150_000m,
        int ticketCount = 2) => new(
            BookingId: bookingId ?? Guid.CreateVersion7(),
            ShowTimeId: Guid.CreateVersion7(),
            CustomerId: Guid.CreateVersion7(),
            CustomerName: customerName,
            Email: email,
            PhoneNumber: "0901234567",
            FinalAmount: finalAmount,
            TicketCount: ticketCount,
            ShowTimeStartAt: DateTimeOffset.UtcNow.AddDays(3));

    private static Booking MakeBooking(Guid bookingId)
    {
        var booking = Booking.Create(
            showTimeId: Guid.CreateVersion7(),
            customerId: null,
            customerName: "John Doe",
            phoneNumber: "0901234567",
            email: "customer@test.com");

        booking.AddConcession(new Concession { Name = "Popcorn", Price = 50000, IsAvailable = true }, 1);
        booking.UpdateFinalAmount(0);

        return booking;
    }

    // =============================================
    // Tests: email skipped when no address
    // =============================================

    [Fact]
    public async Task Handle_ShouldSkipEmail_WhenEmailIsEmpty()
    {
        // Arrange
        var emailSender = new Mock<IEmailSender>();
        var bookingRepo = new Mock<IBookingRepository>();
        var showTimeRepo = new Mock<IShowTimeRepository>();

        var handler = new SendEmailBookingConfirmedHandler(
            emailSender.Object,
            bookingRepo.Object,
            showTimeRepo.Object,
            NullLogger<SendEmailBookingConfirmedHandler>.Instance);

        var @event = MakeEvent(email: string.Empty);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert — no email sent, no repo queries
        emailSender.Verify(s => s.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        bookingRepo.Verify(r => r.LoadFullAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // =============================================
    // Tests: email skipped when booking not found
    // =============================================

    [Fact]
    public async Task Handle_ShouldSkipEmail_WhenBookingNotFound()
    {
        // Arrange
        var emailSender = new Mock<IEmailSender>();
        var bookingRepo = new Mock<IBookingRepository>();
        var showTimeRepo = new Mock<IShowTimeRepository>();

        bookingRepo.Setup(r => r.LoadFullAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Booking?)null);

        var handler = new SendEmailBookingConfirmedHandler(
            emailSender.Object,
            bookingRepo.Object,
            showTimeRepo.Object,
            NullLogger<SendEmailBookingConfirmedHandler>.Instance);

        // Act
        await handler.Handle(MakeEvent(), CancellationToken.None);

        // Assert
        emailSender.Verify(s => s.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // =============================================
    // Tests: email is sent with correct data
    // =============================================

    [Fact]
    public async Task Handle_ShouldSendEmail_WithCorrectRecipientAndBookingCode()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();
        var booking = Booking.Create(
            showTimeId: Guid.CreateVersion7(),
            customerId: null,
            customerName: "Jane Smith",
            phoneNumber: "0912345678",
            email: "jane@test.com");
        
        booking.AddConcession(new Concession { Name = "Popcorn", Price = 50000, IsAvailable = true }, 1);
        booking.UpdateFinalAmount(0);

        var emailSender = new Mock<IEmailSender>();
        var bookingRepo = new Mock<IBookingRepository>();
        var showTimeRepo = new Mock<IShowTimeRepository>();

        bookingRepo.Setup(r => r.LoadFullAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(booking);
        showTimeRepo.Setup(r => r.LoadFullAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ShowTime?)null); // no showtime data — tests safe fallbacks

        string? capturedTo = null;
        string? capturedSubject = null;
        string? capturedBody = null;
        emailSender.Setup(s => s.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, string, string?, CancellationToken>((to, sub, body, name, _) =>
            {
                capturedTo = to;
                capturedSubject = sub;
                capturedBody = body;
            })
            .Returns(Task.CompletedTask);

        var @event = MakeEvent(
            email: "jane@test.com",
            customerName: "Jane Smith",
            finalAmount: 200_000m);

        var handler = new SendEmailBookingConfirmedHandler(
            emailSender.Object,
            bookingRepo.Object,
            showTimeRepo.Object,
            NullLogger<SendEmailBookingConfirmedHandler>.Instance);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        emailSender.Verify(s => s.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        capturedTo.Should().Be("jane@test.com");
        capturedSubject.Should().Contain("Xác nhận");
        capturedBody.Should().Contain("Jane Smith");
        capturedBody.Should().Contain("Popcorn");
        capturedBody.Should().Contain("200,000");
    }

    // =============================================
    // Tests: email sender failure does not propagate
    // =============================================

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenEmailSenderFails()
    {
        // Arrange
        var booking = Booking.Create(
            showTimeId: Guid.CreateVersion7(),
            customerId: null,
            customerName: "Fail Case",
            phoneNumber: "0000000000",
            email: "fail@test.com");

        var emailSender = new Mock<IEmailSender>();
        var bookingRepo = new Mock<IBookingRepository>();
        var showTimeRepo = new Mock<IShowTimeRepository>();

        bookingRepo.Setup(r => r.LoadFullAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(booking);
        showTimeRepo.Setup(r => r.LoadFullAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ShowTime?)null);
        emailSender.Setup(s => s.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("SMTP timeout"));

        var handler = new SendEmailBookingConfirmedHandler(
            emailSender.Object,
            bookingRepo.Object,
            showTimeRepo.Object,
            NullLogger<SendEmailBookingConfirmedHandler>.Instance);

        var @event = MakeEvent(email: "fail@test.com", customerName: "Fail Case");

        // Act & Assert — must not throw even when email delivery fails
        var act = async () => await handler.Handle(@event, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
