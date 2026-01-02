namespace Accounting.Domain.Abstractions;

public abstract class AggregateRoot : BaseEntity
{
    protected AggregateRoot() { }
    protected AggregateRoot(Guid id) : base(id) { }

    // public List<IDomainEvent> DomainEvents { get; private set; } = new();
}