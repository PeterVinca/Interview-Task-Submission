using CustomerRequestProcessing.Core;
using CustomerRequestProcessing.Core.CsvValidations;
using CustomerRequestProcessing.Core.Entities;
using CustomerRequestProcessing.Services.Helpers;

namespace CustomerRequestProcessing.Services;

public class DataProcessor
{
    private readonly ILogger _logger;

    private readonly int _standardSlaTime; 
    private readonly int _premiumSlaTime; 
    private readonly int _followUpActionTime; 

    public DataProcessor(
        int standardSlaTime,
        int premiumSlaTime,
        int followUpActionTime,
        ILogger logger)
    {
        _standardSlaTime = standardSlaTime;
        _premiumSlaTime = premiumSlaTime;
        _followUpActionTime = followUpActionTime;
        _logger = logger;
    }

    public List<ProcessedRequest> ProcessData(
        List<ProcessedRequest> previouslyProcessedRequests,
        FileReadResult<Tariff> tariffData, 
        FileReadResult<Customer> customerData, 
        FileReadResult<Request> requestData)
    {
        _logger.Info("Starting data processing...");

        var newRequestsToProcess = requestData.Records.Where(r => !previouslyProcessedRequests.Any(p => p.RequestId == r.RequestId)).ToList();

         _logger.Info($"Found {newRequestsToProcess.Count} new requests to process.");

        var currentlyProcessedRequests = new List<ProcessedRequest>();

        foreach (var request in newRequestsToProcess)
        {
            var customer = customerData.Records.FirstOrDefault(c => !string.IsNullOrWhiteSpace(request.CustomerId) && c.CustomerId == request.CustomerId);
            var tariff = tariffData.Records.FirstOrDefault(t => !string.IsNullOrWhiteSpace(request.TargetTariffId) && t.TariffId == request?.TargetTariffId);

            var processedRequest = ProcessedRequest(request, customer, tariff);
            currentlyProcessedRequests.Add(processedRequest);
        }

        _logger.Info($"Data processing completed. Processed {currentlyProcessedRequests.Count} requests.");
        return currentlyProcessedRequests;
    }

    private ProcessedRequest ProcessedRequest(Request request, Customer customer, Tariff tariff)
    {
        var reasonMessages = new List<string>();

        if (customer == null)
        {
            reasonMessages.Add(Constants.Errors.UnknownCustomer);
        }

        if (tariff == null)
        {
            reasonMessages.Add(Constants.Errors.UnknownTariff);
        }

        if (IsRequestInvalid(request))
        {
            reasonMessages.Add(Constants.Errors.InvalidRequestData);
        }

        if (customer != null && IsCustomerInvalid(customer))
        {
            reasonMessages.Add(Constants.Errors.InvalidCustomerData);
        }

        if (tariff != null && IsTariffInvalid(tariff))
        {
            reasonMessages.Add(Constants.Errors.InvalidTariffData);
        }

        if (customer != null && customer.HasUnpaidInvoice == true)
        {
            reasonMessages.Add(Constants.Errors.UnpaidInvoice);
        }

        if (reasonMessages.Count > 0)
            return new ProcessedRequest
            {
                RequestId = request.RequestId,
                RequestStatus = RequestStatus.Rejected,
                Reason = string.Join(Constants.DelimiterForJoiningInCsvFiles, reasonMessages),
                FollowUpAction = Constants.ProcessedRequestMessages.NoData,
                DueDate = Constants.ProcessedRequestMessages.NoData
            };

        return GetApprovedRequestRecord(request, customer!, tariff!);
    }

    private ProcessedRequest GetApprovedRequestRecord(Request request, Customer customer, Tariff tariff)
    {
        var requiresUpgrade = 
            customer.MeterType == MeterType.Classic 
            && tariff.IsSmartMeterRequired == true;

        var slaTime = customer.SlaType == SlaType.Standard 
            ? _standardSlaTime 
            : _premiumSlaTime;

        var slaDueTime = requiresUpgrade 
            ? slaTime + _followUpActionTime 
            : slaTime;

        return new ProcessedRequest
        {
            RequestId = request.RequestId,
            DueDate = DateTimeHelper.CalculateDueTime(request.RequestedAt!, slaDueTime),
            RequestStatus = RequestStatus.Approved,
            Reason = Constants.ProcessedRequestMessages.NoData,
            FollowUpAction = requiresUpgrade
                ? Constants.ProcessedRequestMessages.ScheduleMeterUpgrade
                : Constants.ProcessedRequestMessages.NoData
        };
    }

    private bool IsRequestInvalid(Request request)
    {
        return string.IsNullOrWhiteSpace(request.CustomerId) 
            || string.IsNullOrWhiteSpace(request.TargetTariffId)
            || string.IsNullOrWhiteSpace(request.RequestedAt)
            || !DateTimeOffset.TryParse(request.RequestedAt, out _);
    }

    private bool IsCustomerInvalid(Customer customer)
    {
        return string.IsNullOrWhiteSpace(customer.CustomerId)
            || !customer.HasUnpaidInvoice.HasValue
            || !customer.SlaType.HasValue
            || !customer.MeterType.HasValue;
    }

    private bool IsTariffInvalid(Tariff tariff)
    {
        return string.IsNullOrWhiteSpace(tariff.TariffId)
            || !tariff.IsSmartMeterRequired.HasValue;
    }
}

