using FCG.Catalog.Application.Events;
using FCG.Payments.Application.Common.Ports;
using FCG.Payments.Domain.Events;

namespace FCG.Payments.Application.Services;

public class Paymentservice : IPaymentservice
{
    private readonly Random _random = new();

    public Task<PaymentProcessedEvent> ProcessPaymentAsync(OrderPlacedEvent orderEvent)
    {
        var isApproved = _random.Next(1, 101) <= 90;

        var paymentProcessed = new PaymentProcessedEvent
        {
            OrderId = orderEvent.OrderId,
            UserId = orderEvent.UserId,
            GameId = orderEvent.GameId,
            Status = isApproved ? "Approved" : "Rejected",
            ProcessedAt = DateTime.UtcNow
        };

        return Task.FromResult(paymentProcessed);
    }
}