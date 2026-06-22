namespace CryonixVnFormat.types;

public class DialogueFileLine : IFileLine
{
    public required ICharacter Character { get; init; }
    public required string Dialogue { get; init; }

    private bool Equals(DialogueFileLine other)
    {
        return Character.Equals(other.Character) && Dialogue == other.Dialogue;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((DialogueFileLine)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Character, Dialogue);
    }

    public override string ToString()
    {
        return $"{nameof(Character)}: {Character}, {nameof(Dialogue)}: {Dialogue}";
    }
}