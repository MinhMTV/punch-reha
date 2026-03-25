using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PunchReha.Models;
using PunchReha.Services;

namespace PunchReha.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly SessionStorage _storage = App.SessionStorage;

    [ObservableProperty] private List<GameSessionResult> _sessions = new();
    [ObservableProperty] private GlobalStats _globalStats = new();
    [ObservableProperty] private int _selectedTab; // 0 = All, 1 = By Level
    [ObservableProperty] private int _filterLevel;
    [ObservableProperty] private List<GameLevel> _availableLevels = new();

    public HistoryViewModel()
    {
        LoadData();
    }

    private void LoadData()
    {
        Sessions = _storage.GetAll();
        GlobalStats = _storage.GetGlobalStats();
        AvailableLevels = GameLevels.Levels.ToList();
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadData();
    }

    [RelayCommand]
    private void FilterByLevel(GameLevel? level)
    {
        if (level != null)
        {
            Sessions = _storage.GetByLevel(level.Number);
            FilterLevel = level.Number;
        }
        else
        {
            Sessions = _storage.GetAll();
            FilterLevel = 0;
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        _storage.Clear();
        LoadData();
    }

    [RelayCommand]
    private async Task Back()
    {
        await Shell.Current.GoToAsync("..");
    }
}
