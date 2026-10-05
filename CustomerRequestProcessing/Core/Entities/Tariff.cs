using CsvHelper.Configuration;
using System.Formats.Tar;

namespace CustomerRequestProcessing.Core.Entities
{
    public class Tariff : IEntity
    {
        public required string TariffId { get; set; }
        public string? TariffName { get; set; }
        public bool? IsSmartMeterRequired { get; set; }
        public decimal? BaseMonthlyGross { get; set; }
    }

    public class TariffMap : ClassMap<Tariff>
    {
        public TariffMap()
        {
            Map(c => c.TariffId)
                .Name("TariffId");

            Map(c => c.TariffName)
                .Name("Name");

            Map(c => c.IsSmartMeterRequired)
                .Name("RequiresSmartMeter");

            Map(c => c.BaseMonthlyGross)
                .Name("BaseMonthlyGross");
        }
    }

    /// <summary>
    /// Validates a Request entity to ensure that required fields are present and valid.
    /// </summary>
    public sealed class TariffValidator : IEntityValidator<Tariff>
    {
        public IEnumerable<string> Validate(Tariff tariff)
        {
            if (string.IsNullOrWhiteSpace(tariff.TariffId))
            {
                yield return "TariffId is required.";
            }

            if (!tariff.IsSmartMeterRequired.HasValue)
            {
                yield return "RequiresSmartMeter has no value.";
            }
        }
    }
}
