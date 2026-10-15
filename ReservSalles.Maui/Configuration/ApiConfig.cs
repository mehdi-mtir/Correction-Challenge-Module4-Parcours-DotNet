namespace ReservSalles.Maui.Configuration;

// Adresse de l'API selon la plateforme (tâche B1)
public static class ApiConfig
{
    // Dans l'émulateur Android, localhost désigne l'émulateur lui-même : 10.0.2.2 désigne le PC
    public static string AdresseBase =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5080/"
            : "http://localhost:5080/";
}
