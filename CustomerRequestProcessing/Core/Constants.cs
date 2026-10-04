namespace CustomerRequestProcessing.Core;
internal static class Constants
{
    internal static string DateTimeFormatForFile = "yyyy-MM-dd-HH-mm-ss";
    internal static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    internal static string DateTimeFormatISO8601 = "yyyy-MM-ddTHH:mm:sszzz";
    internal static string DelimiterForJoiningInCsvFiles = " && ";

    internal static class Configurations
    {
        internal static string ProcessedRequestsFile = "FilePaths:ProcessedRequestsFile";

        internal static string CustomerInputFile = "FilePaths:Input:CustomerFile";
        internal static string RequestInputFile = "FilePaths:Input:RequestFile";
        internal static string TariffInputFile = "FilePaths:Input:TariffFile";

        internal static string OutputErrorFile = "FilePaths:Output:ErrorFile";

        internal static string PremiumSla = "Sla:Premium";
        internal static string StandardSla = "Sla:Standard";
        internal static string FollowUpActionTime = "Sla:FollowUpActionTime";
    }

    internal static class Errors
    {
        internal static string UnknownCustomer = "Unknown customer.";
        internal static string UnknownTariff = "Unknown tariff.";
        internal static string InvalidRequestData = "Invalid request data.";
        internal static string InvalidCustomerData = "Invalid customer data.";
        internal static string InvalidTariffData = "Invalid tariff data.";
        internal static string UnpaidInvoice = "Unpaid invoice.";
    }

    internal static class ProcessedRequestMessages
    {
        internal static string ScheduleMeterUpgrade = "Schedule meter upgrade";
        internal static string NoData = "NoData";
    }
}
