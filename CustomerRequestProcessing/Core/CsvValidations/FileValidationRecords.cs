
namespace CustomerRequestProcessing.Core.CsvValidations;

public record ValidationError(int Row, string Message, string FileName);

public record FileReadResult<TEntity>(
    IReadOnlyCollection<TEntity> Records, 
    IReadOnlyCollection<ValidationError> Errors);

