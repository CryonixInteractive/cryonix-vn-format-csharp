namespace CryonixVnFormat.types;

public class GoToFileLine : IFileLine
{
    public required string FileName { get; init; }

    private bool Equals(GoToFileLine other)
    {
        return FileName == other.FileName;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((GoToFileLine)obj);
    }

    public override int GetHashCode()
    {
        return FileName.GetHashCode();
    }

    public override string ToString()
    {
        return $"{nameof(FileName)}: {FileName}";
    }
}