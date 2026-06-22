using System.Text.RegularExpressions;
using CryonixVnFormat.types;

namespace CryonixVnFormat;

public static partial class CvnReader
{
    // Note: we trim each line, so we can ignore leading and trailing whitespace
    // Definitions are still included for the sake of being thorough
    private const string NewlineChars = @"[\r\n\u2028\u2029\u000B\u000C\u0085]";
    private const string WhiteSpace = @"[ \t\f\uFEFF]";

    private const string NotNewlineChars = @"[^\r\n\u2028\u2029\u000B\u000C\u0085]";
    private const string NotWhitespace = @"[^ \t\f\r\n\u2028\u2029\u000B\u000C\u0085]";
    

    private const string TextFormat = $"{NotWhitespace}{NotNewlineChars}*";

    private const string Comment = $"//{NotNewlineChars}*";
    
    private const string Override = $"#[A-Z]+({WhiteSpace}*={WhiteSpace}*{NotWhitespace}+)?";

    private const string Macro = @"\$[A-Za-z]+";
    
    private const string CharName = @"[A-Za-z][A-Za-z0-9\-_ ]*";
    private const string CharProp = "(,?[A-Za-z0-9]+=[A-Za-z0-9]+)";
    
    private const string CharacterProfile = $"{CharName}(:{CharProp}+)?";
    private const string MacroDefinition = $"{Macro}{WhiteSpace}*={WhiteSpace}*{CharacterProfile}";
    private const string Dialogue = $@"({Macro}|{CharacterProfile}){WhiteSpace}*;{WhiteSpace}{TextFormat}";

    private const string Location = $@"@[A-Za-z0-9\-_]+{WhiteSpace}*<{WhiteSpace}*{TextFormat}";
    
    private const string EndOfLine = $@"({NewlineChars}|\r\n)";
    private const string LineHeader = $@"({WhiteSpace}*({Comment}|{Override})?{WhiteSpace}*(?:{EndOfLine}|$))";
    private const string LineContent =
        $@"({WhiteSpace}*({Comment}|{Dialogue}|{MacroDefinition}|{Location})?{WhiteSpace}*(?:{EndOfLine}|$))";

    private const string Line = $@"^{LineHeader}*{LineContent}*";

    public static CvnFile ReadStream(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return ReadStream(reader);
    }

    private enum ParserState
    {
        Header,
        Content
    }

    public static CvnFile ReadStream(StreamReader reader)
    {
        var state = ParserState.Header;
        var cvnFile = new CvnFile();

        var macros = new Dictionary<string, CharacterProfile>();

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            if (CommentGen().IsMatch(line)) continue;

            if (OverrideGen().IsMatch(line))
            {
                if (state == ParserState.Content)
                    throw new CvnParserException("Overrides must be before content block");

                if (line.Contains('='))
                {
                    var i = line.IndexOf('=');
                    var key = line[1..i].Trim();
                    var value = line[(i + 1)..].Trim();
                    cvnFile.Overrides[key] = value;
                }
                else
                {
                    var key = line[1..].Trim();
                    cvnFile.Overrides[key] = "TRUE";
                }

                continue;
            }

            if (MacroGen().IsMatch(line))
            {
                state = ParserState.Content;

                var i = line.IndexOf('=');
                var key = line[1..i].Trim();
                var value = line[(i + 1)..].Trim();

                var profile = types.CharacterProfile.FromText(value);
                cvnFile.Lines.Add(new SetMacroFileLine()
                {
                    MacroName = key,
                    Profile = profile
                });

                macros[key] = profile;
                
                continue;
            }

            if (DialogueGen().IsMatch(line))
            {
                state = ParserState.Content;

                var i = line.IndexOf(';');
                var key = line[..i].Trim();
                var dialogue = line[(i + 1)..].Trim();
                ICharacter character;

                if (key[0] == '$')
                {
                    var id = key[1..].Trim();
                    character = new Macro()
                    {
                        Identifier = key[1..].Trim(),
                        Profile = macros.GetValueOrDefault(id)
                    };
                }
                else
                {
                    character = types.CharacterProfile.FromText(key);
                }

                cvnFile.Lines.Add(new DialogueFileLine()
                {
                    Character = character,
                    Dialogue = dialogue
                });
                
                continue;
            }

            if (LocationGen().IsMatch(line))
            {
                state = ParserState.Content;

                var i = line.IndexOf('<');
                
                var location = line[1..i].Trim();
                var displayable = line[(i + 1)..].Trim();

                cvnFile.Lines.Add(new LocationFileLine()
                {
                    Location = location,
                    Displayable = displayable
                });
                
                continue;
            }

            throw new CvnParserException($"Line \"{line}\" did not match any known construct.");
        }

        return cvnFile;
    }

    [GeneratedRegex("^" + Override)]
    private static partial Regex OverrideGen();

    [GeneratedRegex("^" + Comment)]
    private static partial Regex CommentGen();

    [GeneratedRegex("^" + MacroDefinition)]
    private static partial Regex MacroGen();

    [GeneratedRegex("^" + Dialogue)]
    private static partial Regex DialogueGen();
    [GeneratedRegex(@"^@[A-Za-z0-9\-_]+[ \t\f\uFEFF]*<[ \t\f\uFEFF]*[^ \t\f\r\n\u2028\u2029\u000B\u000C\u0085][^\r\n\u2028\u2029\u000B\u000C\u0085]*")]
    private static partial Regex LocationGen();
}