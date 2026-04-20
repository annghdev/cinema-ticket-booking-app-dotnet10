namespace CinemaTicketBooking.Application.Features;

public record SeatSelectionPolicyDto(
    Guid Id,
    string Name,
    bool IsGlobalDefault,
    bool IsActive,
    int MaxTicketsPerCheckout,
    int MaxRowsPerCheckout,
    SeatSelectionPolicyLevel OrphanSeatLevel,
    SeatSelectionPolicyLevel CheckerboardLevel,
    SeatSelectionPolicyLevel SplitAcrossAisleLevel,
    SeatSelectionPolicyLevel IsolatedRowEndSingleLevel,
    SeatSelectionPolicyLevel MisalignedRowsLevel,
    DateTimeOffset CreatedAt);
