using System.Text.RegularExpressions;
using CryonixVnFormat.types;

namespace CryonixVnFormat;

public static partial class CvnReader
{
    #region Regex definitions

    // Note: we trim each line, so we can ignore leading and trailing whitespace
    // Definitions are still included for the sake of being thorough
    private const string NewlineChars = @"[\r\n\u2028\u2029\u000B\u000C\u0085]";
    private const string WhiteSpace = @"[ \t\f\uFEFF]";

    private const string NotNewlineChars = @"[^\r\n\u2028\u2029\u000B\u000C\u0085]";
    private const string NotWhitespace = @"[^ \t\f\r\n\u2028\u2029\u000B\u000C\u0085]";

    private const string DelimitedAlphanumeric = @"[A-Za-z0-9\-_]";

    private const string TextFormat = $"{NotWhitespace}{NotNewlineChars}*";

    private const string Comment = $"//{NotNewlineChars}*";

    private const string Include = $@"&{DelimitedAlphanumeric}+";

    private const string Override = $"#[A-Z_]+(?:{WhiteSpace}*={WhiteSpace}*{NotWhitespace}+)?";

    private const string Macro = @"\$[A-Za-z]+";

    private const string CharName = @"[A-Za-z][A-Za-z0-9\-_ ]*";
    private const string CharProp = $"(?:,?{DelimitedAlphanumeric}+={DelimitedAlphanumeric}+)";

    private const string CharacterProfile = $"{CharName}(:{CharProp}+)?";
    private const string MacroDefinition = $"{Macro}{WhiteSpace}*={WhiteSpace}*{CharacterProfile}";

    private const string Dialogue =
        $@"(?:{Macro}(:{CharProp}+)?|{CharacterProfile}){WhiteSpace}*;{WhiteSpace}*{TextFormat}";

    private const string Location = $@"@{DelimitedAlphanumeric}+{WhiteSpace}*<{WhiteSpace}*{TextFormat}";

    private const string GoToFile = $@":{DelimitedAlphanumeric}+";

    private const string EngineArgument = $@"(?:{WhiteSpace}*{DelimitedAlphanumeric}+)";
    private const string EngineArguments = $@"{EngineArgument}+{WhiteSpace}*";
    private const string EngineCommand = $@"\+[A-Za-z\-_]+{WhiteSpace}*(?:{EngineArguments}|\[{EngineArguments}\])?";


    private const string EndOfLine = $@"(?:{NewlineChars}|\r\n)";

    private const string LineInclude = $@"(?:{WhiteSpace}*({Comment}|{Include})?{WhiteSpace}*(?:{EndOfLine}|$))";
    private const string LineHeader = $@"(?:{WhiteSpace}*({Comment}|{Override})?{WhiteSpace}*(?:{EndOfLine}|$))";

    private const string LineContent =
        $@"(?:{WhiteSpace}*({Comment}|{Dialogue}|{MacroDefinition}|{Location}|{EngineCommand})?{WhiteSpace}*(?:{EndOfLine}|$))";

    private const string LineGoTo = $@"(?:{WhiteSpace}*{GoToFile}{WhiteSpace}*(?:{EndOfLine}|$))";

    /// <summary>
    /// The master regex which determines if a cvn file is valid.
    /// 
    /// Not used for anything, but I think it's really cool to see
    /// once each variable is recursively resolved to its value,
    /// since pasting a file into regex101 will immediately indicate its validity.
    /// </summary>
    private const string File = $@"^{LineInclude}*{LineHeader}*{LineContent}*{LineGoTo}?";

    #endregion // Regex definitions

    /// <summary>
    /// Returns a cvn(h) file from the engine backing store. Required to use include directives in files, as includes are preprocessed in the interpreter.
    /// The interpreter will ask for a stream using either the .cvn or .cvnh suffix depending on the file requested.
    /// Streams sent to CvnReader through this delegate will be closed automatically.
    /// </summary>
    public delegate Stream GetCvnStream(string resourceName);

    private enum ParserState
    {
        Include,
        Header,
        Content
    }

    #region ReadStream
    public static CvnFile ReadStream(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return ReadStream(reader, null);
    }

    public static CvnFile ReadStream(Stream stream, GetCvnStream? cvnStreamDelegate)
    {
        using var reader = new StreamReader(stream);
        return ReadStream(reader, cvnStreamDelegate);
    }

    public static CvnFile ReadStream(StreamReader reader)
    {
        return ReadStream(reader, null);
    }

    public static CvnFile ReadStream(StreamReader reader, GetCvnStream? cvnStreamDelegate)
    {
        var state = ParserState.Include;
        var cvnFile = new CvnFile();

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(line)) continue;

            if (CommentGen().IsMatch(line)) continue;

            if (IncludeGen().IsMatch(line))
            {
                if (state != ParserState.Include)
                    throw new CvnParserException("Includes must be added before override or content block");

                var fileName = line[1..].Trim();

                if (cvnStreamDelegate == null)
                    throw new CvnParserException("Cannot read included file, no resource loader provided");

                var stream = cvnStreamDelegate(fileName + ".cvnh");
                var cvnh = ReadHeader(stream, cvnStreamDelegate);

                foreach (var o in cvnh.Overrides)
                {
                    cvnFile.Overrides[o.Key] = o.Value;
                }

                foreach (var cvnhLine in cvnh.Lines)
                {
                    cvnFile.Lines.Add(cvnhLine);
                }

                stream.Close();

                continue;
            }

            if (OverrideGen().IsMatch(line))
            {
                if (state == ParserState.Include) state = ParserState.Header;
                if (state != ParserState.Header)
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

            var parseContent = ParseLine(line);

            if (parseContent == null)
                throw new CvnParserException($"Line \"{line}\" did not match any known construct.");

            cvnFile.Lines.Add(parseContent);
            state = ParserState.Content;
        }

        return cvnFile;
    }
    #endregion ReadStream
    
    #region ReadHeader
        public static CvnhFile ReadHeader(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return ReadHeader(reader, null);
        }
    
        public static CvnhFile ReadHeader(Stream stream, GetCvnStream? cvnStreamDelegate)
        {
            using var reader = new StreamReader(stream);
            return ReadHeader(reader, cvnStreamDelegate);
        }
    
        public static CvnhFile ReadHeader(StreamReader reader)
        {
            return ReadHeader(reader, null);
        }
    
        public static CvnhFile ReadHeader(StreamReader reader, GetCvnStream? cvnStreamDelegate)
        {
            var state = ParserState.Include;
            var cvnhFile = new CvnhFile();
    
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine()?.Trim();
    
                if (string.IsNullOrEmpty(line)) continue;
    
                if (CommentGen().IsMatch(line)) continue;
    
                if (IncludeGen().IsMatch(line))
                {
                    if (state != ParserState.Include)
                        throw new CvnParserException("Includes must be added before override or content block");
    
                    var fileName = line[1..].Trim();
    
                    if (cvnStreamDelegate == null)
                        throw new CvnParserException("Cannot read included file, no resource loader provided");
    
                    var stream = cvnStreamDelegate(fileName + ".cvnh");
                    var cvnh = ReadHeader(stream);
    
                    foreach (var o in cvnh.Overrides)
                    {
                        cvnhFile.Overrides[o.Key] = o.Value;
                    }
    
                    foreach (var cvnhLine in cvnh.Lines)
                    {
                        cvnhFile.Lines.Add(cvnhLine);
                    }
    
                    stream.Close();
    
                    continue;
                }
    
                if (OverrideGen().IsMatch(line))
                {
                    if (state == ParserState.Include) state = ParserState.Header;
                    if (state != ParserState.Header)
                        throw new CvnParserException("Overrides must be before content block");
    
                    if (line.Contains('='))
                    {
                        var i = line.IndexOf('=');
                        var key = line[1..i].Trim();
                        var value = line[(i + 1)..].Trim();
                        cvnhFile.Overrides[key] = value;
                    }
                    else
                    {
                        var key = line[1..].Trim();
                        cvnhFile.Overrides[key] = "TRUE";
                    }
    
                    continue;
                }
    
                var parseContent = ParseLine(line);
    
                switch (parseContent)
                {
                    case null:
                        throw new CvnParserException($"Line \"{line}\" did not match any known construct.");
                    case SetMacroFileLine macro:
                        cvnhFile.Lines.Add(macro);
                        break;
                    default:
                        throw new CvnParserException(
                            "Only includes, overrides, and macro definitions are allowed in cvn header files.");
                }
    
                state = ParserState.Content;
            }
    
            return cvnhFile;
        }
        #endregion ReadHeader

    public static IFileLine? ParseLine(string? line)
    {
        if (string.IsNullOrEmpty(line)) return null;

        if (CommentGen().IsMatch(line)) return null;

        if (MacroGen().IsMatch(line))
        {
            var i = line.IndexOf('=');
            var key = line[1..i].Trim();
            var value = line[(i + 1)..].Trim();

            CharacterProfile profile;
            if (value.Contains(':'))
            {
                var j = value.IndexOf(':');
                var name = value[..j].Trim();
                var properties = GetProperties(value[(j + 1)..].Trim());
                
                profile = new CharacterProfile
                {
                    Name = name,
                    Properties = properties
                };
            }
            else
            {
                profile = new CharacterProfile
                {
                    Name = value,
                    Properties = new Dictionary<string, string>()
                };
            }
            return new SetMacroFileLine
            {
                MacroName = key,
                Profile = profile
            };
        }

        if (DialogueGen().IsMatch(line))
        {
            var i = line.IndexOf(';');
            var key = line[..i].Trim();
            var dialogue = line[(i + 1)..].Trim();

            var name = "";
            var properties = new Dictionary<string, string>();
            
            if (key.Contains(':'))
            {
                var j = key.IndexOf(':');
                name = key[..j].Trim();
                properties = GetProperties(key[(j + 1)..].Trim());
            }
            else name = key;

            ICharacter character;
            if (name[0] == '$')
            {
                var id = name[1..].Trim();
                character = new Macro()
                {
                    Identifier = id,
                    PropertyOverrides = properties
                };
            }
            else
            {
                character = new CharacterProfile()
                {
                    Name = name,
                    Properties = properties
                };
            }

            return new DialogueFileLine()
            {
                Character = character,
                Dialogue = dialogue
            };
        }

        if (LocationGen().IsMatch(line))
        {
            var i = line.IndexOf('<');

            var location = line[1..i].Trim();
            var displayable = line[(i + 1)..].Trim();

            return new LocationFileLine()
            {
                Location = location,
                Displayable = displayable
            };
        }

        if (EngineCommandGen().IsMatch(line))
        {
            const StringSplitOptions stringSplitOptions =
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
            // Split based on all valid whitespace chars, just in case
            var split = line.Split([' ', '\t', '\f', '\uFEFF'], stringSplitOptions);

            var name = split[0];
            var args = new List<string>();

            if (split.Length <= 1)
                return new PredefinedFunction
                {
                    MethodName = name,
                    Arguments = args
                };

            if (split[1] == "[")
            {
                if (split[^1] != "]") throw new CvnParserException("Argument list was not closed with ]");
                args = split[2..^2].ToList();
            }
            else
            {
                args = split[1..].ToList();
            }

            return new PredefinedFunction
            {
                MethodName = name[1..],
                Arguments = args
            };
        }

        if (GoToFileGen().IsMatch(line))
        {
            return new GoToFileLine
            {
                FileName = line[1..].Trim() + ".cvn",
            };
        }

        throw new CvnParserException($"Line \"{line}\" did not match any known construct.");
    }

    public static Dictionary<string, string> GetProperties(string text)
    {
        var properties = new Dictionary<string, string>();
        
        var propText = text.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var property in propText)
        {
            var j = property.IndexOf('=');
            properties[property[..j]] = property[(j + 1)..];
        }

        return properties;
    }

    [GeneratedRegex("^" + Comment)]
    private static partial Regex CommentGen();

    [GeneratedRegex("^" + Include)]
    private static partial Regex IncludeGen();

    [GeneratedRegex("^" + Override)]
    private static partial Regex OverrideGen();

    [GeneratedRegex("^" + MacroDefinition)]
    private static partial Regex MacroGen();

    [GeneratedRegex("^" + Dialogue)]
    private static partial Regex DialogueGen();

    [GeneratedRegex("^" + Location)]
    private static partial Regex LocationGen();

    [GeneratedRegex("^" + GoToFile)]
    private static partial Regex GoToFileGen();

    [GeneratedRegex("^" + EngineCommand)]
    private static partial Regex EngineCommandGen();
}