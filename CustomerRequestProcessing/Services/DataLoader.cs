using CustomerRequestProcessing.Core;
using CustomerRequestProcessing.Core.CsvValidations;
using CustomerRequestProcessing.Core.Entities;

namespace CustomerRequestProcessing.Services;
internal class DataLoader
{
    private readonly Logger _logger;

    internal DataLoader(Logger logger)
    {
        _logger = logger;
    }

    internal (FileReadResult<Tariff>? tariffData, FileReadResult<Customer>? customerData, FileReadResult<Request>? requestData) LoadRequestData(string tariffFile, string customerFile, string requestFile)
    {
        _logger.Info($"Loading tariff data from {Path.GetFileName(tariffFile)} file");
        FileReadResult<Tariff> tariffFileReadResult = ReadRecords(new FileReader<Tariff, TariffMap>(new TariffValidator()), tariffFile);
        if (tariffFileReadResult == null)
        {
            return (null, null, null);
        }
        _logger.Info($"... loading completed");


        _logger.Info($"Loading customer data from {Path.GetFileName(customerFile)} file");
        FileReadResult<Customer> customerFileReadResult = ReadRecords(new FileReader<Customer, CustomerMap>(new CustomerValidator()), customerFile);
        if (customerFileReadResult == null)
        {
            return (null, null, null);
        }
        _logger.Info($"... loading completed");

        _logger.Info($"Loading request data from {Path.GetFileName(requestFile)} file");
        FileReadResult<Request> requestFileReadResult = ReadRecords(new FileReader<Request, RequestMap>(new RequestValidator()), requestFile);
        if (requestFileReadResult == null)
        {
            return (null, null, null);
        }
        _logger.Info($"... loading completed");

        return (tariffFileReadResult, customerFileReadResult, requestFileReadResult);
    }

    internal FileReadResult<ProcessedRequest> LoadProcessedRequestData(string processedRequestsFilePath)
    {
        FileReadResult<ProcessedRequest> processedRequestFileReadResult = 
            ReadRecords(new FileReader<ProcessedRequest, ProcessedRequestMap>(null), processedRequestsFilePath, shouldCreateIfNotExists: true);

        return processedRequestFileReadResult;
    }

    private FileReadResult<TEntity> ReadRecords<TEntity>(IFileReader<TEntity> fileReader, string filePath, bool shouldCreateIfNotExists = false) where TEntity : IEntity
    {
        FileReadResult<TEntity> result;

        try
        {
            result = fileReader.GetRecords(filePath, shouldCreateIfNotExists);
        }
        catch (FileNotFoundException ex)
        {
            _logger.Error($"File not found: {ex.Message}");
            return null;
        }
        catch (InvalidFileFormatException ex)
        {
            _logger.Error($"Invalid file format: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error($"An error occurred while reading the file: {ex.Message}");
            return null;
        }

        return result;
    }
}
