namespace PackageTracking.Models;

/// Postadress för avsändare eller mottagare.
public class Address
{
    public string Street { get; }
    public string PostalCode { get; }
    public string City { get; }

    public Address(string street, string postalCode, string city)
    {
        if (string.IsNullOrWhiteSpace(street)) throw new ArgumentException("Gatuadress saknas.");
        if (string.IsNullOrWhiteSpace(postalCode)) throw new ArgumentException("Postnummer saknas.");
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("Ort saknas.");
        Street = street.Trim();
        PostalCode = postalCode.Trim();
        City = city.Trim();
    }

    public override string ToString() => $"{Street}, {PostalCode} {City}";
}
