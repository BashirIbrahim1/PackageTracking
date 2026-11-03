namespace PackageTracking.Models;

// Inlämnad, Under transport, Ute för leverans, Levererad, Försenad
public enum PackageStatus
{
    Registered,     
    InTransit,       
    OutForDelivery, 
    Delivered,      
    Delayed         
}

public static class PackageStatusExtensions
{
    public static string ToSwedish(this PackageStatus status) => status switch
    {
        PackageStatus.Registered => "Inlämnad",
        PackageStatus.InTransit => "Under transport",
        PackageStatus.OutForDelivery => "Ute för leverans",
        PackageStatus.Delivered => "Levererad",
        PackageStatus.Delayed => "Försenad",
        _ => status.ToString()
    };
}
