using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReservSalles.Maui.Models;
using ReservSalles.Maui.Services;

namespace ReservSalles.Maui.ViewModels;

// Tâche B3 : détail d'une salle et réservations du jour choisi.
// Utilisé par la page de détail (téléphone) et par le panneau de droite (ordinateur).
public partial class DetailSalleViewModel : VueModeleBase, IQueryAttributable
{
    private readonly IApiReservSalles api;

    public ObservableCollection<Reservation> Reservations { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AUneSalle))]
    [NotifyPropertyChangedFor(nameof(AucuneSalle))]
    [NotifyCanExecuteChangedFor(nameof(ReserverCommand))]
    private Salle? salle;

    public bool AUneSalle => Salle is not null;
    public bool AucuneSalle => Salle is null;

    // Demain par défaut : les réservations de départ sont pour le lendemain
    [ObservableProperty]
    private DateTime date = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private bool aucuneReservation;

    public DetailSalleViewModel(IApiReservSalles api, ISessionService session) : base(session)
    {
        this.api = api;
    }

    // Navigation « salle?id=3 » (téléphone)
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var valeur) && int.TryParse(valeur?.ToString(), out var id))
            _ = ChargerSalleAsync(id);
    }

    public Task ChargerSalleAsync(int id) => ExecuterAsync(async () =>
    {
        Salle = await api.ObtenirSalleAsync(id);
        await ChargerReservationsAsync();
    });

    // Changer de date recharge les réservations
    partial void OnDateChanged(DateTime value)
    {
        if (Salle is not null)
            ActualiserCommand.Execute(null);
    }

    [RelayCommand]
    private Task ActualiserAsync() => ExecuterAsync(ChargerReservationsAsync);

    private async Task ChargerReservationsAsync()
    {
        if (Salle is null)
            return;

        var reservations = await api.ObtenirReservationsAsync(Salle.Id, Date);
        Reservations.Clear();
        foreach (var reservation in reservations)
            Reservations.Add(reservation);
        AucuneReservation = Reservations.Count == 0;
    }

    private bool PeutReserver() => Salle is not null;

    [RelayCommand(CanExecute = nameof(PeutReserver))]
    private async Task ReserverAsync()
    {
        var jour = Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        await Shell.Current.GoToAsync($"reserver?salleId={Salle!.Id}&date={jour}");
    }
}
