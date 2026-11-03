using PackageTracking.Models;

namespace PackageTracking.States;

public class InvalidStatusTransitionException : Exception
{
    public PackageStatus From { get; }
    public PackageStatus To { get; }

    public InvalidStatusTransitionException(PackageStatus from, PackageStatus to)
        : base($"Otillåten statusövergång: {from.ToSwedish()} → {to.ToSwedish()}.")
    {
        From = from;
        To = to;
    }
}
