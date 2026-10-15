using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReservSalles.Maui.Models;
using ReservSalles.Maui.Services;

namespace ReservSalles.Maui.ViewModels;

// Tâches B2 et C1, avec les bonus D2 (cache hors ligne) et D4 (commandes du menu Fichier)
public partial class SallesViewModel : VueModeleBase
{
    private readonly IApiReservSalles api;
    private readonly ICacheSalles cache;

    public ObservableCollection<Salle> Salles { get; } = new();

    public DetailSalleViewModel Detail { get; }

    public static bool EstBureau => DeviceInfo.Idiom == DeviceIdiom.Desktop;

    [ObservableProperty]
    private double capaciteMin;

    // Bonus D2 : bandeau « Hors ligne »
    [ObservableProperty]
    private bool estHorsLigne;

    public SallesViewModel(IApiReservSalles api, ISessionService session,
                           DetailSalleViewModel detail, ICacheSalles cache)
        : base(session)
    {
        this.api = api;
        this.cache = cache;
        Detail = detail;
    }

    partial void OnCapaciteMinChanged(double value) => ChargerCommand.Execute(null);

    [RelayCommand]
    private Task ChargerAsync() => ExecuterAsync(async () =>
    {
        int filtre;
        do
        {
            filtre = (int)CapaciteMin;
            List<Salle> salles;
            try
            {
                // Réseau d'abord, puis mise à jour du cache
                salles = await api.ObtenirSallesAsync(filtre);
                await cache.EnregistrerAsync(salles);
                EstHorsLigne = false;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // API injoignable : dernières salles connues
                salles = await cache.LireAsync(filtre);
                EstHorsLigne = true;
            }

            Salles.Clear();
            foreach (var salle in salles)
                Salles.Add(salle);
        }
        while (filtre != (int)CapaciteMin);
    });

    [RelayCommand]
    private async Task OuvrirSalleAsync(Salle salle)
    {
        if (EstBureau)
            await Detail.ChargerSalleAsync(salle.Id);
        else
            await Shell.Current.GoToAsync($"salle?id={salle.Id}");
    }

    // Bonus D4 : Fichier > Nouvelle réservation (Ctrl+N), sur la salle affichée à droite
    [RelayCommand]
    private async Task NouvelleReservationAsync()
    {
        if (Detail.Salle is null)
        {
            Message = "Sélectionnez d'abord une salle dans la liste.";
            return;
        }
        await Detail.ReserverCommand.ExecuteAsync(null);
    }

    // Bonus D4 : Fichier > Quitter
    [RelayCommand]
    private void Quitter() => Application.Current?.Quit();

    public async Task ApparaitreAsync()
    {
        await ChargerAsync();
        if (EstBureau && Detail.Salle is not null && !EstHorsLigne)
            await Detail.ActualiserCommand.ExecuteAsync(null);
    }
}
