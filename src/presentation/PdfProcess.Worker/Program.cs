using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PdfProcess.Data.Configs;
using PdfProcess.DataAccess;
using PdfProcess.RabbitMq;
using PdfProcess.Worker.Background;
using PdfProcess.Worker.Services;
using PdfProcess.Worker.Services.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<StorageConfig>(options =>
{
    builder.Configuration.GetSection(nameof(StorageConfig)).Bind(options);
});

// Add services
builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddRabbitMq(builder.Configuration);

builder.Services.AddScoped<IPdfTextExtractor, PdfTextExtractorService>();
builder.Services.AddHostedService<BackgroundPdfWorker>();

var host = builder.Build();
host.Run();
