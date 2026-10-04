using CsvHelper.Configuration;
using System;
using static CustomerRequestProcessing.Services.Helpers.CsvHelperExtensions;

namespace CustomerRequestProcessing.Core.Entities
{
    public class Request : IEntity
    {
        public required string RequestId { get; set; }
        public required string CustomerId { get; set; }
        public required string TargetTariffId { get; set; }
        public string? RequestedAt { get; set; }

        public Customer? Customer { get; set; }
        public Tariff? TargetTariff { get; set; }
    }

    public class RequestMap : ClassMap<Request>
    {
        public RequestMap()
        {
            Map(r => r.RequestId)
                .Name("RequestId");

            Map(r => r.CustomerId)
                .Name("CustomerId");

            Map(r => r.TargetTariffId)
                .Name("TargetTariffId");

            Map(r => r.RequestedAt)
                .Name("RequestedAtISO8601");
        }
    }

    public sealed class RequestValidator : IEntityValidator<Request>
    {
        public IEnumerable<string> Validate(Request request)
        {
            if (string.IsNullOrWhiteSpace(request.RequestId))
            {
                yield return "RequestId is required.";
            }

            //if (string.IsNullOrWhiteSpace(request.CustomerId))
            //{
            //    yield return "CustomerId is required.";
            //}

            //if (string.IsNullOrWhiteSpace(request.TargetTariffId))
            //{
            //    yield return "TargetTariffId is required.";
            //}

            //if (!request.RequestedAt.HasValue)
            //{
            //    yield return "RequestedAt is required.";
            //}
        }
    }
}
