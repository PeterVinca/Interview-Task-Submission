using CustomerRequestProcessing;
using CustomerRequestProcessing.Services;
using Microsoft.Extensions.Configuration;

var configBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>();

var configuration = configBuilder.Build();

var logger = new Logger(configuration["FilePaths:LogFile"]);

var app = new Application(configuration, logger);
app.Run();