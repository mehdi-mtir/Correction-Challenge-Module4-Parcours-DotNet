using ReservSalles.Maui.Views;

namespace ReservSalles.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages ouvertes par-dessus les onglets
        Routing.RegisterRoute("salle", typeof(DetailSallePage));               // salle?id=3
        Routing.RegisterRoute("reserver", typeof(NouvelleReservationPage));    // reserver?salleId=3&date=…
    }
}
