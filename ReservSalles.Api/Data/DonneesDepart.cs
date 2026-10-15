using ReservSalles.Api.Domain;

namespace ReservSalles.Api.Data;

// Tâche A1 : création de la base et données de départ, uniquement si la base est vide
public static class DonneesDepart
{
    public static void Initialiser(ReservSallesContext contexte, DateTime aujourdhui, ILogger journal)
    {
        // Crée le fichier reservsalles.db et les tables s'ils n'existent pas
        contexte.Database.EnsureCreated();

        if (contexte.Salles.Any())
            return;   // second démarrage : rien n'est dupliqué

        var newYork = new Salle { Nom = "New York", Capacite = 12, Etage = "1er étage", Equipements = "Écran, Visioconférence" };
        var sydney  = new Salle { Nom = "Sydney",   Capacite = 6,  Etage = "1er étage", Equipements = "Écran" };
        var paris   = new Salle { Nom = "Paris",    Capacite = 20, Etage = "2e étage",  Equipements = "Vidéoprojecteur, Visioconférence" };
        var tokyo   = new Salle { Nom = "Tokyo",    Capacite = 4,  Etage = "2e étage",  Equipements = "Tableau blanc" };
        var londres = new Salle { Nom = "Londres",  Capacite = 40, Etage = "Rez-de-chaussée", Equipements = "Vidéoprojecteur, Sonorisation" };

        // Insertion dans l'ordre : les identifiants 1 à 5 suivent le tableau de l'énoncé
        foreach (var salle in new[] { newYork, sydney, paris, tokyo, londres })
        {
            contexte.Salles.Add(salle);
            contexte.SaveChanges();
        }

        var demain = aujourdhui.Date.AddDays(1);
        contexte.Reservations.AddRange(
            new Reservation
            {
                SalleId = newYork.Id, Objet = "Réunion pédagogique", Participants = 8,
                Debut = demain.AddHours(9), Fin = demain.AddHours(10.5), Organisateur = "formateur"
            },
            new Reservation
            {
                SalleId = paris.Id, Objet = "Jury de soutenance", Participants = 15,
                Debut = demain.AddHours(14), Fin = demain.AddHours(16), Organisateur = "formateur"
            });
        contexte.SaveChanges();

        journal.LogInformation("Base initialisée : 5 salles et 2 réservations pour le {Jour:dd/MM/yyyy}", demain);
    }
}
