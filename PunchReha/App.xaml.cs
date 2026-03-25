using PunchReha.Services;

namespace PunchReha;

public partial class App : Application
{
    /// <summary>
    /// Global session storage (persisted to Preferences).
    /// </summary>
    public static SessionStorage SessionStorage { get; } = new();

    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
