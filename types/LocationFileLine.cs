namespace CryonixVnFormat.types;

public class LocationFileLine : IFileLine
{
    public required string Location { get; init; }
    public required string Displayable { get; init; }

    private bool Equals(LocationFileLine other)
    {
        return Location == other.Location && Displayable == other.Displayable;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((LocationFileLine)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Location, Displayable);
    }

    public override string ToString()
    {
        return $"{nameof(Location)}: {Location}, {nameof(Displayable)}: {Displayable}";
    }
}