using PackageTracking.Models;

namespace PackageTracking.Notifications;

/// Ett mejl till kunden genom att skriva till konsolen.
public class ConsoleNotifier : IPackageObserver
{
    private readonly Customer _customer;
    private readonly TextWriter _output;

    public ConsoleNotifier(Customer customer, TextWriter? output = null)
    {
        _customer = customer;
        _output = output ?? Console.Out;
    }

    public void OnTrackingEvent(Package package, TrackingEvent evt)
    {
        var place = evt.Station != null ? $" vid {evt.Station}" : "";
        _output.WriteLine($"  [Mejl till {_customer.Email}] Paket {package.TrackingNumber}: " +
                          $"{evt.Status.ToSwedish()}{place}");
    }
}
