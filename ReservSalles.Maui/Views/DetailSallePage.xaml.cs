using ReservSalles.Maui.ViewModels;

namespace ReservSalles.Maui.Views;

public partial class DetailSallePage : ContentPage
{
    private readonly DetailSalleViewModel vm;

    public DetailSallePage(DetailSalleViewModel vm)
    {
        InitializeComponent();
        BindingContext = this.vm = vm;
    }

    // Retour de la page de réservation : la liste du jour est rechargée
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (vm.Salle is not null)
            vm.ActualiserCommand.Execute(null);
    }
}
