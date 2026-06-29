namespace CryonixVnFormat.types;

public class CharacterProfile : ICharacter
{
    public required string Name { get; init; }
    public Dictionary<string, string> Properties { get; init; } = new();

    private bool Equals(CharacterProfile other)
    {
        return Name == other.Name && Properties.Equals(other.Properties);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((CharacterProfile)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Properties);
    }

    public override string ToString()
    {
        return Properties.Count == 0 
            ? $"{nameof(Name)}: {Name}" 
            : $"{nameof(Name)}: {Name}, {nameof(Properties)}: {string.Join(',', Properties)}";
    }
}