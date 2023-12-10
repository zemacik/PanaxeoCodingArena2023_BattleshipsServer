using battleships.lib.Models;

namespace battleships.lib;

public record GameCellPosition(int Row, int Column);

/// <summary>
/// Cell value object used by the game targets.
/// </summary>
public record ServerCellValue
{
    // this is needed for deserialization
    // ReSharper disable once UnusedMember.Global
    public ServerCellValue()
    {
    }
    
    public ServerCellValue(CellState CellState, int ShipWeight)
    {
        this.CellState = CellState;
        this.ShipWeight = ShipWeight;
    }

    /// <summary>The state of the cell.</summary>
    public CellState CellState { get; set; }

    /// <summary>The weight of the ship the cell belongs to.</summary>
    public int ShipWeight { get; set; }
}

/// <summary>
/// Game field representation.
/// </summary>
/// <typeparam name="T">The type of the cell.</typeparam>
public class GameField<T>
{
    private readonly T[] _cells;
    public int Rows { get; }
    public int Columns { get; }
    public bool IsInitialized { get; private set; }
    public T[] Cells
    {
        get => _cells;
        set => Initialize(value);
    }

    /// <summary>
    /// Initialize a new instance of GameField.
    /// </summary>
    /// <param name="rows">The number of rows.</param>
    /// <param name="columns">The number of columns.</param>
    public GameField(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        _cells = new T[rows * columns];

        // Fill with default values
        _cells = Enumerable.Repeat(default(T), rows * columns).ToArray();
    }

    public void Initialize(T[] fieldStatuses)
    {
        if (fieldStatuses.Length != _cells.Length)
            return;

        Array.Copy(fieldStatuses, _cells, fieldStatuses.Length);

        IsInitialized = true;
    }

    /// <summary>
    /// Update the game field with new data.
    /// </summary>
    /// <param name="newData">The new data.</param>
    /// <param name="updateFunc">The update function.</param>
    internal void Update(T[] newData, Func<T, T, T> updateFunc)
    {
        if (newData.Length != _cells.Length)
            throw new Exception("Invalid data length");

        for (var i = 0; i < _cells.Length; i++)
        {
            _cells[i] = updateFunc(_cells[i], newData[i]);
        }
    }

    /// <summary>
    /// Get the status of the cell at the given position.
    /// </summary>
    /// <param name="position">The position of the cell.</param>
    /// <returns>The status of the cell.</returns>
    public T GetCellStatus(GameCellPosition position)
    {
        AssertCellPositionInsideField(position);
        return _cells[GetIndex(position)];
    }

    /// <summary>
    /// Get the status of the cell at the given index.
    /// </summary>
    /// <param name="index">The index of the cell.</param>
    /// <returns>The status of the cell.</returns>
    public T GetCellStatus(int index)
    {
        return _cells[index];
    }

    /// <summary>
    /// Set the status of the cell at the given position.
    /// </summary>
    /// <param name="position">The position of the cell.</param>
    /// <param name="newFieldStatus">The new status of the cell.</param>
    public void SetCellStatus(GameCellPosition position, T newFieldStatus)
    {
        AssertCellPositionInsideField(position);
        _cells[GetIndex(position)] = newFieldStatus;
    }

    /// <summary>
    /// Set the status of the cell at the given position.
    /// </summary>
    /// <param name="index">The index of the cell.</param>
    /// <param name="newFieldStatus">The new status of the cell.</param>
    public void SetCellStatus(int index, T newFieldStatus)
    {
        _cells[index] = newFieldStatus;
    }

    /// <summary>
    /// Get the position of the cell at the given index.
    /// </summary>
    /// <param name="index">The index of the cell.</param>
    /// <returns>The position of the cell.</returns>
    public GameCellPosition GetCellPosition(int index)
    {
        var row = index / Columns;
        var column = index % Columns;
        return new GameCellPosition(row, column);
    }

    /// <summary>
    /// Get the index of the cell at the given position.
    /// </summary>
    /// <param name="position">The position of the cell.</param>
    /// <returns>The index of the cell.</returns>
    public int GetIndex(GameCellPosition position)
    {
        AssertCellPositionInsideField(position);
        return position.Row * Columns + position.Column;
    }

    /// <summary>
    /// Check if the cell has a previous cell.
    /// </summary>
    /// <param name="position">The position of the cell.</param>
    /// <returns>True if the cell has a previous cell, false otherwise.</returns>
    public bool HasPreviousCell(GameCellPosition position)
    {
        AssertCellPositionInsideField(position);
        return position.Column > 0;
    }

    /// <summary>
    /// Check if the cell has a next cell.
    /// </summary>
    /// <param name="position">The position of the cell.</param>
    /// <returns>True if the cell has a next cell, false otherwise.</returns>
    public bool HasNextCell(GameCellPosition position)
    {
        AssertCellPositionInsideField(position);
        return position.Column < Columns - 1;
    }

    /// <summary>
    /// Check if the cell has a above cell.
    /// </summary>
    /// <param name="position">The position of the cell.</param>
    /// <returns>True if the cell has a above cell, false otherwise.</returns>
    public bool HasAboveCell(GameCellPosition position)
    {
        AssertCellPositionInsideField(position);
        return position.Row > 0;
    }

    /// <summary>
    /// Check if the cell has a below cell.
    /// </summary>
    /// <param name="position">The position of the cell.</param>
    /// <returns>True if the cell has a below cell, false otherwise.</returns>    
    public bool HasBelowCell(GameCellPosition position)
    {
        AssertCellPositionInsideField(position);
        return position.Row < Rows - 1;
    }

    /// <summary>
    /// Check if the cell is inside the game field.
    /// </summary>
    /// <param name="position">The position of the cell.</param>
    /// <returns>True if the cell is inside the game field, false otherwise.</returns>
    public bool IsCellPositionInsideField(GameCellPosition position)
    {
        return position.Row >= 0 && position.Row < Rows &&
               position.Column >= 0 && position.Column < Columns;
    }

    /// <summary>
    /// Assert that the cell is inside the game field.
    /// </summary>
    /// <param name="position">The position of the cell.</param>
    private void AssertCellPositionInsideField(GameCellPosition position)
    {
        if (!IsCellPositionInsideField(position))
            throw new ArgumentException("Position is not inside game field.");
    }

    /// <summary>
    /// Check if any cell in the game field has the given value.
    /// </summary>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if any cell in the game field has the given value, false otherwise.</returns>
    public bool ExistsValue(ServerCellValue value)
    {
        return _cells.Any(cell => cell != null && cell.Equals(value));
    }

    /// <summary>
    /// Gets all adjacent cell positions around the given position. (All around)
    /// </summary>
    /// <param name="position">The position to get the adjacent cell positions for.</param>
    /// <returns>All adjacent cell positions around the given position.</returns>
    public IEnumerable<GameCellPosition> GetAllAroundAdjacentCellPositions(GameCellPosition position)
    {
        var surroundingOffsets = new List<(int, int)>
            { (-1, 0), (1, 0), (0, -1), (0, 1), (-1, -1), (-1, 1), (1, -1), (1, 1) };

        foreach (var offset in surroundingOffsets)
        {
            var adjacentPosition =
                new GameCellPosition(position.Row + offset.Item1, position.Column + offset.Item2);

            if (IsCellPositionInsideField(adjacentPosition))
                yield return adjacentPosition;
        }
    }

    /// <summary>
    /// Gets all adjacent cell positions around the given position. (cross only)
    /// </summary>
    /// <param name="position">The position to get the adjacent cell positions for.</param>
    /// <returns>All adjacent cell positions around the given position.</returns>
    public IEnumerable<GameCellPosition> GetAllCrossAdjacentCellPositions(GameCellPosition position)
    {
        var surroundingOffsets = new List<(int, int)> { (-1, 0), (1, 0), (0, -1), (0, 1) };

        foreach (var offset in surroundingOffsets)
        {
            var adjacentPosition =
                new GameCellPosition(position.Row + offset.Item1, position.Column + offset.Item2);

            if (IsCellPositionInsideField(adjacentPosition))
                yield return adjacentPosition;
        }
    }

    /// <summary>
    /// Gets all adjacent cell positions around the given position. (top bottom only)
    /// </summary>
    /// <param name="position">The position to get the adjacent cell positions for.</param>
    /// <returns>All adjacent cell positions around the given position.</returns>
    public IEnumerable<GameCellPosition> GetTopBottomAdjacentCellPositions(GameCellPosition position)
    {
        var surroundingOffsets = new List<(int, int)> { (-1, 0), (1, 0) };

        foreach (var offset in surroundingOffsets)
        {
            var adjacentPosition =
                new GameCellPosition(position.Row + offset.Item1, position.Column + offset.Item2);

            if (IsCellPositionInsideField(adjacentPosition))
                yield return adjacentPosition;
        }
    }

    /// <summary>
    /// Gets all adjacent cell positions around the given position. (left right only)
    /// </summary>
    /// <param name="position">The position to get the adjacent cell positions for.</param>
    /// <returns>All adjacent cell positions around the given position.</returns>
    public IEnumerable<GameCellPosition> GetLeftRightAdjacentCellPositions(GameCellPosition position)
    {
        var surroundingOffsets = new List<(int, int)> { (0, -1), (0, -1) };

        foreach (var offset in surroundingOffsets)
        {
            var adjacentPosition =
                new GameCellPosition(position.Row + offset.Item1, position.Column + offset.Item2);

            if (IsCellPositionInsideField(adjacentPosition))
                yield return adjacentPosition;
        }
    }
}