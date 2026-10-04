using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CustomerRequestProcessing.Core;
using CustomerRequestProcessing.Core.Entities;

namespace CustomerRequestProcessing.Services.Helpers;


public static class CsvHelperExtensions
{
    public class SlaTypeConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(
        string? text,
        IReaderRow row,
        MemberMapData memberMapData)
        {
            return text?.ToLowerInvariant() switch
            {
                "standard" => SlaType.Standard,
                "premium" => SlaType.Premium,
                _ => throw new CsvHelperException(row.Context, $"Unknown {nameof(SlaType)}: {text}")
            };
        }

        public override string ConvertToString(
            object? value,
            IWriterRow row,
            MemberMapData memberMapData)
        {
            return value switch
            {
                SlaType.Standard => "Standard",
                SlaType.Premium => "Premium",
                _ => string.Empty
            };
        }
    }

    public class MeterTypeConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(
        string? text,
        IReaderRow row,
        MemberMapData memberMapData)
        {
            return text?.ToLowerInvariant() switch
            {
                "classic" => MeterType.Classic,
                "smart" => MeterType.Smart,
                _ => throw new CsvHelperException(row.Context, $"Unknown {nameof(MeterType)}: {text}")
            };
        }

        public override string ConvertToString(
            object? value,
            IWriterRow row,
            MemberMapData memberMapData)
        {
            return value switch
            {
                MeterType.Classic => "Classic",
                MeterType.Smart => "Smart",
                _ => string.Empty
            };
        }
    }

    public class RequestStatusConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(
        string? text,
        IReaderRow row,
        MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return RequestStatus.NotProcessed;

            return text?.ToLowerInvariant() switch
            {
                "notprocessed" or "not processed" => RequestStatus.NotProcessed,
                "approved" => RequestStatus.Approved,
                "rejected" => RequestStatus.Rejected,
                _ => throw new CsvHelperException(row.Context, $"Unknown {nameof(RequestStatus)}: {text}")
            };
        }

        public override string ConvertToString(
            object? value,
            IWriterRow row,
            MemberMapData memberMapData)
        {
            return value switch
            {
                RequestStatus.NotProcessed => "Not Processed",
                RequestStatus.Approved => "Approved",
                RequestStatus.Rejected => "Rejected",
                _ => string.Empty
            };
        }
    }
}
