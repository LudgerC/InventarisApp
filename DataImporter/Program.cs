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
await SeedPrinters(context, workbook);

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
    var rows = sheet.RangeUsed().RowsUsed().Skip(1); 

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

    var addedLokalen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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

        // Check if already processed in this loop or exists in DB
        if (addedLokalen.Contains(lokaalNaam)) continue;

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
            addedLokalen.Add(lokaalNaam);
        }
        else
        {
            addedLokalen.Add(lokaalNaam);
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

async Task<int?> GetLokaalId(InventarisContext dbContext, string? rawLokaal)
{
    var lokaalNaam = CleanValue(rawLokaal);
    if (lokaalNaam == null) return null;
    
    // Strip "R" if it's there (e.g. R102 -> 102)
    if (lokaalNaam.StartsWith("R", StringComparison.OrdinalIgnoreCase) && lokaalNaam.Length > 1 && char.IsDigit(lokaalNaam[1]))
    {
        lokaalNaam = lokaalNaam.Substring(1);
    }
    
    // Pad to 3 digits if numeric (e.g. 9 -> 009)
    if (int.TryParse(lokaalNaam, out int numericName))
    {
        lokaalNaam = numericName.ToString("D3");
    }
    
    var lokaal = await dbContext.Lokalen.FirstOrDefaultAsync(l => l.Naam == lokaalNaam);
    return lokaal?.ID;
}

async Task<int?> GetPersoonId(InventarisContext dbContext, string? voornaam)
{
    var name = CleanValue(voornaam);
    if (name == null) return null;

    // Pak alleen het eerste woord (ervan uitgaande dat dit de voornaam is)
    var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 0) return null;
    
    var firstName = parts[0].ToLower();

    var persoon = await dbContext.Personen.FirstOrDefaultAsync(p => p.Naam.ToLower() == firstName);
    return persoon?.ID;
}

string? CleanValue(string? val)
{
    if (string.IsNullOrWhiteSpace(val)) return null;
    val = val.Trim();
    if (val.Equals("NVT", StringComparison.OrdinalIgnoreCase) || val.Equals("N.V.T.", StringComparison.OrdinalIgnoreCase)) return null;
    return val;
}

DateTime? ParseExcelDate(IXLCell cell)
{
    if (cell.IsEmpty()) return null;
    if (cell.TryGetValue(out DateTime d)) return d;
    var s = CleanValue(cell.GetString());
    if (s == null) return null;
    if (DateTime.TryParse(s, out DateTime d2)) return d2;
    return null;
}

int GetCol(IXLWorksheet sheet, string headerName)
{
    var firstRow = sheet.Row(1);
    foreach (var cell in firstRow.CellsUsed())
    {
        if (cell.GetString().Trim().Replace(" ", "").ToLower() == headerName.Replace(" ", "").ToLower())
            return cell.Address.ColumnNumber;
    }
    return 0;
}

async Task SeedDocentenPCs(InventarisContext dbContext, XLWorkbook wb)
{
    Console.WriteLine("Seeding Docenten PCs...");
    var sheet = wb.Worksheet("Docenten_pcs");
    var rows = sheet.RangeUsed().RowsUsed().Skip(1);

    int naamIdx = GetCol(sheet, "Naam");
    int typeIdx = GetCol(sheet, "Type");
    int merkIdx = GetCol(sheet, "Merk");
    int modelIdx = GetCol(sheet, "Model");
    int snIdx = GetCol(sheet, "Serienummer");
    int aantalIdx = GetCol(sheet, "Aantal");
    int lokaalIdx = GetCol(sheet, "Lokaal");
    int staatIdx = GetCol(sheet, "Staat");
    int verkoperIdx = GetCol(sheet, "Verkoper");
    int aankoopIdx = GetCol(sheet, "aankoopdatum");
    int garantieIdx = GetCol(sheet, "einde garantie");

    foreach (var row in rows)
    {
        var type = CleanValue(row.Cell(typeIdx).GetString());
        if (string.IsNullOrEmpty(type)) continue;

        var device = new InventarisApp.Models.Device { type = type };
        dbContext.Devices.Add(device);
        await dbContext.SaveChangesAsync();

        var info = new InventarisApp.Models.Info
        {
            type = type,
            device_id = device.device_id,
            apparaatnaam = CleanValue(row.Cell(naamIdx).GetString()),
            merk = CleanValue(row.Cell(merkIdx).GetString()),
            model = CleanValue(row.Cell(modelIdx).GetString()),
            serial_number = CleanValue(row.Cell(snIdx).GetString()),
            aantal = int.TryParse(row.Cell(aantalIdx).GetString(), out int a) ? a : null,
            LokaalId = await GetLokaalId(dbContext, row.Cell(lokaalIdx).GetString()),
            staat = CleanValue(row.Cell(staatIdx).GetString()),
            status = "Active", // Default status
            leverancier = CleanValue(row.Cell(verkoperIdx).GetString()),
            aankoopdatum = ParseExcelDate(row.Cell(aankoopIdx)),
            eind_garantie = ParseExcelDate(row.Cell(garantieIdx))
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

    int naamIdx = GetCol(sheet, "Naam");
    int typeIdx = GetCol(sheet, "Type");
    int merkIdx = GetCol(sheet, "Merk");
    int modelIdx = GetCol(sheet, "Model");
    int snIdx = GetCol(sheet, "Serienummer");
    int aantalIdx = GetCol(sheet, "Aantal");
    int lokaalIdx = GetCol(sheet, "Lokaal");
    int staatIdx = GetCol(sheet, "Staat");
    int verkoperIdx = GetCol(sheet, "Verkoper");
    int aankoopIdx = GetCol(sheet, "Aankoopdatum");
    int garantieIdx = GetCol(sheet, "Einde garantie");

    foreach (var row in rows)
    {
        var type = CleanValue(row.Cell(typeIdx).GetString());
        if (string.IsNullOrEmpty(type)) continue;

        var device = new InventarisApp.Models.Device { type = type };
        dbContext.Devices.Add(device);
        await dbContext.SaveChangesAsync();

        var info = new InventarisApp.Models.Info
        {
            type = type,
            device_id = device.device_id,
            apparaatnaam = CleanValue(row.Cell(naamIdx).GetString()),
            merk = CleanValue(row.Cell(merkIdx).GetString()),
            model = CleanValue(row.Cell(modelIdx).GetString()),
            serial_number = CleanValue(row.Cell(snIdx).GetString()),
            aantal = int.TryParse(row.Cell(aantalIdx).GetString(), out int a) ? a : null,
            LokaalId = await GetLokaalId(dbContext, row.Cell(lokaalIdx).GetString()),
            staat = CleanValue(row.Cell(staatIdx).GetString()),
            status = "Active",
            leverancier = CleanValue(row.Cell(verkoperIdx).GetString()),
            aankoopdatum = ParseExcelDate(row.Cell(aankoopIdx)),
            eind_garantie = ParseExcelDate(row.Cell(garantieIdx))
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

    int naamIdx = GetCol(sheet, "Naam");
    int typeIdx = GetCol(sheet, "Type");
    int merkIdx = GetCol(sheet, "Merk");
    int modelIdx = GetCol(sheet, "Model");
    int snIdx = GetCol(sheet, "Serienummer");
    int aantalIdx = GetCol(sheet, "Aantal");
    int verdiepIdx = GetCol(sheet, "Verdiep");
    int lokaalIdx = GetCol(sheet, "Lokaal");
    int staatIdx = GetCol(sheet, "Staat");
    int verkoperIdx = GetCol(sheet, "Verkoper");
    int aankoopIdx = GetCol(sheet, "aankoopdatum");
    int garantieIdx = GetCol(sheet, "einde garantie");

    foreach (var row in rows)
    {
        var type = CleanValue(row.Cell(typeIdx).GetString());
        if (string.IsNullOrEmpty(type)) continue;

        var device = new InventarisApp.Models.Device { type = type };
        dbContext.Devices.Add(device);
        await dbContext.SaveChangesAsync();

        var verdiep = CleanValue(row.Cell(verdiepIdx).GetString())?.ToLower();
        var lokaalOrPerson = row.Cell(lokaalIdx).GetString();

        var info = new InventarisApp.Models.Info
        {
            type = type,
            device_id = device.device_id,
            apparaatnaam = CleanValue(row.Cell(naamIdx).GetString()),
            merk = CleanValue(row.Cell(merkIdx).GetString()),
            model = CleanValue(row.Cell(modelIdx).GetString()),
            serial_number = CleanValue(row.Cell(snIdx).GetString()),
            aantal = int.TryParse(row.Cell(aantalIdx).GetString(), out int a) ? a : null,
            staat = CleanValue(row.Cell(staatIdx).GetString()),
            status = "Active",
            leverancier = CleanValue(row.Cell(verkoperIdx).GetString()),
            aankoopdatum = ParseExcelDate(row.Cell(aankoopIdx)),
            eind_garantie = ParseExcelDate(row.Cell(garantieIdx))
        };

        if (verdiep == "persoon")
        {
            info.PersoonId = await GetPersoonId(dbContext, lokaalOrPerson);
        }
        else
        {
            info.LokaalId = await GetLokaalId(dbContext, lokaalOrPerson);
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

    int naamIdx = GetCol(sheet, "Naam");
    int typeIdx = GetCol(sheet, "Type");
    int merkIdx = GetCol(sheet, "Merk");
    int modelIdx = GetCol(sheet, "Model");
    int aantalIdx = GetCol(sheet, "Aantal");
    int lokaalIdx = GetCol(sheet, "Lokaal");
    int staatIdx = GetCol(sheet, "Staat");
    int opmIdx = GetCol(sheet, "Opmerkingen");

    foreach (var row in rows)
    {
        var type = CleanValue(row.Cell(typeIdx).GetString());
        if (string.IsNullOrEmpty(type)) continue;

        var device = new InventarisApp.Models.Device { type = type };
        dbContext.Devices.Add(device);
        await dbContext.SaveChangesAsync();

        var info = new InventarisApp.Models.Info
        {
            type = type,
            device_id = device.device_id,
            apparaatnaam = CleanValue(row.Cell(naamIdx).GetString()),
            merk = CleanValue(row.Cell(merkIdx).GetString()),
            model = CleanValue(row.Cell(modelIdx).GetString()),
            aantal = int.TryParse(row.Cell(aantalIdx).GetString(), out int a) ? a : null,
            LokaalId = await GetLokaalId(dbContext, row.Cell(lokaalIdx).GetString()),
            staat = CleanValue(row.Cell(staatIdx).GetString()),
            status = "Active",
            opmerkingen = CleanValue(row.Cell(opmIdx).GetString())
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

    int naamIdx = GetCol(sheet, "Naam");
    int typeIdx = GetCol(sheet, "Type");
    int merkIdx = GetCol(sheet, "Merk");
    int modelIdx = GetCol(sheet, "Model");
    int snIdx = GetCol(sheet, "Serienummer");
    int lokaalIdx = GetCol(sheet, "Lokaal");
    int ipIdx = GetCol(sheet, "IP");

    foreach (var row in rows)
    {
        var type = CleanValue(row.Cell(typeIdx).GetString());
        if (string.IsNullOrEmpty(type)) continue;

        var device = new InventarisApp.Models.Device { type = type };
        dbContext.Devices.Add(device);
        await dbContext.SaveChangesAsync();

        var info = new InventarisApp.Models.Info
        {
            type = type,
            device_id = device.device_id,
            apparaatnaam = CleanValue(row.Cell(naamIdx).GetString()),
            merk = CleanValue(row.Cell(merkIdx).GetString()),
            model = CleanValue(row.Cell(modelIdx).GetString()),
            serial_number = CleanValue(row.Cell(snIdx).GetString()),
            LokaalId = await GetLokaalId(dbContext, row.Cell(lokaalIdx).GetString()),
            ip = CleanValue(row.Cell(ipIdx).GetString()),
            status = "Active"
        };
        dbContext.Infos.Add(info);
    }
    await dbContext.SaveChangesAsync();
}

async Task SeedPrinters(InventarisContext dbContext, XLWorkbook wb)
{
    Console.WriteLine("Seeding Printers...");
    var sheet = wb.Worksheet("Printers");
    var rows = sheet.RangeUsed().RowsUsed().Skip(1);

    int ipIdx = GetCol(sheet, "IP");
    if (ipIdx == 0) ipIdx = GetCol(sheet, "IPadres");
    
    int naamIdx = GetCol(sheet, "Naam");
    int wwIdx = GetCol(sheet, "Wachtwoord");
    int lokaalIdx = GetCol(sheet, "Lokaal");
    int snIdx = GetCol(sheet, "Serienummer");
    int tonerIdx = GetCol(sheet, "Toner");
    int kleurIdx = GetCol(sheet, "Kleur");
    int nietjesIdx = GetCol(sheet, "Nietjes");

    var type = "Printer";
    var existingType = await dbContext.Devices.FirstOrDefaultAsync(d => d.type == type);
    if (existingType == null)
    {
        dbContext.Devices.Add(new InventarisApp.Models.Device { type = type });
        await dbContext.SaveChangesAsync();
    }

    foreach (var row in rows)
    {
        var naam = CleanValue(row.Cell(naamIdx).GetString());
        var ip = CleanValue(row.Cell(ipIdx).GetString());
        
        if (string.IsNullOrEmpty(naam) && string.IsNullOrEmpty(ip)) continue;

        var device = new InventarisApp.Models.Device { type = type };
        dbContext.Devices.Add(device);
        await dbContext.SaveChangesAsync();

        string kleurStr = CleanValue(row.Cell(kleurIdx).GetString());
        bool? heeftKleur = null;
        if (!string.IsNullOrEmpty(kleurStr))
        {
            heeftKleur = (kleurStr == "1" || kleurStr.ToLower() == "true" || kleurStr.ToLower() == "ja");
        }

        string nietjesStr = CleanValue(row.Cell(nietjesIdx).GetString());
        bool? heeftNietjes = null;
        if (!string.IsNullOrEmpty(nietjesStr))
        {
            heeftNietjes = (nietjesStr == "1" || nietjesStr.ToLower() == "true" || nietjesStr.ToLower() == "ja");
        }

        var info = new InventarisApp.Models.Info
        {
            type = type,
            device_id = device.device_id,
            ip = ip,
            apparaatnaam = naam,
            wachtwoord = CleanValue(row.Cell(wwIdx).GetString()),
            LokaalId = await GetLokaalId(dbContext, row.Cell(lokaalIdx).GetString()),
            serial_number = CleanValue(row.Cell(snIdx).GetString()),
            toner = CleanValue(row.Cell(tonerIdx).GetString()),
            kleur = heeftKleur,
            nietjes = heeftNietjes,
            status = "Active"
        };
        dbContext.Infos.Add(info);
    }
    await dbContext.SaveChangesAsync();
}
