namespace CryonixVnFormat.types;

public class Macro : ICharacter
{
    public required string Identifier { get; init; }
    public Dictionary<string, string> PropertyOverrides { get; init; } = new();

    private bool Equals(Macro other)
    {
        return Identifier == other.Identifier && PropertyOverrides.Equals(other.PropertyOverrides);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Macro)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Identifier, PropertyOverrides);
    }

    public override string ToString()
    {
        return PropertyOverrides.Count == 0
            ? $"{nameof(Identifier)}: {Identifier}"
            : $"{nameof(Identifier)}: {Identifier}, {nameof(PropertyOverrides)}: {string.Join(',', PropertyOverrides)}";
    }
}