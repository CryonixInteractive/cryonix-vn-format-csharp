namespace CryonixVnFormat.types;

public class PredefinedFunction : IFileLine
{
    public required string MethodName { get; init; }
    public List<string> Arguments { get; init; } = [];
}