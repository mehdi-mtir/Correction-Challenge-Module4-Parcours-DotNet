using System.Net.Http.Headers;

namespace ReservSalles.Maui.Services;

// Session de l'utilisateur : jeton JWT, entrée dans l'application et déconnexion
public interface ISessionService
{
    Task<string?> LireJetonAsync();
    Task OuvrirAsync(string jeton);   // après une connexion réussie
    Task FermerAsync();               // déconnexion ou jeton refusé (401)
}

public class SessionService : ISessionService
{
    private const string CleJeton = "jwt";
    private string? jetonEnMemoire;   // évite de relire le coffre à chaque requête

    public async Task<string?> LireJetonAsync()
        => jetonEnMemoire ??= await SecureStorage.Default.GetAsync(CleJeton);

    public async Task OuvrirAsync(string jeton)
    {
        jetonEnMemoire = jeton;
        await SecureStorage.Default.SetAsync(CleJeton, jeton);
        await Shell.Current.GoToAsync("//salles");
    }

    public async Task FermerAsync()
    {
        jetonEnMemoire = null;
        SecureStorage.Default.Remove(CleJeton);
        await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//connexion"));
    }
}

// Ajoute « Authorization: Bearer … » à chaque requête du client typé (tâche B1)
public class JetonHandler : DelegatingHandler
{
    private readonly ISessionService session;

    public JetonHandler(ISessionService session) => this.session = session;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage requete, CancellationToken annulation)
    {
        var jeton = await session.LireJetonAsync();
        if (!string.IsNullOrEmpty(jeton))
            requete.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jeton);

        return await base.SendAsync(requete, annulation);
    }
}

// Boîtes de dialogue derrière une interface : le ViewModel ne dépend pas de la page
public interface IDialogueService
{
    Task<bool> ConfirmerAsync(string titre, string message);
}

public class DialogueService : IDialogueService
{
    public Task<bool> ConfirmerAsync(string titre, string message)
        => Shell.Current.DisplayAlert(titre, message, "Oui", "Non");
}
