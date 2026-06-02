namespace ElementSql.Interfaces;

public abstract class EntityBase<TIdentity>
{
    public abstract TIdentity Id { get; set; }
}

public abstract record EntityBase
{
    public abstract object Id { get; set; }
}

public abstract record QueryBase;
