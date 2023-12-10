namespace battleships.lib.GameTargets.Models;

/// <summary>
/// Represents the state of the game that game target uses internally.
/// </summary>
public class GameTargetMatchState
{
    /// <summary>
    /// Initializes a new instance of <see cref="GameTargetMatchState"/>.
    /// </summary>
    /// <param name="rows">The size of the game field in rows.</param>
    /// <param name="columns">The size of the game field in columns.</param>
    public GameTargetMatchState(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        
        SetDefinitionGameField(new GameField<ServerCellValue>(rows, columns));
    }

    /// <summary>
    /// Total number of rows in the map.
    /// </summary>
    public int Rows { get; }

    /// <summary>
    /// Total number of columns in the map.
    /// </summary>
    public int Columns { get; }

    /// <summary>
    /// ID of the map, on which was called last player's move.
    /// This value will change when player beats current map.
    /// </summary>
    public int MapId { get; set; }

    /// <summary>
    /// Avenger availability after the player's move.
    /// </summary>
    public bool AvengerAvailable { get; set; }

    /// <summary>
    /// Weather Avenger was used in the current match.
    /// </summary>
    public bool AvengerUsed { get; set; }
    
    /// <summary>
    /// Number of valid moves which were made on the current map.
    /// Invalid moves such as firing at the same position multiple times are not included.
    /// </summary>
    public int MoveCount { get; set; }

    /// <summary>
    /// Total number of moves which were made on all maps.
    /// </summary>
    public int TotalMoveCount { get; set; }

    /// <summary>
    /// Weather the match was finished.
    /// </summary>
    public bool MatchFinished { get; set; }
    
    /// <summary>
    /// Denotes if player successfully finished currently ongoing game =>
    /// if player completed mapCount maps. Valid move after getting true in this field
    /// results in new game (or error if player has already achieved max number of tries).
    /// </summary>    
    public bool GameFinished { get; set; }

    /// <summary>
    /// 144 chars (12x12 grid) representing updated state of map,
    /// '*' is unknown, 'X' is ship, '.' is water.
    /// </summary>
    public GameField<char> GameField { get; set; }

    /// <summary>
    /// 144 chars (12x12 grid) representing updated state of map,
    /// '*' is unknown, 'X' is ship, '.' is water.
    /// </summary>
    public GameField<ServerCellValue> GameFieldDefinition { get; set; }

    /// <summary>
    /// Fixed number of maps which are required to complete before completing one full game.
    /// </summary>
    public int MapCount { get; init; } = 1;
    
    public void SetDefinitionGameField(GameField<ServerCellValue> gameField)
    {
        GameFieldDefinition = gameField;
        ResetGameField();
    }

    private void ResetGameField()
    {
        var gameField = new GameField<char>(Rows, Columns);
        gameField.Initialize(Enumerable.Repeat(Constants.GridCellUnknown, Rows * Columns).ToArray());

        GameField = gameField;
    }
}