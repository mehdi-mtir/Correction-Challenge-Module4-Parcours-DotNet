using Microsoft.Extensions.Logging;
using ReservSalles.Maui.Configuration;
using ReservSalles.Maui.Services;
using ReservSalles.Maui.ViewModels;
using ReservSalles.Maui.Views;

namespace ReservSalles.Maui;

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

        // ---------- Services ----------
        builder.Services.AddSingleton<ISessionService, SessionService>();
        builder.Services.AddSingleton<IDialogueService, DialogueService>();
        builder.Services.AddSingleton<ICacheSalles, CacheSalles>();   // bonus D2

        // Client HTTP typé + ajout automatique du jeton (tâche B1)
        builder.Services.AddTransient<JetonHandler>();
        builder.Services.AddHttpClient<IApiReservSalles, ApiReservSalles>(client =>
            {
                client.BaseAddress = new Uri(ApiConfig.AdresseBase);
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddHttpMessageHandler<JetonHandler>();

        // ---------- ViewModels : une instance neuve par page ----------
        builder.Services.AddTransient<ConnexionViewModel>();
        builder.Services.AddTransient<SallesViewModel>();
        builder.Services.AddTransient<DetailSalleViewModel>();
        builder.Services.AddTransient<NouvelleReservationViewModel>();
        builder.Services.AddTransient<MesReservationsViewModel>();

        // ---------- Pages ----------
        builder.Services.AddTransient<ConnexionPage>();
        builder.Services.AddTransient<SallesPage>();
        builder.Services.AddTransient<DetailSallePage>();
        builder.Services.AddTransient<NouvelleReservationPage>();
        builder.Services.AddTransient<MesReservationsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
