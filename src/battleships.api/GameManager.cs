using System.Collections.Concurrent;
using battleships.lib;
using battleships.lib.GameTargets;
using battleships.lib.GameTargets.Models;
using battleships.lib.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace battleships.api;

public record FireRequest(
    bool IsSimulation,
    string Token,
    int? Row = null,
    int? Column = null,
    string? Avenger = null);

public record ResetRequest(bool IsSimulation, string Token);

public record StatusRequest(bool IsSimulation, string Token);

public interface IGameManager
{
    Task<FireResponse> Fire(FireRequest request);
    Task<AvengerFireResponse> FireWithAvenger(FireRequest request);
    Task<ResetGameResponse> Reset(ResetRequest fireRequest);
    Task<StatusGameResponse> Status(StatusRequest fireRequest);
}

public class GameManager : IGameManager
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IHubContext<MatchHub> _hubContext;
    private readonly ILogger<GameManager> _logger;

    private readonly GeneratedLocalGameTargetOptions _gameTargetOptions = new()
    {
        Rows = Constants.GridRows,
        Columns = Constants.GridColumns,
        MapCount = 200,
    };

    // Semaphore for each game token for thread safety
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    private readonly ShipPlacementGenerator _shipPlacementGenerator;

    public GameManager(
        GameManagerOptions options,
        IMemoryCache memoryCache,
        ILoggerFactory loggerFactory,
        IHubContext<MatchHub> hubContext)
    {
        _gameTargetOptions.MapCount = options.MapCount;
        _memoryCache = memoryCache;
        _loggerFactory = loggerFactory;
        _hubContext = hubContext;
        _logger = loggerFactory.CreateLogger<GameManager>();
        _shipPlacementGenerator = new ShipPlacementGenerator();
    }

    public async Task<FireResponse> Fire(FireRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new Exception("Unauthorized.");

        var gameId = GetGameId(request.Token, request.IsSimulation);
        var semaphore = GetSemaphoreForGameId(gameId);

        GeneratedLocalGameTarget? target = null;

        void GameTargetGameStateChanged(object? sender, GameStateChangedEventArgs args) =>
            OnGameStateChanged(request.Token, request.IsSimulation, args);

        try
        {
            await semaphore.WaitAsync();

            target = new GeneratedLocalGameTarget(_gameTargetOptions, _shipPlacementGenerator, _loggerFactory);
            target.OnGameStateChanged += GameTargetGameStateChanged;

            var gameStateMemento = GetGameState(gameId);
            if (gameStateMemento != null)
            {
                target.RestoreFromMemento(gameStateMemento);
            }

            FireResponse result;

            if (!request.Row.HasValue || !request.Column.HasValue)
            {
                result = await target.GetFireStatusAsync(request.IsSimulation);
            }
            else
            {
                if (request.Row > _gameTargetOptions.Rows - 1 || request.Column > _gameTargetOptions.Columns - 1)
                    throw new ArgumentException("Invalid values for row or column");

                result = await target.FireAtPositionAsync(request.Row.Value, request.Column.Value,
                    request.IsSimulation);
            }

            var memento = target.SaveToMemento();
            SetGameState(gameId, memento);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Fire request");
            throw;
        }
        finally
        {
            if (target != null)
                target.OnGameStateChanged -= GameTargetGameStateChanged;

            semaphore.Release();
        }
    }

    public async Task<AvengerFireResponse> FireWithAvenger(FireRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new Exception("Unauthorized.");

        var gameId = GetGameId(request.Token, request.IsSimulation);
        var semaphore = GetSemaphoreForGameId(gameId);

        void GameTargetGameStateChanged(object? sender, GameStateChangedEventArgs args) =>
            OnGameStateChanged(request.Token, request.IsSimulation, args);

        GeneratedLocalGameTarget? target = null;
        try
        {
            await semaphore.WaitAsync();

            target = new GeneratedLocalGameTarget(_gameTargetOptions, _shipPlacementGenerator, _loggerFactory);
            target.OnGameStateChanged += GameTargetGameStateChanged;

            var gameStateMemento = GetGameState(gameId);
            if (gameStateMemento != null)
            {
                target.RestoreFromMemento(gameStateMemento);
            }

            if (!request.Row.HasValue || !request.Column.HasValue || string.IsNullOrWhiteSpace(request.Avenger))
            {
                throw new ArgumentException("Invalid fire request");
            }

            if (!Enum.TryParse<Avenger>(request.Avenger, true, out _))
            {
                throw new ArgumentException("Invalid avenger");
            }

            if (request.Row > _gameTargetOptions.Rows - 1 || request.Column > _gameTargetOptions.Columns - 1)
                throw new ArgumentException("Invalid values for row or column");

            var result = await target.FireWithAvengerAsync(request.Row.Value, request.Column.Value,
                request.Avenger.ToLowerInvariant(), request.IsSimulation);

            var memento = target.SaveToMemento();
            SetGameState(gameId, memento);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Fire request");
            throw;
        }
        finally
        {
            if (target != null)
                target.OnGameStateChanged -= GameTargetGameStateChanged;

            semaphore.Release();
        }
    }

    public async Task<ResetGameResponse> Reset(ResetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new Exception("Unauthorized.");

        var gameId = GetGameId(request.Token, request.IsSimulation);
        var semaphore = GetSemaphoreForGameId(gameId);

        void GameTargetGameStateChanged(object? sender, GameStateChangedEventArgs args) =>
            OnGameStateChanged(request.Token, request.IsSimulation, args);

        GeneratedLocalGameTarget? target = null;

        try
        {
            await semaphore.WaitAsync();

            var gameStateMemento = GetGameState(gameId);

            if (gameStateMemento == null)
            {
                throw new Exception("No ongoing game found");
            }

            target = new GeneratedLocalGameTarget(_gameTargetOptions, _shipPlacementGenerator, _loggerFactory);
            target.OnGameStateChanged += GameTargetGameStateChanged;

            target.RestoreFromMemento(gameStateMemento);

            var result = await target.ResetGameAsync(request.IsSimulation);

            var memento = target.SaveToMemento();
            SetGameState(gameId, memento);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reset request");
            throw;
        }
        finally
        {
            if (target != null)
                target.OnGameStateChanged -= GameTargetGameStateChanged;

            semaphore.Release();
        }
    }

    public async Task<StatusGameResponse> Status(StatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new Exception("Unauthorized.");

        var gameId = GetGameId(request.Token, request.IsSimulation);
        var semaphore = GetSemaphoreForGameId(gameId);

        try
        {
            await semaphore.WaitAsync();

            var gameStateMemento = GetGameState(gameId);

            if (gameStateMemento == null)
            {
                throw new Exception("No ongoing game found");
            }

            var target = new GeneratedLocalGameTarget(_gameTargetOptions, _shipPlacementGenerator, _loggerFactory);
            target.RestoreFromMemento(gameStateMemento);

            var result = await target.GetStatusAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reset request");
            throw;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private string GetGameId(string token, bool isSimulation)
    {
        ArgumentNullException.ThrowIfNull(token);

        var tk = token.Replace("bearer", "", StringComparison.InvariantCultureIgnoreCase).Trim();
        return $"{tk}-{isSimulation}";
    }

    private SemaphoreSlim GetSemaphoreForGameId(string gameId)
    {
        return _semaphores.GetOrAdd(gameId, _ => new SemaphoreSlim(1, 1));
    }

    private GameStateMemento? GetGameState(string gameId)
    {
        // Try to retrieve the game state from the cache
        return _memoryCache.TryGetValue(gameId, out GameStateMemento? gameStateMemento)
            ? gameStateMemento
            : null;
    }

    private void SetGameState(string gameId, GameStateMemento gameState)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromDays(7)
        };

        _memoryCache.Set(gameId, gameState, cacheEntryOptions);
    }

    private void OnGameStateChanged(string token, bool isSimulation, GameStateChangedEventArgs args)
    {
        var gameState = new GameStateInfo
        {
            Token = token.Replace("bearer", "", StringComparison.InvariantCultureIgnoreCase).Trim(), // TODO: Remove this ugly piece 
            IsSimulation = isSimulation,
            Rows = args.Rows,
            Columns = args.Columns,
            MapId = args.MapId,
            MapCount = args.MapCount,
            MoveCount = args.MoveCount,
            TotalMoveCount = args.TotalMoveCount,
            AvengerAvailable = args.AvengerAvailable,
            AvengerUsed = args.AvengerUsed,
            MatchFinished = args.MatchFinished,
            GameFinished = args.GameFinished,
            GameField = args.GameField.Cells,
            GameFieldDefinition = args.GameFieldDefinition.Cells,
        };

        _hubContext.Clients.All.SendAsync("GameStateChanged", gameState);
    }
}

public class GameManagerOptions
{
    public int MapCount { get; set; } = 200;
}