using battleships.lib.GameTargets.Models;

namespace battleships.lib.GameTargets;

public interface IGameTarget
{
    /// <summary>
    /// The friendly name of the game target.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Get the status of an ongoing game
    /// </summary>
    Task<FireResponse> GetFireStatusAsync(bool test = false);

    /// <summary>
    /// Fire at a specified position
    /// </summary>
    Task<FireResponse> FireAtPositionAsync(int row, int column, bool test = false);

    /// <summary>
    /// Fire at a specified position with the help of an avenger
    /// <c>
    /// thor    - ability will hit 10 random map points at maximum
    ///           (at maximum = if there are fewer untouched map points available than 10, all of them will be targeted
    ///            by this ability)
    /// ironman - ability will return 1 map point of the smallest non-destroyed ship, this map point will be unaffected
    ///           (the purpose of this ability is to give a hint to the user)
    /// hulk    - ability will destroy the whole ship if the map point specified by the row/column combination at the
    ///           api endpoint hits the ship (all the map points belonging to this ship will be marked as destroyed)
    /// </c>
    /// </summary>
    Task<AvengerFireResponse> FireWithAvengerAsync(int row, int column, string avenger, bool test = false);

    /// <summary>
    /// Resets the ongoing game, but your attempt will be counted as one full game without saving the score.
    /// </summary>
    Task<ResetGameResponse> ResetGameAsync(bool test = false);
}