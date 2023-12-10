namespace battleships.lib.GameTargets.Models;

public class GameStateChangedEventArgs
{
    public int Rows { get; set;  }
    public int Columns { get; set; }
    public int MapId { get; set; }
    public bool AvengerAvailable { get; set; }
    public bool AvengerUsed { get; set; }
    public int MoveCount { get; set; }
    public int TotalMoveCount { get; set; }
    public bool MatchFinished { get; set; }
    public bool GameFinished { get; set; }
    public int MapCount { get; init; } = 1;
    public GameField<char> GameField { get; set; }
    public GameField<ServerCellValue> GameFieldDefinition { get; set; }

}