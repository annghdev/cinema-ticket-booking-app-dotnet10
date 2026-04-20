using CinemaTicketBooking.Domain;
using CinemaTicketBooking.Application.Common.Auth;
using CinemaTicketBooking.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class DataSeeder(
    AppDbContext dbContext,
    UserManager<Account> userManager,
    ILogger<DataSeeder> logger)
{
    // =============================================
    // Fixed IDs for deterministic seed data (Simplified format)
    // =============================================
    public static readonly Guid CinemaId1 = new("00000000-0000-0000-0000-000000000001");
    public static readonly Guid CinemaId2 = new("00000000-0000-0000-0000-000000000002");

    public static readonly Guid MovieId1 = new("00000000-0000-0000-0000-000000010001");
    public static readonly Guid MovieId2 = new("00000000-0000-0000-0000-000000010002");
    public static readonly Guid MovieId3 = new("00000000-0000-0000-0000-000000010003");

    public static readonly Guid ConcessionId1 = new("00000000-0000-0000-0000-000000020001");
    public static readonly Guid ConcessionId2 = new("00000000-0000-0000-0000-000000020002");
    public static readonly Guid ConcessionId3 = new("00000000-0000-0000-0000-000000020003");

    public static readonly Guid ScreenId1 = new("00000000-0000-0000-0000-000000030001");
    public static readonly Guid ScreenId2 = new("00000000-0000-0000-0000-000000030002");
    public static readonly Guid ScreenId3 = new("00000000-0000-0000-0000-000000030003");

    public static readonly Guid ShowTimeId1 = new("00000000-0000-0000-0000-000000040001");
    public static readonly Guid ShowTimeId2 = new("00000000-0000-0000-0000-000000040002");
    public static readonly Guid ShowTimeId3 = new("00000000-0000-0000-0000-000000040003");

    public static readonly Guid TicketId1 = new("00000000-0000-0000-0000-000000050001");

    public static readonly Guid CustomerId1 = new("00000000-0000-0000-0000-000000060001");
    public static readonly Guid CustomerId2 = new("00000000-0000-0000-0000-000000060002");
    public static readonly Guid CustomerId3 = new("00000000-0000-0000-0000-000000060003");
    public static readonly Guid CustomerId4 = new("00000000-0000-0000-0000-000000060004");

    public static readonly Guid BookingId1 = new("00000000-0000-0000-0000-000000070001");
    public static readonly Guid BookingId2 = new("00000000-0000-0000-0000-000000070002");
    public static readonly Guid BookingId3 = new("00000000-0000-0000-0000-000000070003");
    public static readonly Guid BookingId4 = new("00000000-0000-0000-0000-000000070004");
    public static readonly Guid BookingId5 = new("00000000-0000-0000-0000-000000070005");


    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Cinemas.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Skip seed data because cinemas already exist.");
            return;
        }

        // 1. Seed root catalogs first.
        var cinemas = SeedCinemas();
        var movies = SeedMovies();
        var concessions = SeedConcessions();
        var seatSelectionPolicy = SeatSelectionPolicy.CreateDefault();
        var pricingPolicies = SeedPricingPolicies();

        // 2. Build screens and seats for each cinema.
        var screens = SeedScreens(cinemas);

        // 3. Build showtimes and generated tickets.
        var showTimes = SeedShowTimes(movies, screens, pricingPolicies);

        // 4. Build customers and booking with tickets + concessions.
        var customers = SeedCustomers();
        var bookings = SeedBookings(showTimes, customers, concessions);

        dbContext.Cinemas.AddRange(cinemas);
        dbContext.Movies.AddRange(movies);
        dbContext.Concessions.AddRange(concessions);
        dbContext.SeatSelectionPolicies.Add(seatSelectionPolicy);
        dbContext.PricingPolicies.AddRange(pricingPolicies);
        dbContext.Screens.AddRange(screens);
        dbContext.ShowTimes.AddRange(showTimes);
        dbContext.Customers.AddRange(customers);
        dbContext.Bookings.AddRange(bookings);

        await dbContext.SaveChangesAsync(cancellationToken);
        await SeedRegisteredAccountsAsync(customers, cancellationToken);
        logger.LogInformation("Seeded demo data for cinema booking domain successfully.");
    }

    private static List<Cinema> SeedCinemas()
    {
        var cinema1 = Cinema.Create(
            name: "Galaxy Nguyen Du",
            thumbnailUrl: "https://images.unsplash.com/photo-1489599849927-2ee91cede3ba",
            geo: "10.773300,106.699700",
            address: "116 Nguyen Du, District 1, Ho Chi Minh City",
            isActive: true);
        cinema1.Id = CinemaId1;

        var cinema2 = Cinema.Create(
            name: "Beta Tran Quang Khai",
            thumbnailUrl: "https://images.unsplash.com/photo-1517602302552-471fe67acf66",
            geo: "10.790400,106.692700",
            address: "204 Tran Quang Khai, District 1, Ho Chi Minh City",
            isActive: true);
        cinema2.Id = CinemaId2;

        return [cinema1, cinema2];
    }

    private static List<Movie> SeedMovies()
    {
        var movie1 = Movie.Create(
            name: "Chronicles of Orion",
            description: "A rescue mission across collapsing star systems.",
            thumbnailUrl: "https://images.unsplash.com/photo-1536440136628-849c177e76a1",
            studio: "Nova Pictures",
            director: "A. Tran",
            officialTrailerUrl: "https://www.youtube.com/watch?v=2g811Eo7K8U",
            duration: 132,
            genre: MovieGenre.SciFi,
            status: MovieStatus.NowShowing);
        movie1.Id = MovieId1;

        var movie2 = Movie.Create(
            name: "Midnight Verdict",
            description: "A legal thriller where one witness changes everything.",
            thumbnailUrl: "https://images.unsplash.com/photo-1440404653325-ab127d49abc1",
            studio: "Lumen Studio",
            director: "H. Nguyen",
            officialTrailerUrl: "https://www.youtube.com/watch?v=EXeTwQWrcwY",
            duration: 118,
            genre: MovieGenre.Thriller,
            status: MovieStatus.NowShowing);
        movie2.Id = MovieId2;

        var movie3 = Movie.Create(
            name: "Paper Boats",
            description: "A coming-of-age story about friendship and courage.",
            thumbnailUrl: "https://images.unsplash.com/photo-1626814026160-2237a95fc5a0",
            studio: "Blue Harbor",
            director: "K. Pham",
            officialTrailerUrl: "https://www.youtube.com/watch?v=6ZfuNTqbHE8",
            duration: 106,
            genre: MovieGenre.Drama,
            status: MovieStatus.Upcoming);
        movie3.Id = MovieId3;

        return [movie1, movie2, movie3];
    }

    private static List<Concession> SeedConcessions()
    {
        var concession1 = Concession.Create(
            name: "Combo Popcorn + Cola",
            price: 99000m,
            imageUrl: "https://images.unsplash.com/photo-1585647347483-22b66260dfff",
            isAvailable: true);
        concession1.Id = ConcessionId1;

        var concession2 = Concession.Create(
            name: "Caramel Popcorn",
            price: 69000m,
            imageUrl: "https://images.unsplash.com/photo-1578849278619-e73505e9610f",
            isAvailable: true);
        concession2.Id = ConcessionId2;

        var concession3 = Concession.Create(
            name: "Sparkling Orange",
            price: 39000m,
            imageUrl: "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd",
            isAvailable: true);
        concession3.Id = ConcessionId3;

        return [concession1, concession2, concession3];
    }

    private static List<PricingPolicy> SeedPricingPolicies()
    {
        return
        [
            // TwoD
            PricingPolicy.Create(null, ScreenType.TwoD, SeatType.Regular, 85000m, 1.00m, true),
            PricingPolicy.Create(null, ScreenType.TwoD, SeatType.VIP, 120000m, 1.00m, true),
            PricingPolicy.Create(null, ScreenType.TwoD, SeatType.Couple, 170000m, 1.00m, true),

            // ThreeD
            PricingPolicy.Create(null, ScreenType.ThreeD, SeatType.Regular, 85000m, 1.25m, true),
            PricingPolicy.Create(null, ScreenType.ThreeD, SeatType.VIP, 120000m, 1.25m, true),
            PricingPolicy.Create(null, ScreenType.ThreeD, SeatType.Couple, 170000m, 1.25m, true),

            // IMAX
            PricingPolicy.Create(null, ScreenType.IMAX, SeatType.Regular, 85000m, 1.55m, true),
            PricingPolicy.Create(null, ScreenType.IMAX, SeatType.VIP, 120000m, 1.55m, true),
            PricingPolicy.Create(null, ScreenType.IMAX, SeatType.Couple, 170000m, 1.55m, true)
        ];
    }

    private static List<Screen> SeedScreens(List<Cinema> cinemas)
    {
        var standardSeatMap = "[[1,1,0,2,2,2,1,0],[1,1,0,2,2,2,1,0],[1,1,0,2,2,2,1,0],[1,1,0,2,2,2,1,0],[3,0,0,3,0,3,0,0]]";
        var compactSeatMap = "[[1,1,0,2,2,1,0],[1,1,0,2,2,1,0],[1,1,0,2,2,1,0],[3,0,0,3,0,3,0]]";

        var screen1 = CreateScreen(cinemas[0].Id, "ND-S1", standardSeatMap, ScreenType.TwoD);
        screen1.Id = ScreenId1;

        var screen2 = CreateScreen(cinemas[0].Id, "ND-S2", compactSeatMap, ScreenType.ThreeD);
        screen2.Id = ScreenId2;

        var screen3 = CreateScreen(cinemas[1].Id, "BQK-IMAX-1", standardSeatMap, ScreenType.IMAX);
        screen3.Id = ScreenId3;

        return [screen1, screen2, screen3];
    }

    private static Screen CreateScreen(Guid cinemaId, string code, string seatMap, ScreenType type)
    {
        var rowCount = seatMap.Count(c => c == '[') - 1;
        var firstRowStart = seatMap.IndexOf("[", StringComparison.Ordinal) + 1;
        var firstRowEnd = seatMap.IndexOf("]", StringComparison.Ordinal);
        var firstRow = seatMap[firstRowStart..firstRowEnd];
        var columnCount = firstRow.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
        var totalSeats = seatMap.Count(c => c is '1' or '2' or '3');

        var screen = Screen.Create(
            cinemaId: cinemaId,
            code: code,
            rowOfSeats: rowCount,
            columnOfSeats: columnCount,
            totalSeats: totalSeats,
            seatMap: seatMap,
            type: type,
            isActive: true);

        screen.GenerateSeats(seatMap);
        return screen;
    }

    private static List<ShowTime> SeedShowTimes(
        List<Movie> movies,
        List<Screen> screens,
        List<PricingPolicy> pricingPolicies)
    {
        var now = DateTimeOffset.UtcNow;
        var showTime1 = ShowTime.Create(
            movie: movies[0],
            screen: screens[0],
            startAt: now.AddHours(4),
            pricingPolicies: pricingPolicies.Where(p => p.ScreenType == screens[0].Type).ToList());
        showTime1.Id = ShowTimeId1;
        // IMPORTANT: Tickets generated by ShowTime.Create have the old (random) ShowTimeId.
        // We must update them to match the new fixed ShowTimeId.
        foreach (var ticket in showTime1.Tickets)
        {
            ticket.ShowTimeId = ShowTimeId1;
        }

        // Fix Ticket ID for a ticket that is guaranteed to stay available for sample request
        if (showTime1.Tickets.Count > 0)
        {
            showTime1.Tickets.Last().Id = TicketId1;
        }

        var showTime2 = ShowTime.Create(
            movie: movies[1],
            screen: screens[1],
            startAt: now.AddHours(7),
            pricingPolicies: pricingPolicies.Where(p => p.ScreenType == screens[1].Type).ToList());
        showTime2.Id = ShowTimeId2;
        foreach (var ticket in showTime2.Tickets)
        {
            ticket.ShowTimeId = ShowTimeId2;
        }

        var showTime3 = ShowTime.Create(
            movie: movies[0],
            screen: screens[2],
            startAt: now.AddDays(1).AddHours(3),
            pricingPolicies: pricingPolicies.Where(p => p.ScreenType == screens[2].Type).ToList());
        showTime3.Id = ShowTimeId3;
        foreach (var ticket in showTime3.Tickets)
        {
            ticket.ShowTimeId = ShowTimeId3;
        }

        return [showTime1, showTime2, showTime3];
    }

    private static List<Customer> SeedCustomers()
    {
        var customer1 = Customer.Create(
            name: "Nguyen Minh Anh",
            sessionId: "seed-session-registered-01",
            phoneNumber: "0901234567",
            email: "minh.anh@example.com",
            isRegistered: true);
        customer1.Id = CustomerId1;

        var customer2 = Customer.Create(
            name: "Tran Gia Bao",
            sessionId: "seed-session-guest-01",
            phoneNumber: "0912345678",
            email: "gia.bao@example.com",
            isRegistered: false);
        customer2.Id = CustomerId2;

        var customer3 = Customer.Create(
            name: "Le Thanh Vu",
            sessionId: "seed-session-registered-02",
            phoneNumber: "0923456789",
            email: "thanh.vu@example.com",
            isRegistered: true);
        customer3.Id = CustomerId3;

        var customer4 = Customer.Create(
            name: "Pham Quynh Nhu",
            sessionId: "seed-session-guest-02",
            phoneNumber: "0934567890",
            email: "quynh.nhu@example.com",
            isRegistered: false);
        customer4.Id = CustomerId4;

        return [customer1, customer2, customer3, customer4];
    }

    private static List<Booking> SeedBookings(
        List<ShowTime> showTimes,
        List<Customer> customers,
        List<Concession> concessions)
    {
        var bookings = new List<Booking>();

        // 1. Confirmed booking (registered customer, with concessions)
        var bookingConfirmed = CreatePendingBooking(showTimes[0], customers[0]);
        bookingConfirmed.Id = BookingId1;
        AttachTicketsForCheckout(bookingConfirmed, showTimes[0], customers[0], ticketCount: 2, ticketOffset: 0);
        bookingConfirmed.AddConcession(concessions[0], quantity: 1);
        bookingConfirmed.AddConcession(concessions[2], quantity: 2);
        bookingConfirmed.UpdateFinalAmount(discountAmount: 10000m);
        bookingConfirmed.Confirm();
        bookings.Add(bookingConfirmed);

        // 2. Pending booking (guest customer, waiting for payment)
        var bookingPending = CreatePendingBooking(showTimes[1], customers[1]);
        bookingPending.Id = BookingId2;
        AttachTicketsForCheckout(bookingPending, showTimes[1], customers[1], ticketCount: 3, ticketOffset: 0);
        bookingPending.AddConcession(concessions[1], quantity: 1);
        bookingPending.UpdateFinalAmount(discountAmount: 0m);
        bookings.Add(bookingPending);

        // 3. Checked-in booking (registered customer)
        var bookingCheckedIn = CreatePendingBooking(showTimes[2], customers[2]);
        bookingCheckedIn.Id = BookingId3;
        AttachTicketsForCheckout(bookingCheckedIn, showTimes[2], customers[2], ticketCount: 2, ticketOffset: 0);
        bookingCheckedIn.AddConcession(concessions[0], quantity: 1);
        bookingCheckedIn.UpdateFinalAmount(discountAmount: 15000m);
        bookingCheckedIn.Confirm();
        bookingCheckedIn.CheckIn();
        bookings.Add(bookingCheckedIn);

        // 4. Cancelled booking (guest customer, released tickets)
        var bookingCancelled = CreatePendingBooking(showTimes[0], customers[3]);
        bookingCancelled.Id = BookingId4;
        AttachTicketsForCheckout(bookingCancelled, showTimes[0], customers[3], ticketCount: 2, ticketOffset: 3);
        bookingCancelled.AddConcession(concessions[2], quantity: 1);
        bookingCancelled.UpdateFinalAmount(discountAmount: 5000m);
        bookingCancelled.Cancel();
        bookings.Add(bookingCancelled);

        // 5. Another confirmed booking to enrich dashboard/API data
        var bookingConfirmed2 = CreatePendingBooking(showTimes[1], customers[0]);
        bookingConfirmed2.Id = BookingId5;
        AttachTicketsForCheckout(bookingConfirmed2, showTimes[1], customers[0], ticketCount: 1, ticketOffset: 4);
        bookingConfirmed2.AddConcession(concessions[1], quantity: 2);
        bookingConfirmed2.UpdateFinalAmount(discountAmount: 0m);
        bookingConfirmed2.Confirm();
        bookings.Add(bookingConfirmed2);

        return bookings;
    }

    private static Booking CreatePendingBooking(ShowTime showTime, Customer customer)
    {
        var booking = Booking.Create(
            showTimeId: showTime.Id,
            customerId: customer.Id,
            customerName: customer.Name,
            phoneNumber: customer.PhoneNumber,
            email: customer.Email,
            status: BookingStatus.Pending);

        booking.Customer = customer;
        booking.ShowTime = showTime;
        return booking;
    }

    private static void AttachTicketsForCheckout(
        Booking booking,
        ShowTime showTime,
        Customer customer,
        int ticketCount,
        int ticketOffset)
    {
        var lockUntil = DateTimeOffset.UtcNow.AddMinutes(10);
        var paymentUntil = DateTimeOffset.UtcNow.AddMinutes(20);
        var selectedTickets = showTime.Tickets.Skip(ticketOffset).Take(ticketCount).ToList();

        foreach (var ticket in selectedTickets)
        {
            ticket.Lock(customer.SessionId, lockUntil);
            booking.AddTicket(ticket);
            ticket.StartPayment(booking.Id, customer.SessionId, paymentUntil);
        }
    }

    private async Task SeedRegisteredAccountsAsync(
        List<Customer> customers,
        CancellationToken cancellationToken)
    {
        var registeredCustomers = customers.Where(x => x.IsRegistered).ToList();
        foreach (var customer in registeredCustomers)
        {
            var existing = await userManager.FindByEmailAsync(customer.Email);
            if (existing is not null)
            {
                continue;
            }

            var account = new Account
            {
                Id = Guid.CreateVersion7(),
                UserName = customer.Email,
                Email = customer.Email,
                EmailConfirmed = true,
                PhoneNumber = customer.PhoneNumber,
                CustomerId = customer.Id
            };

            var password = "SeedDemo@123";
            var created = await userManager.CreateAsync(account, password);
            if (!created.Succeeded)
            {
                logger.LogWarning(
                    "Failed to create seeded account for {Email}: {Errors}",
                    customer.Email,
                    string.Join(", ", created.Errors.Select(e => e.Description)));
                continue;
            }

            var roleResult = await userManager.AddToRoleAsync(account, RoleNames.Customer);
            if (!roleResult.Succeeded)
            {
                logger.LogWarning(
                    "Failed to add seeded account {Email} to role Customer: {Errors}",
                    customer.Email,
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
