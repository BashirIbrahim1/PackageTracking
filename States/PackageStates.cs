using PackageTracking.Models;

namespace PackageTracking.States;

public abstract class PackageStateBase : IPackageState
{
    public abstract PackageStatus Status { get; }
    protected abstract PackageStatus[] AllowedTargets { get; }

    public bool CanMoveTo(PackageStatus target) => AllowedTargets.Contains(target);

    public IPackageState MoveTo(PackageStatus target)
    {
        if (!CanMoveTo(target))
            throw new InvalidStatusTransitionException(Status, target);
        return PackageStateFactory.Create(target);
    }
}

public class RegisteredState : PackageStateBase
{
    public override PackageStatus Status => PackageStatus.Registered;
    protected override PackageStatus[] AllowedTargets => new[] { PackageStatus.InTransit };
}

public class InTransitState : PackageStateBase
{
    public override PackageStatus Status => PackageStatus.InTransit;
    protected override PackageStatus[] AllowedTargets =>
        new[] { PackageStatus.OutForDelivery, PackageStatus.Delayed };
}

public class OutForDeliveryState : PackageStateBase
{
    public override PackageStatus Status => PackageStatus.OutForDelivery;
    protected override PackageStatus[] AllowedTargets =>
        new[] { PackageStatus.Delivered, PackageStatus.Delayed };
}

public class DelayedState : PackageStateBase
{
    public override PackageStatus Status => PackageStatus.Delayed;
    protected override PackageStatus[] AllowedTargets =>
        new[] { PackageStatus.InTransit, PackageStatus.OutForDelivery };
}

public class DeliveredState : PackageStateBase
{
    public override PackageStatus Status => PackageStatus.Delivered;
    protected override PackageStatus[] AllowedTargets => Array.Empty<PackageStatus>(); 
}

/// (Factory): Skapar rätt State-objekt för en status.
public static class PackageStateFactory
{
    public static IPackageState Create(PackageStatus status) => status switch
    {
        PackageStatus.Registered => new RegisteredState(),
        PackageStatus.InTransit => new InTransitState(),
        PackageStatus.OutForDelivery => new OutForDeliveryState(),
        PackageStatus.Delayed => new DelayedState(),
        PackageStatus.Delivered => new DeliveredState(),
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };
}
