
namespace CustomerRequestProcessing.Core.CsvValidations;

public sealed class InvalidFileFormatException : Exception
{
    public InvalidFileFormatException(string message) : base(message)
    {
    }
}
