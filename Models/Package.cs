using PackageTracking.Notifications;
using PackageTracking.Pricing;
using PackageTracking.States;

namespace PackageTracking.Models;




public class Package
{
    private readonly List<IPackageObserver> _observers = new();
    private readonly List<TrackingEvent> _history = new();
    private readonly Queue<Station> _remainingRoute;   
    private IPackageState _state = PackageStateFactory.Create(PackageStatus.Registered);

    public string TrackingNumber { get; }
    public int Sequence { get; }                        
    public Customer Sender { get; }
    public Customer Recipient { get; }
    public Address FromAddress { get; }
    public Address ToAddress { get; }
    public double WeightKg { get; }
    public double DistanceKm { get; }
    public IPricingStrategy ShippingType { get; }
    public Station? CurrentStation { get; private set; }

    public PackageStatus Status => _state.Status;
    public int RemainingStops => _remainingRoute.Count;
    public IReadOnlyList<TrackingEvent> History => _history;
    public decimal Price => ShippingType.CalculatePrice(WeightKg, DistanceKm);
    public int EstimatedDeliveryDays => ShippingType.EstimatedDeliveryDays(DistanceKm);

    public Package(string trackingNumber, int sequence, Customer sender, Customer recipient,
                   Address from, Address to, double weightKg, double distanceKm,
                   IPricingStrategy shippingType, IEnumerable<Station> route)
    {
        if (weightKg <= 0) throw new ArgumentException("Vikten måste vara större än 0 kg.");
        if (distanceKm < 0) throw new ArgumentException("Avståndet kan inte vara negativt.");
        var stations = route?.ToList() ?? throw new ArgumentException("Rutt saknas.");
        if (stations.Count == 0) throw new ArgumentException("Rutten måste innehålla minst en station.");

        TrackingNumber = trackingNumber;
        Sequence = sequence;
        Sender = sender;
        Recipient = recipient;
        FromAddress = from;
        ToAddress = to;
        WeightKg = weightKg;
        DistanceKm = distanceKm;
        ShippingType = shippingType ?? throw new ArgumentException("Frakttyp saknas.");
        _remainingRoute = new Queue<Station>(stations);

        _history.Add(new TrackingEvent(DateTime.Now, PackageStatus.Registered, null, "Paketet registrerat"));
    }

	// Observer 
	public void Subscribe(IPackageObserver observer) => _observers.Add(observer);
    public void Unsubscribe(IPackageObserver observer) => _observers.Remove(observer);

    private void Notify(TrackingEvent evt)
    {
        foreach (var observer in _observers.ToList())
            observer.OnTrackingEvent(this, evt);
    }

	/// Byter status om övergången är tillåten, annars kastas exception. 
	public void ChangeStatus(PackageStatus target, string note = "")
    {
        _state = _state.MoveTo(target);
        var evt = new TrackingEvent(DateTime.Now, Status, CurrentStation, note);
        _history.Add(evt);
        Notify(evt);
    }

    
    public void ArriveAtNextStation()
    {
        if (_remainingRoute.Count == 0)
            throw new InvalidOperationException("Paketet har inga fler stationer kvar på rutten.");
        if (Status != PackageStatus.InTransit && !_state.CanMoveTo(PackageStatus.InTransit))
            throw new InvalidStatusTransitionException(Status, PackageStatus.InTransit);

        CurrentStation = _remainingRoute.Dequeue();

        if (Status == PackageStatus.InTransit)
        {
            
            var evt = new TrackingEvent(DateTime.Now, Status, CurrentStation, "Ankommit till station");
            _history.Add(evt);
            Notify(evt);
        }
        else
        {
            ChangeStatus(PackageStatus.InTransit, "Lämnat station");
        }
    }
}
