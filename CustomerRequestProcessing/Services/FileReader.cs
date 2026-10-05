using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CustomerRequestProcessing.Core;
using CustomerRequestProcessing.Core.CsvValidations;
using System.Globalization;

namespace CustomerRequestProcessing.Services;

interface IFileReader<TEntity> where TEntity : IEntity
{
    FileReadResult<TEntity> GetRecords(string filePath, bool shouldCreateIfNotExists = false);
}

internal class FileReader<TEntity, TMapper> : IFileReader<TEntity>
    where TEntity : IEntity 
    where TMapper : ClassMap<TEntity>, new()
{
    private readonly IEntityValidator<TEntity>? _validator;

    public FileReader(IEntityValidator<TEntity>? validator)
    {
        _validator = validator;
    }

    public FileReadResult<TEntity> GetRecords(string filePath, bool shouldCreateIfNotExists = false)
    {
        if (shouldCreateIfNotExists && !File.Exists(filePath))
        {
            File.Create(filePath).Dispose();
            return new FileReadResult<TEntity>(new List<TEntity>(), new List<ValidationError>());
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File path: '{filePath}'");
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            BadDataFound = null,
            Delimiter = ";",
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        csv.Context.RegisterClassMap<TMapper>();

        var fileName = Path.GetFileName(filePath);
        if (!shouldCreateIfNotExists)
        {
            ValidateHeaders(csv, new TMapper(), fileName);
        }

        var records = new List<TEntity>();
        var errors = new List<ValidationError>();

        while (true)
        {
            try
            {
                if (!csv.Read())
                {
                    break;
                }

                var rowNumber = csv.Context.Parser.Row;

                var entity = csv.GetRecord<TEntity>();

                IEnumerable<string> validationErrors = [];

                if (_validator != null)
                {
                    validationErrors = _validator.Validate(entity);

                    foreach (var error in validationErrors)
                    {
                        errors.Add(new ValidationError(rowNumber, error, fileName));
                    }
                }

                records.Add(entity);
            }
            catch (CsvHelper.MissingFieldException ex)
            {
                var msg = $"Missing field in '{csv.Context.Parser.RawRecord.Replace("\n", "")}' row data. Check the file.";
                errors.Add(new ValidationError(csv.Context.Parser.Row, msg, fileName));
            }
            catch (TypeConverterException ex)
            {
                var msg = $"Type conversion failed on '{csv.Context.Parser.RawRecord.Replace("\n", "")}' row data. Check the file.";
                errors.Add(new ValidationError(csv.Context.Parser.Row, msg, fileName));
            }
            catch (BadDataException ex)
            {
                throw new InvalidFileFormatException($"Invalid CSV structure on row {csv.Context.Parser.Row} in file '{fileName}'.");
            }
            catch (CsvHelperException ex)
            {
                errors.Add(new ValidationError(csv.Context.Parser.Row, ex.Message, fileName));
            }
        } 

        return new FileReadResult<TEntity>(records, errors);
    }

    private void ValidateHeaders(CsvReader csv, TMapper map, string fileName)
    {
        var requiredHeaders = map.MemberMaps
            .SelectMany(m => m.Data.Names)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        csv.Read();
        csv.ReadHeader();

        var missingHeaders = requiredHeaders
            .Except(csv.HeaderRecord.Select(h => h.ToLowerInvariant()), StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (missingHeaders.Any())
        {
            throw new Exception($"Missing headers in file '{fileName}': {string.Join(", ", missingHeaders)}");
        }
    }
}
