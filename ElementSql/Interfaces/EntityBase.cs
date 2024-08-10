namespace ElementSql.Interfaces;

public abstract class EntityBase<TIdentity>
{
    public abstract TIdentity Id { get; set; }
}