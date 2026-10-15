using Microsoft.EntityFrameworkCore;
using ReservSalles.Api.Domain;

namespace ReservSalles.Api.Data;

public class ReservSallesContext : DbContext
{
    public ReservSallesContext(DbContextOptions<ReservSallesContext> options) : base(options) { }

    public DbSet<Salle> Salles => Set<Salle>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Salle>(salle =>
        {
            salle.Property(s => s.Nom).HasMaxLength(50).IsRequired();
            salle.Property(s => s.Etage).HasMaxLength(30);
            salle.Property(s => s.Equipements).HasMaxLength(200);
            salle.HasIndex(s => s.Nom).IsUnique();
        });

        modelBuilder.Entity<Reservation>(reservation =>
        {
            reservation.Property(r => r.Objet).HasMaxLength(80).IsRequired();
            reservation.Property(r => r.Organisateur).HasMaxLength(50).IsRequired();

            // Une salle a plusieurs réservations ; supprimer une salle supprime ses réservations
            reservation.HasOne(r => r.Salle)
                       .WithMany(s => s.Reservations)
                       .HasForeignKey(r => r.SalleId)
                       .OnDelete(DeleteBehavior.Cascade);

            // Recherche des réservations d'une salle sur une période (règle R5, planning du jour)
            reservation.HasIndex(r => new { r.SalleId, r.Debut });
        });
    }
}
