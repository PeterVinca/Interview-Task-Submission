using CustomerRequestProcessing.Core;
using CustomerRequestProcessing.Core.CsvValidations;
using CustomerRequestProcessing.Core.Entities;
using CustomerRequestProcessing.Services;
using Microsoft.Extensions.Configuration;

namespace CustomerRequestProcessing;

internal class Application
{
    private readonly IConfiguration _configuration;
    private readonly Logger _logger;

    internal Application(IConfiguration configuration, Logger logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    internal void Run()
    {
        var timeAtStart = DateTime.Now;
        _logger.Info($"{Environment.NewLine}=========================NEW RUN {timeAtStart.ToString(Constants.DateTimeFormat)}=========================", onlyToFile: true, includeTimestampAndLevel: false);
        _logger.Info($"Starting application at {timeAtStart.ToString(Constants.DateTimeFormat)}");

        // Access input files configuration values
        if (!GetConfigurationValue(Constants.Configurations.CustomerInputFile, out var customerFile))
            return;

        if (!GetConfigurationValue(Constants.Configurations.RequestInputFile, out var requestFile))
            return;

        if (!GetConfigurationValue(Constants.Configurations.TariffInputFile, out var tariffFile))
            return;

        _logger.Info($"Customer File: {customerFile}");
        _logger.Info($"Request File: {requestFile}");
        _logger.Info($"Tariff File: {tariffFile}");

        // Read and validate the files and data
        var (tariffData, customerData, requestData) = new DataLoader(_logger).LoadRequestData(tariffFile, customerFile, requestFile);
        if (tariffData == null || customerData == null || requestData == null)
            return;

        var fileWriter = new FileWriter(_logger);

        // Write corrupted records to an output file
        var errorFileName = WriteErrorFile(fileWriter, tariffData.Errors, customerData.Errors, requestData.Errors, timeAtStart);

        // Process the data
        if (!GetConfigurationValue(Constants.Configurations.ProcessedRequestsFile, out var processedRequestsFilePath))
            return;

        var previouslyProcessedRequests = new DataLoader(_logger).LoadProcessedRequestData(processedRequestsFilePath);

        GetConfigurationIntValue(Constants.Configurations.StandardSla, out var standardSlaTime);
        GetConfigurationIntValue(Constants.Configurations.PremiumSla, out var premiumSlaTime);
        GetConfigurationIntValue(Constants.Configurations.FollowUpActionTime, out var followUpActionTime);

        var dataProcessor = new DataProcessor(
            standardSlaTime,
            premiumSlaTime, 
            followUpActionTime,
            _logger);

        var newlyProcessedRequests = dataProcessor.ProcessData(previouslyProcessedRequests.Records.ToList(), tariffData, customerData, requestData);

        // Update the processed requests file
        fileWriter.WriteToFile<ProcessedRequest, ProcessedRequestMap>(processedRequestsFilePath, newlyProcessedRequests);

        _logger.Info($"Processing data finished..."); 
        if (!string.IsNullOrWhiteSpace(errorFileName))
            _logger.Info($"Check the errors in error file generated in: {errorFileName}");

        _logger.Info($"Application finished at {DateTime.Now.ToString(Constants.DateTimeFormat)}");
    }

    private string WriteErrorFile(
        FileWriter fileWriter,
        IEnumerable<ValidationError> tariffErrors, 
        IEnumerable<ValidationError> customerErrors, 
        IEnumerable<ValidationError> requestErrors, 
        DateTime timeAtStart)
    {
        if (!tariffErrors.Any() && !customerErrors.Any() && !requestErrors.Any())
            return string.Empty;

        _logger.Warn($"Found some errors when reading the input files");

        GetConfigurationValue(Constants.Configurations.OutputErrorFilePath, out var outputErrorFilePath, Constants.DefaultErrorFilePath);

        var fileName = $"{outputErrorFilePath}ErrorFile_{timeAtStart.ToString(Constants.DateTimeFormatForFile)}.csv";

        var allErrors = new List<ValidationError>();
        allErrors.AddRange(tariffErrors);
        allErrors.AddRange(customerErrors);
        allErrors.AddRange(requestErrors);

        fileWriter.WriteToErrorFile(fileName, allErrors);

        return fileName;
    }

    private bool GetConfigurationValue(string key, out string value, string defaultValue = "")
    {
        value = _configuration[key]!;
        if (string.IsNullOrWhiteSpace(value))
        {
            value = defaultValue;
            var fileName = key.Split(':').Last();
            _logger.Error($"{fileName} path is not configured.");
            return false;
        }
        return true;
    }

    private void GetConfigurationIntValue(string key, out int value)
    {
        var valueString = _configuration[key]!;

        if (int.TryParse(valueString, out int parsedValue) || parsedValue < 0)
        {
            value = parsedValue;
        }
        else
        {
            value = -1;
        }
    }
}
