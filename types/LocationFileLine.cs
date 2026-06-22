namespace CryonixVnFormat.types;

public class LocationFileLine : IFileLine
{
    public required string Location { get; init; }
    public required string Displayable { get; init; }
}