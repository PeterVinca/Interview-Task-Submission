using CustomerRequestProcessing.Core.CsvValidations;
using CustomerRequestProcessing.Core.Entities;
using CustomerRequestProcessing.Core;
using CustomerRequestProcessing.Services;
using NSubstitute;
using Shouldly;

namespace CustomerRequestProcessing.Tests.ServiceTests;

public class DataProcessorTest
{
    private readonly DataProcessor _dataProcessor;

    // Setup before each test
    public DataProcessorTest()
    {
        var logger = Substitute.For<ILogger>();
        _dataProcessor = new DataProcessor(48, 24, 12, logger);
    }


    [Fact]
    public void Given_ClassicMeterAndStandardSlaAndNoUnpaidInvoicesAndNoSmartMeterRequired_When_ProcessingData_Then_ReturnsApprovedWithoutFollowUp()
    {
        var requestedAt = "2025-10-26T02:30:00+02:00";
        var result = _dataProcessor.ProcessData(
            new List<ProcessedRequest>(),
            new FileReadResult<Tariff>(new List<Tariff> { new Tariff { TariffId = "T1", IsSmartMeterRequired = false } }, new List<ValidationError>()),
            new FileReadResult<Customer>(new List<Customer> { new Customer { CustomerId = "C1", MeterType = MeterType.Classic, HasUnpaidInvoice = false, SlaType = SlaType.Standard } }, new List<ValidationError>()),
            new FileReadResult<Request>(new List<Request> { new Request { RequestId = "R1", CustomerId = "C1", TargetTariffId = "T1", RequestedAt = requestedAt } }, new List<ValidationError>())
        );

        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        result[0].RequestId.ShouldBe("R1");
        result[0].RequestStatus.ShouldBe(RequestStatus.Approved);
        result[0].FollowUpAction.ShouldBe(Constants.ProcessedRequestMessages.NoData);
        result[0].DueDate.ShouldBe("2025-10-28T02:30:00+01:00");
        result[0].Reason.ShouldBe(Constants.ProcessedRequestMessages.NoData);
    }
}
