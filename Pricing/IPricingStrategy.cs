namespace PackageTracking.Pricing;


/// Strategy: varje frakttyp har sin egen prisberäkning, leveranstid och prioritet.
/// LSP: Alla frakttyper kan användas på exakt samma sätt av resten av systemet.

public interface IPricingStrategy
{
    string Name { get; }
/// Lägre tal = högre prioritet i transportkedjan.
    int Priority { get; }
    decimal CalculatePrice(double weightKg, double distanceKm);
    int EstimatedDeliveryDays(double distanceKm);
}
