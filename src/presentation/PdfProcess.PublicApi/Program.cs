using PdfProcess.Data.Configs;
using PdfProcess.DataAccess;
using PdfProcess.PublicApi.Controllers;
using PdfProcess.RabbitMq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<StorageConfig>(options =>
{
    builder.Configuration.GetSection(nameof(StorageConfig)).Bind(options);
});

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRabbitMq(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPdfEndpoints();

app.Run();
