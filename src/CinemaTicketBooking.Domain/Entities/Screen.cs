namespace CinemaTicketBooking.Domain;

public class Screen : AuditableEntity
{
    public Guid CinemaId { get; set; }
    public Cinema? Cinema { get; set; }
    public required string Code { get; set; }
    public int RowOfSeats { get; set; }
    public int ColumnOfSeats { get; set; }
    public int TotalSeats { get; set; }
    public string SeatMap { get; set; } = string.Empty;
    public ScreenType Type { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Seat> Seat { get; set; } = [];


    // example of seat map
    //       ----------Screen---------
    //                                    <---door
    // A6, A5, stair, A4, A3, A2, A1, stair
    // B6, B5, stair, B4, B3, B2, B1, stair
    // C6, C5, stair, C4, C3, C2, C1, stair
    // D6, D5, stair, D4, D3, D2, D1, stair
    // Sweet3, stair, Sweet2, Sweet1, stair

    // example of seat map in json format
    // 1 1 0 2 2 2 1 0
    // 1 1 0 2 2 2 1 0
    // 1 1 0 2 2 2 1 0
    // 1 1 0 2 2 2 1 0
    // 3 0 0 3 0 3 0 0
    public void GenerateSeats(string seatMap)
    {
        // Parse the seat map to 2D array, validate and generate seats based on the provided map
        var seatArray = ParseSeatMap(seatMap);
        ValidateSeatMap(seatArray);
        var seats = new List<Seat>();
        
        for(int i = 0; i < seatArray.GetLength(0); i++)
        {
            int seatNumber = 1;

            // Generate seats from right to left as demonstrated in the example
            for(int j = seatArray.GetLength(1) - 1; j >= 0; j--)
            {
                if (seatArray[i, j] != 0)
                {
                    string seatCode;
                    if (seatArray[i, j] == 3) // SweetBox
                    {
                        seatCode = $"Sweet{seatNumber}";
                    }
                    else
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
    }

    // Parse the seat map json string to 2D array of integers, where 0 = stair, 1 = regular seat, 2 = VIP seat, 3 = sweet box
    private int[,] ParseSeatMap(string seatMap)
    {
        if (string.IsNullOrWhiteSpace(seatMap))
        {
            return new int[0, 0];
        }

        seatMap = seatMap.Trim();
        if (seatMap.StartsWith("["))
        {
            var array = System.Text.Json.JsonSerializer.Deserialize<int[][]>(seatMap);
            if (array == null || array.Length == 0) return new int[0, 0];

            int rows = array.Length;
            int cols = array[0].Length;
            var seatArray = new int[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                if (array[i].Length != cols)
                    throw new FormatException($"Row {i + 1} does not have the same number of columns.");
                
                for (int j = 0; j < cols; j++)
                {
                    seatArray[i, j] = array[i][j];
                }
            }
            return seatArray;
        }

        // Fallback for custom sample plain text spaces/comma separated formats
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

    private void ValidateStairsInSeatMap(int[,] seatArray)
    {
        for (int i = 0; i < seatArray.GetLength(1); i++)
        {
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

public class Seat : IEntity
{
    public Guid Id { get; set; }
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