namespace CryonixVnFormat.types;

public class SetMacroFileLine : IFileLine
{
    public required string MacroName { get; init; }
    public required CharacterProfile Profile { get; init; }
}