namespace CryonixVnFormat.types;

public class Macro : ICharacter
{
    public required string Identifier { get; init; }
    public CharacterProfile? Profile { get; init; }
}