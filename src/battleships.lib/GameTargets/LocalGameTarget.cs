using battleships.lib.GameTargets.Models;
using battleships.lib.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace battleships.lib.GameTargets;

/// <summary>
/// Base class for local game targets.
/// </summary>
public abstract class LocalGameTarget : IGameTarget
{
    /// <summary>
    /// The logger.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// The options to use for the game target.
    /// </summary>
    private readonly LocalGameTargetOptions _options;

    /// <summary>
    /// Represents the current state of the match.
    /// <c>
    ///   null! - because it is initialized in the constructor calling InitializeMatch()
    /// </c>
    /// </summary>
    private GameTargetMatchState _state = null;

    /// <summary>
    /// The number of available tries.
    /// </summary>
    private int AvailableTries { get; set; } = int.MaxValue;

    protected LocalGameTarget(LocalGameTargetOptions options, ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger<LocalGameTarget>();
        _options = options;
    }

    /// <inheritdoc />
    public abstract string Name { get; }

    public event EventHandler<GameStateChangedEventArgs> OnGameStateChanged;

    /// < inheritdoc />
    public Task<FireResponse> GetFireStatusAsync(bool test = false)
    {
        if (_state == null)
        {
            InitializeMatch();
        }

        return Task.FromResult(CreateFireResponseFromState());
    }

    public Task<StatusGameResponse> GetStatusAsync()
    {
        return Task.FromResult(new StatusGameResponse(_state.MapId, _state.MapCount, _state.MoveCount,
            _state.TotalMoveCount));
    }

    /// < inheritdoc />
    public Task<FireResponse> FireAtPositionAsync(int row, int column, bool test = false)
    {
        // If the game is finished and the all maps are completed then return the current state
        // otherwise initialize a new game field
        if (_state.MatchFinished)
        {
            if (_state.MapCount == _state.MapId)
                return Task.FromResult(CreateFireResponseFromState());

            if (_state.MatchFinished)
            {
                if (_state.MapCount - 1 != _state.MapId)
                {
                    InitializeNewGameField();
                }
                else
                {
                    // start a new game from the beginning
                    // the match is finished and all the maps are completed
                    InitializeMatch();
                }
            }
        }

        var position = new GameCellPosition(row, column);

        // If the position is not a valid position or already revealed
        if (!_state.GameFieldDefinition.IsCellPositionInsideField(position)
            || _state.GameField.GetCellStatus(position) != Constants.GridCellUnknown)
        {
            var err = CreateFireResponseFromState() with { Cell = string.Empty, Result = false };
            OnOnGameStateChanged(CreateGameStateChangedEventArgsFromState());
            
            return Task.FromResult(err);
        }
        
        _state.MoveCount++;
        _state.TotalMoveCount++;

        string cellResponseValue;

        // If any of the ships are hit
        var cellStatus = _state.GameFieldDefinition.GetCellStatus(position);

        switch (cellStatus.CellState)
        {
            case CellState.Ship:
                _state.GameFieldDefinition.SetCellStatus(position,
                    cellStatus with { ShipWeight = cellStatus.ShipWeight * -1 });
                _state.GameField.SetCellStatus(position, Constants.GridCellShip);
                cellResponseValue = Constants.GridCellShip.ToString();
                break;
            // If the position is water
            case CellState.Water:
                _state.GameField.SetCellStatus(position, Constants.GridCellWater);
                cellResponseValue = Constants.GridCellWater.ToString();
                break;
            case CellState.Unknown:
            default:
                throw new Exception(
                    "Invalid cell state for game target. (Game target acts as `server`. He knows everything.)");
        }

        // If he sunk the AvengersHelicarrier ship then he can use the avenger
        if (!_state.GameFieldDefinition.ExistsValue(new ServerCellValue(CellState.Ship,
                Constants.AvengersHelicarrierWeight)) && !_state.AvengerUsed)
        {
            _state.AvengerAvailable = true;
        }

        //  if he sunk all the ships
        if (!_state.GameFieldDefinition.Cells.Any(p => p.ShipWeight > 0))
        {
            _state.MatchFinished = true;
            _state.GameFinished = (_state.MapCount - 1) == _state.MapId;
        }

        var result = CreateFireResponseFromState() with
        {
            Cell = cellResponseValue,
            Result = true
        };

        // If the match is finished the panaxeo api returns new map id
        if (_state.MatchFinished)
        {
            result = result with { MapId = result.MapId + 1 };
        }

        OnOnGameStateChanged(CreateGameStateChangedEventArgsFromState());

        return Task.FromResult(result);
    }

    /// < inheritdoc />
    public Task<AvengerFireResponse> FireWithAvengerAsync(int row, int column, string avenger, bool test = false)
    {
        // If the game is finished and the all maps are completed then return the current state
        // otherwise initialize a new game field
        if (_state.MatchFinished)
        {
            if (_state.MapCount == _state.MapId)
                return Task.FromResult(CreateAvengerFireResponseFromState());

            if (_state.MatchFinished)
            {
                if (_state.MapCount - 1 != _state.MapId)
                {
                    InitializeNewGameField();
                }
                else
                {
                    // start a new game from the beginning
                    // the match is finished and all the maps are completed
                    InitializeMatch();
                }
            }
        }

        var position = new GameCellPosition(row, column);

        if (!_state.AvengerAvailable)
            return Task.FromResult(CreateAvengerFireResponseFromState() with { Cell = string.Empty, Result = false });

        _state.MoveCount++;
        _state.TotalMoveCount++;

        var result = CreateAvengerFireResponseFromState() with { MoveCount = _state.MoveCount + 1 };

        // If any of the ships are hit
        var cellStatus = _state.GameFieldDefinition.GetCellStatus(position);

        _state.AvengerUsed = true;
        var avengerToUse = Enum.Parse<Avenger>(avenger.ToLowerInvariant(), ignoreCase: true);

        // If the position is not a valid position or already revealed
        if (!_state.GameFieldDefinition.IsCellPositionInsideField(position)
            || (_state.GameField.GetCellStatus(position) != Constants.GridCellUnknown
                && avengerToUse !=
                Avenger.Hulk)) // Hulk can fire at already revealed position (then the whole ship will be destroyed)
        {
            result.Cell = string.Empty;
            result.Result = false;

            OnOnGameStateChanged(CreateGameStateChangedEventArgsFromState());
            
            return Task.FromResult(result);
        }

        string cellResponseValue;

        switch (cellStatus.CellState)
        {
            case CellState.Ship:
                _state.GameFieldDefinition.SetCellStatus(position,
                    cellStatus with { ShipWeight = cellStatus.ShipWeight * -1 });
                _state.GameField.SetCellStatus(position, Constants.GridCellShip);
                cellResponseValue = Constants.GridCellShip.ToString();
                break;
            // If the position is water
            case CellState.Water:
                _state.GameField.SetCellStatus(position, Constants.GridCellWater);
                cellResponseValue = Constants.GridCellWater.ToString();
                break;
            case CellState.Unknown:
            default:
                throw new Exception(
                    "Invalid cell state for game target. (Game target acts as `server`. He knows everything.)");
        }

        // use the avenger
        var avengerResults = avengerToUse switch
        {
            Avenger.Hulk => HulkTheAvengerFiringStrategy.Fire(_state, position),
            Avenger.Thor => ThorTheAvengerFiringStrategy.Fire(_state),
            Avenger.Ironman => IronmanTheAvengerFiringStrategy.Fire(_state),
            _ => throw new Exception("Invalid avenger")
        };

        result.Cell = cellResponseValue;
        result.Result = true;

        result.AvengerResult = avengerResults;

        // If he sunk all the ships
        if (!_state.GameFieldDefinition.Cells.Any(p => p.ShipWeight > 0))
        {
            _state.MatchFinished = true;
            _state.GameFinished = _state.MapCount == _state.MapId;
        }

        result.Grid = string.Join("", _state.GameField.Cells);

        _state.AvengerAvailable = false;

        // If the match is finished the panaxeo api returns new map id
        if (_state.MatchFinished)
        {
            result = result with { MapId = result.MapId + 1 };
        }

        OnOnGameStateChanged(CreateGameStateChangedEventArgsFromState());

        return Task.FromResult(result);
    }

    /// <summary>
    /// Resets the ongoing game, but your attempt will be counted as one full game without saving the score.
    /// </summary>
    /// <param name="test">Indicates whether the reset is a test or not.</param>
    /// <returns>The reset game response.</returns>/// 
    public Task<ResetGameResponse> ResetGameAsync(bool test = false)
    {
        if (test)
        {
            AvailableTries--;
        }

        if (!test)
        {
            InitializeMatch();
        }

        return Task.FromResult(new ResetGameResponse(AvailableTries));
    }

    public GameStateMemento SaveToMemento()
    {
        if (_state == null)
        {
            InitializeMatch();
        }

        var memento = new GameStateMemento(
            MatchStateSerialized: JsonConvert.SerializeObject(_state),
            AvailableTries);

        return memento;
    }

    public void RestoreFromMemento(GameStateMemento memento)
    {
        ArgumentNullException.ThrowIfNull(memento);

        _state = JsonConvert.DeserializeObject<GameTargetMatchState>(memento.MatchStateSerialized)!;
        AvailableTries = memento.AvailableTries;
    }

    /// <summary>
    ///  Creates a new game field.
    /// </summary>
    /// <returns>The new game field.</returns>
    protected abstract GameField<ServerCellValue> CreateNewGameField();

    private GameStateChangedEventArgs CreateGameStateChangedEventArgsFromState()
        => new()
        {
            Rows = _state.GameFieldDefinition.Rows,
            Columns = _state.GameFieldDefinition.Columns,
            MapId = _state.MapId,
            MapCount = _state.MapCount,
            MoveCount = _state.MoveCount,
            TotalMoveCount = _state.TotalMoveCount,
            AvengerAvailable = _state.AvengerAvailable,
            AvengerUsed = _state.AvengerUsed,
            MatchFinished = _state.MatchFinished,
            GameFinished = _state.GameFinished,
            GameField = _state.GameField,
            GameFieldDefinition = _state.GameFieldDefinition
        };

    /// <summary>
    /// Initializes the match.
    /// </summary>
    private void InitializeMatch()
    {
        _state = new GameTargetMatchState(_options.Rows, _options.Columns)
        {
            MapCount = _options.MapCount,
            MapId = 0
        };

        var gameField = CreateNewGameField();
        _state.SetDefinitionGameField(gameField);

        OnOnGameStateChanged(CreateGameStateChangedEventArgsFromState());
    }

    /// <summary>
    /// Initializes a new game field.
    /// </summary>
    private void InitializeNewGameField()
    {
        _state.MapId++;
        _state.MatchFinished = false;
        _state.AvengerAvailable = false;
        _state.AvengerUsed = false;
        _state.MoveCount = 0;

        var gameField = CreateNewGameField();

        _state.SetDefinitionGameField(gameField);
    }

    /// <summary>
    /// Creates a fire response from the current state.
    /// </summary>
    /// <returns>The fire response.</returns>
    private FireResponse CreateFireResponseFromState()
        => new()
        {
            MoveCount = _state.MoveCount,
            AvengerAvailable = _state.AvengerAvailable,
            GameFinished = _state.GameFinished,
            MapId = _state.MapId,
            MapCount = _state.MapCount,
            Grid = string.Join("", _state.GameField.Cells),
        };

    /// <summary>
    /// Creates an avenger fire response from the current state.
    /// </summary>
    /// <returns>The avenger fire response.</returns>
    private AvengerFireResponse CreateAvengerFireResponseFromState()
        => new(CreateFireResponseFromState());

    private void OnOnGameStateChanged(GameStateChangedEventArgs e)
    {
        OnGameStateChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Fires the Thor's ability.
    /// Thor's ability will hit 10 random map points at maximum (at maximum = if there are fewer untouched map points
    /// available than 10, all of them will be targeted by this ability)
    /// </summary>
    private static class ThorTheAvengerFiringStrategy
    {
        /// <summary>
        /// Number of hits Thor can make.
        /// </summary>
        private const int MaxHits = 10;

        /// <summary>
        /// The random number generator.
        /// </summary>
        private static readonly Random Random = new();

        /// <summary>
        /// Fires with Thor's ability.
        /// </summary>
        /// <param name="state">The current match state.</param>
        /// <returns>The avenger results.</returns>
        public static List<AvengerResult> Fire(GameTargetMatchState state)
        {
            var avengerResults = new List<AvengerResult>();

            var getUntouchedCells = state.GameField.Cells
                .Select((cellValue, index) => new { cellValue, index })
                .Where(x => x.cellValue == Constants.GridCellUnknown)
                .ToList();

            var maxCount = Math.Min(getUntouchedCells.Count, MaxHits);

            for (var i = 0; i < maxCount; i++)
            {
                var randomCellIndex = Random.Next(getUntouchedCells.Count());
                var randomCell = getUntouchedCells.ElementAt(randomCellIndex);

                var randomCellPosition = state.GameFieldDefinition.GetCellPosition(randomCell.index);

                var cellStatus = state.GameFieldDefinition.GetCellStatus(randomCellPosition);

                if (cellStatus.CellState == CellState.Ship)
                {
                    state.GameFieldDefinition.SetCellStatus(randomCellPosition,
                        cellStatus with { ShipWeight = cellStatus.ShipWeight * -1 });
                    state.GameField.SetCellStatus(randomCellPosition, Constants.GridCellShip);

                    avengerResults.Add(new AvengerResult(
                        new MapPoint(X: randomCellPosition.Column, Y: randomCellPosition.Row),
                        true
                    ));
                }
                else if (cellStatus.CellState == CellState.Water)
                {
                    state.GameField.SetCellStatus(randomCellPosition, Constants.GridCellWater);

                    avengerResults.Add(new AvengerResult(
                        new MapPoint(X: randomCellPosition.Column, Y: randomCellPosition.Row),
                        Hit: false));
                }

                getUntouchedCells.RemoveAt(randomCellIndex);
            }

            return avengerResults;
        }
    }

    /// <summary>
    /// Fires the Ironman's ability.
    /// Ironman's ability will return 1 map point of the smallest non-destroyed ship, this map point will be
    /// unaffected (the purpose of this ability is to give a hint to the user)
    /// </summary>
    private static class IronmanTheAvengerFiringStrategy
    {
        private static readonly Random Random = new();

        /// <summary>
        /// Fires with Ironman's ability.
        /// </summary>
        /// <param name="state">The current match state.</param>
        /// <returns>The avenger results.</returns>
        public static List<AvengerResult> Fire(GameTargetMatchState state)
        {
            // Find non-destroyed ship cells, grouped by ship weight
            var shipCellsGroupedByWeight = state.GameFieldDefinition.Cells
                .Select((cellValue, index) => new { cellValue.ShipWeight, Index = index })
                .Where(x => x.ShipWeight > 0)
                .GroupBy(x => x.ShipWeight)
                .OrderBy(group => group.Key);

            // Get the group with the smallest ship weight
            var smallestShipGroup = shipCellsGroupedByWeight.FirstOrDefault();

            if (smallestShipGroup == null)
                return new List<AvengerResult>();

            // Select a random cell from the smallest ship group
            var randomCellIndex = Random.Next(smallestShipGroup.Count());
            var randomCell = smallestShipGroup.ElementAt(randomCellIndex);

            var randomCellPosition = state.GameFieldDefinition.GetCellPosition(randomCell.Index);

            return
            [
                new AvengerResult(new MapPoint(X: randomCellPosition.Column, Y: randomCellPosition.Row), Hit: false)
            ];
        }
    }

    /// <summary>
    /// Fires the Hulk's ability.
    /// Hulk's ability will destroy the whole ship if the map point specified by the row/column combination at the
    /// api endpoint hits the ship (all the map points belonging to this ship will be marked as destroyed)
    /// </summary>
    private static class HulkTheAvengerFiringStrategy
    {
        /// <summary>
        /// Fires with Hulk's ability.
        /// </summary>
        /// <param name="state">The current match state.</param>
        /// <param name="position">The position the user fired at.</param>
        /// <returns>The avenger results.</returns>
        public static List<AvengerResult> Fire(GameTargetMatchState state, GameCellPosition position)
        {
            var avengerResults = new List<AvengerResult>();
            var gameField = state.GameFieldDefinition;

            // Check if the targeted position is part of a ship
            var targetedCell = gameField.GetCellStatus(position);
            if (targetedCell.CellState != CellState.Ship)
            {
                // Cell content is not part of a ship
                return avengerResults;
            }

            // need to multiply by -1 because the ship weight is negative (already hit). but we need the the parts of the ship which are not hit
            var targetedShipId = targetedCell.ShipWeight * -1; // Ship ID is the ship weight (unique for each ship)

            var liveShipCells = state.GameFieldDefinition.Cells
                .Select((cellValue, index) => new { CellValue = cellValue, Index = index })
                .Where(x => x.CellValue.ShipWeight == targetedShipId);

            foreach (var shipCell in liveShipCells)
            {
                var shipCellPosition = state.GameFieldDefinition.GetCellPosition(shipCell.Index);
                state.GameFieldDefinition.SetCellStatus(shipCellPosition,
                    shipCell.CellValue with { ShipWeight = shipCell.CellValue.ShipWeight * -1 });
                state.GameField.SetCellStatus(shipCellPosition, Constants.GridCellShip);

                avengerResults.Add(new AvengerResult(
                    new MapPoint(X: shipCellPosition.Column, Y: shipCellPosition.Row), Hit: true));
            }

            return avengerResults;
        }
    }
}

/// <summary>
/// The options object for <see cref="LocalGameTarget"/>.
/// </summary>
public abstract record LocalGameTargetOptions
{
    /// <summary>
    /// The number of rows int the map.
    /// </summary>
    public int Rows { get; set; }

    /// <summary>
    /// The number of columns int the map.
    /// </summary>
    public int Columns { get; set; }

    /// <summary>
    /// Fixed number of maps which are required to complete before completing one full game.
    /// </summary>
    public int MapCount { get; set; }
}