namespace PackageTracking.Pricing;

public class StandardShipping : IPricingStrategy
{
    public string Name => "Standard";
    public int Priority => 3;
    public decimal CalculatePrice(double weightKg, double distanceKm) =>
        29m + (decimal)weightKg * 5m + (decimal)distanceKm * 0.10m;
    public int EstimatedDeliveryDays(double distanceKm) => 2 + (int)(distanceKm / 400);
}

public class ExpressShipping : IPricingStrategy
{
    public string Name => "Express";
    public int Priority => 1;
    public decimal CalculatePrice(double weightKg, double distanceKm) =>
        79m + (decimal)weightKg * 9m + (decimal)distanceKm * 0.20m;
    public int EstimatedDeliveryDays(double distanceKm) => 1;
}

public class FragileShipping : IPricingStrategy
{
    public string Name => "Skör gods";
    public int Priority => 2;
    public decimal CalculatePrice(double weightKg, double distanceKm) =>
        59m + (decimal)weightKg * 7m + (decimal)distanceKm * 0.15m + 25m; 
    public int EstimatedDeliveryDays(double distanceKm) => 2 + (int)(distanceKm / 600);
}

/// Register över tillgängliga frakttyper. 
public static class ShippingTypes
{
    public static IReadOnlyList<IPricingStrategy> All { get; } = new List<IPricingStrategy>
    {
        new StandardShipping(),
        new ExpressShipping(),
        new FragileShipping()
    };
}
