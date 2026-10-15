namespace ReservSalles.Api.Domain;

// Une salle de réunion du centre de formation
public class Salle
{
    public int Id { get; set; }
    public string Nom { get; set; } = "";
    public int Capacite { get; set; }
    public string Etage { get; set; } = "";
    public string Equipements { get; set; } = "";

    public List<Reservation> Reservations { get; set; } = new();
}

// Une réservation d'une salle sur un créneau d'une même journée
public class Reservation
{
    public int Id { get; set; }
    public int SalleId { get; set; }
    public Salle? Salle { get; set; }

    public string Objet { get; set; } = "";
    public int Participants { get; set; }
    public DateTime Debut { get; set; }
    public DateTime Fin { get; set; }

    // Règle R7 : toujours le nom de l'utilisateur du jeton
    public string Organisateur { get; set; } = "";
}
