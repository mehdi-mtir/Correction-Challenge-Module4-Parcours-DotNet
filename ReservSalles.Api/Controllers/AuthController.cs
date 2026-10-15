using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservSalles.Api.Dtos;
using ReservSalles.Api.Services;

namespace ReservSalles.Api.Controllers;

// Tâche A3 : POST /api/auth/login
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ServiceJeton _serviceJeton;
    private readonly ILogger<AuthController> _journal;

    public AuthController(IConfiguration configuration, ServiceJeton serviceJeton, ILogger<AuthController> journal)
    {
        _configuration = configuration;
        _serviceJeton = serviceJeton;
        _journal = journal;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(JetonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<JetonDto> Login(LoginDto demande)
    {
        // Comptes lus dans la configuration (pas de table d'utilisateurs dans ce challenge)
        var comptes = _configuration.GetSection("Comptes").Get<List<Compte>>() ?? new();
        var compte = comptes.FirstOrDefault(c =>
            c.Identifiant.Equals(demande.Identifiant, StringComparison.OrdinalIgnoreCase)
            && c.MotDePasse == demande.MotDePasse);

        if (compte is null)
        {
            _journal.LogWarning("Échec de connexion pour {Identifiant}", demande.Identifiant);
            return Problem(title: "Connexion refusée",
                           detail: "Identifiant ou mot de passe incorrect.",
                           statusCode: StatusCodes.Status401Unauthorized);
        }

        var (jeton, expiration) = _serviceJeton.Creer(compte.Identifiant);
        return Ok(new JetonDto(jeton, expiration));
    }
}
