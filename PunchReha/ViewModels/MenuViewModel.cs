using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PunchReha.Models;
using PunchReha.Services;

namespace PunchReha.ViewModels;

public partial class MenuViewModel : ObservableObject
{
    public IReadOnlyList<GameLevel> Levels => GameLevels.Levels;

    [RelayCommand]
    private async Task StartLevel(GameLevel level)
    {
        await Shell.Current.GoToAsync($"game?level={level.Number}");
    }
}
