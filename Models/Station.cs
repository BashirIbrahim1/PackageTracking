namespace PackageTracking.Models;

/// En station (terminal) som ett paket passerar på vägen.
public class Station
{
    public string Name { get; }
    public string City { get; }

    public Station(string name, string city)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Stationsnamn saknas.");
        Name = name.Trim();
        City = city.Trim();
    }

    public override string ToString() => $"{Name} ({City})";
}
