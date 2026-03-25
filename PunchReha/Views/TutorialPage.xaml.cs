namespace PunchReha.Views;

public partial class TutorialPage : ContentPage
{
    public TutorialPage()
    {
        InitializeComponent();
    }

    private async void OnStartClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//menu");
    }
}
