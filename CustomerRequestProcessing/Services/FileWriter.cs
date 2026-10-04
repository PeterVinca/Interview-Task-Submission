using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CustomerRequestProcessing.Core;
using CustomerRequestProcessing.Core.CsvValidations;
using System.Globalization;

namespace CustomerRequestProcessing.Services;

internal interface IFileWriter
{
    void WriteToErrorFile(string filePath, IEnumerable<ValidationError> records);

    void WriteToFile<TEntity, TMapper>(string filePath, List<TEntity> records) 
        where TEntity : IEntity
        where TMapper : ClassMap<TEntity>;
}

internal class FileWriter : IFileWriter
{
    private readonly Logger _logger;

    internal FileWriter(Logger logger)
    {
        _logger = logger;
    }

    public void WriteToErrorFile(string filePath, IEnumerable<ValidationError> records)
    {
        _logger.Info($"Writing {records.Count()} records to error file");

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
        };

        using var writer = new StreamWriter(filePath, append: true);
        using var csv = new CsvWriter(writer, csvConfig);

        csv.WriteRecords(records);

        _logger.Info($"...Finished writing records to error file");
    }

    public void WriteToFile<TEntity, TMapper>(string filePath, List<TEntity> records)
        where TEntity : IEntity
        where TMapper : ClassMap<TEntity>
    {
        if (records == null || records.Count == 0)
        {
            _logger.Info($"No records to write to {filePath}");
            return;
        }

        _logger.Info($"Writing {records.Count} records to file");

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var shouldWriteHeader =
            !File.Exists(filePath) ||
            new FileInfo(filePath).Length == 0;

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = false
        };

        using var stream = File.Open(
            filePath,
            FileMode.Append,
            FileAccess.Write);

        using var writer = new StreamWriter(stream);
        using var csv = new CsvWriter(writer, config);

        csv.Context.RegisterClassMap<TMapper>();

        if (shouldWriteHeader)
        {
            csv.WriteHeader<TEntity>();
            csv.NextRecord();
        }

        csv.WriteRecords(records);

        _logger.Info($"...Finished writing records to file");
    }
}
