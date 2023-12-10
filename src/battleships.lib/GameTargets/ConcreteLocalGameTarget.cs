using battleships.lib.Models;
using Microsoft.Extensions.Logging;

namespace battleships.lib.GameTargets;

/// <summary>
/// Concrete implementation of <see cref="LocalGameTarget"/> that uses a predefined map state grid.
/// The map state grid is a 12x12 grid of integers, where 0 represents water and 1-9 represents a ship with the given weight.
/// </summary>
public class ConcreteLocalGameTarget : LocalGameTarget
{
    /// <summary>
    /// The options to use for the game target.
    /// </summary>
    private readonly ConcreteGameTargetOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="ConcreteLocalGameTarget"/>.
    /// </summary>
    /// <param name="options">The options to use for the game target.</param>
    /// <param name="loggerFactory">The logger factory to use.</param>
    public ConcreteLocalGameTarget(ConcreteGameTargetOptions options, ILoggerFactory loggerFactory)
        : base(options, loggerFactory)
    {
        _options = options;
    }

    /// <inheritdoc />
    public override string Name => "Concrete Local Game Target";
    
    /// <inheritdoc />
    protected override GameField<ServerCellValue> CreateNewGameField()
    {
        // iterate over the string characters (0 - 9) and convert them to the correct CellState.
        var gameFieldInitialValue = _options.MapStateGrid
            .Select(c => new ServerCellValue(
                CellState: c switch
                {
                    0 => CellState.Water,
                    _ => CellState.Ship
                },
                ShipWeight: int.Parse(c.ToString())));

        // Initialize the game field with the generated initial value.
        var gameField = new GameField<ServerCellValue>(_options.Rows, _options.Columns);
        gameField.Initialize(gameFieldInitialValue.ToArray());

        return gameField;
    }
}

/// <summary>
/// The options object for <see cref="ConcreteLocalGameTarget"/>.
/// </summary>
public record ConcreteGameTargetOptions : LocalGameTargetOptions
{
    /// <summary>
    /// 144 items (12x12 grid) representing updated state of map,
    /// '0' is water, '1'  - 'X' is shipWeight value (the whole ship consists of this number). 
    /// </summary>
    public int[] MapStateGrid { get; init; } = Array.Empty<int>();

    /// <summary>
    /// Initializes a new instance of <see cref="ConcreteGameTargetOptions"/>.
    /// </summary>
    public ConcreteGameTargetOptions()
    {
        MapCount = 1;
    }
}