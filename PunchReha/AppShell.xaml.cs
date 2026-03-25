namespace PunchReha;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute("game", typeof(Views.GamePage));
        Routing.RegisterRoute("stats", typeof(Views.StatsPage));
        Routing.RegisterRoute("history", typeof(Views.HistoryPage));
    }
}
