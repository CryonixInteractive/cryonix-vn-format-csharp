namespace CryonixVnFormat.types;

public class CharacterProfile : ICharacter
{
    public required string Name { get; init; }
    public Dictionary<string, string> Properties { get; init; } = new();

    public static CharacterProfile FromText(string text)
    {
        string name;
        var properties = new Dictionary<string, string>();

        if (text.Contains(':'))
        {
            var i = text.IndexOf(':');
            name = text[..i];
            var propText = text[(i + 1)..].Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var property in propText)
            {
                var j = property.IndexOf('=');
                properties[property[..j]] = property[(j + 1)..];
            }
        }
        else
        {
            name = text;
        }

        return new CharacterProfile
        {
            Name = name,
            Properties = properties
        };
    }

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
        return $"{nameof(Name)}: {Name}, {nameof(Properties)}: {Properties}";
    }
}