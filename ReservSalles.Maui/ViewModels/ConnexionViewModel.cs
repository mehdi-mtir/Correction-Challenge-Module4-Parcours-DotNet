using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReservSalles.Maui.Services;

namespace ReservSalles.Maui.ViewModels;

// Tâche B1 : connexion
public partial class ConnexionViewModel : VueModeleBase
{
    private readonly IApiReservSalles api;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SeConnecterCommand))]
    private string identifiant = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SeConnecterCommand))]
    private string motDePasse = "";

    public ConnexionViewModel(IApiReservSalles api, ISessionService session) : base(session)
    {
        this.api = api;
    }

    private bool PeutSeConnecter()
        => !string.IsNullOrWhiteSpace(Identifiant) && !string.IsNullOrEmpty(MotDePasse);

    [RelayCommand(CanExecute = nameof(PeutSeConnecter))]
    private async Task SeConnecterAsync()
    {
        string? jeton = null;
        await ExecuterAsync(async () =>
        {
            jeton = await api.SeConnecterAsync(Identifiant.Trim(), MotDePasse);
            if (jeton is null)
                Message = "Identifiant ou mot de passe incorrect.";
        });

        if (jeton is not null)
        {
            MotDePasse = "";
            await session.OuvrirAsync(jeton);   // range le jeton puis ouvre l'onglet Salles
        }
    }
}
