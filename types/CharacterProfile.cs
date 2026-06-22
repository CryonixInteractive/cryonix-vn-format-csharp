namespace CryonixVnFormat.types;

public class CharacterProfile : ICharacter
{
    public string? Name;
    public Dictionary<string, string> Properties = new();

    public static CharacterProfile FromText(string text)
    {
        var profile = new CharacterProfile();

        if (text.Contains(':'))
        {
            var i = text.IndexOf(':');
            profile.Name = text[..i];
            var properties = text[(i + 1)..].Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var property in properties)
            {
                var j = property.IndexOf('=');
                profile.Properties[property[..j]] = property[(j + 1)..];
            }
        }
        else
        {
            profile.Name = text;
        }

        return profile;
    }
}