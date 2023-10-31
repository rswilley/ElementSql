namespace ElementSql.Interfaces
{
    internal interface ISqlDatabase
    {
        IConnectionSession Session { get; set; }
        IUnitOfWork UnitOfWork { get; set; }
        string Name { get; set; }
    }
}
