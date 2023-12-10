namespace battleships.lib.GameTargets;

/// <summary>
/// Randomly generates a game field with ships.
/// </summary>
public class ShipPlacementGenerator
{
    // I've should have used Weight values from Constants, but this looks nicer :))
    private static readonly int[,] AvengersHelicarrier = { { 0, 9, 0, 9, 0 }, { 9, 9, 9, 9, 9 }, { 0, 9, 0, 9, 0 } };
    private static readonly int[,] Carrier = { { 6, 6, 6, 6, 6 } };
    private static readonly int[,] Battleship = { { 5, 5, 5, 5 } };
    private static readonly int[,] Destroyer = { { 4, 4, 4 } };
    private static readonly int[,] Submarine = { { 3, 3, 3 } };
    private static readonly int[,] Boat = { { 2, 2 } };

    private readonly Random _rand = new();
    private readonly List<int[,]> _ships = new();

    /// <summary>
    /// Creates a new instance of <see cref="ShipPlacementGenerator"/>.
    /// </summary>
    public ShipPlacementGenerator()
    {
        _ships.Add(AvengersHelicarrier);
        _ships.Add(Carrier);
        _ships.Add(Battleship);
        _ships.Add(Destroyer);
        _ships.Add(Submarine);
        _ships.Add(Boat);
    }

    /// <summary>
    /// Generates a new game field with ships.
    /// </summary>
    /// <param name="columns">The number of columns.</param>
    /// <param name="rows">The number of rows.</param>
    /// <returns>The generated game field.</returns>
    public int[,] Generate(int columns, int rows)
    {
        var gameField = new int[rows, columns];

        foreach (var ship in _ships)
        {
            PlaceShip(gameField, ship);
        }

        return gameField;
    }

    /// <summary>
    /// Places a ship on the game field.
    /// </summary>
    /// <param name="gameField">The game field.</param>
    /// <param name="ship">The ship to place.</param>
    private void PlaceShip(int[,] gameField, int[,] ship)
    {
        var placed = false;

        while (!placed)
        {
            var shipToPlace = RotateShipRandomly(ship);
            var shipRows = shipToPlace.GetLength(0);
            var shipCols = shipToPlace.GetLength(1);

            // Random position
            var row = _rand.Next(gameField.GetLength(0) - shipRows + 1);
            var col = _rand.Next(gameField.GetLength(1) - shipCols + 1);

            if (CanPlaceShip(gameField, shipToPlace, row, col))
            {
                for (var r = 0; r < shipRows; r++)
                for (var c = 0; c < shipCols; c++)
                    if (shipToPlace[r, c] != 0)
                        gameField[row + r, col + c] = shipToPlace[r, c];

                placed = true;
            }
        }
    }

    /// <summary>
    /// Checks if a ship can be placed on the game field.
    /// </summary>
    /// <param name="gameField">The game field.</param>
    /// <param name="ship">The ship to place.</param>
    /// <param name="startRow">The starting row.</param>
    /// <param name="startCol">The starting column.</param>
    /// <returns>True if the ship can be placed, false otherwise.</returns>
    private static bool CanPlaceShip(int[,] gameField, int[,] ship, int startRow, int startCol)
    {
        var shipRows = ship.GetLength(0);
        var shipCols = ship.GetLength(1);

        for (var r = 0; r < shipRows; r++)
        for (var c = 0; c < shipCols; c++)
        {
            if (ship[r, c] != 0)
            {
                // Check if the ship part is outside the game field
                if (startRow + r >= gameField.GetLength(0) || startCol + c >= gameField.GetLength(1))
                    return false;

                // Check for overlap with existing ships
                if (gameField[startRow + r, startCol + c] != 0)
                    return false;

                // Check adjacent cells (including diagonals)
                for (var y = -1; y <= 1; y++)
                for (var x = -1; x <= 1; x++)
                {
                    var checkRow = startRow + r + y;
                    var checkCol = startCol + c + x;

                    // Boundary check for adjacent cells
                    if (checkRow >= 0 && checkRow < gameField.GetLength(0) && checkCol >= 0 &&
                        checkCol < gameField.GetLength(1))
                    {
                        if (gameField[checkRow, checkCol] != 0)
                            return false;
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Randomly rotates a ship.
    /// </summary>
    /// <param name="ship">The ship to rotate.</param>
    /// <returns>The rotated ship.</returns>
    private int[,] RotateShipRandomly(int[,] ship)
    {
        if (_rand.Next(2) != 0) 
            return ship; // 50% chance to rotate
        
        var rows = ship.GetLength(0);
        var cols = ship.GetLength(1);
        var rotatedShip = new int[cols, rows];

        for (var r = 0; r < rows; r++)
        for (var c = 0; c < cols; c++)
            rotatedShip[c, r] = ship[r, c];

        return rotatedShip;
    }
}