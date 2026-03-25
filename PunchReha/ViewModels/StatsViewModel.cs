using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PunchReha.Models;
using PunchReha.Services;

namespace PunchReha.ViewModels;

[QueryProperty(nameof(LevelNumber), "level")]
[QueryProperty(nameof(Hits), "hits")]
[QueryProperty(nameof(Misses), "misses")]
[QueryProperty(nameof(TotalPunches), "punches")]
[QueryProperty(nameof(Accuracy), "accuracy")]
[QueryProperty(nameof(MaxPower), "maxpower")]
[QueryProperty(nameof(AvgPower), "avgpower")]
[QueryProperty(nameof(MaxCombo), "maxcombo")]
[QueryProperty(nameof(AvgReactionMs), "reaction")]
public partial class StatsViewModel : ObservableObject
{
    private readonly SessionStorage _storage = App.SessionStorage;

    [ObservableProperty] private int _levelNumber;
    [ObservableProperty] private GameLevel? _level;
    [ObservableProperty] private int _totalScore;
    [ObservableProperty] private int _bestScore;
    [ObservableProperty] private int _sessionCount;

    // Stats from game (passed as query params)
    [ObservableProperty] private int _hits;
    [ObservableProperty] private int _misses;
    [ObservableProperty] private int _totalPunches;
    [ObservableProperty] private float _accuracy;
    [ObservableProperty] private float _maxPower;
    [ObservableProperty] private float _avgPower;
    [ObservableProperty] private int _maxCombo;
    [ObservableProperty] private long _avgReactionMs;

    private bool _saved;

    partial void OnLevelNumberChanged(int value)
    {
        Level = GameLevels.GetLevel(value);
        BestScore = _storage.GetBestScore(value);
        SessionCount = _storage.GetByLevel(value).Count;
        SaveIfReady();
    }

    partial void OnHitsChanged(int value) => SaveIfReady();

    private void SaveIfReady()
    {
        if (_saved || Level == null || Hits == 0 && Misses == 0) return;
        _saved = true;

        TotalScore = Hits * 10 + MaxCombo * 5 + (int)(AvgPower * 100);

        var result = new GameSessionResult
        {
            LevelNumber = LevelNumber,
            LevelName = Level.Name,
            TotalScore = TotalScore,
            TotalPunches = TotalPunches,
            Hits = Hits,
            Misses = Misses,
            Accuracy = Accuracy,
            MaxPower = MaxPower,
            AvgPower = AvgPower,
            MaxCombo = MaxCombo,
            AvgReactionMs = AvgReactionMs,
            DurationSeconds = Level.DurationSeconds,
        };

        _storage.SaveSession(result);
        BestScore = _storage.GetBestScore(LevelNumber);
        SessionCount = _storage.GetByLevel(LevelNumber).Count;
    }

    [RelayCommand]
    private async Task PlayAgain()
    {
        await Shell.Current.GoToAsync($"game?level={LevelNumber}");
    }

    [RelayCommand]
    private async Task BackToMenu()
    {
        await Shell.Current.GoToAsync("//menu");
    }

    [RelayCommand]
    private async Task ViewHistory()
    {
        await Shell.Current.GoToAsync("history");
    }
}
