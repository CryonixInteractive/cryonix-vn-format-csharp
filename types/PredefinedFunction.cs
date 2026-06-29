namespace CryonixVnFormat.types;

public class PredefinedFunction : IFileLine
{
    public required string MethodName { get; init; }
    public List<string> Arguments { get; init; } = [];

    private bool Equals(PredefinedFunction other)
    {
        return MethodName == other.MethodName && Arguments.Equals(other.Arguments);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((PredefinedFunction)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MethodName, Arguments);
    }

    public override string ToString()
    {
        return Arguments.Count == 0
            ? $"{nameof(MethodName)}: {MethodName}"
            : $"{nameof(MethodName)}: {MethodName}, {nameof(Arguments)}: {Arguments}";
    }
}