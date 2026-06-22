namespace CryonixVnFormat.types;

public class Macro : ICharacter
{
    public required string Identifier { get; init; }
    public CharacterProfile? Profile { get; init; }

    private bool Equals(Macro other)
    {
        return Identifier == other.Identifier && Equals(Profile, other.Profile);
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
        return HashCode.Combine(Identifier, Profile);
    }

    public override string ToString()
    {
        return $"{nameof(Identifier)}: {Identifier}, {nameof(Profile)}: {Profile}";
    }
}