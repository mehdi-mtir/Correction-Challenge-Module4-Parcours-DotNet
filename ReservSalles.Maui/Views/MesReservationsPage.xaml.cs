using ReservSalles.Maui.ViewModels;

namespace ReservSalles.Maui.Views;

public partial class MesReservationsPage : ContentPage
{
    private readonly MesReservationsViewModel vm;

    public MesReservationsPage(MesReservationsViewModel vm)
    {
        InitializeComponent();
        BindingContext = this.vm = vm;
    }

    // Rechargée à chaque affichage de l'onglet (tâche B5)
    protected override void OnAppearing()
    {
        base.OnAppearing();
        vm.ChargerCommand.Execute(null);
    }
}
