using CsvHelper.Configuration;

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
                yield return "IsSmartMeterRequired has no value.";
            }
        }
    }
}
