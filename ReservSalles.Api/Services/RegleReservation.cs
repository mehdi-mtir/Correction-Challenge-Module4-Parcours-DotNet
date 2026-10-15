namespace ReservSalles.Api.Services;

// Règles métier sur les créneaux, sans dépendance à la base : faciles à tester (bonus D1)
public static class RegleReservation
{
    public const int DureeMaxHeures = 4;

    // Règle R5 : [d1, f1[ et [d2, f2[ se chevauchent si d1 < f2 et d2 < f1.
    // Une réservation qui commence à la fin d'une autre (10:30 → 10:30) ne la chevauche donc pas.
    public static bool SeChevauchent(DateTime debut1, DateTime fin1, DateTime debut2, DateTime fin2)
        => debut1 < fin2 && debut2 < fin1;

    // Règles R2 et R3. Renvoie les erreurs par champ (vide si le créneau est valide)
    public static Dictionary<string, string> VerifierCreneau(DateTime debut, DateTime fin, DateTime maintenant)
    {
        var erreurs = new Dictionary<string, string>();

        if (debut <= maintenant)
            erreurs["Debut"] = "Le début de la réservation doit être dans le futur.";

        if (fin <= debut)
            erreurs["Fin"] = "La fin doit être après le début.";
        else if (fin.Date != debut.Date)
            erreurs["Fin"] = "La réservation doit commencer et finir le même jour.";
        else if (fin - debut > TimeSpan.FromHours(DureeMaxHeures))
            erreurs["Fin"] = $"Une réservation dure {DureeMaxHeures} h au plus.";

        return erreurs;
    }
}
