using ReservSalles.Maui.ViewModels;

namespace ReservSalles.Maui.Views;

public partial class SallesPage : ContentPage
{
    private readonly SallesViewModel vm;

    public SallesPage(SallesViewModel vm)
    {
        InitializeComponent();
        BindingContext = this.vm = vm;
    }

    // À chaque affichage : liste à jour (et détail à jour sur ordinateur)
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await vm.ApparaitreAsync();
    }
}
