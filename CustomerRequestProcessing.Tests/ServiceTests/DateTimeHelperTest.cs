using CustomerRequestProcessing.Core.CsvValidations;
using CustomerRequestProcessing.Core.Entities;
using CustomerRequestProcessing.Core;
using CustomerRequestProcessing.Services;
using NSubstitute;
using Shouldly;
using CustomerRequestProcessing.Services.Helpers;

namespace CustomerRequestProcessing.Tests.ServiceTests;

public class DateTimeHelperTest
{
    public DateTimeHelperTest()
    {
    }

    [Theory]
    [InlineData("2025-03-30T01:15:00+01:00", 24, "2025-03-31T01:15:00+02:00")]
    [InlineData("2025-10-26T02:30:00+02:00", 48, "2025-10-28T02:30:00+01:00")]
    [InlineData("2025-06-15T11:20:00+02:00", 60, "2025-06-17T23:20:00+02:00")]
    [InlineData("2025-12-29T23:40:00+01:00", 72, "2026-01-01T23:40:00+01:00")]
    public void Given_DateAndTime_When_ProcessingData_Then_CorrectDate(string inputDate, int hoursToAdd, string expectedDueDate)
    {
        // Arrange & Act
        var result = DateTimeHelper.CalculateDueTime(inputDate, hoursToAdd);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedDueDate);
    }
}
