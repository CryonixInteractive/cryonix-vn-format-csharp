using System.Collections;
using CryonixVnFormat.types;

namespace CryonixVnFormat;

public class CvnFile : IEnumerable<IFileLine>
{
    public Dictionary<string, string> Overrides = new();
    public readonly List<IFileLine> Lines = [];
    
    public IEnumerator<IFileLine> GetEnumerator()
    {
        return Lines.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}