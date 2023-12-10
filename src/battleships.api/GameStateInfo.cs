using battleships.lib;

namespace battleships.api;

public class GameStateInfo
{
    public string Token { get; set; }
    public int Rows { get; set; }
    public int Columns { get; set; }
    public int MapId { get; set; }
    public bool AvengerAvailable { get; set; }
    public bool AvengerUsed { get; set; }
    public int MoveCount { get; set; }
    public int TotalMoveCount { get; set; }
    public bool MatchFinished { get; set; }
    public bool GameFinished { get; set; }
    public int MapCount { get; init; }
    public bool IsSimulation { get; set; }
    public char[] GameField { get; set; }
    public ServerCellValue[] GameFieldDefinition { get; set; }
}