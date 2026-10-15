using ReservSalles.Maui.Models;
using SQLite;

namespace ReservSalles.Maui.Services;

// Bonus D2 : dernière liste de salles connue, conservée dans SQLite pour le mode hors ligne
public class SalleEnCache
{
    [PrimaryKey]
    public int Id { get; set; }
    public string Nom { get; set; } = "";
    public int Capacite { get; set; }
    public string Etage { get; set; } = "";
    public string Equipements { get; set; } = "";
}

public interface ICacheSalles
{
    Task EnregistrerAsync(IEnumerable<Salle> salles);
    Task<List<Salle>> LireAsync(int capaciteMin);
}

public class CacheSalles : ICacheSalles
{
    private SQLiteAsyncConnection? connexion;

    private async Task<SQLiteAsyncConnection> ConnexionAsync()
    {
        if (connexion is null)
        {
            var chemin = Path.Combine(FileSystem.AppDataDirectory, "reservsalles-cache.db3");
            connexion = new SQLiteAsyncConnection(chemin);
            await connexion.CreateTableAsync<SalleEnCache>();
        }
        return connexion;
    }

    public async Task EnregistrerAsync(IEnumerable<Salle> salles)
    {
        var db = await ConnexionAsync();
        foreach (var s in salles)
            await db.InsertOrReplaceAsync(new SalleEnCache
            {
                Id = s.Id, Nom = s.Nom, Capacite = s.Capacite, Etage = s.Etage, Equipements = s.Equipements
            });
    }

    public async Task<List<Salle>> LireAsync(int capaciteMin)
    {
        var db = await ConnexionAsync();
        var lignes = await db.Table<SalleEnCache>()
                             .Where(s => s.Capacite >= capaciteMin)
                             .OrderBy(s => s.Nom)
                             .ToListAsync();
        return lignes.Select(l => new Salle
        {
            Id = l.Id, Nom = l.Nom, Capacite = l.Capacite, Etage = l.Etage, Equipements = l.Equipements
        }).ToList();
    }
}
