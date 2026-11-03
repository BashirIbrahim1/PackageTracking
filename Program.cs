using System.Globalization;
using System.Text;
using PackageTracking.Models;
using PackageTracking.Notifications;
using PackageTracking.Pricing;
using PackageTracking.Services;
using PackageTracking.States;

namespace PackageTracking;

public static class Program
{
    private static readonly List<Station> Stations = new()
    {
        new Station("Stockholmsterminalen", "Stockholm"),
        new Station("Örebroterminalen", "Örebro"),
        new Station("Jönköpingsterminalen", "Jönköping"),
        new Station("Göteborgsterminalen", "Göteborg"),
        new Station("Malmöterminalen", "Malmö"),
        new Station("Umeåterminalen", "Umeå")
    };

    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        CultureInfo.CurrentCulture = new CultureInfo("sv-SE");

        var service = new ShippingService();
        try
        {
            RunMenu(service);
        }
        catch (EndOfStreamException)
        {
            Console.WriteLine("\nIndata avslutad. Programmet stängs.");
        }
    }

    private static void RunMenu(ShippingService service)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Paketleverans och spårning ===");
            Console.WriteLine("1. Registrera paket");
            Console.WriteLine("2. Flytta paket ett steg framåt");
            Console.WriteLine("3. Markera paket som försenat");
            Console.WriteLine("4. Spåra paket");
            Console.WriteLine("5. Lista paket efter prioritet");
            Console.WriteLine("6. Demo: statusövergångar (State)");
            Console.WriteLine("7. Demo: frakttyper och pris (Strategy)");
            Console.WriteLine("8. Demo: integrerat scenario");
            Console.WriteLine("0. Avsluta");

            var choice = ReadInt("Val: ", 0, 8);
            try
            {
                switch (choice)
                {
                    case 0: return;
                    case 1: RegisterPackage(service); break;
                    case 2: AdvancePackage(service); break;
                    case 3: DelayPackage(service); break;
                    case 4: TrackPackage(service); break;
                    case 5: ListByPriority(service); break;
                    case 6: DemoStates(); break;
                    case 7: DemoPricing(); break;
                    case 8: DemoIntegrated(); break;
                }
            }
            catch (InvalidStatusTransitionException ex) { Console.WriteLine($"Fel: {ex.Message}"); }
            catch (ArgumentException ex) { Console.WriteLine($"Fel: {ex.Message}"); }
            catch (InvalidOperationException ex) { Console.WriteLine($"Fel: {ex.Message}"); }
        }
    }

    // ---------------- Menyval ----------------

    private static void RegisterPackage(ShippingService service)
    {
        Console.WriteLine("\n--- Avsändare ---");
        var sender = new Customer(ReadText("Namn: "), ReadText("E-post: "));
        var fromAddress = new Address(ReadText("Gatuadress: "), ReadText("Postnummer: "), ReadText("Ort: "));

        Console.WriteLine("\n--- Mottagare ---");
        var recipient = new Customer(ReadText("Namn: "), ReadText("E-post: "));
        var toAddress = new Address(ReadText("Gatuadress: "), ReadText("Postnummer: "), ReadText("Ort: "));

        Console.WriteLine("\n--- Paketet ---");
        var weight = ReadDouble("Vikt i kg (0,1–50): ", 0.1, 50);
        var distance = ReadDouble("Avstånd i km (0–2000): ", 0, 2000);

        Console.WriteLine("\nFrakttyper:");
        for (int i = 0; i < ShippingTypes.All.Count; i++)
        {
            var t = ShippingTypes.All[i];
            Console.WriteLine($"{i + 1}. {t.Name} – {t.CalculatePrice(weight, distance):0.00} kr, " +
                              $"ca {t.EstimatedDeliveryDays(distance)} dygn");
        }
        var type = ShippingTypes.All[ReadInt("Välj frakttyp: ", 1, ShippingTypes.All.Count) - 1];

        var route = ReadRoute();
        var package = service.RegisterPackage(sender, recipient, fromAddress, toAddress,
                                              weight, distance, type, route,
                                              new ConsoleNotifier(recipient));

        Console.WriteLine($"\nPaketet är registrerat. Spårningsnummer: {package.TrackingNumber}");
        Console.WriteLine($"Pris: {package.Price:0.00} kr, beräknad leverans om {package.EstimatedDeliveryDays} dygn.");
    }

    private static void AdvancePackage(ShippingService service)
    {
        var package = AskForPackage(service);
        if (package == null) return;
        service.Advance(package);
    }

    private static void DelayPackage(ShippingService service)
    {
        var package = AskForPackage(service);
        if (package == null) return;
        service.MarkDelayed(package, ReadText("Orsak till förseningen: "));
    }

    private static void TrackPackage(ShippingService service)
    {
        var package = AskForPackage(service);
        if (package == null) return;
        PrintTracking(package);
    }

    private static void ListByPriority(ShippingService service)
    {
        var sorted = service.GetPackagesByPriority();
        if (sorted.Count == 0)
        {
            Console.WriteLine("Inga paket registrerade ännu.");
            return;
        }
        Console.WriteLine("\nPaket i prioritetsordning:");
        foreach (var p in sorted)
            Console.WriteLine($"  {p.TrackingNumber}  {p.ShippingType.Name,-10} {p.Status.ToSwedish()}");
    }

    // Demo

    /// Demo 1: State – tillåtna och otillåtna statusövergångar.
    private static void DemoStates()
    {
        Console.WriteLine("\n=== Demo: State ===");
        var service = new ShippingService();
        var package = CreateSamplePackage(service, new StandardShipping(), 2.5, 450, 3);
        Console.WriteLine($"Start: {package.Status.ToSwedish()}");

        TryChange(package, PackageStatus.Delivered);      // otillåtet: hoppar över steg
        service.Advance(package);                         // Inlämnad -> Under transport
        service.MarkDelayed(package, "Väderförhinder");   // Under transport -> Försenad
        TryChange(package, PackageStatus.Delivered);      // otillåtet från Försenad
        service.Advance(package);                         // Försenad -> Under transport (nästa station)
        while (package.Status != PackageStatus.Delivered)
            service.Advance(package);
        TryChange(package, PackageStatus.InTransit);      // otillåtet: Levererad är slutstatus
    }

    /// Demo 2: Strategy – samma paket, tre frakttyper.
    private static void DemoPricing()
    {
        Console.WriteLine("\n=== Demo: Strategy (3 kg, 450 km) ===");
        foreach (var type in ShippingTypes.All)
        {
            Console.WriteLine($"{type.Name,-10} pris {type.CalculatePrice(3, 450),8:0.00} kr   " +
                              $"leverans {type.EstimatedDeliveryDays(450)} dygn   prioritet {type.Priority}");
        }
    }

    /// Integrerat scenario: 1.registrera 2.pris 3.transport 4.notifiering 5.spåra 6.prioritetslista.
    private static void DemoIntegrated()
    {
        Console.WriteLine("\n=== Integrerat scenario ===");
        var service = new ShippingService();
        var standard = CreateSamplePackage(service, new StandardShipping(), 4, 500, 3);
        var express = CreateSamplePackage(service, new ExpressShipping(), 1.2, 500, 2);
        var fragile = CreateSamplePackage(service, new FragileShipping(), 6, 500, 3);

        Console.WriteLine($"\nRegistrerade: {standard.TrackingNumber} (Standard, {standard.Price:0.00} kr), " +
                          $"{express.TrackingNumber} (Express, {express.Price:0.00} kr), " +
                          $"{fragile.TrackingNumber} (Skör gods, {fragile.Price:0.00} kr)");

        Console.WriteLine("\nExpresspaketet skickas hela vägen:");
        while (express.Status != PackageStatus.Delivered)
            service.Advance(express);

        Console.WriteLine("\nDet sköra godset försenas men fortsätter sedan:");
        service.Advance(fragile);
        service.MarkDelayed(fragile, "Skadekontroll");
        service.Advance(fragile);

        Console.WriteLine("\nSpårning av det sköra godset:");
        PrintTracking(fragile);

        Console.WriteLine("\nPrioritetsordning:");
        foreach (var p in service.GetPackagesByPriority())
            Console.WriteLine($"  {p.TrackingNumber}  {p.ShippingType.Name,-10} {p.Status.ToSwedish()}");
    }

    // Hjälpmetoder 

    private static Package CreateSamplePackage(ShippingService service, IPricingStrategy type,
                                               double weight, double distance, int stops)
    {
        var sender = new Customer("Anna Avsändare", "anna@example.com");
        var recipient = new Customer("Bo Mottagare", "bo@example.com");
        return service.RegisterPackage(
            sender, recipient,
            new Address("Storgatan 1", "111 22", "Stockholm"),
            new Address("Kungsgatan 5", "411 19", "Göteborg"),
            weight, distance, type, Stations.Take(stops),
            new ConsoleNotifier(recipient));
    }

    private static void TryChange(Package package, PackageStatus target)
    {
        try
        {
            Console.WriteLine($"Försöker ändra {package.Status.ToSwedish()} → {target.ToSwedish()}...");
            package.ChangeStatus(target);
        }
        catch (InvalidStatusTransitionException ex)
        {
            Console.WriteLine($"  Avvisat: {ex.Message}");
        }
    }

    private static void PrintTracking(Package package)
    {
        Console.WriteLine($"\nPaket {package.TrackingNumber} ({package.ShippingType.Name})");
        Console.WriteLine($"Från: {package.FromAddress}");
        Console.WriteLine($"Till: {package.ToAddress}");
        Console.WriteLine($"Status: {package.Status.ToSwedish()}");
        Console.WriteLine($"Senaste station: {package.CurrentStation?.ToString() ?? "–"}");
        Console.WriteLine("Historik:");
        foreach (var e in package.History)
        {
            var place = e.Station != null ? $" vid {e.Station}" : "";
            var note = string.IsNullOrEmpty(e.Note) ? "" : $" ({e.Note})";
            Console.WriteLine($"  {e.Time:yyyy-MM-dd HH:mm:ss}  {e.Status.ToSwedish()}{place}{note}");
        }
    }

    private static Package? AskForPackage(ShippingService service)
    {
        var number = ReadText("Spårningsnummer: ");
        if (service.TryGetPackage(number, out var package)) return package;
        Console.WriteLine("Hittade inget paket med det spårningsnumret.");
        return null;
    }

    private static List<Station> ReadRoute()
    {
        Console.WriteLine("\nTillgängliga stationer:");
        for (int i = 0; i < Stations.Count; i++)
            Console.WriteLine($"{i + 1}. {Stations[i]}");

        while (true)
        {
            var input = ReadText("Ange stationernas nummer i ordning, separerade med komma (t.ex. 1,2,4): ");
            var route = new List<Station>();
            var ok = true;
            foreach (var part in input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (int.TryParse(part, out var n) && n >= 1 && n <= Stations.Count)
                    route.Add(Stations[n - 1]);
                else
                {
                    ok = false;
                    break;
                }
            }
            if (ok && route.Count > 0) return route;
            Console.WriteLine($"Ogiltig rutt. Ange minst en siffra mellan 1 och {Stations.Count}.");
        }
    }

    // Robust inmatning 

    private static string ReadLine()
        => Console.ReadLine() ?? throw new EndOfStreamException();

    private static string ReadText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var text = ReadLine().Trim();
            if (text.Length > 0) return text;
            Console.WriteLine("Fältet får inte vara tomt.");
        }
    }

    private static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(ReadLine().Trim(), out var value) && value >= min && value <= max)
                return value;
            Console.WriteLine($"Ange ett heltal mellan {min} och {max}.");
        }
    }

    private static double ReadDouble(string prompt, double min, double max)
    {
        while (true)
        {
            Console.Write(prompt);
            var text = ReadLine().Trim().Replace(',', '.');
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                && value >= min && value <= max)
                return value;
            Console.WriteLine($"Ange ett tal mellan {min} och {max}.");
        }
    }
}
