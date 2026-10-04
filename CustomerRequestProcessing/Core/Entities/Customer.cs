using CsvHelper.Configuration;
using System;
using static CustomerRequestProcessing.Services.Helpers.CsvHelperExtensions;

namespace CustomerRequestProcessing.Core.Entities
{
    public class Customer : IEntity
    {
        public required string CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public bool? HasUnpaidInvoice { get; set; }
        public SlaType? SlaType { get; set; }
        public MeterType? MeterType { get; set; }
    }

    public class CustomerMap : ClassMap<Customer>
    {
        public CustomerMap()
        {
            Map(c => c.CustomerId)
                .Name("CustomerId");

            Map(c => c.CustomerName)
                .Name("Name");

            Map(c => c.HasUnpaidInvoice)
                .Name("HasUnpaidInvoice");

            Map(c => c.SlaType)
                .Name("SLA")
                .TypeConverter<SlaTypeConverter>();

            Map(c => c.MeterType)
                .Name("MeterType")
                .TypeConverter<MeterTypeConverter>();
        }
    }

    /// <summary>
    /// Validates a Customer entity to ensure that required fields are present and valid.
    /// 
    /// Depends on how we would like to look at overall validation, if empty values are acceptable and in the reason of processed request for that customer would be Invalid Data
    /// or empty values are invalid and requests with such customers would not be processed at all
    /// </summary>
    public sealed class CustomerValidator: IEntityValidator<Customer>
    {
        public IEnumerable<string> Validate(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.CustomerId))
            {
                yield return "CustomerId is required.";
            }

            if (!customer.HasUnpaidInvoice.HasValue)
            {
                yield return "HasUnpaidInvoice has no value.";
            }

            if (!customer.SlaType.HasValue)
            {
                yield return "SLA has no value.";
            }

            if (!customer.MeterType.HasValue)
            {
                yield return "MeterType has no value.";
            }
        }
    }
}
