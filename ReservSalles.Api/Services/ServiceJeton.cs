using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ReservSalles.Api.Services;

// Section "Jwt" de appsettings.json : rien n'est écrit en dur dans le code (tâche A3)
public class JwtOptions
{
    public string Cle { get; set; } = "";
    public string Emetteur { get; set; } = "";
    public string Audience { get; set; } = "";
    public int DureeHeures { get; set; } = 2;

    public SymmetricSecurityKey CleDeSignature() => new(Encoding.UTF8.GetBytes(Cle));
}

// Section "Comptes" de appsettings.json
public class Compte
{
    public string Identifiant { get; set; } = "";
    public string MotDePasse { get; set; } = "";
}

public class ServiceJeton
{
    private readonly JwtOptions _options;

    public ServiceJeton(IOptions<JwtOptions> options) => _options = options.Value;

    public (string Jeton, DateTime Expiration) Creer(string identifiant)
    {
        var expiration = DateTime.UtcNow.AddHours(_options.DureeHeures);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, identifiant),
            new Claim("name", identifiant),   // lu par User.Identity.Name (NameClaimType = "name")
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var jeton = new JwtSecurityToken(
            issuer: _options.Emetteur,
            audience: _options.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: new SigningCredentials(_options.CleDeSignature(), SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(jeton), expiration);
    }
}
