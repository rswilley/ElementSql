namespace ElementSql.Interfaces;

public abstract class EntityBase<TIdentity>
{
    public abstract TIdentity Id { get; set; }
}

// Entity contract used by context methods
public interface IEntityRecordBase
{
    object Id { get; }
}

public abstract record EntityRecordBase<TIdentity> : QueryBase, IEntityRecordBase
{
    public abstract TIdentity Id { get; init; }
    object IEntityRecordBase.Id => Id!;
}

public abstract record QueryBase;
