namespace PackageTracking.Models;

/// En kund (avsändare eller mottagare) som kan få notifieringar.
public class Customer
{
    public string Name { get; }
    public string Email { get; }

    public Customer(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Namn saknas.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Ogiltig e-postadress.");
        Name = name.Trim();
        Email = email.Trim();
    }
}
