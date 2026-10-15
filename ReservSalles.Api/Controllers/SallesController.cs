using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservSalles.Api.Data;
using ReservSalles.Api.Dtos;

namespace ReservSalles.Api.Controllers;

// Tâche A2 (lecture des salles) et A4 (planning d'une salle)
[ApiController]
[Route("api/salles")]
public class SallesController : ControllerBase
{
    private readonly ReservSallesContext _contexte;

    public SallesController(ReservSallesContext contexte) => _contexte = contexte;

    // GET /api/salles?capaciteMin=10 (anonyme)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalleDto>>> Lister([FromQuery] int? capaciteMin)
    {
        var requete = _contexte.Salles.AsNoTracking();
        if (capaciteMin is > 0)
            requete = requete.Where(s => s.Capacite >= capaciteMin.Value);

        var salles = await requete.OrderBy(s => s.Nom).ToListAsync();
        return Ok(salles.Select(SalleDto.Depuis));
    }

    // GET /api/salles/3 (anonyme)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SalleDto>> Obtenir(int id)
    {
        var salle = await _contexte.Salles.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (salle is null)
            return Problem(title: "Salle introuvable", detail: $"Aucune salle n'a l'identifiant {id}.",
                           statusCode: StatusCodes.Status404NotFound);

        return Ok(SalleDto.Depuis(salle));
    }

    // GET /api/salles/3/reservations?date=2026-10-20 (jeton)
    [HttpGet("{id:int}/reservations")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> ReservationsDuJour(int id, [FromQuery] DateTime? date)
    {
        if (!await _contexte.Salles.AnyAsync(s => s.Id == id))
            return Problem(title: "Salle introuvable", detail: $"Aucune salle n'a l'identifiant {id}.",
                           statusCode: StatusCodes.Status404NotFound);

        var jour = (date ?? DateTime.Today).Date;
        var lendemain = jour.AddDays(1);

        var reservations = await _contexte.Reservations.AsNoTracking()
            .Include(r => r.Salle)
            .Where(r => r.SalleId == id && r.Debut >= jour && r.Debut < lendemain)
            .OrderBy(r => r.Debut)
            .ToListAsync();

        return Ok(reservations.Select(ReservationDto.Depuis));
    }
}
