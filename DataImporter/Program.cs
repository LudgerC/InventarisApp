using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ClosedXML.Excel;
using InventarisApp.Database;
using System.Text.Json;

// Probeer appsettings.json te vinden (ofwel in de huidige map, ofwel een map hoger)
var appSettingsPath = "appsettings.json";
if (!File.Exists(appSettingsPath))
{
    appSettingsPath = Path.Combine("..", "appsettings.json");
}

if (!File.Exists(appSettingsPath))
{
    Console.WriteLine("Kon appsettings.json niet vinden in de huidige map of de bovenliggende map.");
    return;
}

var json = File.ReadAllText(appSettingsPath);
var doc = JsonDocument.Parse(json);
var connectionString = doc.RootElement.GetProperty("ConnectionStrings").GetProperty("LocalConnection").GetString();

var services = new ServiceCollection();

services.AddDbContext<InventarisContext>(options =>
{
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 35));
    options.UseMySql(connectionString, serverVersion);
});

var provider = services.BuildServiceProvider();
var context = provider.GetRequiredService<InventarisContext>();

Console.WriteLine("Import gestart...");

var path = Path.Combine(
    AppContext.BaseDirectory,
    "data",
    "Inventaris29012026.xlsx"
);

using var workbook = new XLWorkbook(path);

// var docentenPCs = workbook.Worksheet("Docenten PCs");
// var studentenPCs = workbook.Worksheet("Studenten PCs");
// var administratiePCs = workbook.Worksheet("Administratie PCs");
// var projectie = workbook.Worksheet("Projectie");
// var netwerk = workbook.Worksheet("Netwerk");
// var servers = workbook.Worksheet("Servers");
// var printers = workbook.Worksheet("Printers");

await SeedLokalen(context, workbook);
await SeedPersonen(context, workbook);
await SeedTypes(context, workbook);
await SeedDocentenPCs(context, workbook);
await SeedStudentenPCs(context, workbook);
await SeedAdministratiePCs(context, workbook);
await SeedProjectie(context, workbook);
await SeedNetwerk(context, workbook);

Console.WriteLine("\nImport klaar!");

async Task SeedLokalen(InventarisContext dbContext, XLWorkbook wb)
{
    Console.WriteLine("Seeding Locaties & Lokalen...");
    
    // Create Locaties if they don't exist
    var rouppeLocatie = await dbContext.Locaties.FirstOrDefaultAsync(l => l.Afkorting == "ROUP");
    if (rouppeLocatie == null)
    {
        rouppeLocatie = new InventarisApp.Models.Locatie { Afkorting = "ROUP", Naam = "Campus Rouppe" };
        dbContext.Locaties.Add(rouppeLocatie);
    }
    
    var overigeLocatie = await dbContext.Locaties.FirstOrDefaultAsync(l => l.Afkorting == "OVER");
    if (overigeLocatie == null)
    {
        overigeLocatie = new InventarisApp.Models.Locatie { Afkorting = "OVER", Naam = "Overige" };
        dbContext.Locaties.Add(overigeLocatie);
    }
    
    var karrenLocatie = await dbContext.Locaties.FirstOrDefaultAsync(l => l.Afkorting == "KAR");
    if (karrenLocatie == null)
    {
        karrenLocatie = new InventarisApp.Models.Locatie { Afkorting = "KAR", Naam = "Karren" };
        dbContext.Locaties.Add(karrenLocatie);
    }
    
    await dbContext.SaveChangesAsync();

    var sheet = wb.Worksheet("Lokalen");
    var rows = sheet.RangeUsed().RowsUsed().Skip(1); // Skip header

    // Find columns by header name
    var headerRow = sheet.Row(1);
    int naamCol = 0, plaatsenCol = 0, isExternCol = 0, beschrijvingCol = 0;
    foreach (var cell in headerRow.CellsUsed())
    {
        var val = cell.GetString().Trim().ToLower();
        if (val == "naam") naamCol = cell.Address.ColumnNumber;
        else if (val == "plaatsen") plaatsenCol = cell.Address.ColumnNumber;
        else if (val == "isextern") isExternCol = cell.Address.ColumnNumber;
        else if (val == "beschrijving") beschrijvingCol = cell.Address.ColumnNumber;
    }

    foreach (var row in rows)
    {
        var rawNaam = naamCol > 0 ? row.Cell(naamCol).GetString().Trim() : "";
        if (string.IsNullOrEmpty(rawNaam)) continue;

        var plaatsenStr = plaatsenCol > 0 ? row.Cell(plaatsenCol).GetString().Trim() : "";
        var isExternStr = isExternCol > 0 ? row.Cell(isExternCol).GetString().Trim() : "";
        var beschrijving = beschrijvingCol > 0 ? row.Cell(beschrijvingCol).GetString().Trim() : "";

        int aantalPlaatsen = 0;
        if (int.TryParse(plaatsenStr, out int parsedPlaatsen))
        {
            aantalPlaatsen = parsedPlaatsen;
        }

        bool isExtern = isExternStr == "1" || isExternStr.ToLower() == "true";

        int? locatieId = rouppeLocatie.ID;
        string lokaalNaam = rawNaam;

        if (lokaalNaam.StartsWith("R") && lokaalNaam.Length > 1 && char.IsDigit(lokaalNaam[1]))
        {
            lokaalNaam = lokaalNaam.Substring(1);
        }
        else if (lokaalNaam.StartsWith("A"))
        {
            locatieId = overigeLocatie.ID;
            isExtern = true;
        }
        else if (lokaalNaam.StartsWith("KarC", StringComparison.OrdinalIgnoreCase))
        {
            locatieId = karrenLocatie.ID;
        }
        
        if (int.TryParse(lokaalNaam, out int numericName))
        {
            lokaalNaam = numericName.ToString("D3");
        }

        // Check if exists
        var existing = await dbContext.Lokalen.FirstOrDefaultAsync(l => l.Naam == lokaalNaam);
        if (existing == null)
        {
            var lokaal = new InventarisApp.Models.Lokaal
            {
                Naam = lokaalNaam,
                AantalPlaatsen = aantalPlaatsen,
                IsExtern = isExtern,
                Beschrijving = string.IsNullOrEmpty(beschrijving) ? null : beschrijving,
                LocatieId = locatieId
            };
            dbContext.Lokalen.Add(lokaal);
        }
    }

    await dbContext.SaveChangesAsync();
    Console.WriteLine("Lokalen succesvol geïmporteerd!");
}

async Task SeedPersonen(InventarisContext dbContext, XLWorkbook wb)
{
    Console.WriteLine("Seeding Personen...");
    var sheet = wb.Worksheet("Personen");
    var rows = sheet.RangeUsed().RowsUsed().Skip(1);

    var headerRow = sheet.Row(1);
    int naamCol = 0, voornaamCol = 0;
    foreach (var cell in headerRow.CellsUsed())
    {
        var val = cell.GetString().Trim().ToLower();
        if (val == "naam") naamCol = cell.Address.ColumnNumber;
        else if (val == "voornaam") voornaamCol = cell.Address.ColumnNumber;
    }

    foreach (var row in rows)
    {
        var achternaam = naamCol > 0 ? row.Cell(naamCol).GetString().Trim() : "";
        var voornaam = voornaamCol > 0 ? row.Cell(voornaamCol).GetString().Trim() : "";

        if (string.IsNullOrEmpty(achternaam) && string.IsNullOrEmpty(voornaam)) continue;

        if (string.IsNullOrEmpty(voornaam)) voornaam = "Onbekend";
        if (string.IsNullOrEmpty(achternaam)) achternaam = "Onbekend";

        var existing = await dbContext.Personen.FirstOrDefaultAsync(p => p.Naam == voornaam && p.Achternaam == achternaam);
        if (existing == null)
        {
            var persoon = new InventarisApp.Models.Persoon
            {
                Naam = voornaam,
                Achternaam = achternaam
            };
            dbContext.Personen.Add(persoon);
        }
    }

    await dbContext.SaveChangesAsync();
    Console.WriteLine("Personen succesvol geïmporteerd!");
}

async Task SeedTypes(InventarisContext dbContext, XLWorkbook wb)
{
    Console.WriteLine("Seeding Types...");
    var sheet = wb.Worksheet("Types");

    var rows = sheet.RangeUsed().RowsUsed().Skip(1);
    var headerRow = sheet.Row(1);
    int naamCol = 0;
    
    foreach (var cell in headerRow.CellsUsed())
    {
        if (cell.GetString().Trim().ToLower() == "naam")
        {
            naamCol = cell.Address.ColumnNumber;
            break;
        }
    }

    foreach (var row in rows)
    {
        var typeNaam = naamCol > 0 ? row.Cell(naamCol).GetString().Trim() : "";
        if (string.IsNullOrEmpty(typeNaam)) continue;

        var existing = await dbContext.Devices.FirstOrDefaultAsync(d => d.type == typeNaam);
        if (existing == null)
        {
            var device = new InventarisApp.Models.Device
            {
                type = typeNaam
            };
            dbContext.Devices.Add(device);
        }
    }

    await dbContext.SaveChangesAsync();
    Console.WriteLine("Types succesvol geïmporteerd!");
}

async Task<int?> GetLokaalId(InventarisContext dbContext, string lokaalNaam)
{
    if (string.IsNullOrWhiteSpace(lokaalNaam)) return null;
    
    // Normalize name (padding)
    string searchName = lokaalNaam.Trim();
    if (int.TryParse(searchName, out int numericName))
    {
        searchName = numericName.ToString("D3");
    }
    
    var lokaal = await dbContext.Lokalen.FirstOrDefaultAsync(l => l.Naam == searchName);
    return lokaal?.ID;
}

async Task<int?> GetPersoonId(InventarisContext dbContext, string voornaam)
{
    if (string.IsNullOrWhiteSpace(voornaam)) return null;
    var persoon = await dbContext.Personen.FirstOrDefaultAsync(p => p.Naam.ToLower() == voornaam.Trim().ToLower());
    return persoon?.ID;
}

async Task SeedDocentenPCs(InventarisContext dbContext, XLWorkbook wb)
{
    Console.WriteLine("Seeding Docenten PCs...");
    var sheet = wb.Worksheet("Docenten_pcs");
    var rows = sheet.RangeUsed().RowsUsed().Skip(1);

    foreach (var row in rows)
    {
        var type = row.Cell(3).GetString().Trim();
        if (string.IsNullOrEmpty(type)) continue;

        var device = new InventarisApp.Models.Device { type = type };
        dbContext.Devices.Add(device);
        await dbContext.SaveChangesAsync();

        var info = new InventarisApp.Models.Info
        {
            type = type,
            device_id = device.device_id,
            apparaatnaam = row.Cell(2).GetString().Trim(),
            merk = row.Cell(4).GetString().Trim(),
            serial_number = row.Cell(5).GetString().Trim(),
            aantal = int.TryParse(row.Cell(6).GetString(), out int a) ? a : null,
            LokaalId = await GetLokaalId(dbContext, row.Cell(8).GetString()),
            status = row.Cell(9).GetString().Trim(),
            staat = row.Cell(9).GetString().Trim(),
            leverancier = row.Cell(10).GetString().Trim(),
            aankoopdatum = row.Cell(11).TryGetValue(out DateTime d1) ? d1 : null,
            eind_garantie = row.Cell(12).TryGetValue(out DateTime d2) ? d2 : null
        };
        dbContext.Infos.Add(info);
    }
    await dbContext.SaveChangesAsync();
}

async Task SeedStudentenPCs(InventarisContext dbContext, XLWorkbook wb)
{
    Console.WriteLine("Seeding Studenten PCs...");
    var sheet = wb.Worksheet("Studenten_pcs");
    var rows = sheet.RangeUsed().RowsUsed().Skip(1);

    foreach (var row in rows)
    {
        var type = row.Cell(3).GetString().Trim();
        if (string.IsNullOrEmpty(type)) continue;

        var device = new InventarisApp.Models.Device { type = type };
        dbContext.Devices.Add(device);
        await dbContext.SaveChangesAsync();

        var info = new InventarisApp.Models.Info
        {
            type = type,
            device_id = device.device_id,
            apparaatnaam = row.Cell(2).GetString().Trim(),
            merk = row.Cell(4).GetString().Trim(),
            serial_number = row.Cell(5).GetString().Trim(),
            aantal = int.TryParse(row.Cell(6).GetString(), out int a) ? a : null,
            LokaalId = await GetLokaalId(dbContext, row.Cell(8).GetString()),
            status = row.Cell(9).GetString().Trim(),
            staat = row.Cell(9).GetString().Trim(),
            leverancier = row.Cell(10).GetString().Trim(),
            aankoopdatum = row.Cell(11).TryGetValue(out DateTime d1) ? d1 : null,
            eind_garantie = row.Cell(12).TryGetValue(out DateTime d2) ? d2 : null
        };
        dbContext.Infos.Add(info);
    }
    await dbContext.SaveChangesAsync();
}

async Task SeedAdministratiePCs(InventarisContext dbContext, XLWorkbook wb)
{
    Console.WriteLine("Seeding Administratie PCs...");
    var sheet = wb.Worksheet("Administratie_pcs");
    var rows = sheet.RangeUsed().RowsUsed().Skip(1);

    foreach (var row in rows)
    {
        var type = row.Cell(3).GetString().Trim();
        if (string.IsNullOrEmpty(type)) continue;

        var device = new InventarisApp.Models.Device { type = type };
        dbContext.Devices.Add(device);
        await dbContext.SaveChangesAsync();

        var verdiep = row.Cell(7).GetString().Trim().ToLower();
        var lokaalOfPersoon = row.Cell(8).GetString().Trim();

        var info = new InventarisApp.Models.Info
        {
            type = type,
            device_id = device.device_id,
            apparaatnaam = row.Cell(2).GetString().Trim(),
            merk = row.Cell(4).GetString().Trim(),
            serial_number = row.Cell(5).GetString().Trim(),
            aantal = int.TryParse(row.Cell(6).GetString(), out int a) ? a : null,
            status = row.Cell(9).GetString().Trim(),
            staat = row.Cell(9).GetString().Trim(),
            leverancier = row.Cell(10).GetString().Trim(),
            aankoopdatum = row.Cell(11).TryGetValue(out DateTime d1) ? d1 : null,
            eind_garantie = row.Cell(12).TryGetValue(out DateTime d2) ? d2 : null
        };

        if (verdiep == "persoon")
        {
            info.PersoonId = await GetPersoonId(dbContext, lokaalOfPersoon);
        }
        else
        {
            info.LokaalId = await GetLokaalId(dbContext, lokaalOfPersoon);
        }

        dbContext.Infos.Add(info);
    }
    await dbContext.SaveChangesAsync();
}

async Task SeedProjectie(InventarisContext dbContext, XLWorkbook wb)
{
    Console.WriteLine("Seeding Projectie...");
    var sheet = wb.Worksheet("Projectie");
    var rows = sheet.RangeUsed().RowsUsed().Skip(1);

    foreach (var row in rows)
    {
        var type = row.Cell(3).GetString().Trim();
        if (string.IsNullOrEmpty(type)) continue;

        var device = new InventarisApp.Models.Device { type = type };
        dbContext.Devices.Add(device);
        await dbContext.SaveChangesAsync();

        var info = new InventarisApp.Models.Info
        {
            type = type,
            device_id = device.device_id,
            apparaatnaam = row.Cell(2).GetString().Trim(),
            merk = row.Cell(4).GetString().Trim(),
            model = row.Cell(5).GetString().Trim(),
            aantal = int.TryParse(row.Cell(6).GetString(), out int a) ? a : null,
            LokaalId = await GetLokaalId(dbContext, row.Cell(8).GetString()),
            staat = row.Cell(9).GetString().Trim(),
            status = row.Cell(9).GetString().Trim(),
            opmerkingen = row.Cell(10).GetString().Trim()
        };
        dbContext.Infos.Add(info);
    }
    await dbContext.SaveChangesAsync();
}

async Task SeedNetwerk(InventarisContext dbContext, XLWorkbook wb)
{
    Console.WriteLine("Seeding Netwerk...");
    var sheet = wb.Worksheet("Netwerk");
    var rows = sheet.RangeUsed().RowsUsed().Skip(1);

    foreach (var row in rows)
    {
        var type = row.Cell(2).GetString().Trim();
        if (string.IsNullOrEmpty(type)) continue;

        var device = new InventarisApp.Models.Device { type = type };
        dbContext.Devices.Add(device);
        await dbContext.SaveChangesAsync();

        var info = new InventarisApp.Models.Info
        {
            type = type,
            device_id = device.device_id,
            apparaatnaam = row.Cell(1).GetString().Trim(),
            merk = row.Cell(3).GetString().Trim(),
            serial_number = row.Cell(4).GetString().Trim(),
            LokaalId = await GetLokaalId(dbContext, row.Cell(6).GetString()),
            ip = row.Cell(7).GetString().Trim()
        };
        dbContext.Infos.Add(info);
    }
    await dbContext.SaveChangesAsync();
}
