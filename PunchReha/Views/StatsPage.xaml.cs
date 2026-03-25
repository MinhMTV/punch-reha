using PunchReha.ViewModels;

namespace PunchReha.Views;

public partial class StatsPage : ContentPage
{
    public StatsPage(StatsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
