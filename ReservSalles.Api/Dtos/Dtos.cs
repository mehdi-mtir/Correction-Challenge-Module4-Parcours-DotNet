using System.ComponentModel.DataAnnotations;
using ReservSalles.Api.Domain;

namespace ReservSalles.Api.Dtos;

// Ce que l'API renvoie : jamais les entités EF Core directement (tâche A2)
public record SalleDto(int Id, string Nom, int Capacite, string Etage, string Equipements)
{
    public static SalleDto Depuis(Salle s) => new(s.Id, s.Nom, s.Capacite, s.Etage, s.Equipements);
}

public record ReservationDto(int Id, int SalleId, string SalleNom, string Objet, int Participants,
                             DateTime Debut, DateTime Fin, string Organisateur)
{
    public static ReservationDto Depuis(Reservation r) =>
        new(r.Id, r.SalleId, r.Salle?.Nom ?? "", r.Objet, r.Participants, r.Debut, r.Fin, r.Organisateur);
}

// Corps de POST /api/reservations. Pas d'organisateur : il vient du jeton (règle R7)
public class CreerReservationDto
{
    [Range(1, int.MaxValue, ErrorMessage = "La salle est obligatoire.")]
    public int SalleId { get; set; }

    // Règle R1
    [Required(ErrorMessage = "L'objet est obligatoire.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "L'objet doit contenir de 3 à 80 caractères.")]
    public string Objet { get; set; } = "";

    // Règle R4 (première partie ; la capacité de la salle est vérifiée par le contrôleur)
    [Range(1, int.MaxValue, ErrorMessage = "Il faut au moins un participant.")]
    public int Participants { get; set; }

    [Required] public DateTime? Debut { get; set; }
    [Required] public DateTime? Fin { get; set; }
}

// Attributs sur les paramètres du constructeur (et non [property: …]) : c'est ce que MVC valide pour un record
public record LoginDto([Required] string Identifiant, [Required] string MotDePasse);

public record JetonDto(string Jeton, DateTime Expiration);
