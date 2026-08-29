using CSharpFunctionalExtensions;

namespace Shop.Domain.Abstractions;

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : IComparable<TId>      // тащим ограничение дальше по цепочке
{
    protected AggregateRoot(TId id) : base(id) { }
    protected AggregateRoot() { }     // пустой ctor для EF, как в базе
}
