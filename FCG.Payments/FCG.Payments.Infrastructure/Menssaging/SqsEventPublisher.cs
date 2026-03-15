using Amazon.SQS;
using Amazon.SQS.Model;
using FCG.Payments.Application.Common.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace FCG.Payments.Infrastructure.Menssaging;

public class SqsEventPublisher : IEventPublisher
{
    private readonly AmazonSQSClient _sqsClient;
    private readonly string _queueUrl;
    private readonly ILogger<SqsEventPublisher> _logger;

    public SqsEventPublisher(IConfiguration configuration, ILogger<SqsEventPublisher> logger)
    {
        _sqsClient = new AmazonSQSClient(Amazon.RegionEndpoint.USEast1);
        _queueUrl = configuration["AWS:SQS:QueueUrl"]
            ?? throw new InvalidOperationException("URL da fila SQS não configurada.");
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : class
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? "";
        var spanId = Activity.Current?.SpanId.ToString() ?? "";

        var response = await _sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = JsonSerializer.Serialize(@event),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                ["TraceId"] = new MessageAttributeValue
                {
                    DataType = "String",
                    StringValue = traceId
                },
                ["SpanId"] = new MessageAttributeValue
                {
                    DataType = "String",
                    StringValue = spanId
                }
            }
        }, cancellationToken);

        _logger.LogInformation(
                   "Mensagem publicada no SQS. MessageId: {MessageId}, TraceId: {TraceId}",
                   response.MessageId, traceId);
    }
}