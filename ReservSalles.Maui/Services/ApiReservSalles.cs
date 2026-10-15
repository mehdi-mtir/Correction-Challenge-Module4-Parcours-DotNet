using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using ReservSalles.Maui.Models;

namespace ReservSalles.Maui.Services;

// Erreur renvoyée par l'API (400, 401, 403, 404, 409…), avec le message lisible du ProblemDetails
public class ApiException : Exception
{
    public HttpStatusCode Statut { get; }

    public ApiException(HttpStatusCode statut, string message) : base(message) => Statut = statut;
}

// Contrat du client : les ViewModels ne connaissent que cette interface
public interface IApiReservSalles
{
    Task<string?> SeConnecterAsync(string identifiant, string motDePasse);   // null si refusé
    Task<List<Salle>> ObtenirSallesAsync(int capaciteMin);
    Task<Salle?> ObtenirSalleAsync(int id);
    Task<List<Reservation>> ObtenirReservationsAsync(int salleId, DateTime jour);
    Task<Reservation?> CreerReservationAsync(NouvelleReservation reservation);
    Task<List<Reservation>> MesReservationsAsync();
    Task AnnulerAsync(int id);
}

// Client HTTP typé (tâche B1) : le jeton est ajouté par JetonHandler, pas ici
public class ApiReservSalles : IApiReservSalles
{
    private readonly HttpClient http;

    public ApiReservSalles(HttpClient http) => this.http = http;

    public async Task<string?> SeConnecterAsync(string identifiant, string motDePasse)
    {
        var reponse = await http.PostAsJsonAsync("api/auth/login", new { identifiant, motDePasse });
        if (reponse.StatusCode == HttpStatusCode.Unauthorized)
            return null;   // identifiants refusés : ce n'est pas une panne

        await VerifierAsync(reponse);
        var jeton = await reponse.Content.ReadFromJsonAsync<JetonReponse>();
        return jeton?.Jeton;
    }

    public async Task<List<Salle>> ObtenirSallesAsync(int capaciteMin)
    {
        var url = capaciteMin > 0 ? $"api/salles?capaciteMin={capaciteMin}" : "api/salles";
        return await LireAsync<List<Salle>>(url) ?? new();
    }

    public Task<Salle?> ObtenirSalleAsync(int id) => LireAsync<Salle>($"api/salles/{id}");

    public async Task<List<Reservation>> ObtenirReservationsAsync(int salleId, DateTime jour)
    {
        var date = jour.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return await LireAsync<List<Reservation>>($"api/salles/{salleId}/reservations?date={date}") ?? new();
    }

    public async Task<Reservation?> CreerReservationAsync(NouvelleReservation reservation)
    {
        var reponse = await http.PostAsJsonAsync("api/reservations", reservation);
        await VerifierAsync(reponse);
        return await reponse.Content.ReadFromJsonAsync<Reservation>();
    }

    public async Task<List<Reservation>> MesReservationsAsync()
        => await LireAsync<List<Reservation>>("api/reservations/miennes") ?? new();

    public async Task AnnulerAsync(int id)
    {
        var reponse = await http.DeleteAsync($"api/reservations/{id}");
        await VerifierAsync(reponse);
    }

    private async Task<T?> LireAsync<T>(string url)
    {
        var reponse = await http.GetAsync(url);
        await VerifierAsync(reponse);
        return await reponse.Content.ReadFromJsonAsync<T>();
    }

    // Transforme une réponse d'erreur en ApiException portant le message de l'API (tâche B4)
    private static async Task VerifierAsync(HttpResponseMessage reponse)
    {
        if (reponse.IsSuccessStatusCode)
            return;

        var message = $"Erreur de l'API (code {(int)reponse.StatusCode}).";
        try
        {
            var probleme = await reponse.Content.ReadFromJsonAsync<ProblemeApi>();
            message = probleme?.Message() ?? message;
        }
        catch
        {
            // Corps vide ou non JSON : on garde le message générique
        }
        throw new ApiException(reponse.StatusCode, message);
    }
}
