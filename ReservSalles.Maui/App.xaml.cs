namespace ReservSalles.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Couleurs prévues pour un fond clair
        UserAppTheme = AppTheme.Light;

        MainPage = new AppShell();
    }

    // Fenêtre de l'application sur ordinateur (tâche C1)
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Title = "ReservSalles";

        if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
        {
            window.Width = 1100;
            window.Height = 750;
            window.MinimumWidth = 800;
            window.MinimumHeight = 600;
        }
        return window;
    }
}
