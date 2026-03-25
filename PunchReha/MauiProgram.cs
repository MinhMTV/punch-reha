using PunchReha.ViewModels;
using PunchReha.Views;
using PunchReha.Services;
using Microsoft.Extensions.Logging;

namespace PunchReha;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Services
        builder.Services.AddSingleton<SessionStorage>();
        builder.Services.AddSingleton<SensorService>();

        // ViewModels
        builder.Services.AddSingleton<MenuViewModel>();
        builder.Services.AddTransient<GameViewModel>();
        builder.Services.AddTransient<StatsViewModel>();
        builder.Services.AddTransient<HistoryViewModel>();

        // Views
        builder.Services.AddSingleton<MenuPage>();
        builder.Services.AddTransient<GamePage>();
        builder.Services.AddTransient<StatsPage>();
        builder.Services.AddTransient<HistoryPage>();

        return builder.Build();
    }
}
