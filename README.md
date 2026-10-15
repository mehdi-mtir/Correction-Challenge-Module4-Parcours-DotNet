# ReservSalles — solution de référence du challenge final (modules 3 et 4)

API web **ReservSalles.Api** (ASP.NET Core 8, EF Core SQLite, JWT) et application **ReservSalles.Maui** (.NET MAUI 8, Android et Windows).

## Lancer l'API

```powershell
cd ReservSalles.Api
dotnet run --launch-profile http
```

- Écoute sur **http://localhost:5080**, Swagger sur http://localhost:5080/swagger.
- Au premier démarrage, la base `reservsalles.db` est créée avec 5 salles (New York, Sydney, Paris, Tokyo, Londres) et 2 réservations pour le lendemain.
- Fichier de test : `ReservSalles.Api/ReservSalles.http` (adapter `@demain`, puis coller le jeton dans `@jeton`).

## Lancer l'application

Ouvrir le dossier `ReservSalles.Maui` dans VS Code, ouvrir un fichier `.xaml`, choisir la cible (**{ }** > Debug Target : émulateur Android ou Windows), puis **F5**. L'API doit être démarrée.

| Plateforme | Adresse de l'API utilisée |
|---|---|
| Émulateur Android | http://10.0.2.2:5080 (HTTP autorisé vers cette adresse seulement) |
| Windows | http://localhost:5080 |

## Comptes de test

| Identifiant | Mot de passe |
|---|---|
| formateur | formateur123 |
| stagiaire | stagiaire123 |

## État (solution complète, bonus compris)

| Partie | État |
|---|---|
| A — API (A1 à A5) | Fonctionne : contrat respecté, règles R1 à R7, ProblemDetails, journalisation |
| B — MAUI (B1 à B5) | Fonctionne sur Android |
| C — Desktop et robustesse (C1 à C3) | Maître-détail sous Windows, fenêtre 1100 × 750, « Serveur injoignable », retour à la connexion sur 401 |
| Bonus D1 — Tests | `ReservSalles.Tests` : 11 tests des règles R2, R3, R5 (`dotnet test ReservSalles.Tests`) |
| Bonus D2 — Hors ligne | Salles mises en cache dans SQLite, bandeau « Hors ligne » |
| Bonus D3 — Package | Commandes ci-dessous |
| Bonus D4 — Menu | Menu Fichier, Ctrl+R et Ctrl+N sous Windows |

## Bonus D3 — package Release

Android (APK signé avec la clé de débogage, suffisant pour une installation de test ; voir l'atelier A10 pour un keystore) :

```powershell
cd ReservSalles.Maui
dotnet publish -f net8.0-android -c Release -p:AndroidPackageFormats=apk
adb install -r .\bin\Release\net8.0-android\publish\com.correction.reservsalles-Signed.apk
```

Windows (MSIX signé avec un certificat de test, voir l'atelier A10, étapes 6 et 7) :

```powershell
dotnet publish -f net8.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win10-x64 `
  -p:WindowsPackageType=MSIX -p:GenerateAppxPackageOnBuild=true `
  -p:AppxPackageSigningEnabled=true -p:PackageCertificateThumbprint=<empreinte>
```
