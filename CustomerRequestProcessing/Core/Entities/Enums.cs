using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace CustomerRequestProcessing.Core.Entities;

public enum SlaType
{
    Standard,
    Premium,
}

public enum MeterType
{
    Classic,
    Smart,
}

public enum TariffId
{
    T_ECO,
    T_BASIC,
    T_PRO,
}

public enum RequestStatus
{
    NotProcessed,
    Approved,
    Rejected,
}