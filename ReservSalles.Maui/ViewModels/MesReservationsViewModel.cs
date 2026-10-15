using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReservSalles.Maui.Models;
using ReservSalles.Maui.Services;

namespace ReservSalles.Maui.ViewModels;

// Tâche B5 : mes réservations à venir, annulation confirmée
public partial class MesReservationsViewModel : VueModeleBase
{
    private readonly IApiReservSalles api;
    private readonly IDialogueService dialogue;

    public ObservableCollection<Reservation> Reservations { get; } = new();

    [ObservableProperty]
    private bool aucuneReservation;

    public MesReservationsViewModel(IApiReservSalles api, ISessionService session, IDialogueService dialogue)
        : base(session)
    {
        this.api = api;
        this.dialogue = dialogue;
    }

    [RelayCommand]
    private Task ChargerAsync() => ExecuterAsync(async () =>
    {
        var reservations = await api.MesReservationsAsync();
        Reservations.Clear();
        foreach (var reservation in reservations)
            Reservations.Add(reservation);
        AucuneReservation = Reservations.Count == 0;
    });

    [RelayCommand]
    private async Task AnnulerAsync(Reservation reservation)
    {
        var confirme = await dialogue.ConfirmerAsync("Annuler la réservation",
            $"Annuler « {reservation.Objet} » ({reservation.SalleNom}, {reservation.Jour}, {reservation.Creneau}) ?");
        if (!confirme)
            return;

        if (await ExecuterAsync(() => api.AnnulerAsync(reservation.Id)))
        {
            Reservations.Remove(reservation);
            AucuneReservation = Reservations.Count == 0;
        }
    }
}
