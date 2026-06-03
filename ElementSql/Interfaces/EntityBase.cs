namespace ElementSql.Interfaces;

// Entity contract used by context methods
public interface IEntityBase
{
    object Id { get; }
}

public abstract record EntityBase<TIdentity> : QueryBase, IEntityBase
{
    public abstract TIdentity Id { get; init; }
    object IEntityBase.Id => Id!;
}

public abstract record QueryBase;
