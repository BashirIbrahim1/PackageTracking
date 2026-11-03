using PackageTracking.Models;

namespace PackageTracking.States;

/// State-interface: varje status vet vilka övergångar som är tillåtna från den.
public interface IPackageState
{
    PackageStatus Status { get; }
    bool CanMoveTo(PackageStatus target);
    IPackageState MoveTo(PackageStatus target);
}
