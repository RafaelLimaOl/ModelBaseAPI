using TelemetryApp.Aspire.ServiceDefaults;
using WorkerService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
//builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
