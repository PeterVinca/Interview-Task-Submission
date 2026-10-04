using CsvHelper.Configuration;
using static CustomerRequestProcessing.Services.Helpers.CsvHelperExtensions;

namespace CustomerRequestProcessing.Core.Entities;

public class ProcessedRequest : IEntity
{
    public required string RequestId { get; set; }
    public RequestStatus? RequestStatus { get; set; }
    public string? Reason { get; set; }
    public string? FollowUpAction { get; set; }
    public string? DueDate { get; set; }
}

public class ProcessedRequestMap : ClassMap<ProcessedRequest>
{
    public ProcessedRequestMap()
    {
        Map(r => r.RequestId)
            .Name("RequestId");

        Map(r => r.RequestStatus)
            .Name("Status")
            .TypeConverter<RequestStatusConverter>();

        Map(r => r.Reason)
            .Name("Reason");

        Map(r => r.FollowUpAction)
            .Name("FollowUpAction");

        Map(r => r.DueDate)
            .Name("DueDate");
    }
}
