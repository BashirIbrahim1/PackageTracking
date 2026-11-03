using PackageTracking.Models;
using PackageTracking.Notifications;
using PackageTracking.Pricing;

namespace PackageTracking.Services;


/// Controller (GRASP): tar emot systemhändelser och koordinerar domänmodellen.
/// Creator (GRASP): skapar och äger alla paket.

public class ShippingService
{
 
    private readonly Dictionary<string, Package> _packages = new(StringComparer.OrdinalIgnoreCase);
    private int _counter;

    public IReadOnlyCollection<Package> Packages => _packages.Values;

    public Package RegisterPackage(Customer sender, Customer recipient, Address from, Address to,
                                   double weightKg, double distanceKm, IPricingStrategy shippingType,
                                   IEnumerable<Station> route, IPackageObserver? observer = null)
    {
        var sequence = _counter + 1;
        var trackingNumber = $"PKT{sequence:D6}";
        var package = new Package(trackingNumber, sequence, sender, recipient, from, to,
                                  weightKg, distanceKm, shippingType, route);
        _counter = sequence; 
        if (observer != null) package.Subscribe(observer);
        _packages.Add(trackingNumber, package);
        return package;
    }

    public bool TryGetPackage(string trackingNumber, out Package? package)
    {
        package = null;
        if (string.IsNullOrWhiteSpace(trackingNumber)) return false;
        return _packages.TryGetValue(trackingNumber.Trim(), out package);
    }

    /// Flyttar paketet ett steg framåt i transportkedjan.
    public void Advance(Package package)
    {
        switch (package.Status)
        {
            case PackageStatus.Registered:
            case PackageStatus.InTransit:
            case PackageStatus.Delayed:
                if (package.RemainingStops > 0)
                    package.ArriveAtNextStation();
                else
                    package.ChangeStatus(PackageStatus.OutForDelivery, "Lämnat sista terminalen");
                break;

            default:
                package.ChangeStatus(PackageStatus.Delivered, "Levererat till mottagaren");
                break;
        }
    }

    public void MarkDelayed(Package package, string reason) =>
        package.ChangeStatus(PackageStatus.Delayed, reason);

    /// Alla paket sorterade efter prioritet (Express först).
    public List<Package> GetPackagesByPriority() => PackageSorter.SortByPriority(_packages.Values);
}
