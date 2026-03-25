using System.Timers;
using PunchReha.Models;
using Timer = System.Timers.Timer;

namespace PunchReha.Services;

public enum GameState { Idle, Countdown, Playing, Paused, Finished }

public class GameSession
{
    public GameState State { get; set; } = GameState.Idle;
    public GameLevel? Level { get; set; }
    public long TimeRemainingMs { get; set; }
    public List<Target> Targets { get; set; } = new();
    public GameStats Stats { get; set; } = new();
    public int CountdownValue { get; set; }
}

/// <summary>
/// Core game engine. Manages targets, timing, scoring, and punch matching.
/// </summary>
public class GameEngine : IDisposable
{
    public event EventHandler<GameSession>? StateChanged;

    private GameSession _session = new();
    private Timer? _gameTimer;
    private Timer? _targetTimer;
    private Timer? _countdownTimer;
    private long _nextTargetId;
    private DateTime _gameStartTime;
    private readonly Random _random = new();

    public GameSession Session => _session;

    public void StartLevel(GameLevel level)
    {
        Stop();

        _session = new GameSession
        {
            State = GameState.Countdown,
            Level = level,
            TimeRemainingMs = level.DurationSeconds * 1000L,
            CountdownValue = 3
        };

        NotifyStateChanged();

        // Countdown timer
        _countdownTimer = new Timer(1000);
        _countdownTimer.Elapsed += OnCountdownTick;
        _countdownTimer.Start();
    }

    private void OnCountdownTick(object? sender, ElapsedEventArgs e)
    {
        _session.CountdownValue--;

        if (_session.CountdownValue <= 0)
        {
            _countdownTimer?.Stop();
            _countdownTimer?.Dispose();
            _countdownTimer = null;

            _session.State = GameState.Playing;
            _session.CountdownValue = 0;
            StartGameLoop();
        }

        NotifyStateChanged();
    }

    private void StartGameLoop()
    {
        if (_session.Level == null) return;

        _gameStartTime = DateTime.UtcNow;
        var level = _session.Level;

        // Main game timer (20 FPS update)
        _gameTimer = new Timer(50);
        _gameTimer.Elapsed += OnGameTick;
        _gameTimer.Start();

        // Target spawn timer
        _targetTimer = new Timer(level.TargetIntervalMs / level.SpeedMultiplier);
        _targetTimer.Elapsed += OnTargetSpawn;
        _targetTimer.Start();
    }

    private void OnGameTick(object? sender, ElapsedEventArgs e)
    {
        if (_session.State != GameState.Playing) return;

        var elapsed = (long)(DateTime.UtcNow - _gameStartTime).TotalMilliseconds;
        var remaining = (_session.Level!.DurationSeconds * 1000L) - elapsed;

        if (remaining <= 0)
        {
            EndGame();
            return;
        }

        // Remove expired targets and count misses
        var expired = _session.Targets.Where(t => t.WasMissed).ToList();
        if (expired.Any())
        {
            _session.Stats = _session.Stats.AddMiss();
            _session.Targets.RemoveAll(t => t.WasMissed);
        }

        _session.TimeRemainingMs = remaining;
        NotifyStateChanged();
    }

    private void OnTargetSpawn(object? sender, ElapsedEventArgs e)
    {
        if (_session.State != GameState.Playing || _session.Level == null) return;

        // Max 3 targets at once
        if (_session.Targets.Count >= 3) return;

        var direction = _session.Level.Directions[_random.Next(_session.Level.Directions.Length)];
        var target = new Target
        {
            Id = _nextTargetId++,
            Direction = direction,
            LifetimeMs = (long)(_session.Level.TargetLifetimeMs / _session.Level.SpeedMultiplier)
        };

        _session.Targets.Add(target);
        NotifyStateChanged();
    }

    public void HandlePunch(PunchEvent punchEvent)
    {
        if (_session.State != GameState.Playing || _session.Level == null) return;

        var matchingTarget = _session.Targets.FirstOrDefault(t => !t.IsHit && !t.IsExpired);

        if (matchingTarget != null && punchEvent.Power >= _session.Level.MinPower)
        {
            // HIT!
            var reactionMs = (long)(DateTime.UtcNow - matchingTarget.CreatedAt).TotalMilliseconds;
            _session.Stats = _session.Stats.AddHit(punchEvent.Power, reactionMs, punchEvent.Direction);

            _session.Targets.RemoveAll(t => t.Id == matchingTarget.Id);

            System.Diagnostics.Debug.WriteLine(
                $"[GameEngine] HIT! power={punchEvent.Power:F2} reaction={reactionMs}ms combo={_session.Stats.Combo}");
        }
        else
        {
            // MISS
            _session.Stats = _session.Stats.AddMiss();
        }

        NotifyStateChanged();
    }

    private void EndGame()
    {
        _session.State = GameState.Finished;
        _session.TimeRemainingMs = 0;
        _session.Targets.Clear();

        _gameTimer?.Stop();
        _targetTimer?.Stop();

        System.Diagnostics.Debug.WriteLine($"[GameEngine] Game ended! Stats: {_session.Stats}");
        NotifyStateChanged();
    }

    public void Pause()
    {
        if (_session.State == GameState.Playing)
        {
            _gameTimer?.Stop();
            _targetTimer?.Stop();
            _session.State = GameState.Paused;
            NotifyStateChanged();
        }
    }

    public void Resume()
    {
        if (_session.State == GameState.Paused)
        {
            _session.State = GameState.Playing;
            _gameTimer?.Start();
            _targetTimer?.Start();
            NotifyStateChanged();
        }
    }

    public void Stop()
    {
        _countdownTimer?.Stop();
        _countdownTimer?.Dispose();
        _gameTimer?.Stop();
        _gameTimer?.Dispose();
        _targetTimer?.Stop();
        _targetTimer?.Dispose();
        _session = new GameSession();
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke(this, _session);
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
