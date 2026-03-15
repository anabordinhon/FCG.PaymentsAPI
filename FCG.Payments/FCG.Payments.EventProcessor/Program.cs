using FCG.Payments.Application;
using FCG.Payments.Infrastructure;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);

var awsLoggerConfig = new AWS.Logger.AWSLoggerConfig
{
    Region = builder.Configuration["AWS:Region"] ?? "us-east-1",
    LogGroup = builder.Configuration["AWS.Logging:LogGroup"] ?? "/fcg/payments/worker"
};
builder.Logging.AddAWSProvider(awsLoggerConfig);

const string serviceName = "FCG.Payments.Worker";
const string serviceVersion = "1.0.0";

var collectorEndpoint = builder.Configuration["OpenTelemetry:CollectorEndpoint"]
    ?? "http://host.docker.internal:4317";

builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: serviceVersion))
        .AddSource("MassTransit")
        .SetSampler(new AlwaysOnSampler())
        .AddConsoleExporter()
    )
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: serviceVersion))
        .AddRuntimeInstrumentation()
        .AddConsoleExporter()
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(collectorEndpoint);
            opts.Protocol = OtlpExportProtocol.Grpc;
        })
    )
    .WithLogging(logging => logging
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: serviceVersion))
        .AddConsoleExporter()
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(collectorEndpoint);
            opts.Protocol = OtlpExportProtocol.Grpc;
        })
    );


builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();
await host.RunAsync();