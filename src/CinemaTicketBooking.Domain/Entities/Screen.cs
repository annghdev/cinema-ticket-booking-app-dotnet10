namespace CinemaTicketBooking.Domain;

/// <summary>
/// Screen represents a theater auditorium within a Cinema.
/// Each Screen has a seat layout (SeatMap) and can host ShowTimes.
///
/// Seat Map Encoding (integer grid):
///   0 = Stair/Aisle (not a seat)
///   1 = Regular seat
///   2 = VIP seat
///   3 = SweetBox seat
///
/// Example — physical layout:
///       ----------Screen---------
///                                    ←---door
/// A6, A5, stair, A4, A3, A2, A1, stair
/// B6, B5, stair, B4, B3, B2, B1, stair
/// C6, C5, stair, C4, C3, C2, C1, stair
/// D6, D5, stair, D4, D3, D2, D1, stair
/// Sweet3, stair, Sweet2, Sweet1, stair
///
/// Equivalent SeatMap (JSON or plain text):
/// [[1,1,0,2,2,2,1,0],
///  [1,1,0,2,2,2,1,0],
///  [1,1,0,2,2,2,1,0],
///  [1,1,0,2,2,2,1,0],
///  [3,0,0,3,0,3,0,0]]
/// </summary>
public class Screen : AuditableEntity
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
    public List<Seat> Seat { get; set; } = [];

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

        // 2. Validate all cell values and stair column consistency
        ValidateSeatMap(seatArray);
        var seats = new List<Seat>();
        
        for(int i = 0; i < seatArray.GetLength(0); i++)
        {
            int seatNumber = 1;

            // 3. Generate seats from right to left (j = last column → 0)
            for(int j = seatArray.GetLength(1) - 1; j >= 0; j--)
            {
                // Skip stair/aisle positions (value = 0)
                if (seatArray[i, j] != 0)
                {
                    string seatCode;
                    if (seatArray[i, j] == 3) // SweetBox uses "Sweet{N}" naming
                    {
                        seatCode = $"Sweet{seatNumber}";
                    }
                    else // Regular & VIP use "{RowLetter}{N}" naming (A1, B3, etc.)
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
                        Type = (SeatType)seatArray[i, j]
                    });
                    
                    seatNumber++;
                }
            }
        }
        Seat = seats;

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
    // Validation: seat type range and stair column consistency
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
                if (seatArray[i, j] < 0 || seatArray[i, j] > 3)
                {
                    throw new FormatException($"Invalid seat type at row {i + 1}, column {j + 1}");
                }
            }
        }
        ValidateStairsInSeatMap(seatArray);
    }

    /// <summary>
    /// Validates that stair columns (value = 0 in the first row) are consistent
    /// across all rows. A stair must span the entire column.
    /// </summary>
    private void ValidateStairsInSeatMap(int[,] seatArray)
    {
        for (int i = 0; i < seatArray.GetLength(1); i++)
        {
            // If the first row has a stair in this column, all rows must also have a stair
            if (seatArray[0,i] == 0)
            {
                for (int j = 0; j < seatArray.GetLength(0); j++)
                {
                    if (seatArray[j, i] != 0)
                    {
                        throw new FormatException($"Stair must be in the same column at row {j + 1}, column {i + 1}");
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