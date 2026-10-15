using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReservSalles.Maui.Models;
using ReservSalles.Maui.Services;

namespace ReservSalles.Maui.ViewModels;

// Tâche B4 : saisie d'une réservation, validée côté client puis par l'API
public partial class NouvelleReservationViewModel : VueModeleBase, IQueryAttributable
{
    private readonly IApiReservSalles api;
    private int salleId;

    [ObservableProperty]
    private Salle? salle;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EnregistrerCommand))]
    private string objet = "";

    [ObservableProperty]
    private DateTime date = DateTime.Today.AddDays(1);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EnregistrerCommand))]
    private TimeSpan heureDebut = new(9, 0, 0);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EnregistrerCommand))]
    private TimeSpan heureFin = new(10, 0, 0);

    // double : la valeur d'un Stepper est un double
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EnregistrerCommand))]
    private double participants = 1;

    public DateTime DateMin => DateTime.Today;

    public NouvelleReservationViewModel(IApiReservSalles api, ISessionService session) : base(session)
    {
        this.api = api;
    }

    // Navigation « reserver?salleId=3&date=2026-10-20 »
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("salleId", out var id) && int.TryParse(id?.ToString(), out var valeur))
            salleId = valeur;

        if (query.TryGetValue("date", out var texte)
            && DateTime.TryParseExact(texte?.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                      DateTimeStyles.None, out var jour)
            && jour >= DateTime.Today)
            Date = jour;

        EnregistrerCommand.NotifyCanExecuteChanged();
        _ = ExecuterAsync(async () => Salle = await api.ObtenirSalleAsync(salleId));
    }

    // Validation côté client : Enregistrer reste grisé tant qu'elle échoue
    private bool PeutEnregistrer()
        => salleId > 0
           && Objet.Trim().Length >= 3
           && HeureFin > HeureDebut
           && Participants >= 1;

    [RelayCommand(CanExecute = nameof(PeutEnregistrer))]
    private async Task EnregistrerAsync()
    {
        var demande = new NouvelleReservation(
            salleId, Objet.Trim(), (int)Participants,
            Date.Date + HeureDebut, Date.Date + HeureFin);

        var reussi = await ExecuterAsync(() => api.CreerReservationAsync(demande));

        // Succès : retour au détail, qui se recharge en réapparaissant.
        // Échec (400, 409…) : le message de l'API reste affiché et la saisie est conservée.
        if (reussi)
            await Shell.Current.GoToAsync("..");
    }
}
