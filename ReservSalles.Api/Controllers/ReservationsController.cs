using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservSalles.Api.Data;
using ReservSalles.Api.Domain;
using ReservSalles.Api.Dtos;
using ReservSalles.Api.Services;

namespace ReservSalles.Api.Controllers;

// Tâches A4 (création) et A5 (mes réservations, annulation). Toutes les routes exigent un jeton.
[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly ReservSallesContext _contexte;
    private readonly ILogger<ReservationsController> _journal;

    public ReservationsController(ReservSallesContext contexte, ILogger<ReservationsController> journal)
    {
        _contexte = contexte;
        _journal = journal;
    }

    // Règle R7 : l'organisateur est l'utilisateur authentifié, jamais une valeur du corps de la requête
    private string Utilisateur => User.Identity?.Name ?? "";

    // GET /api/reservations/7 : cible de l'en-tête Location renvoyé par la création
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationDto>> Obtenir(int id)
    {
        var reservation = await _contexte.Reservations.AsNoTracking()
            .Include(r => r.Salle).FirstOrDefaultAsync(r => r.Id == id);

        return reservation is null
            ? Problem(title: "Réservation introuvable", statusCode: StatusCodes.Status404NotFound)
            : Ok(ReservationDto.Depuis(reservation));
    }

    // POST /api/reservations → 201 · 400 · 404 · 409
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> Creer(CreerReservationDto demande)
    {
        // R1 et participants ≥ 1 : déjà vérifiés par les annotations ([ApiController] → 400 automatique)
        var debut = demande.Debut!.Value;
        var fin = demande.Fin!.Value;

        var salle = await _contexte.Salles.FirstOrDefaultAsync(s => s.Id == demande.SalleId);
        if (salle is null)
            return Problem(title: "Salle introuvable",
                           detail: $"Aucune salle n'a l'identifiant {demande.SalleId}.",
                           statusCode: StatusCodes.Status404NotFound);

        // R2 et R3 : créneau
        foreach (var (champ, message) in RegleReservation.VerifierCreneau(debut, fin, DateTime.Now))
            ModelState.AddModelError(champ, message);

        // R4 : capacité de la salle
        if (demande.Participants > salle.Capacite)
            ModelState.AddModelError(nameof(demande.Participants),
                $"La salle {salle.Nom} accueille au plus {salle.Capacite} participants.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // R5 : aucun chevauchement dans la même salle (condition traduite en SQL)
        var conflit = await _contexte.Reservations.AsNoTracking()
            .Where(r => r.SalleId == salle.Id && r.Debut < fin && debut < r.Fin)
            .OrderBy(r => r.Debut)
            .FirstOrDefaultAsync();

        if (conflit is not null)
            return Problem(title: "Créneau déjà réservé",
                           detail: $"La salle {salle.Nom} est déjà réservée de {conflit.Debut:HH:mm} à {conflit.Fin:HH:mm}.",
                           statusCode: StatusCodes.Status409Conflict);

        var reservation = new Reservation
        {
            SalleId = salle.Id,
            Salle = salle,
            Objet = demande.Objet.Trim(),
            Participants = demande.Participants,
            Debut = debut,
            Fin = fin,
            Organisateur = Utilisateur,
        };
        _contexte.Reservations.Add(reservation);
        await _contexte.SaveChangesAsync();

        _journal.LogInformation("Réservation {Id} créée par {Organisateur} : {Salle} le {Debut:dd/MM HH:mm}-{Fin:HH:mm}",
            reservation.Id, reservation.Organisateur, salle.Nom, debut, fin);

        return CreatedAtAction(nameof(Obtenir), new { id = reservation.Id }, ReservationDto.Depuis(reservation));
    }

    // GET /api/reservations/miennes : réservations à venir de l'utilisateur connecté
    [HttpGet("miennes")]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> Miennes()
    {
        var maintenant = DateTime.Now;
        var utilisateur = Utilisateur;   // valeur simple, traduite en paramètre SQL
        var reservations = await _contexte.Reservations.AsNoTracking()
            .Include(r => r.Salle)
            .Where(r => r.Organisateur == utilisateur && r.Fin > maintenant)
            .OrderBy(r => r.Debut)
            .ToListAsync();

        return Ok(reservations.Select(ReservationDto.Depuis));
    }

    // DELETE /api/reservations/7 → 204 · 403 (règle R6) · 404
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Annuler(int id)
    {
        var reservation = await _contexte.Reservations.FirstOrDefaultAsync(r => r.Id == id);
        if (reservation is null)
            return Problem(title: "Réservation introuvable", detail: $"Aucune réservation n'a l'identifiant {id}.",
                           statusCode: StatusCodes.Status404NotFound);

        if (!string.Equals(reservation.Organisateur, Utilisateur, StringComparison.OrdinalIgnoreCase))
            return Problem(title: "Annulation refusée",
                           detail: "Seul l'organisateur peut annuler cette réservation.",
                           statusCode: StatusCodes.Status403Forbidden);

        _contexte.Reservations.Remove(reservation);
        await _contexte.SaveChangesAsync();

        _journal.LogInformation("Réservation {Id} annulée par {Utilisateur}", id, Utilisateur);
        return NoContent();
    }
}
