namespace CinemaTicketBooking.Domain;

/// <summary>
/// Screen represents a theater auditorium within a Cinema.
/// Each Screen has a seat layout (SeatMap) and can host ShowTimes.
///
/// Seat Map Encoding (integer grid):
///   0 = Aisle (walking corridor — treated as segment boundary by the validator)
///   1 = Regular seat
///   2 = VIP seat
///   3 = SweetBox seat
///   4 = SweetBox couple gap spacer (visual only — NOT a seat, NOT an aisle boundary)
///
/// The value 4 marks the display-only gap between the two cells of a SweetBox couple
/// seat. It is skipped during seat generation and is invisible to the aisle-split logic,
/// so Sweet1+Sweet2 are correctly treated as neighbours.
///
/// Example — physical layout:
///       ----------Screen---------
///                                    ←---door
/// A6, A5, aisle, A4, A3, A2, A1, aisle
/// B6, B5, aisle, B4, B3, B2, B1, aisle
/// C6, C5, aisle, C4, C3, C2, C1, aisle
/// D6, D5, aisle, D4, D3, D2, D1, aisle
/// Sweet3, aisle, aisle, Sweet2, [gap], Sweet1, aisle, aisle
///
/// Equivalent SeatMap (JSON or plain text):
/// [[1,1,0,2,2,2,1,0],
///  [1,1,0,2,2,2,1,0],
///  [1,1,0,2,2,2,1,0],
///  [1,1,0,2,2,2,1,0],
///  [4,3,0,4,3,4,3,0]]
/// </summary>
public class Screen : AggregateRoot
{
    public Guid CinemaId { get; set; }
    public Cinema? Cinema { get; set; }
    public required string Code { get; set; }
    public int RowOfSeats { get; set; }
    public int ColumnOfSeats { get; set; }
    public int TotalSeats { get; set; }

    /// <summary>
    /// Seat map stored as a JSON 2D array string or plain-text grid.
    /// Parsed by GenerateSeats() to create Seat entities.
    /// </summary>
    public string SeatMap { get; set; } = string.Empty;
    public ScreenType Type { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Seat> Seats { get; set; } = [];

    // =============================================================
    // Factory and data mutation
    // =============================================================

    /// <summary>
    /// Creates a new screen and raises a creation event.
    /// </summary>
    public static Screen Create(
        Guid cinemaId,
        string code,
        int rowOfSeats,
        int columnOfSeats,
        int totalSeats,
        string seatMap,
        ScreenType type,
        bool isActive = true)
    {
        var screen = new Screen
        {
            CinemaId = cinemaId,
            Code = code,
            RowOfSeats = rowOfSeats,
            ColumnOfSeats = columnOfSeats,
            TotalSeats = totalSeats,
            SeatMap = seatMap,
            Type = type,
            IsActive = isActive
        };

        screen.RaiseEvent(new ScreenCreated(
            ScreenId: screen.Id,
            CinemaId: screen.CinemaId,
            ScreenCode: screen.Code,
            ScreenType: screen.Type,
            RowOfSeats: screen.RowOfSeats,
            ColumnOfSeats: screen.ColumnOfSeats,
            TotalSeats: screen.TotalSeats,
            IsActive: screen.IsActive));

        return screen;
    }

    /// <summary>
    /// Updates basic screen fields and raises an update event.
    /// </summary>
    public void UpdateBasicInfo(
        string code,
        int rowOfSeats,
        int columnOfSeats,
        int totalSeats,
        string seatMap,
        ScreenType type)
    {
        Code = code;
        RowOfSeats = rowOfSeats;
        ColumnOfSeats = columnOfSeats;
        TotalSeats = totalSeats;
        SeatMap = seatMap;
        Type = type;

        RaiseEvent(new ScreenBasicInfoUpdated(
            ScreenId: Id,
            CinemaId: CinemaId,
            ScreenCode: Code,
            ScreenType: Type,
            RowOfSeats: RowOfSeats,
            ColumnOfSeats: ColumnOfSeats,
            TotalSeats: TotalSeats));
    }

    /// <summary>
    /// Activates this screen. No-op when already active (idempotent).
    /// </summary>
    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        RaiseEvent(new ScreenActivated(Id, CinemaId, Code));
    }

    /// <summary>
    /// Deactivates this screen. No-op when already inactive (idempotent).
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        RaiseEvent(new ScreenDeactivated(Id, CinemaId, Code));
    }

    /// <summary>
    /// Activates a seat in this screen.
    /// </summary>
    public void ActivateSeat(Guid seatId)
    {
        var seat = Seats.FirstOrDefault(x => x.Id == seatId)
            ?? throw new InvalidOperationException($"Seat with ID '{seatId}' was not found in screen '{Code}'.");

        if (seat.IsActive)
        {
            return;
        }

        seat.IsActive = true;
        RaiseEvent(new ScreenSeatActivated(Id, seat.Id, Code, seat.Code));
    }

    /// <summary>
    /// Deactivates a seat in this screen.
    /// </summary>
    public void DeactivateSeat(Guid seatId)
    {
        var seat = Seats.FirstOrDefault(x => x.Id == seatId)
            ?? throw new InvalidOperationException($"Seat with ID '{seatId}' was not found in screen '{Code}'.");

        if (!seat.IsActive)
        {
            return;
        }

        seat.IsActive = false;
        seat.IsAvailable = false;
        RaiseEvent(new ScreenSeatDeactivated(Id, seat.Id, Code, seat.Code));
    }

    // =============================================================
    // Generate Seats from SeatMap
    // =============================================================

    /// <summary>
    /// Parses the SeatMap string and generates Seat entities for every non-zero cell.
    /// Seats are numbered right-to-left (matching the cinema convention where
    /// seat 1 is closest to the door on the right side).
    /// </summary>
    public void GenerateSeats(string seatMap)
    {
        // 1. Parse the seat map string into a 2D integer array
        var seatArray = ParseSeatMap(seatMap);

        // 2. Validate all cell values and aisle column consistency
        ValidateSeatMap(seatArray);
        var seats = new List<Seat>();
        
        int sweetBoxCounter = 1;
        
        for(int i = 0; i < seatArray.GetLength(0); i++)
        {
            int seatNumber = 1;

            // 3. Generate seats from right to left (j = last column → 0)
            for(int j = seatArray.GetLength(1) - 1; j >= 0; j--)
            {
                var cellValue = seatArray[i, j];

                // Skip aisle/aisle (0) and SweetBox couple gap spacer (4) — neither is a seat.
                if (cellValue == SeatMapCellValue.Aisle || cellValue == SeatMapCellValue.SweetBoxGap)
                {
                    continue;
                }

                string seatCode;
                if (cellValue == SeatMapCellValue.SweetBox) // SweetBox uses "Sweet{N}" naming globally across the screen
                {
                    seatCode = $"Sweet{sweetBoxCounter++}";
                }
                else // Regular & VIP use "{RowLetter}{N}" naming (A1, B3, etc.) per row
                {
                    seatCode = $"{(char)('A' + i)}{seatNumber}";
                }

                seats.Add(new Seat
                {
                    Id = Guid.CreateVersion7(),
                    Code = seatCode,
                    Row = i + 1,
                    Column = j + 1,
                    IsAvailable = true,
                    Type = (SeatType)cellValue
                });

                seatNumber++;
            }
        }
        Seats = seats;

        RaiseEvent(new ScreenSeatsGenerated(
            ScreenId: Id,
            CinemaId: CinemaId,
            ScreenCode: Code,
            ScreenType: Type,
            TotalSeatsGenerated: seats.Count));
    }

    // =============================================================
    // Parse SeatMap: supports JSON array and plain-text formats
    // =============================================================

    /// <summary>
    /// Parses the seat map string into a 2D integer array.
    /// Supports two formats:
    ///   - JSON: [[1,1,0],[2,2,0]] (starts with '[')
    ///   - Plain text: rows separated by newlines, values by spaces/commas
    /// </summary>
    private int[,] ParseSeatMap(string seatMap)
    {
        if (string.IsNullOrWhiteSpace(seatMap))
        {
            return new int[0, 0];
        }

        seatMap = seatMap.Trim();

        // JSON format detection
        if (seatMap.StartsWith("["))
        {
            var array = System.Text.Json.JsonSerializer.Deserialize<int[][]>(seatMap);
            if (array == null || array.Length == 0) return new int[0, 0];

            int rows = array.Length;
            int cols = array[0].Length;
            var seatArray = new int[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                // All rows must have the same number of columns
                if (array[i].Length != cols)
                    throw new FormatException($"Row {i + 1} does not have the same number of columns.");
                
                for (int j = 0; j < cols; j++)
                {
                    seatArray[i, j] = array[i][j];
                }
            }
            return seatArray;
        }

        // Fallback: plain-text format (spaces or commas as delimiters)
        var textRows = seatMap.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var firstRowCols = textRows[0].Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new int[textRows.Length, firstRowCols.Length];
        
        for (int i = 0; i < textRows.Length; i++)
        {
            var columns = textRows[i].Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < columns.Length; j++)
            {
                if (int.TryParse(columns[j].Trim(), out int seatType))
                {
                    result[i, j] = seatType;
                }
                else
                {
                    throw new FormatException($"Invalid seat type at row {i + 1}, column {j + 1}");
                }
            }
        }
        return result;
    }

    // =============================================================
    // Validation: seat type range and aisle column consistency
    // =============================================================

    /// <summary>
    /// Validates that all seat type values are within the valid range [0..3].
    /// </summary>
    private void ValidateSeatMap(int[,] seatArray)
    {
        for (int i = 0; i < seatArray.GetLength(0); i++)
        {
            for (int j = 0; j < seatArray.GetLength(1); j++)
            {
                // Valid values: 0 (aisle), 1-3 (seat types), 4 (SweetBox couple gap spacer).
                if (seatArray[i, j] < 0 || seatArray[i, j] > SeatMapCellValue.SweetBoxGap)
                {
                    throw new FormatException($"Invalid seat map value {seatArray[i, j]} at row {i + 1}, column {j + 1}.");
                }
            }
        }
        ValidateaislesInSeatMap(seatArray);
    }

    /// <summary>
    /// Validates that true aisle columns (value = 0 in the first row) remain 0
    /// across all rows. SweetBox gap spacers (value = 4) are row-local and are exempt.
    /// </summary>
    private void ValidateaislesInSeatMap(int[,] seatArray)
    {
        for (int i = 0; i < seatArray.GetLength(1); i++)
        {
            // If the first row has a true aisle in this column, all rows must also have a true aisle.
            if (seatArray[0, i] == 0)
            {
                for (int j = 0; j < seatArray.GetLength(0); j++)
                {
                    if (seatArray[j, i] != 0)
                    {
                        throw new FormatException($"Aisle column {i + 1} must be 0 in all rows, but row {j + 1} has value {seatArray[j, i]}.");
                    }
                }
            }
        }
    }
}

/// <summary>
/// Seat represents a single physical seat in a Screen.
/// Created by Screen.GenerateSeats() from the SeatMap configuration.
/// </summary>
public class Seat : IEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// Seat code displayed to customers. Format: "{RowLetter}{Number}" for Regular/VIP,
    /// "Sweet{Number}" for SweetBox. Example: "A1", "B3", "Sweet2".
    /// </summary>
    public string Code { get; set; } = string.Empty;
    public int Row { get; set; }
    public int Column { get; set; }
    public bool IsAvailable { get; set; }
    public SeatType Type { get; set; }
    public bool IsActive { get; set; } = true;
}

//public enum ScreenType
//{
//    TwoD,
//    ThreeD,
//    IMAX
//}

//public enum SeatType
//{
//    Regular = 1,
//    VIP = 2,
//    SweetBox = 3,
//}