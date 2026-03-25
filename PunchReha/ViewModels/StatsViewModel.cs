using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PunchReha.Models;

namespace PunchReha.ViewModels;

[QueryProperty(nameof(LevelNumber), "level")]
public partial class StatsViewModel : ObservableObject
{
    [ObservableProperty] private int _levelNumber;
    [ObservableProperty] private GameLevel? _level;
    [ObservableProperty] private GameStats _stats = new();
    [ObservableProperty] private int _totalScore;

    partial void OnLevelNumberChanged(int value)
    {
        Level = GameLevels.GetLevel(value);
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
}
