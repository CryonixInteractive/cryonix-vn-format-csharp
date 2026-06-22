namespace CryonixVnFormat.types;

public class SetMacroFileLine : IFileLine
{
    public required string MacroName { get; init; }
    public required CharacterProfile Profile { get; init; }

    private bool Equals(SetMacroFileLine other)
    {
        return MacroName == other.MacroName && Profile.Equals(other.Profile);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((SetMacroFileLine)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MacroName, Profile);
    }

    public override string ToString()
    {
        return $"{nameof(MacroName)}: {MacroName}, {nameof(Profile)}: {Profile}";
    }
}