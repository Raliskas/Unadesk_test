using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PdfProcess.DataAccess;
using PdfProcess.Services.RabbitMq;
using PdfProcess.Worker.Background;
using PdfProcess.Worker.Services;
using PdfProcess.Worker.Services.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

// Add services
builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddRabbitMq(builder.Configuration);

builder.Services.AddScoped<IPdfTextExtractor, PdfTextExtractorService>();
builder.Services.AddHostedService<BackgroundPdfWorker>();

var host = builder.Build();
host.Run();
