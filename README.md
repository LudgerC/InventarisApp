# InventarisApp

Volg deze stappen om het project lokaal op te zetten en te draaien.

### 1. Vereisten
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [MySQL](https://dev.mysql.com/downloads/installer/) of [MariaDB](https://mariadb.org/download/) 
- Pas de connection string aan in appsettings.json

### 2. Installatie & Build
Clone de repository en bouw het project:

```powershell
# Bouw de applicatie
dotnet build
```

### 3. Database Setup
Voer de migraties uit om de database-tabellen aan te maken:

```powershell
# Voer database migraties uit
dotnet ef database update
```

### 4. Data Import
Om de data vanuit het Excel bestand naar de database te importeren, run het volgende commando:

```powershell
# Importeer data vanuit Excel naar de database
dotnet run --project DataImporter/DataImporter.csproj
```

### 5. Applicatie Runnen
Start de webapplicatie:

```powershell
# Start de webapplicatie
dotnet run
```

De applicatie is nu toegankelijk via de URL die in de console verschijnt (`http://localhost:5224`).

---

### 6. Admin Login
- **Gebruikersnaam:** `admin`
- **Wachtwoord:** `admin`

