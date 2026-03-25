using PunchReha.ViewModels;

namespace PunchReha.Views;

public partial class MenuPage : ContentPage
{
    public MenuPage(MenuViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private async void OnHistoryTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("history");
    }
}
