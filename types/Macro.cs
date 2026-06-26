namespace CryonixVnFormat.types;

public class Macro : ICharacter
{
    public required string Identifier { get; init; }

    private bool Equals(Macro other)
    {
        return Identifier == other.Identifier;
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
        return Identifier.GetHashCode();
    }

    public override string ToString()
    {
        return $"{nameof(Identifier)}: {Identifier}";
    }
}