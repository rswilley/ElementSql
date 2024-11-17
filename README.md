# Element Sql

ElementSql is a lightweight wrapper for Dapper that implements the repository pattern and makes it easy to work with database connections and transactions.

ElementSql supports all databases that Dapper supports.

## ElementSql Setup

### Setup a repository and its entity

ElementSql supports all Dapper methods. Take a look at RepositoryBase and QueryBase to see more.

```csharp
public interface IElementRepository : IRepository<Element>
{
    Task<Element?> GetByName(string name, IConnectionContext context);
}

public class ElementRepository : RepositoryBase<Element, int>, IElementRepository
{
    public async Task<Element?> GetByName(string name, IConnectionContext context)
    {
        return await QuerySingleOrDefaultAsync($"WHERE {nameof(Element.Name)} = @Name", new { name }, context);
    }
}

[Table("elements")]
public class Element : EntityBase<int>
{
    [Key]
    public override int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Symbol { get; set; } = null!;
}
```

### Setup a Query

Queries are for sql queries that don't map directly to an entity.

```csharp
public interface ITestQuery : IElementSqlQuery
{
    Task<DateTime> GetCurrentTime(IConnectionContext context);
}

public class TestQuery : QueryBase, ITestQuery
{
    public async Task<DateTime> GetCurrentTime(IConnectionContext context)
    {
        return await ExecuteScalarAsync<DateTime>("SELECT UTC_TIMESTAMP()", null!, context);
    }
}
```

### Optional: Create a ```DbContext```. 

This pattern can be used to add your repositories to ```StorageManager```. It is not required, but it has worked pretty well for me.

```csharp
public interface IDbContext
{
    IElementRepository ElementRepository { get; }
    ITestQuery TestQuery { get; }
}

public class DbContext : IDbContext
{
    public IElementRepository ElementRepository => _elementRepository ??= _serviceProvider.GetRequiredService<IElementRepository>();
    public ITestQuery TestQuery => _testQuery ??= _serviceProvider.GetRequiredService<ITestQuery>();

    public DbContext(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private readonly IServiceProvider _serviceProvider;

    private IElementRepository _elementRepository;
    private ITestQuery _testQuery;
}
```

### Setup your StorageManager

StorageManager is used to open up database connections and transactions.

```csharp
public interface IMyStorageManager : IStorageManager
{
    IDbContext DbContext { get; set; }
}

public class MyStorageManager : StorageManager, IMyStorageManager
{
    private readonly IServiceProvider _serviceProvider;

    public MyStorageManager(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        DbContext = new DbContext(serviceProvider);
    }

    public IDbContext DbContext { get; set; }
}
```

### Register your ```StorageManager```, repositories/queries, and setup your database. The following is a MySql example, but ElementSql will work with Sql Server, Postgres, Sqlite, etc (any database that implements ```IDbConnection```).

```csharp
//Program.cs
var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString")!;
builder.Services.AddTransient<IMyStorageManager, MyStorageManager>();
builder.Services.AddTransient<IElementRepository, ElementRepository>();
builder.Services.AddTransient<ITestQuery, TestQuery>();
builder.Services.AddElementSql(config =>
{
    config.Databases.Add("Default", () => new MySqlConnection(connectionString));
});
```

### Starting a transaction and writing data to the database

```csharp
//UnitOfWorkContext implements IDisposable. The default action is to rollback on error.
//Setting WasSuccessful to true will Commit the transaction when Dispose is called.
using var transaction = await _storageManager.StartUnitOfWorkAsync();
_ = await _storageManager.DbContext.ElementRepository.Add(new Element { Name = "Selenium" }, transaction);
transaction.WasSuccessful = true; //Commits the transaction when Dispose is called
```

### Opening a database connection and reading data from the database

```csharp
//SessionContext implements IDisposable and will automatically close the
//database connection when Dispose is called
using var session = await _storageManager.StartSessionAsync();
var user = await _storageManager.DbContext.ElementRepository.GetById(id, session);
```
