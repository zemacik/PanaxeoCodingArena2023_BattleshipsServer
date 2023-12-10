using battleships.lib.GameTargets.Models;
using battleships.lib.Models;
using Microsoft.Extensions.Logging;

namespace battleships.lib.GameTargets;

/// <summary>
/// Generated local game target implementation of <see cref="LocalGameTarget"/> that uses a randomly generates maps.
/// The map is a 12x12 grid of integers, where 0 represents water and 1-9 represents a ship with the given weight.
/// </summary>
public class GeneratedLocalGameTarget : LocalGameTarget
{
    /// <summary>
    /// The options to use for the game target.
    /// </summary>
    private readonly GeneratedLocalGameTargetOptions _options;
    
    /// <summary>
    /// The ship placement generator.
    /// See <see cref="ShipPlacementGenerator"/> for more information.
    /// </summary>
    private readonly ShipPlacementGenerator _shipPlacementGenerator;

    /// <summary>
    /// Initializes a new instance of <see cref="GeneratedLocalGameTarget"/>.
    /// </summary>
    /// <param name="options">The options to use for the game target</param>
    /// <param name="shipPlacementGenerator">The ship placement generator</param>
    /// <param name="loggerFactory">The logger factory</param>
    public GeneratedLocalGameTarget(
        GeneratedLocalGameTargetOptions options,
        ShipPlacementGenerator shipPlacementGenerator,
        ILoggerFactory loggerFactory)
        : base(options, loggerFactory)
    {
        _options = options;
        _shipPlacementGenerator = shipPlacementGenerator;
    }

    /// <inheritdoc />
    public override string Name => "Generated Local Game Target";

    /// <inheritdoc />
    protected override GameField<ServerCellValue> CreateNewGameField()
    {
        const int waterWeight = 0;
        // Generate the ship placements on a grid of the given size.
        var generateShipPlacements = _shipPlacementGenerator.Generate(_options.Rows, _options.Columns);

        // Transform the generated ship placements into a initial game field value.
        var gameFieldInitialValue =
            generateShipPlacements.Cast<int>()
                .ToArray()
                .Select(shipWeight =>
                    new ServerCellValue(shipWeight == waterWeight ? CellState.Water : CellState.Ship, shipWeight));

        // Initialize the game field with the generated initial value.
        var gameField = new GameField<ServerCellValue>(_options.Rows, _options.Columns);
        gameField.Initialize(gameFieldInitialValue.ToArray());

        return gameField;
    }
}

/// <summary>
/// The options object for <see cref="GeneratedLocalGameTarget"/>.
/// </summary>
public record GeneratedLocalGameTargetOptions : LocalGameTargetOptions;