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
    private const string _noData = "NoData";
    private const string _followUpAction = "Schedule meter upgrade";

    // Setup before each test
    public DataProcessorTest()
    {
        var logger = Substitute.For<ILogger>();
        _dataProcessor = new DataProcessor(48, 24, 12, logger);
    }

    [Fact]
    public void Given_RequestIdsAreAlreadyProcessed_When_ProcessingData_Then_ReturnsEmptyResult()
    {
        // Arrange
        var previouslyProcessedRequests = new List<ProcessedRequest>
        {
            new ProcessedRequest { RequestId = "R1", RequestStatus = RequestStatus.Approved, DueDate = "2025-10-28T02:30:00+01:00" },
            new ProcessedRequest { RequestId = "R2", RequestStatus = RequestStatus.Approved, DueDate = "2025-11-22T11:30:00+01:00" },
            new ProcessedRequest { RequestId = "R3", RequestStatus = RequestStatus.Rejected, DueDate = Constants.ProcessedRequestMessages.NoData }
        };

        // Act
        var result = _dataProcessor.ProcessData(
            previouslyProcessedRequests,
            new FileReadResult<Tariff>(new List<Tariff> { new Tariff { TariffId = "T1", IsSmartMeterRequired = true } }, new List<ValidationError>()),
            new FileReadResult<Customer>(new List<Customer> { new Customer { CustomerId = "C1", MeterType = MeterType.Classic, HasUnpaidInvoice = false, SlaType = SlaType.Premium } }, new List<ValidationError>()),
            new FileReadResult<Request>(new List<Request> { new Request { RequestId = "R2", CustomerId = "C1", TargetTariffId = "T1", RequestedAt = "2025-10-25T02:30:00+02:00" } }, new List<ValidationError>())
        );

        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(MeterType.Classic, false, SlaType.Standard, "2025-10-26T02:30:00+02:00", "2025-10-28T02:30:00+01:00", _noData, _noData)]
    [InlineData(MeterType.Classic, false, SlaType.Premium, "2025-10-25T17:30:00+02:00", "2025-10-26T17:30:00+01:00", _noData, _noData)]
    [InlineData(MeterType.Classic, true, SlaType.Standard, "2025-10-26T02:30:00+02:00", "2025-10-28T14:30:00+01:00", _noData, _followUpAction)]
    [InlineData(MeterType.Classic, true, SlaType.Premium, "2025-10-25T17:30:00+02:00", "2025-10-27T05:30:00+01:00", _noData, _followUpAction)]
    [InlineData(MeterType.Smart, true, SlaType.Standard, "2025-10-26T02:30:00+02:00", "2025-10-28T02:30:00+01:00", _noData, _noData)]
    [InlineData(MeterType.Smart, true, SlaType.Premium, "2025-10-25T17:30:00+02:00", "2025-10-26T17:30:00+01:00", _noData, _noData)]
    [InlineData(MeterType.Smart, false, SlaType.Standard, "2025-10-26T02:30:00+02:00", "2025-10-28T02:30:00+01:00", _noData, _noData)]
    [InlineData(MeterType.Smart, false, SlaType.Premium, "2025-10-25T17:30:00+02:00", "2025-10-26T17:30:00+01:00", _noData, _noData)]
    public void Given_CustomerNoUnpaidInvoices_When_ProcessingData_Then_ReturnsApprovedResultWithExpectedDueDate(
        MeterType meterType, 
        bool isSmartMeterRequired, 
        SlaType slaType, 
        string requestedAt, 
        string expectedDueDate,
        string expectedReason,
        string expectedFollowUpAction)
    {
        // Arrange & Act
        var result = _dataProcessor.ProcessData(
            new List<ProcessedRequest>(),
            new FileReadResult<Tariff>(new List<Tariff> { new Tariff { TariffId = "T1", IsSmartMeterRequired = isSmartMeterRequired } }, new List<ValidationError>()),
            new FileReadResult<Customer>(new List<Customer> { new Customer { CustomerId = "C1", MeterType = meterType, HasUnpaidInvoice = false, SlaType = slaType } }, new List<ValidationError>()),
            new FileReadResult<Request>(new List<Request> { new Request { RequestId = "R1", CustomerId = "C1", TargetTariffId = "T1", RequestedAt = requestedAt } }, new List<ValidationError>())
        );

        //Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        result[0].RequestId.ShouldBe("R1");
        result[0].RequestStatus.ShouldBe(RequestStatus.Approved);
        result[0].FollowUpAction.ShouldBe(expectedFollowUpAction);
        result[0].DueDate.ShouldBe(expectedDueDate);
        result[0].Reason.ShouldBe(expectedReason);
    }

    [Fact]
    public void Given_InvalidCustomerData_When_ProcessingData_Then_ReturnsRejectedResult()
    {
        // Arrange & Act
        var result = _dataProcessor.ProcessData(
            new List<ProcessedRequest>(),
            new FileReadResult<Tariff>(new List<Tariff> { new Tariff { TariffId = "T1", IsSmartMeterRequired = false } }, new List<ValidationError>()),
            new FileReadResult<Customer>(new List<Customer> { new Customer { CustomerId = "C1", MeterType = MeterType.Classic, SlaType = SlaType.Standard } }, new List<ValidationError>()),
            new FileReadResult<Request>(new List<Request> { new Request { RequestId = "R1", CustomerId = "C1", TargetTariffId = "T1", RequestedAt = "2025-10-26T02:30:00+02:00" } }, new List<ValidationError>())
        );

        //Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        result[0].RequestId.ShouldBe("R1");
        result[0].RequestStatus.ShouldBe(RequestStatus.Rejected);
        result[0].FollowUpAction.ShouldBe(Constants.ProcessedRequestMessages.NoData);
        result[0].DueDate.ShouldBe(Constants.ProcessedRequestMessages.NoData);
        result[0].Reason.ShouldBe(Constants.Errors.InvalidCustomerData);
    }

    [Fact]
    public void Given_CustomerWithUnpaidInvoice_When_ProcessingData_Then_ReturnsRejectedResult()
    {
        // Arrange & Act
        var result = _dataProcessor.ProcessData(
            new List<ProcessedRequest>(),
            new FileReadResult<Tariff>(new List<Tariff> { new Tariff { TariffId = "T1", IsSmartMeterRequired = false } }, new List<ValidationError>()),
            new FileReadResult<Customer>(new List<Customer> { new Customer { CustomerId = "C1", MeterType = MeterType.Classic, HasUnpaidInvoice = true, SlaType = SlaType.Standard } }, new List<ValidationError>()),
            new FileReadResult<Request>(new List<Request> { new Request { RequestId = "R1", CustomerId = "C1", TargetTariffId = "T1", RequestedAt = "2025-10-26T02:30:00+02:00" } }, new List<ValidationError>())
        );

        //Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        result[0].RequestId.ShouldBe("R1");
        result[0].RequestStatus.ShouldBe(RequestStatus.Rejected);
        result[0].FollowUpAction.ShouldBe(Constants.ProcessedRequestMessages.NoData);
        result[0].DueDate.ShouldBe(Constants.ProcessedRequestMessages.NoData);
        result[0].Reason.ShouldBe(Constants.Errors.UnpaidInvoice);
    }

    [Fact]
    public void Given_InvalidTariffData_When_ProcessingData_Then_ReturnsRejectedResult()
    {
        // Arrange & Act
        var result = _dataProcessor.ProcessData(
            new List<ProcessedRequest>(),
            new FileReadResult<Tariff>(new List<Tariff> { new Tariff { TariffId = "T1" } }, new List<ValidationError>()),
            new FileReadResult<Customer>(new List<Customer> { new Customer { CustomerId = "C1", MeterType = MeterType.Classic, HasUnpaidInvoice = false, SlaType = SlaType.Standard } }, new List<ValidationError>()),
            new FileReadResult<Request>(new List<Request> { new Request { RequestId = "R1", CustomerId = "C1", TargetTariffId = "T1", RequestedAt = "2025-10-26T02:30:00+02:00" } }, new List<ValidationError>())
        );

        //Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        result[0].RequestId.ShouldBe("R1");
        result[0].RequestStatus.ShouldBe(RequestStatus.Rejected);
        result[0].FollowUpAction.ShouldBe(Constants.ProcessedRequestMessages.NoData);
        result[0].DueDate.ShouldBe(Constants.ProcessedRequestMessages.NoData);
        result[0].Reason.ShouldBe(Constants.Errors.InvalidTariffData);
    }

    [Fact]
    public void Given_InvalidRequestData_When_ProcessingData_Then_ReturnsRejectedResult()
    {
        // Arrange & Act
        // TimeStamp is invalid
        var result = _dataProcessor.ProcessData(
            new List<ProcessedRequest>(),
            new FileReadResult<Tariff>(new List<Tariff> { new Tariff { TariffId = "T1", IsSmartMeterRequired = false } }, new List<ValidationError>()),
            new FileReadResult<Customer>(new List<Customer> { new Customer { CustomerId = "C1", MeterType = MeterType.Classic, HasUnpaidInvoice = false, SlaType = SlaType.Standard } }, new List<ValidationError>()),
            new FileReadResult<Request>(new List<Request> { new Request { RequestId = "R1", CustomerId = "C1", TargetTariffId = "T1", RequestedAt = "2025-10-2602:30:00+02:00" } }, new List<ValidationError>())
        );

        //Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        result[0].RequestId.ShouldBe("R1");
        result[0].RequestStatus.ShouldBe(RequestStatus.Rejected);
        result[0].FollowUpAction.ShouldBe(Constants.ProcessedRequestMessages.NoData);
        result[0].DueDate.ShouldBe(Constants.ProcessedRequestMessages.NoData);
        result[0].Reason.ShouldBe(Constants.Errors.InvalidRequestData);
    }

    [Fact]
    public void Given_MultipleInvalidData_When_ProcessingData_Then_ReturnsRejectedResult()
    {
        // Arrange & Act
        var result = _dataProcessor.ProcessData(
            new List<ProcessedRequest>(),
            new FileReadResult<Tariff>(new List<Tariff> { new Tariff { TariffId = "T1" } }, new List<ValidationError>()),
            new FileReadResult<Customer>(new List<Customer> { new Customer { CustomerId = "C1", MeterType = MeterType.Classic, HasUnpaidInvoice = false } }, new List<ValidationError>()),
            new FileReadResult<Request>(new List<Request> { new Request { RequestId = "R1", CustomerId = "C1", TargetTariffId = "T1", RequestedAt = "2025-10-2602:30:00+02:00" } }, new List<ValidationError>())
        );

        //Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        result[0].RequestId.ShouldBe("R1");
        result[0].RequestStatus.ShouldBe(RequestStatus.Rejected);
        result[0].FollowUpAction.ShouldBe(Constants.ProcessedRequestMessages.NoData);
        result[0].DueDate.ShouldBe(Constants.ProcessedRequestMessages.NoData);

        result[0].Reason!.ShouldContain(Constants.Errors.InvalidCustomerData);
        result[0].Reason!.ShouldContain(Constants.Errors.InvalidTariffData);
        result[0].Reason!.ShouldContain(Constants.Errors.InvalidRequestData);
    }

    [Fact]
    public void Given_RequestHasUnknownCustomerIdAndTariffId_When_ProcessingData_Then_ReturnsRejectedResult()
    {
        // Arrange & Act
        var result = _dataProcessor.ProcessData(
            new List<ProcessedRequest>(),
            new FileReadResult<Tariff>(new List<Tariff> { new Tariff { TariffId = "T01" } }, new List<ValidationError>()),
            new FileReadResult<Customer>(new List<Customer> { new Customer { CustomerId = "C01", MeterType = MeterType.Classic, HasUnpaidInvoice = false } }, new List<ValidationError>()),
            new FileReadResult<Request>(new List<Request> { new Request { RequestId = "R1", CustomerId = "C1", TargetTariffId = "T1", RequestedAt = "2025-10-2602:30:00+02:00" } }, new List<ValidationError>())
        );

        //Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        result[0].RequestId.ShouldBe("R1");
        result[0].RequestStatus.ShouldBe(RequestStatus.Rejected);
        result[0].FollowUpAction.ShouldBe(Constants.ProcessedRequestMessages.NoData);
        result[0].DueDate.ShouldBe(Constants.ProcessedRequestMessages.NoData);

        result[0].Reason!.ShouldContain(Constants.Errors.UnknownCustomer);
        result[0].Reason!.ShouldContain(Constants.Errors.UnknownTariff);
    }
}
