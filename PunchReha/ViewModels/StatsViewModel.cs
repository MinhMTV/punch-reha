using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PunchReha.Models;
using PunchReha.Services;

namespace PunchReha.ViewModels;

[QueryProperty(nameof(LevelNumber), "level")]
public partial class StatsViewModel : ObservableObject
{
    private readonly SessionStorage _storage = App.SessionStorage;

    [ObservableProperty] private int _levelNumber;
    [ObservableProperty] private GameLevel? _level;
    [ObservableProperty] private GameStats _stats = new();
    [ObservableProperty] private int _totalScore;
    [ObservableProperty] private int _bestScore;
    [ObservableProperty] private int _sessionCount;

    partial void OnLevelNumberChanged(int value)
    {
        Level = GameLevels.GetLevel(value);
        BestScore = _storage.GetBestScore(value);
        SessionCount = _storage.GetByLevel(value).Count;
    }

    /// <summary>
    /// Call this from GamePage when game finishes.
    /// Saves the session result.
    /// </summary>
    public void SaveGameResult(GameStats gameStats)
    {
        Stats = gameStats;
        TotalScore = gameStats.Hits * 10 + gameStats.MaxCombo * 5 + (int)(gameStats.AvgPower * 100);
        BestScore = _storage.GetBestScore(LevelNumber);

        if (Level != null)
        {
            var result = new GameSessionResult
            {
                LevelNumber = LevelNumber,
                LevelName = Level.Name,
                TotalScore = TotalScore,
                TotalPunches = gameStats.TotalPunches,
                Hits = gameStats.Hits,
                Misses = gameStats.Misses,
                Accuracy = gameStats.Accuracy,
                MaxPower = gameStats.MaxPower,
                AvgPower = gameStats.AvgPower,
                MaxCombo = gameStats.MaxCombo,
                AvgReactionMs = gameStats.AvgReactionMs,
                DurationSeconds = Level.DurationSeconds,
                PunchByDirection = new Dictionary<PunchDirection, int>(gameStats.PunchByDirection)
            };

            _storage.SaveSession(result);
            SessionCount = _storage.GetByLevel(LevelNumber).Count;
            BestScore = _storage.GetBestScore(LevelNumber);
        }
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
        await Shell.Current.GoToAsync($"history?level={LevelNumber}");
    }
}
