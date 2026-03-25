using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PunchReha.Models;
using PunchReha.Services;

namespace PunchReha.ViewModels;

[QueryProperty(nameof(LevelNumber), "level")]
public partial class GameViewModel : ObservableObject, IDisposable
{
    private readonly GameEngine _engine = new();
    private readonly SensorService _sensor = new();

    [ObservableProperty] private int _levelNumber;
    [ObservableProperty] private GameLevel? _level;
    [ObservableProperty] private GameState _gameState = GameState.Idle;
    [ObservableProperty] private long _timeRemainingMs;
    [ObservableProperty] private int _countdownValue;
    [ObservableProperty] private List<Target> _targets = new();
    [ObservableProperty] private GameStats _stats = new();
    [ObservableProperty] private int _score;
    [ObservableProperty] private float _screenWidth;
    [ObservableProperty] private float _screenHeight;
    [ObservableProperty] private string _sensorMode = "Touch";

    public SensorService Sensor => _sensor;

    public GameViewModel()
    {
        _engine.StateChanged += OnEngineStateChanged;
        _sensor.PunchDetected += (_, e) => _engine.HandlePunch(e);
    }

    partial void OnLevelNumberChanged(int value)
    {
        Level = GameLevels.GetLevel(value);
        SensorMode = _sensor.Mode;
        _sensor.StartTouchMode();
        _engine.StartLevel(Level);
    }

    public void OnTouchUp()
    {
        _sensor.TouchDetector.OnTouchUp();
    }

    /// <summary>
    /// Called by the engine when state changes — also triggers feedback.
    /// </summary>
    private int _lastCombo;

    private void OnEngineStateChanged(object? sender, GameSession session)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var prevHits = _session.Hits;
            GameState = session.State;
            TimeRemainingMs = session.TimeRemainingMs;
            CountdownValue = session.CountdownValue;
            Targets = new List<Target>(session.Targets);
            Stats = session.Stats;
            Score = session.Stats.Hits * 10 + session.Stats.MaxCombo * 5;

            // Trigger feedback on new hit
            if (session.Stats.Hits > prevHits)
            {
                FeedbackService.OnHit(session.Stats.MaxPower);
                if (session.Stats.Combo > _lastCombo && session.Stats.Combo > 1)
                {
                    FeedbackService.OnCombo(session.Stats.Combo);
                }
                _lastCombo = session.Stats.Combo;
            }

            if (session.State == GameState.Finished)
            {
                NavigateToStats();
            }
        });
    }

    private async void NavigateToStats()
    {
        var stats = _session.Stats;
        var url = $"stats?level={LevelNumber}" +
                  $"&hits={stats.Hits}" +
                  $"&misses={stats.Misses}" +
                  $"&punches={stats.TotalPunches}" +
                  $"&accuracy={stats.Accuracy:F4}" +
                  $"&maxpower={stats.MaxPower:F4}" +
                  $"&avgpower={stats.AvgPower:F4}" +
                  $"&maxcombo={stats.MaxCombo}" +
                  $"&reaction={stats.AvgReactionMs}";

        _sensor.Stop();
        await Shell.Current.GoToAsync(url);
    }

    public void OnTouchDown(float x, float y)
    {
        _sensor.SetScreenSize(ScreenWidth, ScreenHeight);
        _sensor.TouchDetector.OnTouchDown(x, y);
    }

    public void OnTouchUp()
    {
        _sensor.TouchDetector.OnTouchUp();
    }

    [RelayCommand]
    private void Pause() => _engine.Pause();

    [RelayCommand]
    private void Resume() => _engine.Resume();

    [RelayCommand]
    private async Task GoBack()
    {
        _engine.Stop();
        _sensor.Stop();
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task ToggleSensorMode()
    {
        if (_sensor.IsUsingBle)
        {
            _sensor.DisconnectBle();
            SensorMode = _sensor.Mode;
        }
        else
        {
            var connected = await _sensor.TryConnectBleAsync();
            SensorMode = _sensor.Mode;
            if (!connected)
            {
                // Could show an alert here
            }
        }
    }

    public void Dispose()
    {
        _engine.Dispose();
        _sensor.Dispose();
        GC.SuppressFinalize(this);
    }
}
