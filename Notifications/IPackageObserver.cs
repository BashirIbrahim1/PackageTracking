using PackageTracking.Models;

namespace PackageTracking.Notifications;

/// Observer: får besked när ett paket får en ny spårningshändelse.
public interface IPackageObserver
{
    void OnTrackingEvent(Package package, TrackingEvent evt);
}
