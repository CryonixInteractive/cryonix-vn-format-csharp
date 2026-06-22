namespace CryonixVnFormat;

public class CvnParserException : ApplicationException
{
    public CvnParserException()
    {
    }

    public CvnParserException(string? message) : base(message)
    {
    }

    public CvnParserException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}