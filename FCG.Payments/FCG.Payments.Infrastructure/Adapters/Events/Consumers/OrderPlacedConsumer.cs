using FCG.Catalog.Application.Events;
using FCG.Payments.Application.Common.Ports;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Payments.Infrastructure.Adapters.Events.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly IPaymentservice _Paymentservice;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OrderPlacedEvent> _logger;

    public OrderPlacedConsumer(
                IPaymentservice Paymentservice,
        IPublishEndpoint publishEndpoint,
               ILogger<OrderPlacedEvent> logger)
    {
        _Paymentservice = Paymentservice;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var orderEvent = context.Message;

        _logger.LogInformation(
            "Recebido OrderPlacedEvent - OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}, Price: {Price}",
            orderEvent.OrderId, orderEvent.UserId, orderEvent.GameId, orderEvent.Price);

        var paymentResult = await _Paymentservice.ProcessPaymentAsync(orderEvent);

        await _publishEndpoint.Publish(paymentResult);

        _logger.LogInformation(
            "PaymentProcessedEvent publicado - OrderId: {OrderId}, Status: {Status}, UserId: {UserId}, GameId: {GameId},",
            paymentResult.OrderId, paymentResult.Status, orderEvent.UserId, orderEvent.GameId);
    }
}