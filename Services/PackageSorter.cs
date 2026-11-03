using PackageTracking.Models;

namespace PackageTracking.Services;

/// (GRASP): hjälpklass för sortering.

public static class PackageSorter
{
    public static List<Package> SortByPriority(IEnumerable<Package> packages)
    {
        var list = packages.ToList();
        for (int i = 1; i < list.Count; i++)
        {
            var current = list[i];
            int j = i - 1;
            while (j >= 0 && Compare(list[j], current) > 0)
            {
                list[j + 1] = list[j];
                j--;
            }
            list[j + 1] = current;
        }
        return list;
    }

    // Först prioritet (lägst tal först), sedan registreringsordning.
    private static int Compare(Package a, Package b)
    {
        int byPriority = a.ShippingType.Priority.CompareTo(b.ShippingType.Priority);
        return byPriority != 0 ? byPriority : a.Sequence.CompareTo(b.Sequence);
    }
}
