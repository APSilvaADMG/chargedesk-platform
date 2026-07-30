// Autor: Anderson Pereira Silva
// Data: 29/07/2026
// Descrição: Contratos de eventos de domínio (desacoplamento entre módulos).

namespace ChargeDesk.BuildingBlocks.Events;

public interface IDomainEvent
{
    Guid EventId { get; }
    Guid EmpresaId { get; }
    string EventType { get; }
    DateTime OccurredAt { get; }
    string Version { get; }
}

public abstract record DomainEventBase(
    Guid EmpresaId,
    DateTime OccurredAt,
    string Version = "v1") : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public abstract string EventType { get; }
}
