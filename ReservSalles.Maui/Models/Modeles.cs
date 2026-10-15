using System.Globalization;

namespace ReservSalles.Maui.Models;

// Copie côté client du SalleDto de l'API (mêmes noms de propriétés)
public class Salle
{
    public int Id { get; set; }
    public string Nom { get; set; } = "";
    public int Capacite { get; set; }
    public string Etage { get; set; } = "";
    public string Equipements { get; set; } = "";

    // Propriétés d'affichage
    public string CapaciteTexte => $"{Capacite} places";
    public string Resume => $"{Etage} · {Equipements}";
}

// Copie côté client du ReservationDto de l'API
public class Reservation
{
    private static readonly CultureInfo Francais = new("fr-FR");

    public int Id { get; set; }
    public int SalleId { get; set; }
    public string SalleNom { get; set; } = "";
    public string Objet { get; set; } = "";
    public int Participants { get; set; }
    public DateTime Debut { get; set; }
    public DateTime Fin { get; set; }
    public string Organisateur { get; set; } = "";

    public string Creneau => $"{Debut:HH:mm} - {Fin:HH:mm}";
    public string Jour => Debut.ToString("dddd d MMMM", Francais);
    public string Details => $"{Participants} participant(s) · organisée par {Organisateur}";
}

// Corps de POST /api/reservations (pas d'organisateur : l'API le prend dans le jeton)
public record NouvelleReservation(int SalleId, string Objet, int Participants, DateTime Debut, DateTime Fin);

// Réponse de POST /api/auth/login
public record JetonReponse(string Jeton, DateTime Expiration);

// Corps d'une erreur de l'API (format ProblemDetails)
public class ProblemeApi
{
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public int? Status { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }

    // Le message le plus utile pour l'utilisateur
    public string? Message()
    {
        if (Errors is { Count: > 0 })
            return string.Join(Environment.NewLine, Errors.SelectMany(e => e.Value));
        return Detail ?? Title;
    }
}
