namespace ElementSql.Interfaces
{
    public interface IConnectionContext : IDisposable
    {
        bool WasSuccessful { get; set; }
    }
}
