namespace FCG.Payments.Domain.Events;

public class PaymentProcessedEvent
{
    public Guid OrderId { get; init; }
    public int UserId { get; init; }
    public Guid GameId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; }
}