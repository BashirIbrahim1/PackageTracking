namespace PackageTracking.Models;

/// Spårningshistorik.
public record TrackingEvent(DateTime Time, PackageStatus Status, Station? Station, string Note);
