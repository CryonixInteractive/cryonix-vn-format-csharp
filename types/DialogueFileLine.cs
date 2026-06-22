namespace CryonixVnFormat.types;

public class DialogueFileLine : IFileLine
{
    public required ICharacter Character { get; init; }
    public required string Dialogue { get; init; }
}