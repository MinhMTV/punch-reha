using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PunchReha.Models;
using PunchReha.Services;

namespace PunchReha.ViewModels;

[QueryProperty(nameof(LevelNumber), "level")]
public partial class GameViewModel : ObservableObject, IDisposable
{
    private readonly GameEngine _engine = new();
    private readonly TouchPunchDetector _touchDetector = new();

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

    public TouchPunchDetector TouchDetector => _touchDetector;

    public GameViewModel()
    {
        _engine.StateChanged += OnEngineStateChanged;
        _touchDetector.PunchDetected += (_, e) => _engine.HandlePunch(e);
    }

    partial void OnLevelNumberChanged(int value)
    {
        Level = GameLevels.GetLevel(value);
        _touchDetector.Start();
        _engine.StartLevel(Level);
    }

    private void OnEngineStateChanged(object? sender, GameSession session)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            GameState = session.State;
            TimeRemainingMs = session.TimeRemainingMs;
            CountdownValue = session.CountdownValue;
            Targets = new List<Target>(session.Targets);
            Stats = session.Stats;
            Score = session.Stats.Hits * 10 + session.Stats.MaxCombo * 5;

            if (session.State == GameState.Finished)
            {
                NavigateToStats();
            }
        });
    }

    private async void NavigateToStats()
    {
        await Shell.Current.GoToAsync($"stats?level={LevelNumber}");
    }

    public void OnTouchDown(float x, float y)
    {
        _touchDetector.SetScreenSize(ScreenWidth, ScreenHeight);
        _touchDetector.OnTouchDown(x, y);
    }

    public void OnTouchUp()
    {
        _touchDetector.OnTouchUp();
    }

    [RelayCommand]
    private void Pause() => _engine.Pause();

    [RelayCommand]
    private void Resume() => _engine.Resume();

    [RelayCommand]
    private async Task GoBack()
    {
        _engine.Stop();
        _touchDetector.Stop();
        await Shell.Current.GoToAsync("..");
    }

    public void Dispose()
    {
        _engine.Dispose();
        _touchDetector.Stop();
        GC.SuppressFinalize(this);
    }
}
