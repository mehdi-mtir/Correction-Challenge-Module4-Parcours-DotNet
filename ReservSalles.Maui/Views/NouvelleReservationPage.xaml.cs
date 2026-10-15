using ReservSalles.Maui.ViewModels;

namespace ReservSalles.Maui.Views;

public partial class NouvelleReservationPage : ContentPage
{
    public NouvelleReservationPage(NouvelleReservationViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
