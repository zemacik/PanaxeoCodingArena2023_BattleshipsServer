using System.Text;
using battleships.lib.Models;

namespace battleships.lib.Helpers;

/// <summary>
/// Helper class for printing grid to console.
/// </summary>
public static class GridHelper
{
    /// <summary>
    /// Prints grid to console.
    /// </summary>
    /// <param name="gameField">The game field to print.</param>
    /// <param name="asGrid">If true, prints grid with rows and columns.</param>
    /// <returns>String representation of the grid.</returns>
    public static string PrettyPrint(GameField<CellState> gameField, bool asGrid = false)
    {
        return asGrid
            ? PrettyPrint(gameField.Cells, gameField.Rows, gameField.Columns)
            : PrettyPrint(gameField.Cells);
    }

    /// <summary>
    /// Calculates index of the cell in the grid based on row and column.
    /// </summary>
    /// <param name="row">The row number.</param>
    /// <param name="column">The column number.</param>
    /// <returns>Index of the cell in the grid.</returns>
    private static int GetGridCellIndex(int row, int column)
        => row * Constants.GridColumns + column;

    private static string PrettyPrint(CellState[] grid, int rows, int columns)
    {
        var sb = new StringBuilder();
        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                var strToAppend = grid[GetGridCellIndex(row, column)] switch
                {
                    CellState.Unknown => "*",
                    CellState.Water => ".",
                    CellState.Ship => "X",
                    _ => throw new ArgumentOutOfRangeException()
                };

                sb.Append(strToAppend);
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string PrettyPrint(CellState[] grid)
    {
        var sb = new StringBuilder();

        foreach (var gi in grid)
        {
            var strToAppend = gi switch
            {
                CellState.Unknown => "*",
                CellState.Water => ".",
                CellState.Ship => "X",
                _ => throw new ArgumentOutOfRangeException()
            };

            sb.Append(strToAppend);
        }

        return sb.ToString();
    }
}