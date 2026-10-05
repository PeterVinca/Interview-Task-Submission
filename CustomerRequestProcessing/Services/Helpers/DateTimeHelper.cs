using CustomerRequestProcessing.Core;
using NodaTime;

namespace CustomerRequestProcessing.Services.Helpers;

public static class DateTimeHelper
{
    public static string CalculateDueTime(string requestedAtString, int hoursToAdd)
    {
        // the value for parsing is checked beforehand
        var requestedAt = DateTimeOffset.Parse(requestedAtString);

        var zone = DateTimeZoneProviders.Tzdb["Europe/Vienna"];

        var instant = Instant.FromDateTimeOffset(requestedAt);
        var zoned = instant.InZone(zone);

        // local time semantics
        var dueLocal = zoned.LocalDateTime.PlusHours(hoursToAdd);

        // back into Vienna zone
        var dueTimeZoned = zone.AtLeniently(dueLocal);

        string result = dueTimeZoned
            .ToDateTimeOffset()
            .ToString(Constants.DateTimeFormatISO8601);

        return result;
    }
}
