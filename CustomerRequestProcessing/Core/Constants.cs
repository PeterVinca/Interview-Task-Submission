namespace CustomerRequestProcessing.Core;
public static class Constants
{
    public static string DateTimeFormatForFile = "yyyy-MM-dd-HH-mm-ss";
    public static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    public static string DateTimeFormatISO8601 = "yyyy-MM-ddTHH:mm:sszzz";
    public static string DelimiterForJoiningInCsvFiles = " && ";

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

    public static class Errors
    {
        public static string UnknownCustomer = "Unknown customer.";
        public static string UnknownTariff = "Unknown tariff.";
        public static string InvalidRequestData = "Invalid request data.";
        public static string InvalidCustomerData = "Invalid customer data.";
        public static string InvalidTariffData = "Invalid tariff data.";
        public static string UnpaidInvoice = "Unpaid invoice.";
    }

    public static class ProcessedRequestMessages
    {
        public static string ScheduleMeterUpgrade = "Schedule meter upgrade";
        public static string NoData = "NoData";
    }
}
