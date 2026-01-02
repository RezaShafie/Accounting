// File: Accounting.Domain/Common/BaseEntity.cs
namespace Accounting.Domain.Abstractions;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    protected BaseEntity() { }

    protected BaseEntity(Guid id)
    {
        Id = id;
    }

    // بررسی برابری بر اساس ID (Identity Equality)
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id == Guid.Empty || other.Id == Guid.Empty)
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return (GetType().ToString() + Id).GetHashCode();
    }
}