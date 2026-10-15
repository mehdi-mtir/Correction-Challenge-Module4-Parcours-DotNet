using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReservSalles.Maui.Services;

namespace ReservSalles.Maui.ViewModels;

// Base commune : indicateur d'activité, message et traitement uniforme des erreurs (tâche C2)
public abstract partial class VueModeleBase : ObservableObject
{
    protected readonly ISessionService session;

    [ObservableProperty]
    private bool estOccupe;

    [ObservableProperty]
    private string? message;

    protected VueModeleBase(ISessionService session) => this.session = session;

    // Exécute un appel à l'API ; renvoie true s'il a réussi.
    // 401 → retour à la connexion ; API arrêtée → « Serveur injoignable » ; jamais de plantage.
    protected async Task<bool> ExecuterAsync(Func<Task> action)
    {
        if (EstOccupe)
            return false;

        EstOccupe = true;
        Message = null;
        try
        {
            await action();
            return true;
        }
        catch (ApiException ex) when (ex.Statut == HttpStatusCode.Unauthorized)
        {
            Message = "Session expirée : reconnectez-vous.";
            await session.FermerAsync();
        }
        catch (ApiException ex)
        {
            Message = ex.Message;   // message de l'API : 400, 403, 404, 409
        }
        catch (HttpRequestException)
        {
            Message = "Serveur injoignable. Vérifiez que l'API est démarrée.";
        }
        catch (TaskCanceledException)
        {
            Message = "Le serveur ne répond pas (délai dépassé).";
        }
        finally
        {
            EstOccupe = false;
        }
        return false;
    }

    // Bouton Déconnexion des pages Salles et Mes réservations (tâche B5)
    [RelayCommand]
    private Task SeDeconnecterAsync() => session.FermerAsync();
}
