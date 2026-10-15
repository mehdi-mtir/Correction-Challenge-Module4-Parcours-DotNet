using ReservSalles.Maui.ViewModels;

namespace ReservSalles.Maui.Views;

public partial class ConnexionPage : ContentPage
{
    public ConnexionPage(ConnexionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
