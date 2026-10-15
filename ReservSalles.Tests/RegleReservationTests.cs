using ReservSalles.Api.Services;

namespace ReservSalles.Tests;

// Bonus D1 : règles R2, R3 et R5, testées sans base de données ni serveur
public class RegleReservationTests
{
    private static readonly DateTime Demain = DateTime.Today.AddDays(1);
    private static DateTime A(double heure) => Demain.AddHours(heure);

    // ---------- R5 : chevauchement ----------

    [Fact]
    public void CreneauxQuiSeRecouvrent_SeChevauchent()
    {
        // Réunion 09:00-10:30 contre demande 10:00-10:45
        Assert.True(RegleReservation.SeChevauchent(A(9), A(10.5), A(10), A(10.75)));
    }

    [Fact]
    public void CreneauQuiCommenceALaFinDeLAutre_NeChevauchePas()
    {
        // 09:00-10:30 puis 10:30-11:30 : accepté
        Assert.False(RegleReservation.SeChevauchent(A(9), A(10.5), A(10.5), A(11.5)));
    }

    [Fact]
    public void CreneauInclusDansUnAutre_SeChevauche()
    {
        Assert.True(RegleReservation.SeChevauchent(A(9), A(12), A(10), A(11)));
    }

    [Theory]
    [InlineData(7, 8)]      // avant
    [InlineData(11, 12)]    // après
    public void CreneauxDisjoints_NeSeChevauchentPas(double debut, double fin)
    {
        Assert.False(RegleReservation.SeChevauchent(A(9), A(10.5), A(debut), A(fin)));
    }

    // ---------- R2 et R3 : créneau ----------

    [Fact]
    public void CreneauValide_AucuneErreur()
    {
        var erreurs = RegleReservation.VerifierCreneau(A(11), A(12), DateTime.Now);
        Assert.Empty(erreurs);
    }

    [Fact]
    public void FinAvantDebut_ErreurSurFin()
    {
        var erreurs = RegleReservation.VerifierCreneau(A(12), A(11), DateTime.Now);
        Assert.True(erreurs.ContainsKey("Fin"));
    }

    [Fact]
    public void PlusDeQuatreHeures_ErreurSurFin()
    {
        var erreurs = RegleReservation.VerifierCreneau(A(8), A(12.5), DateTime.Now);
        Assert.Contains("4 h", erreurs["Fin"]);
    }

    [Fact]
    public void ExactementQuatreHeures_Accepte()
    {
        var erreurs = RegleReservation.VerifierCreneau(A(8), A(12), DateTime.Now);
        Assert.Empty(erreurs);
    }

    [Fact]
    public void DebutDansLePasse_ErreurSurDebut()
    {
        var hier = DateTime.Today.AddDays(-1);
        var erreurs = RegleReservation.VerifierCreneau(hier.AddHours(9), hier.AddHours(10), DateTime.Now);
        Assert.True(erreurs.ContainsKey("Debut"));
    }

    [Fact]
    public void FinLeLendemain_ErreurSurFin()
    {
        var erreurs = RegleReservation.VerifierCreneau(A(22), A(25), DateTime.Now);   // 22:00 → 01:00
        Assert.Contains("même jour", erreurs["Fin"]);
    }
}
