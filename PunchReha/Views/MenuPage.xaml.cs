using PunchReha.ViewModels;

namespace PunchReha.Views;

public partial class MenuPage : ContentPage
{
    public MenuPage(MenuViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
